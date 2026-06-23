using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Domain.Helpers;

/// <summary>
/// Builds and sanitizes OpenAI-compatible function parameter schemas for LLM providers.
/// Moonshot/Kimi rejects non-array <c>enum</c> values and some JSON Schema draft features.
/// </summary>
public static class OpenAiToolSchemaBuilder
{
    private static readonly JsonSerializerOptions SchemaSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static FunctionParameters BuildMoonshotSafeParameters(string? parametersJson)
    {
        var parameters = SanitizeForMoonshot(Build(parametersJson));
        var node = JsonSerializer.SerializeToNode(parameters, SchemaSerializerOptions);
        if (node is JsonObject obj)
        {
            MoonshotToolSchemaSanitizer.SanitizeParametersObject(obj);
            return node.Deserialize<FunctionParameters>(SchemaSerializerOptions) ?? parameters;
        }

        return parameters;
    }
    public static FunctionParameters Build(string? parametersJson)
    {
        if (string.IsNullOrWhiteSpace(parametersJson))
        {
            return new FunctionParameters();
        }

        try
        {
            using var doc = JsonDocument.Parse(parametersJson);
            var root = doc.RootElement;

            return root.ValueKind switch
            {
                JsonValueKind.Array => BuildFromParameterArray(root),
                JsonValueKind.Object when root.TryGetProperty("properties", out _) => BuildFromJsonSchema(root),
                _ => new FunctionParameters(),
            };
        }
        catch (JsonException)
        {
            return new FunctionParameters();
        }
    }

    public static FunctionParameters SanitizeForMoonshot(FunctionParameters parameters)
    {
        var sanitized = new FunctionParameters
        {
            Type = string.IsNullOrWhiteSpace(parameters.Type) ? "object" : parameters.Type,
            Required = parameters.Required ?? Array.Empty<string>(),
        };

        foreach (var (name, property) in parameters.Properties)
        {
            sanitized.Properties[name] = SanitizeProperty(property);
        }

        return sanitized;
    }

    private static FunctionParameters BuildFromParameterArray(JsonElement array)
    {
        var parameters = new FunctionParameters();
        var required = new List<string>();

        foreach (var param in array.EnumerateArray())
        {
            if (!param.TryGetProperty("name", out var nameElement))
            {
                continue;
            }

            var name = nameElement.GetString();
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            parameters.Properties[name] = ParseProperty(param);

            if (param.TryGetProperty("required", out var requiredElement)
                && requiredElement.ValueKind == JsonValueKind.True)
            {
                required.Add(name);
            }
        }

        parameters.Required = required.ToArray();
        return parameters;
    }

    private static FunctionParameters BuildFromJsonSchema(JsonElement schema)
    {
        var parameters = new FunctionParameters();

        if (schema.TryGetProperty("properties", out var propertiesElement)
            && propertiesElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in propertiesElement.EnumerateObject())
            {
                parameters.Properties[property.Name] = ParseProperty(property.Value);
            }
        }

        if (schema.TryGetProperty("required", out var requiredElement)
            && requiredElement.ValueKind == JsonValueKind.Array)
        {
            parameters.Required = requiredElement.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString()!)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToArray();
        }

        return parameters;
    }

    private static FunctionProperty ParseProperty(JsonElement element)
    {
        var property = new FunctionProperty
        {
            Type = ResolveType(element),
            Description = element.TryGetProperty("description", out var descriptionElement)
                ? descriptionElement.GetString() ?? string.Empty
                : string.Empty,
        };

        var enumValues = ExtractEnumValues(element);
        if (enumValues is { Length: > 0 })
        {
            property.Enum = enumValues;
        }

        if (element.TryGetProperty("items", out var itemsElement))
        {
            property.Items = ParseProperty(itemsElement);
        }

        return property;
    }

    private static FunctionProperty SanitizeProperty(FunctionProperty property)
    {
        var sanitized = new FunctionProperty
        {
            Type = string.IsNullOrWhiteSpace(property.Type) ? "string" : property.Type,
            Description = property.Description ?? string.Empty,
        };

        var enumValues = NormalizeEnumValues(property.Enum);
        if (enumValues is { Length: > 0 })
        {
            sanitized.Enum = enumValues;
        }

        if (property.Items != null)
        {
            sanitized.Items = SanitizeProperty(property.Items);
        }

        return sanitized;
    }

    private static string ResolveType(JsonElement element)
    {
        if (element.TryGetProperty("type", out var typeElement)
            && typeElement.ValueKind == JsonValueKind.String)
        {
            return MapParameterType(typeElement.GetString() ?? "string");
        }

        if (element.TryGetProperty("anyOf", out var anyOfElement)
            && anyOfElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var option in anyOfElement.EnumerateArray())
            {
                if (option.TryGetProperty("type", out var optionType)
                    && optionType.ValueKind == JsonValueKind.String)
                {
                    return MapParameterType(optionType.GetString() ?? "string");
                }
            }
        }

        return "string";
    }

    private static string[]? ExtractEnumValues(JsonElement element)
    {
        if (element.TryGetProperty("enum", out var enumElement))
        {
            return NormalizeEnumElement(enumElement);
        }

        if (element.TryGetProperty("const", out var constElement))
        {
            return NormalizeEnumElement(constElement);
        }

        var compositeValues = ExtractEnumValuesFromComposite(element);
        if (compositeValues is { Length: > 0 })
        {
            return compositeValues;
        }

        return null;
    }

    private static string[]? ExtractEnumValuesFromComposite(JsonElement element)
    {
        foreach (var compositeProperty in new[] { "anyOf", "oneOf", "allOf" })
        {
            if (!element.TryGetProperty(compositeProperty, out var compositeElement)
                || compositeElement.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var collected = new List<string>();
            foreach (var option in compositeElement.EnumerateArray())
            {
                var values = ExtractEnumValues(option);
                if (values is { Length: > 0 })
                {
                    collected.AddRange(values);
                }
            }

            if (collected.Count > 0)
            {
                return collected
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
            }
        }

        return null;
    }

    private static string[]? NormalizeEnumValues(string[]? values)
    {
        if (values is not { Length: > 0 })
        {
            return null;
        }

        var normalized = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return normalized.Length > 0 ? normalized : null;
    }

    private static string[]? NormalizeEnumElement(JsonElement enumElement) =>
        enumElement.ValueKind switch
        {
            JsonValueKind.Array => enumElement.EnumerateArray()
                .Select(ConvertJsonValueToString)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToArray(),
            JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False
                => new[] { ConvertJsonValueToString(enumElement) },
            _ => null,
        };

    private static string ConvertJsonValueToString(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => element.GetRawText(),
        };

    private static string MapParameterType(string toolParameterType) =>
        toolParameterType.ToLowerInvariant() switch
        {
            "string" => "string",
            "number" => "number",
            "integer" => "integer",
            "boolean" => "boolean",
            "array" => "array",
            "object" => "object",
            _ => "string",
        };
}

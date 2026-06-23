using System.Text.Json;
using System.Text.Json.Nodes;

namespace Andy.Agentic.Domain.Helpers;

/// <summary>
/// Patches tool parameter JSON so Moonshot/Kimi accepts it (e.g. <c>enum</c> must be an array).
/// Applied to the final HTTP request body as a safety net after C# model serialization.
/// </summary>
public static class MoonshotToolSchemaSanitizer
{
    public static bool SanitizeToolsNode(JsonNode? toolsNode)
    {
        if (toolsNode is not JsonArray tools)
        {
            return false;
        }

        var modified = false;
        foreach (var tool in tools)
        {
            if (tool?["function"]?["parameters"] is JsonObject parameters
                && SanitizeParametersObject(parameters))
            {
                modified = true;
            }
        }

        return modified;
    }

    public static bool SanitizeParametersObject(JsonObject parameters)
    {
        if (parameters["properties"] is not JsonObject properties)
        {
            return false;
        }

        var modified = false;
        foreach (var (_, node) in properties.ToList())
        {
            if (node is JsonObject propertyObject && SanitizePropertyObject(propertyObject))
            {
                modified = true;
            }
        }

        return modified;
    }

    public static bool SanitizePropertyObject(JsonObject propertyObject)
    {
        var modified = false;

        if (propertyObject.TryGetPropertyValue("enum", out var enumNode) && enumNode is not null)
        {
            var normalized = NormalizeEnumNode(enumNode);
            if (normalized is null)
            {
                propertyObject.Remove("enum");
                modified = true;
            }
            else if (!JsonNode.DeepEquals(normalized, enumNode))
            {
                propertyObject["enum"] = normalized;
                modified = true;
            }
        }

        if (propertyObject.ContainsKey("const"))
        {
            var constValues = NormalizeEnumNode(propertyObject["const"]!);
            if (constValues is not null)
            {
                propertyObject["enum"] = constValues;
            }

            propertyObject.Remove("const");
            modified = true;
        }

        foreach (var compositeKey in new[] { "anyOf", "oneOf", "allOf" })
        {
            if (propertyObject[compositeKey] is not JsonArray compositeArray)
            {
                continue;
            }

            foreach (var option in compositeArray)
            {
                if (option is JsonObject optionObject && SanitizePropertyObject(optionObject))
                {
                    modified = true;
                }
            }
        }

        if (propertyObject["items"] is JsonObject items && SanitizePropertyObject(items))
        {
            modified = true;
        }

        if (propertyObject["properties"] is JsonObject nestedProperties)
        {
            foreach (var (_, node) in nestedProperties.ToList())
            {
                if (node is JsonObject nestedProperty && SanitizePropertyObject(nestedProperty))
                {
                    modified = true;
                }
            }
        }

        return modified;
    }

    public static JsonArray? NormalizeEnumNode(JsonNode enumNode)
    {
        if (enumNode is JsonArray array)
        {
            return NormalizeEnumArray(array);
        }

        if (enumNode is JsonValue value)
        {
            return value.GetValueKind() switch
            {
                JsonValueKind.String => new JsonArray(value.GetValue<string>()),
                JsonValueKind.Number => new JsonArray(value.ToJsonString()),
                JsonValueKind.True => new JsonArray(true),
                JsonValueKind.False => new JsonArray(false),
                _ => null,
            };
        }

        if (enumNode is JsonObject obj && obj["values"] is JsonArray valuesArray)
        {
            return NormalizeEnumArray(valuesArray);
        }

        if (enumNode is JsonObject enumObject)
        {
            return ExtractEnumValuesFromObject(enumObject);
        }

        return null;
    }

    private static JsonArray? NormalizeEnumArray(JsonArray array)
    {
        var normalized = new JsonArray();
        foreach (var item in array)
        {
            if (item is null)
            {
                continue;
            }

            normalized.Add(item.DeepClone());
        }

        return normalized.Count > 0 ? normalized : null;
    }

    private static JsonArray? ExtractEnumValuesFromObject(JsonObject obj)
    {
        var values = new JsonArray();
        foreach (var (_, value) in obj)
        {
            if (value is JsonValue jsonValue)
            {
                values.Add(jsonValue.DeepClone());
            }
        }

        return values.Count > 0 ? values : null;
    }
}

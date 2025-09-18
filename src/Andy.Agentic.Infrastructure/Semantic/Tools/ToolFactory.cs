using System.Globalization;
using System.Text.Json;
using Andy.Agentic.Domain.Interfaces.Llm.Semantic;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Domain.Models.Semantic;
using Microsoft.SemanticKernel;

namespace Andy.Agentic.Infrastructure.Semantic.Tools;

/// <summary>
/// Represents an abstract base class for creating tool instances.
/// Implements the IToolFactory interface.
/// </summary>
public abstract class ToolFactory : IToolFactory
{
    private static readonly JsonDocumentOptions DefaultDocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>
    /// Parses a JSON configuration string into a dictionary.
    /// </summary>
    protected static Dictionary<string, object> ParseConfiguration(string? configuration)
    {
        if (string.IsNullOrEmpty(configuration))
        {
            return new Dictionary<string, object>();
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(configuration)
                   ?? new Dictionary<string, object>();
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Invalid JSON in tool configuration", nameof(configuration), ex);
        }
    }

    /// <summary>
    /// Gets a required configuration value with type conversion.
    /// </summary>
    protected static T GetRequiredConfigValue<T>(Dictionary<string, object> config, string key)
    {
        if (!config.TryGetValue(key, out var value))
        {
            throw new ArgumentException($"Required configuration value '{key}' is missing");
        }

        if (value is JsonElement jsonElement)
        {
            return JsonSerializer.Deserialize<T>(jsonElement.GetRawText())!;
        }

        return (T)Convert.ChangeType(value, typeof(T));
    }

    /// <summary>
    /// Converts JSON parameter specification to a list of ParameterSpec objects.
    /// </summary>
    protected static List<ParameterSpec> ConvertParamsToDictionary(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<ParameterSpec>();
        }

        return JsonSerializer.Deserialize<List<ParameterSpec>>(json) ?? new List<ParameterSpec>();
    }

    /// <summary>
    /// Parses headers from JSON array format into a dictionary.
    /// </summary>
    protected static Dictionary<string, object> ParseHeaders(string? headers)
    {
        if (string.IsNullOrEmpty(headers))
        {
            return new Dictionary<string, object>();
        }

        try
        {
            var headerArray = JsonSerializer.Deserialize<JsonElement[]>(headers);
            if (headerArray == null)
            {
                return new Dictionary<string, object>();
            }

            var result = new Dictionary<string, object>();

            foreach (var item in headerArray)
            {
                if (item.TryGetProperty("name", out var nameElement) &&
                    item.TryGetProperty("value", out var valueElement))
                {
                    var name = nameElement.GetString();
                    var value = valueElement.GetString();

                    if (!string.IsNullOrEmpty(name))
                    {
                        result[name] = value ?? string.Empty;
                    }
                }
            }

            return result;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Invalid JSON in tool headers", nameof(headers), ex);
        }
    }

    /// <summary>
    /// Parses tool call arguments from KernelArguments.
    /// </summary>
    protected static Dictionary<string, object> ParseToolCallArguments(KernelArguments args)
    {
        try
        {
            var result = new Dictionary<string, object>(StringComparer.Ordinal);

            if (args.Count <= 0)
            {
                return result;
            }

            foreach (var kvp in args)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    continue;
                }

                var processedValue = ProcessArgumentValue(kvp.Value);
                if (processedValue != null)
                {
                    result[kvp.Key] = processedValue;
                }
            }

            return result;
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Processes argument values with type conversion and JSON parsing.
    /// </summary>
    private static object? ProcessArgumentValue(object? valueObj)
    {
        return valueObj switch
        {
            JsonElement el => ConvertJsonElement(el),
            string s when IsLikelyJson(s) && TryParseJson(s, out var parsed) => parsed,
            _ => valueObj
        };
    }

    /// <summary>
    /// Attempts to parse a JSON string.
    /// </summary>
    private static bool TryParseJson(string json, out object? result)
    {
        try
        {
            using var doc = JsonDocument.Parse(json, DefaultDocumentOptions);
            result = ConvertJsonElement(doc.RootElement);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Converts a JsonElement to the appropriate .NET type.
    /// </summary>
    private static object? ConvertJsonElement(JsonElement el)
    {
        return el.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.True or JsonValueKind.False => el.GetBoolean(),
            JsonValueKind.Number => ConvertNumber(el),
            JsonValueKind.String => ConvertString(el),
            JsonValueKind.Array => ConvertArray(el),
            JsonValueKind.Object => ConvertObject(el),
            _ => null
        };
    }

    /// <summary>
    /// Converts a numeric JsonElement to the most appropriate numeric type.
    /// </summary>
    private static object ConvertNumber(JsonElement el)
    {
        if (el.TryGetInt64(out var longValue))
        {
            return longValue;
        }

        if (el.TryGetDecimal(out var decimalValue))
        {
            return decimalValue;
        }

        return el.GetDouble();
    }

    /// <summary>
    /// Converts a string JsonElement with type inference and nested JSON parsing.
    /// </summary>
    private static object? ConvertString(JsonElement el)
    {
        var stringValue = el.GetString();
        if (stringValue == null)
        {
            return null;
        }

        if (bool.TryParse(stringValue, out var boolValue))
        {
            return boolValue;
        }

        if (long.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue))
        {
            return longValue;
        }

        if (decimal.TryParse(stringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue))
        {
            return decimalValue;
        }

        var trimmed = stringValue.Trim();
        if (!IsLikelyJson(trimmed))
        {
            return stringValue;
        }

        try
        {
            using var innerDoc = JsonDocument.Parse(trimmed);
            return ConvertJsonElement(innerDoc.RootElement);
        }
        catch
        {
            // Fall through to return original string
        }

        return stringValue;
    }

    /// <summary>
    /// Converts a JSON array to a List of objects.
    /// </summary>
    private static List<object?> ConvertArray(JsonElement el)
    {
        var list = new List<object?>();
        foreach (var item in el.EnumerateArray())
        {
            list.Add(ConvertJsonElement(item));
        }
        return list;
    }

    /// <summary>
    /// Converts a JSON object to a Dictionary.
    /// </summary>
    private static Dictionary<string, object> ConvertObject(JsonElement el)
    {
        var dict = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (var prop in el.EnumerateObject())
        {
            var value = ConvertJsonElement(prop.Value);
            if (value != null)
            {
                dict[prop.Name] = value;
            }
        }
        return dict;
    }

    /// <summary>
    /// Determines if a string looks like JSON (object or array).
    /// </summary>
    private static bool IsLikelyJson(string s)
        => s.Length >= 2 && ((s[0] == '{' && s[^1] == '}') || (s[0] == '[' && s[^1] == ']'));

    /// <summary>
    /// Creates a tool asynchronously based on the provided configuration.
    /// </summary>
    /// <param name="agent"></param>
    /// <param name="config">The configuration settings for the tool.</param>
    /// <returns>A task that represents the asynchronous operation, containing the created KernelFunction.</returns>
    public abstract KernelFunction CreateToolAsync(Agent agent, Tool config);
}

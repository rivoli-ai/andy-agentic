using System.Text.Json;
using System.Text.Json.Serialization;

namespace Andy.Agentic.Domain.Models.Semantic;

public sealed class ParameterSpecConverter : JsonConverter<Type?>
{
    public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var typeStr = reader.GetString();
            return MapSchemaTypeToClr(typeStr, format: null);
        }

        reader.Skip();
        return null;
    }

    public override void Write(Utf8JsonWriter writer, Type? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(MapClrTypeToSchemaType(value, out _));
    }

    private static Type MapSchemaTypeToClr(string? type, string? format)
    {
        switch (type?.Trim().ToLowerInvariant())
        {
            case "string":
                return typeof(string);

            case "integer":
                return (format?.ToLowerInvariant()) switch
                {
                    "int32" => typeof(int),
                    "int64" => typeof(long),
                    _ => typeof(long) // default integer -> Int64
                };

            case "number":
                return (format?.ToLowerInvariant()) switch
                {
                    "float" => typeof(float),
                    "double" => typeof(double),
                    _ => typeof(double) // default number -> Double
                };

            case "boolean":
                return typeof(bool);

            case "array":
                return typeof(object[]);

            case "object":
                return typeof(object);

            default:
                // Unknown -> treat as string
                return typeof(string);
        }
    }

    private static string MapClrTypeToSchemaType(Type? type, out string? format)
    {
        format = null;

        if (type == typeof(string))
            return "string";
        if (type == typeof(bool))
            return "boolean";

        if (type == typeof(int))
        {
            format = "int32";
            return "integer";
        }

        if (type == typeof(long))
        {
            format = "int64";
            return "integer";
        }

        if (type == typeof(float))
        {
            format = "float";
            return "number";
        }

        if (type == typeof(double))
        {
            format = "double";
            return "number";
        }

        if (type == typeof(object[]))
            return "array";

        // Fallback
        return "object";
    }
}

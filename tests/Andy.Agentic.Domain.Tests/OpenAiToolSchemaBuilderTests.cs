using System.Text.Json;
using Andy.Agentic.Domain.Helpers;
using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Domain.Tests;

public class OpenAiToolSchemaBuilderTests
{
    [Fact]
    public void Build_ArrayParameters_PreservesValidEnumArray()
    {
        const string parameters = """
            [
              {
                "name": "libraryName",
                "type": "string",
                "required": true,
                "description": "Library name",
                "enum": ["/microsoft/dotnet", "/microsoft/azure"]
              }
            ]
            """;

        var schema = OpenAiToolSchemaBuilder.SanitizeForMoonshot(OpenAiToolSchemaBuilder.Build(parameters));

        Assert.Equal(new[] { "/microsoft/dotnet", "/microsoft/azure" }, schema.Properties["libraryName"].Enum);
        Assert.Equal(new[] { "libraryName" }, schema.Required);
    }

    [Fact]
    public void SanitizeForMoonshot_ConvertsScalarEnumToArray()
    {
        const string parameters = """
            {
              "type": "object",
              "properties": {
                "libraryName": {
                  "type": "string",
                  "enum": "/microsoft/dotnet"
                }
              }
            }
            """;

        var schema = OpenAiToolSchemaBuilder.SanitizeForMoonshot(OpenAiToolSchemaBuilder.Build(parameters));

        Assert.Equal(new[] { "/microsoft/dotnet" }, schema.Properties["libraryName"].Enum);
    }

    [Fact]
    public void SanitizeForMoonshot_ConvertsConstToEnumArray()
    {
        const string parameters = """
            {
              "type": "object",
              "properties": {
                "libraryName": {
                  "type": "string",
                  "const": "/microsoft/dotnet"
                }
              }
            }
            """;

        var schema = OpenAiToolSchemaBuilder.SanitizeForMoonshot(OpenAiToolSchemaBuilder.Build(parameters));

        Assert.Equal(new[] { "/microsoft/dotnet" }, schema.Properties["libraryName"].Enum);
    }

    [Fact]
    public void SanitizeForMoonshot_DropsInvalidEnumObject()
    {
        const string parameters = """
            {
              "type": "object",
              "properties": {
                "libraryName": {
                  "type": "string",
                  "enum": { "values": ["/microsoft/dotnet"] }
                }
              }
            }
            """;

        var schema = OpenAiToolSchemaBuilder.SanitizeForMoonshot(OpenAiToolSchemaBuilder.Build(parameters));

        Assert.Null(schema.Properties["libraryName"].Enum);
    }

    [Fact]
    public void Build_OneOfConstValues_CollectsEnumArray()
    {
        const string parameters = """
            {
              "type": "object",
              "properties": {
                "libraryName": {
                  "oneOf": [
                    { "const": "/microsoft/dotnet" },
                    { "const": "/microsoft/azure" }
                  ]
                }
              }
            }
            """;

        var schema = OpenAiToolSchemaBuilder.SanitizeForMoonshot(OpenAiToolSchemaBuilder.Build(parameters));

        Assert.Equal(new[] { "/microsoft/dotnet", "/microsoft/azure" }, schema.Properties["libraryName"].Enum);
    }

    [Fact]
    public void SerializeToolParameters_EnumIsJsonArray()
    {
        const string parameters = """
            {
              "type": "object",
              "properties": {
                "libraryName": {
                  "type": "string",
                  "enum": "/microsoft/dotnet"
                }
              }
            }
            """;

        var tool = new OpenAiTool
        {
            Function = new Function
            {
                Name = "resolve_library",
                Parameters = OpenAiToolSchemaBuilder.SanitizeForMoonshot(OpenAiToolSchemaBuilder.Build(parameters)),
            },
        };

        var request = new Dictionary<string, object>
        {
            ["model"] = "kimi-k2.6",
            ["tools"] = new List<OpenAiTool> { tool },
        };

        var json = JsonSerializer.Serialize(request);
        Assert.Contains("\"enum\":[\"/microsoft/dotnet\"]", json);
    }
}

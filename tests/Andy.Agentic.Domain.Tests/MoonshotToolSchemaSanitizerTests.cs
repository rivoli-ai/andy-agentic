using System.Text.Json;
using System.Text.Json.Nodes;
using Andy.Agentic.Domain.Helpers;

namespace Andy.Agentic.Domain.Tests;

public class MoonshotToolSchemaSanitizerTests
{
    [Fact]
    public void SanitizeToolsNode_ConvertsScalarEnumToArray()
    {
        const string input = """
            [
              {
                "type": "function",
                "function": {
                  "name": "resolve_library",
                  "parameters": {
                    "type": "object",
                    "properties": {
                      "libraryName": {
                        "type": "string",
                        "enum": "/microsoft/dotnet"
                      }
                    }
                  }
                }
              }
            ]
            """;

        var tools = JsonNode.Parse(input);
        Assert.True(MoonshotToolSchemaSanitizer.SanitizeToolsNode(tools));

        var enumNode = tools![0]!["function"]!["parameters"]!["properties"]!["libraryName"]!["enum"];
        Assert.IsType<JsonArray>(enumNode);
        Assert.Equal("/microsoft/dotnet", enumNode!.AsArray()[0]!.GetValue<string>());
    }

    [Fact]
    public void SanitizeToolsNode_ConvertsConstToEnumArray()
    {
        const string input = """
            [
              {
                "type": "function",
                "function": {
                  "name": "resolve_library",
                  "parameters": {
                    "type": "object",
                    "properties": {
                      "libraryName": {
                        "type": "string",
                        "const": "/microsoft/dotnet"
                      }
                    }
                  }
                }
              }
            ]
            """;

        var tools = JsonNode.Parse(input);
        Assert.True(MoonshotToolSchemaSanitizer.SanitizeToolsNode(tools));

        var property = tools![0]!["function"]!["parameters"]!["properties"]!["libraryName"]!.AsObject();
        Assert.False(property.ContainsKey("const"));
        Assert.Equal("/microsoft/dotnet", property["enum"]!.AsArray()[0]!.GetValue<string>());
    }

    [Fact]
    public void SanitizeToolsNode_ConvertsEnumObjectWithValuesArray()
    {
        const string input = """
            [
              {
                "type": "function",
                "function": {
                  "name": "resolve_library",
                  "parameters": {
                    "type": "object",
                    "properties": {
                      "libraryName": {
                        "type": "string",
                        "enum": { "values": ["/microsoft/dotnet"] }
                      }
                    }
                  }
                }
              }
            ]
            """;

        var tools = JsonNode.Parse(input);
        Assert.True(MoonshotToolSchemaSanitizer.SanitizeToolsNode(tools));

        var property = tools![0]!["function"]!["parameters"]!["properties"]!["libraryName"]!.AsObject();
        Assert.IsType<JsonArray>(property["enum"]);
        Assert.Equal("/microsoft/dotnet", property["enum"]!.AsArray()[0]!.GetValue<string>());
    }
}

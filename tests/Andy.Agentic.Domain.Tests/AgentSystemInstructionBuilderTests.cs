using Andy.Agentic.Domain.Helpers;
using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Domain.Tests;

public class AgentSystemInstructionBuilderTests
{
    [Fact]
    public void Build_WithNoPrompts_ReturnsNull()
    {
        var agent = new Agent { Prompts = [] };

        Assert.Null(AgentSystemInstructionBuilder.Build(agent));
    }

    [Fact]
    public void Build_WithEmptyPromptContent_ReturnsNull()
    {
        var agent = new Agent
        {
            Prompts =
            [
                new Prompt { Content = "   " },
                new Prompt { Content = string.Empty },
            ],
        };

        Assert.Null(AgentSystemInstructionBuilder.Build(agent));
    }

    [Fact]
    public void Build_JoinsNonEmptyPromptsWithNewline()
    {
        var agent = new Agent
        {
            Prompts =
            [
                new Prompt { Content = "  You are helpful.  " },
                new Prompt { Content = "Be concise." },
            ],
        };

        var instruction = AgentSystemInstructionBuilder.Build(agent);

        Assert.Equal("You are helpful.\nBe concise.", instruction);
    }
}

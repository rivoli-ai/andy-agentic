using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Domain.Models;
using Mapster;
using TagDto = Andy.Agentic.Application.DTOs.TagDto;
using ToolDto = Andy.Agentic.Application.DTOs.ToolDto;

namespace Andy.Agentic.Application.Mapping;

/// <summary>
///     Mapster registration for domain model ↔ DTO mappings.
/// </summary>
public class DtosMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Agent, AgentDto>().TwoWays();
        config.NewConfig<LlmConfig, LlmConfigDto>().TwoWays();
        config.NewConfig<Tool, ToolDto>().TwoWays();
        config.NewConfig<Prompt, PromptDto>().TwoWays();
        config.NewConfig<PromptVariable, PromptVariableDto>().TwoWays();

        config.NewConfig<ChatMessage, ChatMessageDto>()
            .Map(dest => dest.Images, src => src.Images);
        config.NewConfig<ChatMessageDto, ChatMessage>()
            .Map(dest => dest.Images, src => src.Images);

        config.NewConfig<AgentTool, AgentToolDto>().TwoWays();
        config.NewConfig<McpServer, McpServerDto>().TwoWays();
        config.NewConfig<Tag, TagDto>().TwoWays();
        config.NewConfig<ToolExecutionLog, ToolExecutionLogDto>().TwoWays();
        config.NewConfig<AgentTag, AgentTagDto>().TwoWays();
        config.NewConfig<AgentMcpServer, AgentMcpServerDto>().TwoWays();
        config.NewConfig<Document, DocumentDto>().TwoWays();
        config.NewConfig<AgentDocument, AgentDocumentDto>().TwoWays();
        config.NewConfig<ChatHistory, ChatHistoryDto>().TwoWays();
        config.NewConfig<ChatMessage, ChatHistoryDto>().TwoWays();
        config.NewConfig<ChatMessage, ChatMessagePreviewDto>().TwoWays();
        config.NewConfig<ChatMessage, ChatSessionDto>().TwoWays();
        config.NewConfig<ChatMessage, ChatSessionSummaryDto>().TwoWays();
        config.NewConfig<TestConnection, TestConnectionDto>().TwoWays();
        config.NewConfig<ChatImage, ChatImageDto>().TwoWays();
    }
}

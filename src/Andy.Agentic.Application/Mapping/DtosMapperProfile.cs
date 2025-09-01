using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Domain.Models;
using AutoMapper;
using TagDto = Andy.Agentic.Application.DTOs.TagDto;
using ToolDto = Andy.Agentic.Application.DTOs.ToolDto;

namespace Andy.Agentic.Application.Mapping;

/// <summary>
///     AutoMapper profile for mapping between Domain entities and Application DTOs.
///     Provides bidirectional mappings for all entities in the system.
/// </summary>
public class DtosMapperProfile : Profile
{
    /// <summary>
    ///     Maps domain entities to their corresponding DTOs and vice versa.
    /// </summary>
    public DtosMapperProfile()
    {
        Map();
    }

    private void Map()
    {
        CreateMap<Agent, AgentDto>().ReverseMap();

        CreateMap<LlmConfig, LlmConfigDto>().ReverseMap();

        CreateMap<Tool, ToolDto>().ReverseMap();

        CreateMap<Prompt, PromptDto>().ReverseMap();

        CreateMap<PromptVariable, PromptVariableDto>().ReverseMap();

        CreateMap<ChatMessage, ChatMessageDto>().ReverseMap();

        CreateMap<AgentTool, AgentToolDto>().ReverseMap();

        CreateMap<McpServer, McpServerDto>().ReverseMap();

        CreateMap<Tag, TagDto>().ReverseMap();

        CreateMap<ToolExecutionLog, ToolExecutionLogDto>().ReverseMap();

        CreateMap<AgentTag, AgentTagDto>().ReverseMap();

        CreateMap<AgentMcpServer, AgentMcpServerDto>().ReverseMap();

        CreateMap<ChatHistory, ChatHistoryDto>().ReverseMap();

        CreateMap<ChatMessage, ChatHistoryDto>().ReverseMap();

        CreateMap<ChatMessage, ChatMessagePreviewDto>().ReverseMap();

        CreateMap<ChatMessage, ChatSessionDto>().ReverseMap();

        CreateMap<ChatMessage, ChatSessionSummaryDto>().ReverseMap();

        CreateMap<TestConnection, TestConnectionDto>().ReverseMap();
    }
}

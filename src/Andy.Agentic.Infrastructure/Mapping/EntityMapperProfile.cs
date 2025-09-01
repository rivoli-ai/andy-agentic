using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Models;
using AutoMapper;

namespace Andy.Agentic.Infrastructure.Mapping;

/// <summary>
///     AutoMapper profile for mapping between Domain entities and Application s.
///     Provides bidirectional mappings for all entities in the system.
/// </summary>
public class EntityMapperProfile : Profile
{
    /// <summary>
    ///     Maps domain entities to their corresponding s and vice versa.
    /// </summary>
    public EntityMapperProfile()
    {
        Map();
    }


    private void Map()
    {
        CreateMap<AgentEntity, Agent>()
            .ReverseMap();

        CreateMap<LlmConfigEntity, LlmConfig>()
            .ReverseMap();

        CreateMap<ToolEntity, Tool>()
            .ReverseMap();

        CreateMap<PromptEntity, Prompt>()
            .ReverseMap();

        CreateMap<PromptVariableEntity, PromptVariable>()
            .ReverseMap();

        CreateMap<ChatMessageEntity, ChatMessage>()
            .ReverseMap();

        CreateMap<AgentToolEntity, AgentTool>()
            .ReverseMap();

        CreateMap<McpServerEntity, McpServer>()
            .ReverseMap();

        CreateMap<TagEntity, Tag>()
            .ReverseMap();

        CreateMap<ToolExecutionLogEntity, ToolExecutionLog>()
            .ReverseMap();

        CreateMap<AgentTagEntity, AgentTag>()
            .ReverseMap();

        CreateMap<AgentMcpServerEntity, AgentMcpServer>()
            .ReverseMap();

        CreateMap<ChatHistoryEntity, ChatHistory>()
            .ReverseMap();

        CreateMap<ChatMessageEntity, ChatHistory>()
          .ForMember(dest => dest.AgentName, opt => opt.MapFrom(src => src.AgentName ?? string.Empty))
          .ForMember(dest => dest.AgentId, opt => opt.MapFrom(src => src.AgentId ?? Guid.Empty))
          .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.SessionId ?? string.Empty))
          .ForMember(dest => dest.TokenCount, opt => opt.MapFrom(src => src.TokenCount ?? 0));

        CreateMap<ChatMessageEntity, ChatMessagePreview>();

        CreateMap<ChatMessageEntity, ChatSession>()
            .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.SessionId ?? string.Empty))
            .ForMember(dest => dest.AgentId, opt => opt.MapFrom(src => src.AgentId ?? Guid.Empty))
            .ForMember(dest => dest.AgentName, opt => opt.MapFrom(src => src.AgentName ?? string.Empty))
            .ForMember(dest => dest.StartedAt, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.LastActivityAt, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.MessageCount, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.TotalTokens, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.SessionTitle, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.Description, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.IsActive, opt => opt.Ignore()); // Will be set manually

        CreateMap<ChatMessageEntity, ChatSessionSummary>()
            .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.SessionId ?? string.Empty))
            .ForMember(dest => dest.AgentId, opt => opt.MapFrom(src => src.AgentId ?? Guid.Empty))
            .ForMember(dest => dest.AgentName, opt => opt.MapFrom(src => src.AgentName ?? string.Empty))
            .ForMember(dest => dest.StartedAt, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.LastActivityAt, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.MessageCount, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.TotalTokens, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.SessionTitle, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.Description, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.RecentMessages, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.IsActive, opt => opt.Ignore()); // Will be set manually
    }
}

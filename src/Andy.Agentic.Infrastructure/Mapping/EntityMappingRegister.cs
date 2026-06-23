using System.Text.Json;
using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Models;
using Mapster;

namespace Andy.Agentic.Infrastructure.Mapping;

/// <summary>
///     Mapster registration for entity ↔ domain model mappings.
/// </summary>
public class EntityMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AgentEntity, Agent>()
            .Map(dest => dest.AgentDocuments, src => src.AgentDocuments);
        config.NewConfig<Agent, AgentEntity>()
            .Map(dest => dest.AgentDocuments, src => src.AgentDocuments)
            .Ignore(dest => dest.LlmConfig)
            .Ignore(dest => dest.EmbeddingLlmConfig);

        config.NewConfig<LlmConfigEntity, LlmConfig>();
        config.NewConfig<LlmConfig, LlmConfigEntity>()
            .Ignore(dest => dest.CreatedByUser);
        config.NewConfig<ToolEntity, Tool>();
        config.NewConfig<Tool, ToolEntity>()
            .Ignore(dest => dest.CreatedByUser);
        config.NewConfig<PromptEntity, Prompt>().TwoWays();
        config.NewConfig<PromptVariableEntity, PromptVariable>().TwoWays();

        config.NewConfig<ChatMessageEntity, ChatMessage>()
            .Map(dest => dest.Images, src => DeserializeImages(src.Images))
            .Map(dest => dest.SkillsUsed, src => DeserializeStringList(src.SkillsUsed));
        config.NewConfig<ChatMessage, ChatMessageEntity>()
            .Map(dest => dest.Images, src => SerializeImages(src.Images))
            .Map(dest => dest.SkillsUsed, src => SerializeStringList(src.SkillsUsed));

        config.NewConfig<AgentToolEntity, AgentTool>().TwoWays();
        config.NewConfig<McpServerEntity, McpServer>().TwoWays();
        config.NewConfig<SkillRegistryEntity, SkillRegistry>().TwoWays();
        config.NewConfig<AgentSkillEntity, AgentSkill>().TwoWays();
        config.NewConfig<TagEntity, Tag>().TwoWays();

        config.NewConfig<ToolExecutionLogEntity, ToolExecutionLog>()
            .Map(dest => dest.Parameters, src => DeserializeParameters(src.Parameters));
        config.NewConfig<ToolExecutionLog, ToolExecutionLogEntity>()
            .Map(dest => dest.Parameters, src => SerializeParameters(src.Parameters));

        config.NewConfig<AgentTagEntity, AgentTag>().TwoWays();
        config.NewConfig<AgentMcpServerEntity, AgentMcpServer>().TwoWays();
        config.NewConfig<DocumentEntity, Document>().TwoWays();
        config.NewConfig<AgentDocumentEntity, AgentDocument>().TwoWays();
        config.NewConfig<ChatHistoryEntity, ChatHistory>().TwoWays();

        config.NewConfig<ChatMessageEntity, ChatHistory>()
            .Map(dest => dest.AgentName, src => src.AgentName ?? string.Empty)
            .Map(dest => dest.AgentId, src => src.AgentId ?? Guid.Empty)
            .Map(dest => dest.SessionId, src => src.SessionId ?? string.Empty)
            .Map(dest => dest.TokenCount, src => src.TokenCount ?? 0)
            .Map(dest => dest.Images, src => DeserializeImages(src.Images))
            .Map(dest => dest.SkillsUsed, src => DeserializeStringList(src.SkillsUsed));

        config.NewConfig<ChatMessageEntity, ChatMessagePreview>();

        config.NewConfig<ChatMessageEntity, ChatSession>()
            .Map(dest => dest.SessionId, src => src.SessionId ?? string.Empty)
            .Map(dest => dest.AgentId, src => src.AgentId ?? Guid.Empty)
            .Map(dest => dest.AgentName, src => src.AgentName ?? string.Empty)
            .Ignore(dest => dest.StartedAt)
            .Ignore(dest => dest.LastActivityAt)
            .Ignore(dest => dest.MessageCount)
            .Ignore(dest => dest.TotalTokens)
            .Ignore(dest => dest.SessionTitle)
            .Ignore(dest => dest.Description)
            .Ignore(dest => dest.IsActive);

        config.NewConfig<ChatMessageEntity, ChatSessionSummary>()
            .Map(dest => dest.SessionId, src => src.SessionId ?? string.Empty)
            .Map(dest => dest.AgentId, src => src.AgentId ?? Guid.Empty)
            .Map(dest => dest.AgentName, src => src.AgentName ?? string.Empty)
            .Ignore(dest => dest.StartedAt)
            .Ignore(dest => dest.LastActivityAt)
            .Ignore(dest => dest.MessageCount)
            .Ignore(dest => dest.TotalTokens)
            .Ignore(dest => dest.SessionTitle)
            .Ignore(dest => dest.Description)
            .Ignore(dest => dest.RecentMessages)
            .Ignore(dest => dest.IsActive);
    }

    private static string SerializeParameters(Dictionary<string, object> parameters)
    {
        try
        {
            return JsonSerializer.Serialize(parameters);
        }
        catch
        {
            return "{}";
        }
    }

    private static Dictionary<string, object> DeserializeParameters(string? parameters)
    {
        if (string.IsNullOrEmpty(parameters))
        {
            return new Dictionary<string, object>();
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(parameters) ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    private static string? SerializeStringList(List<string>? values)
    {
        if (values == null || values.Count == 0)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Serialize(values);
        }
        catch
        {
            return null;
        }
    }

    private static List<string>? DeserializeStringList(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json);
        }
        catch
        {
            return null;
        }
    }

    private static string? SerializeImages(List<ChatImage>? images)
    {
        if (images == null || !images.Any())
        {
            return null;
        }

        try
        {
            return JsonSerializer.Serialize(images);
        }
        catch
        {
            return null;
        }
    }

    private static List<ChatImage>? DeserializeImages(string? imagesJson)
    {
        if (string.IsNullOrEmpty(imagesJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<List<ChatImage>>(imagesJson);
        }
        catch
        {
            return null;
        }
    }
}

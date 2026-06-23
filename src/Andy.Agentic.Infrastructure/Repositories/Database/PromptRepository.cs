using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Andy.Agentic.Infrastructure.Repositories.Database;

/// <summary>
/// Repository implementation for managing Prompt entities and their associated variables in the database.
/// Provides comprehensive prompt management functionality including creation, updates, deletion,
/// and synchronization of prompts with their variables. Implements complex update logic for
/// managing prompt collections and their relationships with agents.
/// </summary>
public class PromptRepository(AndyDbContext context) : IPromptRepository
{
    public async Task UpdatePromptsAsync(AgentEntity agent, List<PromptEntity> promptDtos)
    {
        try
        {
            if (agent == null) throw new InvalidOperationException("Agent not found in database");

            var existingPrompts = agent.Prompts.ToDictionary(x=>x.Id);
            var updatedPrompts = new List<PromptEntity>();

            foreach (var promptDto in promptDtos)
            {
                if (existingPrompts.TryGetValue(promptDto.Id, out var existingPrompt))
                {
                    existingPrompt.Content = promptDto.Content;
                    existingPrompt.IsActive = promptDto.IsActive;
                    existingPrompt.UpdatedAt = DateTime.UtcNow;

                    await UpdatePromptVariablesAsync(existingPrompt, promptDto.Variables);

                    updatedPrompts.Add(existingPrompt);
                    existingPrompts.Remove(promptDto.Id);
                }
                else
                {
                    var newPrompt = CreateNewPrompt(promptDto, agent.Id);
                    context.Prompts.Add(newPrompt);
                    updatedPrompts.Add(newPrompt);
                }
            }

            foreach (var removedPrompt in existingPrompts.Values)
            {
                context.PromptVariables.RemoveRange(removedPrompt.Variables);
                context.Prompts.Remove(removedPrompt);
            }

            agent.Prompts.Clear();
            foreach (var prompt in updatedPrompts) agent.Prompts.Add(prompt);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            try
            {
                await context.Entry(agent).ReloadAsync();
                throw new InvalidOperationException(
                    "The prompt data was modified by another operation. Please refresh and try again.", ex);
            }
            catch
            {
                throw new InvalidOperationException(
                    "The prompt data was modified by another operation. Please refresh and try again.", ex);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to update prompts: {ex.Message}", ex);
        }
    }

    private PromptEntity CreateNewPrompt(PromptEntity promptDto, Guid agentId)
    {
        var newPrompt = new PromptEntity
        {
            Id = Guid.NewGuid(),
            Content = promptDto.Content,
            IsActive = promptDto.IsActive,
            AgentId = agentId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create variables for new prompt
        foreach (var variableDto in promptDto.Variables)
        {
            var variable = new PromptVariableEntity
            {
                Id = Guid.NewGuid(),
                Name = variableDto.Name,
                Type = variableDto.Type,
                Required = variableDto.Required,
                DefaultValue = variableDto.DefaultValue,
                Description = variableDto.Description,
                PromptId = newPrompt.Id
            };
            newPrompt.Variables.Add(variable);
        }

        return newPrompt;
    }
    private Task UpdatePromptVariablesAsync(PromptEntity prompt, ICollection<PromptVariableEntity> variableDtos)
    {
        try
        {
            var existingVariables = prompt.Variables.ToDictionary(v => v.Name, v => v);
            var updatedVariables = new List<PromptVariableEntity>();

            foreach (var variableDto in variableDtos)
                if (existingVariables.TryGetValue(variableDto.Name, out var existingVariable))
                {
                    existingVariable.Type = variableDto.Type;
                    existingVariable.Required = variableDto.Required;
                    existingVariable.DefaultValue = variableDto.DefaultValue;
                    existingVariable.Description = variableDto.Description;
                    updatedVariables.Add(existingVariable);
                    existingVariables.Remove(variableDto.Name);
                }
                else
                {
                    var newVariable = new PromptVariableEntity
                    {
                        Id = Guid.NewGuid(),
                        Name = variableDto.Name,
                        Type = variableDto.Type,
                        Required = variableDto.Required,
                        DefaultValue = variableDto.DefaultValue,
                        Description = variableDto.Description,
                        PromptId = prompt.Id
                    };
                    context.PromptVariables.Add(newVariable);
                    updatedVariables.Add(newVariable);
                }

            foreach (var removedVariable in existingVariables.Values) context.PromptVariables.Remove(removedVariable);

            prompt.Variables.Clear();
            foreach (var variable in updatedVariables) prompt.Variables.Add(variable);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to update prompt variables: {ex.Message}", ex);
        }

        return Task.CompletedTask;
    }

    private string GetPromptHash(PromptEntity prompt)
    {
        var content = prompt.Content;
        var variablesHash = string.Join("|",
            prompt.Variables
                .OrderBy(v => v.Name)
                .Select(v => $"{v.Name}:{v.Type}:{v.Required}"));
        return $"{content}|{variablesHash}";
    }


}

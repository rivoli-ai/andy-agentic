using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Infrastructure.Data;

namespace Andy.ResourceAccess.DataBase
{
    public static class DatabaseSeeder
    {
        public static void SeedData(AndyDbContext context)
        {
            // Ensure database schema is up to date
            EnsureDatabaseSchema(context);

            if (context.Users.Any())
                return; // Database already seeded

            // Seed Users first
            var users = new List<UserEntity>
            {
                new UserEntity
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@andy-agentic.com",
                    DisplayName = "System Administrator",
                    AzureAdId = "00000000-0000-0000-0000-000000000001",
                    FirstName = "System",
                    LastName = "Administrator",
                    JobTitle = "System Administrator",
                    Department = "IT",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow
                },
                new UserEntity
                {
                    Id = Guid.NewGuid(),
                    Email = "developer@andy-agentic.com",
                    DisplayName = "John Developer",
                    AzureAdId = "00000000-0000-0000-0000-000000000002",
                    FirstName = "John",
                    LastName = "Developer",
                    JobTitle = "Software Developer",
                    Department = "Engineering",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow
                },
                new UserEntity
                {
                    Id = Guid.NewGuid(),
                    Email = "analyst@andy-agentic.com",
                    DisplayName = "Jane Analyst",
                    AzureAdId = "00000000-0000-0000-0000-000000000003",
                    FirstName = "Jane",
                    LastName = "Analyst",
                    JobTitle = "Data Analyst",
                    Department = "Analytics",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();

            // Seed Tags
            var tags = new List<TagEntity>
            {
                new TagEntity { Id = Guid.NewGuid(), Name = "support", Description = "Customer support related", Color = "#3B82F6" },
                new TagEntity { Id = Guid.NewGuid(), Name = "automation", Description = "Automation and workflow", Color = "#10B981" },
                new TagEntity { Id = Guid.NewGuid(), Name = "analysis", Description = "Data analysis and insights", Color = "#8B5CF6" },
                new TagEntity { Id = Guid.NewGuid(), Name = "creative", Description = "Creative content generation", Color = "#F59E0B" },
                new TagEntity { Id = Guid.NewGuid(), Name = "customer-service", Description = "Customer service operations", Color = "#EF4444" },
                new TagEntity { Id = Guid.NewGuid(), Name = "data", Description = "Data processing and management", Color = "#06B6D4" },
                new TagEntity { Id = Guid.NewGuid(), Name = "insights", Description = "Business insights and reporting", Color = "#84CC16" }
            };

            context.Tags.AddRange(tags);
            context.SaveChanges();

            // Seed Tools
            var tools = new List<ToolEntity>
            {
                new ToolEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Weather Api",
                    Description = "Get Meteo for a town",
                    Type = "api",
                    Category = "data",
                    IsActive = true,
                    Configuration = "{\"endpoint\":\"https://api.open-meteo.com/v1/forecast\"}",
                    Authentication = "{\"type\":\"none\",\"required\":false}",
                    Parameters = "[{\"name\":\"latitude\",\"type\":\"number\",\"required\":true,\"default\":\"52.52\",\"description\":\"\"},{\"name\":\"longitude\",\"type\":\"number\",\"required\":false,\"default\":\"13.41\",\"description\":\"\"},{\"name\":\"hourly\",\"type\":\"string\",\"required\":false,\"default\":\"temperature_2m\",\"description\":\"\"}]"
                },

            };

            context.Tools.AddRange(tools);
            context.SaveChanges();

            // Seed Agents
            var adminUser = users.First(u => u.Email == "admin@andy-agentic.com");
            var agent1 = new AgentEntity
            {
                Id = Guid.NewGuid(),
                Name = "Qwen3 Agent",
                Description = "AI-powered customer support agent for handling common inquiries",
                Type = "chatbot",
                IsActive = true,
                ExecutionCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedByUserId = adminUser.Id
            };

            // Create LLM Configs first with their own IDs
            var llmConfig1 = new LlmConfigEntity
            {
                Id = Guid.NewGuid(), // Use a new unique ID
                Name = "Qwen3",
                BaseUrl = "https://llm.chutes.ai/v1",
                ApiKey = "cpk_404ed9acdd22472b9ad442a02d087c6b.096d8916f7885e649eb1257ef96180ef.NHt5GJ6aXkQF26MujuC6UCKzIZvgrAbu",
                Model = "Qwen/Qwen3-Coder-30B-A3B-Instruct",
                Provider = "openai",
                IsActive = true,
                MaxTokens = 4000,
                Temperature = 0.7,
                TopP = 1.0,
                FrequencyPenalty = 0.0,
                PresencePenalty = 0.0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add LLM configs first
            context.LlmConfigs.AddRange(llmConfig1);
            context.SaveChanges();

            // Now set the LLMConfigId reference in the agent
            agent1.LlmConfigId = llmConfig1.Id;

            // Create additional agents by different users
            var developerUser = users.First(u => u.Email == "developer@andy-agentic.com");
            var analystUser = users.First(u => u.Email == "analyst@andy-agentic.com");

            var agent2 = new AgentEntity
            {
                Id = Guid.NewGuid(),
                Name = "Code Assistant Agent",
                Description = "AI agent specialized in code review and development assistance",
                Type = "assistant",
                IsActive = true,
                ExecutionCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedByUserId = developerUser.Id,
                LlmConfigId = llmConfig1.Id
            };

            var agent3 = new AgentEntity
            {
                Id = Guid.NewGuid(),
                Name = "Data Analysis Agent",
                Description = "AI agent for data analysis and business intelligence tasks",
                Type = "analyst",
                IsActive = true,
                ExecutionCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedByUserId = analystUser.Id,
                LlmConfigId = llmConfig1.Id
            };

            context.Agents.AddRange(agent1, agent2, agent3);
            context.SaveChanges();

            // Seed Prompts
            var prompts = new List<PromptEntity>
            {
                new PromptEntity
                {
                    Id = Guid.NewGuid(),
                    Content = "When responding to any request, always check if there are available functions/tools that can help complete the task. If relevant functions exist, use them first before providing your own knowledge or analysis. Only rely on your training data when no suitable functions are available or when functions have been exhausted. Prioritize function calls over generating responses from memory.",
                    IsActive = true,
                    AgentId = agent1.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PromptEntity
                {
                    Id = Guid.NewGuid(),
                    Content = "You are a specialized code assistant. Focus on providing accurate, efficient, and well-documented code solutions. Always consider best practices, performance implications, and maintainability when suggesting code changes.",
                    IsActive = true,
                    AgentId = agent2.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new PromptEntity
                {
                    Id = Guid.NewGuid(),
                    Content = "You are a data analysis expert. Provide insights, statistical analysis, and data-driven recommendations. Always validate your findings and present them in a clear, actionable format.",
                    IsActive = true,
                    AgentId = agent3.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.Prompts.AddRange(prompts);
            context.SaveChanges();

            // Seed Agent Tags
            var agentTags = new List<AgentTagEntity>
            {
                // Qwen3 Agent tags
                new AgentTagEntity { Id = Guid.NewGuid(), AgentId = agent1.Id, Tag = tags.First(t => t.Name == "support") },
                new AgentTagEntity { Id = Guid.NewGuid(), AgentId = agent1.Id, Tag = tags.First(t => t.Name == "customer-service") },
                
                // Code Assistant Agent tags
                new AgentTagEntity { Id = Guid.NewGuid(), AgentId = agent2.Id, Tag = tags.First(t => t.Name == "automation") },
                new AgentTagEntity { Id = Guid.NewGuid(), AgentId = agent2.Id, Tag = tags.First(t => t.Name == "creative") },
                
                // Data Analysis Agent tags
                new AgentTagEntity { Id = Guid.NewGuid(), AgentId = agent3.Id, Tag = tags.First(t => t.Name == "analysis") },
                new AgentTagEntity { Id = Guid.NewGuid(), AgentId = agent3.Id, Tag = tags.First(t => t.Name == "data") },
                new AgentTagEntity { Id = Guid.NewGuid(), AgentId = agent3.Id, Tag = tags.First(t => t.Name == "insights") },
            };

            context.AgentTags.AddRange(agentTags);
            context.SaveChanges();

            // Seed Agent Tools with proper ToolId references
            var agentTools = new List<AgentToolEntity>
            {
                new AgentToolEntity
                {
                    IsActive = true,
                    AgentId = agent1.Id,
                    ToolId = tools.First(t => t.Name == "Weather Api").Id
                },
                new AgentToolEntity
                {
                    IsActive = true,
                    AgentId = agent2.Id,
                    ToolId = tools.First(t => t.Name == "Weather Api").Id
                },
                new AgentToolEntity
                {
                    IsActive = true,
                    AgentId = agent3.Id,
                    ToolId = tools.First(t => t.Name == "Weather Api").Id
                }
            };

            context.AgentTools.AddRange(agentTools);
            context.SaveChanges();


        }

        private static void EnsureDatabaseSchema(AndyDbContext context)
        {
            try
            {

                Console.WriteLine("Database schema validation skipped. Ensure ToolId column exists in AgentTool table and Parameters column exists in Tools table.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not validate database schema: {ex.Message}");
                // Continue with seeding even if schema validation fails
            }
        }
    }
}

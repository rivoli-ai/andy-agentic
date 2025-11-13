using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Mcps
{
    /// <summary>
    /// MCP server for executing agents programmatically
    /// </summary>
    [McpServerToolType]
    public class AgentMcp(IChatService chatService, IDataBaseService databaseService, IHttpContextAccessor contextAccessor)
    {

        /// <summary>
        /// Executes an agent with a given prompt and returns the complete response.
        /// This is similar to the ChatController streaming but waits for the complete response.
        /// </summary>
        /// <param name="prompt">The prompt/message to send to the agent</param>
        /// <param name="agentId">The ID of the agent to execute</param>
        /// <param name="sessionId">Optional session ID. If not provided, a new session will be created.</param>
        /// <returns>The complete response from the agent</returns>
        [McpServerTool, Description("Execute an agent with a prompt and get the complete response")]
        public async Task<string> CallAgent(
            [Description("The prompt or message to send to the agent")] string prompt,
            [Description("The unique identifier of the agent to execute")] Guid agentId,
            [Description("Optional session ID for conversation context")] string? sessionId = null)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(prompt))
                {
                    return "Error: Prompt cannot be empty";
                }

                // Get the agent
                var agent = await databaseService.GetAgentByIdAsync(agentId);
                if (agent == null)
                {
                    return $"Error: Agent with ID {agentId} not found";
                }

                // Create or use existing session
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    sessionId = await databaseService.CreateNewChatSessionAsync(agentId);
                }

                // Create the chat message
                var chatMessage = new ChatMessage
                {
                    Content = prompt,
                    AgentId = agentId,
                    SessionId = sessionId,
                    Role = "user"
                };

                // Collect the complete response
                var responseBuilder = new StringBuilder();
                var thinkingBuilder = new StringBuilder();

                // Stream the response and collect all chunks
                await foreach (var chunk in chatService.SendMessageStreamAsync(chatMessage, CancellationToken.None))
                {
                    if (!string.IsNullOrEmpty(chunk.Thinking))
                    {
                        thinkingBuilder.Append(chunk.Thinking);
                    }
                    
                    if (!string.IsNullOrEmpty(chunk.Content))
                    {
                        responseBuilder.Append(chunk.Content);
                    }
                }

                var response = responseBuilder.ToString();
                var thinking = thinkingBuilder.ToString();

                // Return the complete response with thinking if available
                if (!string.IsNullOrEmpty(thinking))
                {
                    return $"[Thinking]\n{thinking}\n\n[Response]\n{response}";
                }

                return response;
            }
            catch (Exception ex)
            {
                return $"Error executing agent: {ex.Message}";
            }
        }
    }
}

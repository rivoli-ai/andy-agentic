using System.Diagnostics;
using System.Text.Json;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Infrastructure.Services.ToolProviders;

/// <summary>
/// Tool provider for executing MCP (Model Context Protocol) based tools
/// </summary>
public class McpToolProvider : IToolProvider
{
    public string ToolType => "mcp";

    public async Task<object?> ExecuteToolAsync(Tool tool, Dictionary<string, object> requestParameters)
    {
        try
        {
            // Parse MCP configuration from tool
            var configuration = tool.Configuration != null
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(tool.Configuration)
                : new Dictionary<string, object>();

            var auth = tool.Authentication != null
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(tool.Authentication)
                : new Dictionary<string, object>();

            // Extract MCP configuration
            var command = configuration.GetValueOrDefault("command", "").ToString();
            var arguments = configuration.GetValueOrDefault("arguments", new List<string>()) as List<string>;
            var workingDirectory = configuration.GetValueOrDefault("workingDirectory", "").ToString();
            var timeout = configuration.GetValueOrDefault("timeout", 30000) as int? ?? 30000;

            if (string.IsNullOrEmpty(command))
            {
                throw new ArgumentException("MCP command is required in tool configuration");
            }

            // Prepare command arguments
            var processArgs = new List<string>();
            
            // Add static arguments from configuration
            if (arguments != null)
            {
                processArgs.AddRange(arguments);
            }

            // Add dynamic parameters as arguments
            //foreach (var param in parameters)
            //{
            //    processArgs.Add($"--{param.Key}");
            //    processArgs.Add(param.Value?.ToString() ?? "");
            //}

            // Execute the MCP command
            var result = await ExecuteMcpCommandAsync(command, processArgs, workingDirectory, timeout);
            
            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to execute MCP tool '{tool.Name}': {ex.Message}", ex);
        }
    }

    public bool CanHandleToolType(string toolType)
    {
        return string.Equals(toolType, ToolType, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<object?> ExecuteMcpCommandAsync(string command, List<string> arguments, string? workingDirectory, int timeoutMs)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = string.Join(" ", arguments.Select(arg => $"\"{arg}\"")),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        if (!string.IsNullOrEmpty(workingDirectory))
        {
            startInfo.WorkingDirectory = workingDirectory;
        }

        using var process = new Process { StartInfo = startInfo };
        
        var outputBuilder = new List<string>();
        var errorBuilder = new List<string>();

        // Set up event handlers for output
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputBuilder.Add(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                errorBuilder.Add(e.Data);
            }
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for process to complete with timeout
            //var completed = await process.WaitForExitAsync(TimeSpan.FromMilliseconds(timeoutMs));
            
            //if (!completed)
            //{
            //    process.Kill();
            //    throw new TimeoutException($"MCP command timed out after {timeoutMs}ms");
            //}

            var output = string.Join("\n", outputBuilder);
            var error = string.Join("\n", errorBuilder);

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"MCP command failed with exit code {process.ExitCode}. Error: {error}");
            }

            // Try to parse output as JSON, fallback to string
            if (!string.IsNullOrEmpty(output))
            {
                try
                {
                    return JsonSerializer.Deserialize<object>(output);
                }
                catch
                {
                    return output;
                }
            }

            return null;
        }
        catch (Exception ex) when (!(ex is TimeoutException || ex is InvalidOperationException))
        {
            throw new InvalidOperationException($"Failed to execute MCP command: {ex.Message}", ex);
        }
    }
}

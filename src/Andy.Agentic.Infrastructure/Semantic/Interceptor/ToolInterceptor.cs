using System.Text.Json;using Andy.Agentic.Domain.Models;using Microsoft.SemanticKernel;using Tool = Andy.Agentic.Domain.Models.Tool;

namespace Andy.Agentic.Infrastructure.Semantic.Interceptor;/// <summary>
/// Represents a filter that intercepts function execution and records tool execution details.
/// </summary>
public sealed class FunctionInterceptorFilter(ToolExecutionRecorder recorder,    Agent agent,    string session,    List<Tool> tools) : IFunctionInvocationFilter{
    /// <summary>
    /// Invokes the function asynchronously, logs the execution details, and handles any exceptions.
    /// </summary>
    /// <param name="context">The context of the function invocation.</param>
    /// <param name="next">The delegate to invoke the next function in the pipeline.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    public async Task OnFunctionInvocationAsync(
            FunctionInvocationContext context,
            Func<FunctionInvocationContext, Task> next)    {        var tool = tools.FirstOrDefault(x => x.Name == context.Function.Name);        var rec = new ToolExecutionLog        {            Id = Guid.NewGuid(),            AgentId = agent.Id,            ToolName = context.Function.Name,            SessionId = session,            Parameters = SerializeArguments(context.Arguments)!,            ExecutedAt = DateTime.UtcNow,        };        if (tool != null)        {            rec.ToolId = tool.Id;            rec.ToolName = tool.Name;        }

        try
        {
            await next(context);
            rec.Success = true;
            var resultObj = context.Result.GetValue<object>();
            rec.Result = resultObj is string s ? s : JsonSerializer.Serialize(resultObj);
        }
        catch (Exception ex)
        {
            rec.Success = false;
            rec.ErrorMessage = ex.ToString();
            throw;
        }
        finally
        {
            recorder.Add(rec);
        }    }

    /// <summary>
    /// Serializes the given KernelArguments into a dictionary where the keys are strings and the values are objects.
    /// </summary>
    /// <param name="args">The KernelArguments to be serialized.</param>
    /// <returns>A dictionary with the serialized arguments.</returns>
    private static Dictionary<string, object?> SerializeArguments(KernelArguments args)
            => args.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);}
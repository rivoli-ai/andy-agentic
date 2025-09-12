using Andy.Agentic.Domain.Interfaces.Llm.Semantic;
using Andy.Agentic.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Andy.Agentic.Infrastructure.Semantic.Tools;

public class NativeFunctionToolFactory : IToolFactory
{
    private readonly ILogger<NativeFunctionToolFactory>? _logger;

    public NativeFunctionToolFactory(ILogger<NativeFunctionToolFactory>? logger = null)
    {
        _logger = logger;
    }

    public KernelFunction CreateToolAsync(Tool config)
    {
        try
        {
            // Load assembly and create function from native method
            //var assembly = System.Reflection.Assembly.LoadFrom(config.NativeFunctionAssembly!);
            //var type = assembly.GetType(config.NativeFunctionType!)!;
            //var instance = Activator.CreateInstance(type);

            //var function = KernelFunctionFactory.CreateFromMethod(
            //    type.GetMethod(config.NativeFunctionMethod!)!,
            //    instance,
            //    config.Name,
            //    config.Description);

            //return Task.FromResult(function);

            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error creating native function tool: {ToolName}", config.Name);
            throw;
        }
    }
}

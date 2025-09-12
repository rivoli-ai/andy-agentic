using System.Net.Http.Headers;using Andy.Agentic.Domain.Models;using Microsoft.SemanticKernel;namespace Andy.Agentic.Infrastructure.Semantic.Tools;/// <summary>/// Initializes a new instance of the <see cref="ApiToolFactory"/> class./// </summary>public class ApiToolFactory: ToolFactory{    /// <summary>    /// Creates a KernelFunction asynchronously using the provided Tool object.     /// The function is created from the DynamicApiCall method, with the tool's name and description.    /// </summary>    /// <param name="tool">The Tool object containing the name and description for the KernelFunction.</param>    /// <returns>A KernelFunction created from the DynamicApiCall method.</returns>    public override KernelFunction CreateToolAsync(Tool tool)    {        IEnumerable<KernelParameterMetadata>? parameters = null;
        async Task<string> DynamicApiCall(KernelArguments args)        {            using var client = new HttpClient();

            var headers = ParseHeaders(tool.Headers);

            foreach (var header in headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value.ToString());
            }            if (!string.IsNullOrEmpty(tool.Authentication))            {                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tool.Authentication);            }            var callArgs = ParseToolCallArguments(args);

            var configuration = ParseConfiguration(tool.Configuration);            var endpoint = GetRequiredConfigValue<string>(configuration, "endpoint");            var url = $"{endpoint}?{string.Join("&", callArgs)}";            var response = await client.GetAsync(url);            response.EnsureSuccessStatusCode();            return await response.Content.ReadAsStringAsync();        }

        if (!string.IsNullOrEmpty(tool.Parameters))        {            var paramSchema = ConvertParamsToDictionary(tool.Parameters);            parameters = paramSchema.Select(p =>            {                var metadata = new KernelParameterMetadata(p.Name)                {                    Description = $"Parameter for {p.Name}",                    ParameterType = p.Type,                    IsRequired = true                };                return metadata;            }).ToList();        }        return KernelFunctionFactory.CreateFromMethod(            method: DynamicApiCall,            functionName: tool.Name,            description: tool.Description,            parameters: parameters        );    }

}

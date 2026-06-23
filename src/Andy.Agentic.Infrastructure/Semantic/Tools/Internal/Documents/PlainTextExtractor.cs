using Andy.Agentic.Domain.Interfaces.Llm.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andy.Agentic.Infrastructure.Semantic.Tools.Internal.Documents
{
    public class PlainTextExtractor : IDocumentTextExtractor
    {
        public Task<string> ExtractTextAsync(byte[] binaryContent)
        {
            try
            {
                var text = Encoding.UTF8.GetString(binaryContent);
                return Task.FromResult(text);
            }
            catch
            {
                return Task.FromResult(string.Empty);
            }
        }
    }
}

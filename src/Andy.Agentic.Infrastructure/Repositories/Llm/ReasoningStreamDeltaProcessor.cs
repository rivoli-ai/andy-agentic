using Andy.Agentic.Domain.Models;
using System.Text;

namespace Andy.Agentic.Infrastructure.Repositories.Llm;

/// <summary>
/// Maps OpenAI-compatible streaming deltas to thinking vs answer chunks.
/// Handles DashScope <c>reasoning_content</c>, vLLM <c>reasoning</c>, and inline
/// <c>&lt;think&gt;</c> tags when self-hosted servers embed reasoning in content.
/// </summary>
internal sealed class ReasoningStreamDeltaProcessor
{
    private const string ThinkOpen = "\u003cthink\u003e";
    private const string ThinkClose = "\u003c/think\u003e";

    private bool _answerPhaseStarted;
    private bool _insideThinkTag;
    private readonly StringBuilder _carry = new();

    public IEnumerable<StreamingResult> Process(string? reasoningContent, string? reasoning, string content)
    {
        var explicitReasoning = FirstNonEmpty(reasoningContent, reasoning);
        if (!string.IsNullOrEmpty(explicitReasoning) && !_answerPhaseStarted)
        {
            yield return new StreamingResult { Thinking = explicitReasoning };
            yield break;
        }

        if (string.IsNullOrEmpty(content))
        {
            yield break;
        }

        _carry.Append(content);
        var buffer = _carry.ToString();
        _carry.Clear();

        var index = 0;
        while (index <= buffer.Length)
        {
            if (index == buffer.Length)
            {
                break;
            }

            if (!_insideThinkTag && !_answerPhaseStarted)
            {
                var thinkStart = buffer.IndexOf(ThinkOpen, index, StringComparison.Ordinal);
                if (thinkStart >= 0)
                {
                    if (thinkStart > index)
                    {
                        _answerPhaseStarted = true;
                        yield return new StreamingResult { Content = buffer[index..thinkStart] };
                    }

                    _insideThinkTag = true;
                    index = thinkStart + ThinkOpen.Length;
                    continue;
                }

                if (HasPartialSuffix(buffer[index..], ThinkOpen))
                {
                    _carry.Append(buffer[index..]);
                    yield break;
                }
            }

            if (_insideThinkTag)
            {
                var thinkEnd = buffer.IndexOf(ThinkClose, index, StringComparison.Ordinal);
                if (thinkEnd >= 0)
                {
                    if (thinkEnd > index)
                    {
                        yield return new StreamingResult { Thinking = buffer[index..thinkEnd] };
                    }

                    _insideThinkTag = false;
                    index = thinkEnd + ThinkClose.Length;
                    continue;
                }

                if (HasPartialSuffix(buffer[index..], ThinkClose))
                {
                    _carry.Append(buffer[index..]);
                    yield break;
                }

                yield return new StreamingResult { Thinking = buffer[index..] };
                yield break;
            }

            _answerPhaseStarted = true;
            yield return new StreamingResult { Content = buffer[index..] };
            yield break;
        }
    }

    private static string? FirstNonEmpty(string? first, string? second) =>
        !string.IsNullOrEmpty(first) ? first : string.IsNullOrEmpty(second) ? null : second;

    private static bool HasPartialSuffix(string text, string token)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        var max = Math.Min(text.Length, token.Length - 1);
        for (var len = max; len > 0; len--)
        {
            if (token.StartsWith(text[^len..], StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}

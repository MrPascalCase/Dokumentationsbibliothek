using System.Diagnostics.Contracts;
using System.Text;
using Microsoft.AspNetCore.Components;

namespace ImageSearch.Services.Justifications;

/// <summary>
///     Builds "justifications". Justifications are explanations why results show up in the search.
///     Justifications take the form of text excerpts, where the match is highlighted:
///     Example: "...und rechts davon Neues &lt;span class="match"&gt;Post&lt;/span&gt;hotel (heute Wohn- und
///     Geschäftshaus)..."
/// </summary>
public class JustificationBuilder
{
    public int ContextLength { get; init; } = 40;
    public bool RequireWhitespace { get; init; } = true;
    public bool RespectHardBreak { get; init; } = true;
    public string MatchStyle { get; init; } = "background: lightgreen; font-weight: bold;";

    [Pure]
    public MarkupString Justify(ImageQuery query, Image image)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        if (image == null) throw new ArgumentNullException(nameof(image));
        if (image.Description == null) throw new ArgumentNullException(nameof(image.Description));

        return Justify(query.Description.ToArray(), image.Description);
    }

    [Pure]
    public MarkupString Justify(string[] query, string text)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        if (text == null) throw new ArgumentNullException(nameof(text));
        if (!query.Any()) return new MarkupString();

        // Find all query matches, expand each with surrounding context, and sort them by their first match position in the text.
        Match[] matches =
            GetMatches(query, text)
                .Select(m => AddContext(m, text))
                .OrderBy(m => m.MatchRanges[0].Start)
                .ToArray();

        // Incrementally merge overlapping or touching matches. The stack always contains non-overlapping, combined matches.
        Stack<Match> stack = new();
        if (matches.Any())
        {
            stack.Push(matches.First());
        }

        foreach (Match match in matches.Skip(1))
        {
            // Try to merge the current match with the previous one.
            // If their context ranges overlap or touch, combine them into a single match.
            if (TryCombine(stack.Peek(), match, out Match? result))
            {
                stack.Pop();
                stack.Push(result!.Value);
            }
            else
            {
                stack.Push(match);
            }
        }

        StringBuilder sb = new();
        foreach (Match match in stack.Reverse())
        {
            AddToResult(sb, match, text);
        }

        return new MarkupString(sb.ToString());
    }

    [Pure]
    internal TextRange[] GetMatches(string[] query, string text)
    {
        List<TextRange> result = new();
        foreach (string element in query)
        {
            // Collect all occurrences of all query elements.
            // Matches may overlap at this stage and are merged later.
            foreach (int startIndex in GetIndexesOf(element, text))
            {
                result.Add(new TextRange(startIndex, element.Length));
            }
        }

        return result.ToArray();
    }

    [Pure]
    internal static int[] GetIndexesOf(string query, string text)
    {
        List<int> result = new();
        int offset = 0;
        while (true)
        {
            // Search iteratively, advancing by one character each time
            // to allow overlapping matches (e.g. "ana" in "banana").
            // TODO does that work??

            int index = text.IndexOf(query, StringComparison.InvariantCultureIgnoreCase);
            if (index >= 0)
            {
                result.Add(index + offset);
                text = text.Substring(index + 1);
                offset += index + 1;
            }
            else
            {
                break;
            }
        }

        return result.ToArray();
    }

    [Pure]
    internal Match AddContext(TextRange range, string text)
    {
        Match match = Match.FromTextRange(range);

        // Extend context to the right until:
        // - the desired context length is reached AND
        // - we are allowed to break at this position (usually whitespace),
        // unless a hard break (sentence end) is encountered earlier.
        int end = match.MatchRanges.Last().End;
        for (; end < text.Length && (end < match.MatchRanges.Last().End + ContextLength || !CanBreak(end)); end++)
        {
            if (RespectHardBreak && MustBreak(end))
            {
                // Stop early at sentence boundaries and suppress trailing ellipsis,
                // because the context ends naturally.
                match = match.RemoveElipsesEnd();
                break;
            }
        }

        match = match.AddContextEnd(end);

        int start = match.MatchRanges.First().Start;
        for (; 0 < start && (match.MatchRanges.First().Start - ContextLength < start || !CanBreak(start)); start--)
        {
            if (RespectHardBreak && MustBreak(start))
            {
                match = match.RemoveElipsesStart();
                break;
            }
        }

        match = match.AddContextStart(start);

        if (match.ContextRange.Start == 0) match = match.RemoveElipsesStart();
        if (match.ContextRange.End == text.Length) match = match.RemoveElipsesEnd();

        return match;

        bool CanBreak(int index)
        {
            // Determines whether the context may end at this character.
            // Typically, restricts breaks to whitespace to avoid cutting words.
            if (!RequireWhitespace) return true;
            return char.IsWhiteSpace(text[index]);
        }

        bool MustBreak(int index)
        {
            return new[] { '.', '?', '!', }.Contains(text[index]);
        }
    }

    internal bool TryCombine(Match left, Match right, out Match? result)
    {
        // Only matches with overlapping or directly adjacent context ranges
        // can be combined into a single justification block.

        if (!left.ContextRange.DoesOverlapOrTouch(right.ContextRange))
        {
            result = null;
            return false;
        }

        TextRange context = left.ContextRange.Union(right.ContextRange);

        Elipses elipses = Elipses.None;
        elipses |=
            left.ContextRange.Start < right.ContextRange.Start
                ? left.Elipses & Elipses.Start
                : right.Elipses & Elipses.Start;

        elipses |=
            left.ContextRange.End > right.ContextRange.End
                ? left.Elipses & Elipses.End
                : right.Elipses & Elipses.End;

        // Merge overlapping or touching match ranges while preserving order.
        Stack<TextRange> ranges = new();
        foreach (TextRange range in left.MatchRanges.Concat(right.MatchRanges).OrderBy(r => r.Start))
        {
            if (ranges.Any() && ranges.Peek().DoesOverlapOrTouch(range))
            {
                TextRange top = ranges.Pop();
                ranges.Push(top.Union(range));
            }
            else
            {
                ranges.Push(range);
            }
        }

        result = new Match(ranges.Reverse().ToArray(), context, elipses);
        return true;
    }

    internal void AddToResult(StringBuilder sb, Match match, string text)
    {
        // Renders a single combined match as HTML:
        // - one <div> per match group
        // - <span> elements around matched text
        // - optional leading/trailing ellipses

        sb.Append("<div>");
        if ((match.Elipses & Elipses.Start) != 0) sb.Append("...");
        sb.Append(text.Substring(match.ContextRange.Start, match.MatchRanges[0].Start - match.ContextRange.Start));

        for (int i = 0; i < match.MatchRanges.Count; i++)
        {
            sb.Append($"<span style=\"{MatchStyle}\">");
            sb.Append(text.Substring(match.MatchRanges[i].Start, match.MatchRanges[i].Length));
            sb.Append("</span>");

            int next = i + 1 < match.MatchRanges.Count ? match.MatchRanges[i + 1].Start : match.ContextRange.End;
            sb.Append(text.Substring(match.MatchRanges[i].End, next - match.MatchRanges[i].End));
        }

        if ((match.Elipses & Elipses.End) != 0) sb.Append("...");
        sb.Append("</div>");
    }
}
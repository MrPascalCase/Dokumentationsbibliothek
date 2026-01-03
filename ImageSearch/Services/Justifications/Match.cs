using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;

namespace ImageSearch.Services.Justifications;

[Flags]
internal enum Elipses
{
    None = 0,
    Start = 1 << 0,
    End = 1 << 1,
    Both = Start | End,
}

internal readonly struct Match
{
    // Represents one justification block consisting of:
    // - one or more non-overlapping match ranges
    // - a surrounding context range
    // - optional ellipses at the start and/or end

    internal TextRange ContextRange { get; }
    internal IReadOnlyList<TextRange> MatchRanges { get; }
    internal Elipses Elipses { get; }

    internal Match(IReadOnlyList<TextRange> matchRanges, TextRange contextRange, Elipses elipses)
    {
        if (matchRanges == null) throw new ArgumentNullException(nameof(matchRanges));

        MatchRanges = matchRanges;
        ContextRange = contextRange;
        Elipses = elipses;

        CheckClassInvariant();
    }

    [Pure]
    internal static Match FromTextRange(TextRange range)
    {
        return new Match(new[] { range, }, range, Elipses.Both);
    }

    [Pure]
    internal Match RemoveElipsesStart()
    {
        return new Match(MatchRanges, ContextRange, Elipses & Elipses.End);
    }

    [Pure]
    internal Match RemoveElipsesEnd()
    {
        return new Match(MatchRanges, ContextRange, Elipses & Elipses.Start);
    }

    [Pure]
    internal Match AddContextStart(int contextStart)
    {
        TextRange range = TextRange.CreateFromStartAndEnd(contextStart, ContextRange.End);
        return new Match(MatchRanges, range, Elipses);
    }

    [Pure]
    internal Match AddContextEnd(int contextEnd)
    {
        TextRange range = TextRange.CreateFromStartAndEnd(ContextRange.Start, contextEnd);
        return new Match(MatchRanges, range, Elipses);
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        if ((Elipses & Elipses.Start) != 0) sb.Append("..");
        sb.Append($"{{{ContextRange.Start}}}");
        for (int i = 0; i < MatchRanges.Count; i++)
        {
            sb.Append($"[{MatchRanges[i].Start}, {MatchRanges[i].End}]");
        }

        sb.Append($"{{{ContextRange.End}}}");
        if ((Elipses & Elipses.End) != 0) sb.Append("..");
        return sb.ToString();
    }

    [Conditional("DEBUG")]
    private void CheckClassInvariant()
    {
        // Ensures that:
        // - all match ranges are contained within the context
        // - match ranges are sorted
        // - match ranges do not overlap or touch

        foreach (TextRange range in MatchRanges)
        {
            if (!ContextRange.Contains(range))
            {
                throw new Exception($"The context range must contain all match ranges. But {ContextRange} does not contain {range}.");
            }
        }

        int lastStart = int.MinValue;
        foreach (TextRange range in MatchRanges)
        {
            if (range.Start <= lastStart)
            {
                throw new Exception("The ranges must be sorted in ascending order.");
            }

            lastStart = range.Start;
        }

        for (int i = 0; i < MatchRanges.Count - 1; i++)
        {
            if (MatchRanges[i].DoesOverlapOrTouch(MatchRanges[i + 1]))
            {
                throw new Exception($"Match ranges must not overlap. But {MatchRanges[i]} and {MatchRanges[i + 1]} do overlap.");
            }
        }
    }
}
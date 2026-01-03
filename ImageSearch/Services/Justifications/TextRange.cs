namespace ImageSearch.Services.Justifications;

internal readonly struct TextRange : IEquatable<TextRange>
{
    internal int Start { get; }
    internal int Length { get; }
    internal int End => Start + Length;

    internal static TextRange CreateFromStartAndEnd(int start, int end)
    {
        return new TextRange(start, end - start);
    }

    internal TextRange(int start, int length)
    {
        if (start < 0) throw new ArgumentException($"start must be non-negative (start={start}).");
        if (length <= 0) throw new ArgumentException($"length must be positive (length={length}).");

        Start = start;
        Length = length;
    }

    internal bool Contains(TextRange other)
    {
        return Start <= other.Start && other.End <= End;
    }

    internal bool DoesOverlapOrTouch(TextRange other)
    {
        // Returns true if the ranges overlap, touch at the boundary,
        // or one fully contains the other.

        return (Start <= other.Start && other.Start < End)
               || (Start <= other.End && other.End < End)
               || Contains(other)
               || other.Contains(this)
               || other.End == Start
               || End == other.Start;
    }

    internal TextRange Union(TextRange other)
    {
        return CreateFromStartAndEnd(Math.Min(Start, other.Start), Math.Max(End, other.End));
    }

    public override string ToString()
    {
        return $"[{Start}, {End})";
    }

    public bool Equals(TextRange other)
    {
        return Start == other.Start && Length == other.Length;
    }

    public override bool Equals(object? obj)
    {
        return obj is TextRange other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, Length);
    }

    public static bool operator ==(TextRange left, TextRange right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TextRange left, TextRange right)
    {
        return !left.Equals(right);
    }
}
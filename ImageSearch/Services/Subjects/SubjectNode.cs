using ImageSearch.Services.Utils;

namespace ImageSearch.Services.Subjects;

public sealed class SubjectNode
{
    public string? Id { get; init; }
    public string? Label { get; init; }
    public int? Position { get; init; }

    public List<SubjectNode> Children { get; } = new();

    public override string ToString()
    {
        TreeFormatter formatter = new();
        return formatter.Format(this, obj => obj.Children, obj => obj.Label);
    }
}
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ImageSearch.Services.Utils;

public class TreeFormatter
{
    public string LineNodeSeparator { get; init; } = string.Empty;
    public int MaxDepth { get; init; } = 10_000;

    public string Format<T>([DisallowNull] T root, Func<T, IEnumerable<T>> getChildren, Func<T, string>? toString = null)
    {
        if (root == null) throw new ArgumentNullException(nameof(root));

        toString ??= arg => arg?.ToString() ?? "null";
        return Format((object)root, GetObjectChildren, ObjectToString);

        string ObjectToString(object o)
        {
            if (o is T t) return toString(t);
            return o?.ToString() ?? "null";
        }

        IEnumerable<object> GetObjectChildren(object o)
        {
            if (o is T t) return getChildren(t).Cast<object>();
            return Array.Empty<object>();
        }
    }

    public string Format(object root, Func<object, IEnumerable<object>> getChildren, Func<object, string>? toString = null)
    {
        if (root == null) throw new ArgumentNullException(nameof(root));
        if (getChildren == null) throw new ArgumentNullException(nameof(getChildren));

        toString ??= t => t.ToString() ?? "null";
        StringBuilder sb = new();
        PrintNode(root, string.Empty, 0);
        string result = sb.ToString();
        if (LineNodeSeparator.Length >= 1)
        {
            result = result.Substring(LineNodeSeparator.Length - 1);
        }

        return result;

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local (depth is used to precondition checks only)
        void PrintNode(object current, string indent, int depth)
        {
            if (depth >= MaxDepth)
            {
                throw new InvalidOperationException($"Maximum recursion depth of '{MaxDepth}' exceeded.");
            }

            string currentIndent = indent;
            if (currentIndent.Length > 3)
            {
                currentIndent = currentIndent[..^3].Replace(" ├─", " │ ").Replace(" └─", "   ") + currentIndent[^3..];
            }

            string[] lines = toString(current).Split(new[] { "\r\n", "\n", }, StringSplitOptions.None).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            if (!lines.Any()) lines = new[] { string.Empty, };

            sb.AppendLine(RemoveLeadingSingleWhitespace(currentIndent + LineNodeSeparator) + lines[0]);
            for (int i = 1; i < lines.Length; i++)
            {
                sb.AppendLine(RemoveLeadingSingleWhitespace(indent.Replace(" ├─", " │ ").Replace(" └─", "   ") + LineNodeSeparator) + lines[i]);
            }

            object[] children = getChildren(current).ToArray();
            for (int i = 0; i < children.Length; i++)
            {
                bool isLast = i == children.Length - 1;
                PrintNode(children[i], indent + (isLast ? " └─" : " ├─"), depth + 1);
            }
        }
    }

    private static string RemoveLeadingSingleWhitespace(string line)
    {
        return line.StartsWith(" ") ? line.Substring(1) : line;
    }
}
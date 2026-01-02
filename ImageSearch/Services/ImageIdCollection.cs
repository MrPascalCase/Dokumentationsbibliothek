using System.Collections;

namespace ImageSearch.Services;

public readonly struct ImageIdCollection : IReadOnlyList<string>
{
    public IReadOnlyList<string> List { get; }
    public int PagesQueried { get; }

    public ImageIdCollection(string[] list, int pagesQueried)
    {
        if (list == null) throw new ArgumentNullException(nameof(list));
        foreach (string id in list)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new Exception($"{nameof(id)} cannot be null or whitespace.");
        }

        foreach (IGrouping<string, string> group in list.GroupBy(id => id).Where(group => group.Count() > 1))
        {
            throw new Exception($"The id={group.Key} is {group.Count()} times present in the result-list.");
        }

        List = list;
        PagesQueried = pagesQueried;
    }

    public IEnumerator<string> GetEnumerator()
    {
        return List.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => List.Count;

    public string this[int index] => List[index];
}
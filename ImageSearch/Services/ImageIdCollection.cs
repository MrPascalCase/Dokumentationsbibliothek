using System.Collections;

namespace ImageSearch.Services;

public readonly struct ImageIdCollection : IReadOnlyList<string>
{
    public IReadOnlyList<string> List { get; }
    public int PagesQueried { get; }

    public ImageIdCollection(string[] list, int pagesQueried)
    {
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
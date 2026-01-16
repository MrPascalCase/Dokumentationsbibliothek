using System.CodeDom.Compiler;

namespace ImageSearch.Services.QueryProcessing;

public abstract class FilterComponent
{
    protected int FilterNumber { get; }

    protected FilterComponent(int filterNumber)
    {
        FilterNumber = filterNumber;
    }

    internal abstract void AddConstruct(IndentedTextWriter writer);
    internal abstract void AddFilter(IndentedTextWriter writer);
}
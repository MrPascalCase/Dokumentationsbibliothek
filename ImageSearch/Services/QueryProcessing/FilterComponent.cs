using System.CodeDom.Compiler;

namespace ImageSearch.Services.QueryProcessing;

public abstract class FilterComponent
{
    protected int FilterNumber { get; }

    protected FilterComponent(QueryProcessor processor)
    {
        FilterNumber = processor.ReserveFilterNumber();
    }

    internal abstract void AddConstruct(IndentedTextWriter writer);
    internal abstract void AddFilter(IndentedTextWriter writer);
}
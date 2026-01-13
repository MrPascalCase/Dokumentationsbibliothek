namespace ImageSearch.Services.Dto;

public class Person
{
    public string Id { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;

    public override string ToString()
    {
        return $"{LastName}, {FirstName} ({City})";
    }
}
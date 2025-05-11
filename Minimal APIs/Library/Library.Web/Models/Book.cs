namespace Library.Web.Models;

public class Book
{
    public required string Isbn { get; init; }

    public required string Title { get; init; }

    public required string Author { get; init; }

    public required string ShortDescription { get; init; }

    public required int PageCount { get; init; }

    public required DateTime ReleaseDate { get; init; }
}

using System.Text.RegularExpressions;

namespace RottenRest.Application.Models;

public partial class Movie
{
    public required Guid Id { get; init; }

    public required string Title { get; set; }

    public string Slug => SlugRegex().Replace(Title + '-' + YearOfRelease.ToString(), "-").ToLower();

    public required int YearOfRelease { get; set; }

    public required List<string> Genres { get; init; } = [];

    public float? Rating { get; set; }

    public int? UserRating { get; set; }

    [GeneratedRegex(@"[^a-zA-Z0-9_-]+", RegexOptions.NonBacktracking)]
    private static partial Regex SlugRegex();
}

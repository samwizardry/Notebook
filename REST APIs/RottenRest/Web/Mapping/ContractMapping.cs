using RottenRest.Application.Models;
using RottenRest.Contracts.Requests;
using RottenRest.Contracts.Responses;

namespace RottenRest.Web.Mapping;

public static class ContractMapping
{
    public static Movie MapToMovie(this CreateMovieRequest request)
    {
        return new Movie
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            YearOfRelease = request.YearOfRelease,
            Genres = request.Genres.ToList()
        };
    }

    public static Movie MapToMovie(this UpdateMovieRequest request, Guid id)
    {
        return new Movie
        {
            Id = id,
            Title = request.Title,
            YearOfRelease = request.YearOfRelease,
            Genres = request.Genres.ToList()
        };
    }

    public static MovieResponse MapToResponse(this Movie movie)
    {
        return new MovieResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            Slug = movie.Slug,
            YearOfRelease = movie.YearOfRelease,
            Genres = movie.Genres,
            Rating = movie.Rating,
            UserRating = movie.UserRating
        };
    }

    public static MoviesResponse MapToResponse(this IEnumerable<Movie> movies, int page, int pageSize, int total)
    {
        return new MoviesResponse
        {
            Items = movies.Select(MapToResponse),
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public static IEnumerable<MovieRatingResponse> MapToResponse(this IEnumerable<MovieRating> ratings)
    {
        return ratings.Select(x => new MovieRatingResponse
        {
            MovieId = x.MovieId,
            Slug = x.Slug,
            Rating = x.Rating
        });
    }

    public static GetAllMoviesOptions MapToOptions(this GetAllMoviesRequest request)
    {
        return new GetAllMoviesOptions
        {
            Title = request.Title,
            YearOfRelease = request.Year,
            SortField = request.SortBy?.Trim('-'),
            SortOrder = string.IsNullOrEmpty(request.SortBy)
                ? SortOrder.Unsorted
                : (request.SortBy.StartsWith('-')
                    ? SortOrder.Descending
                    : SortOrder.Ascending),
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public static GetAllMoviesOptions WithUser(this GetAllMoviesOptions options, Guid? userId)
    {
        options.UserId = userId;
        return options;
    }
}

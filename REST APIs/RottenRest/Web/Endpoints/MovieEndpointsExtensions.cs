using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

using RottenRest.Application.Models;
using RottenRest.Application.Services;
using RottenRest.Contracts.Requests;
using RottenRest.Contracts.Responses;
using RottenRest.Web.Auth;
using RottenRest.Web.Mapping;

namespace RottenRest.Web.Endpoints;

public static class MovieEndpointsExtensions
{
    public static IEndpointRouteBuilder MapMovieEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiEndpoints.Movies.Prefix)
            .WithApiVersionSet(ApiVersioning.VersionSet)
            .HasApiVersion(1.0d);

        group.MapPost(ApiEndpoints.Movies.Create, CreateMovieAsync)
            .WithName(nameof(CreateMovieAsync))
            .Produces<MovieResponse>(StatusCodes.Status201Created)
            .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest)
            .RequireAuthorization(AuthConstants.TrustedMemberPolicyName);

        group.MapGet(ApiEndpoints.Movies.Get, GetMovieAsync)
            .WithName(nameof(GetMovieAsync))
            .Produces<MovieResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .MapToApiVersion(1.0d)
            .CacheOutput("MovieCache");

        group.MapGet(ApiEndpoints.Movies.Get, GetMovieV2Async)
            .WithName(nameof(GetMovieV2Async))
            .Produces<MovieResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .MapToApiVersion(2.0d)
            .CacheOutput("MovieCache");

        group.MapGet(ApiEndpoints.Movies.GetAll, GetAllMoviesAsync)
            .Produces<MovieResponse>(StatusCodes.Status200OK)
            .WithName(nameof(GetAllMoviesAsync))
            .CacheOutput("MovieCache");

        group.MapPut(ApiEndpoints.Movies.Update, UpdateMovieAsync)
            .Produces<MovieResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest)
            .WithName(nameof(UpdateMovieAsync))
            .RequireAuthorization(AuthConstants.TrustedMemberPolicyName);

        group.MapDelete(ApiEndpoints.Movies.Delete, DeleteMovieAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithName(nameof(DeleteMovieAsync))
            .RequireAuthorization(AuthConstants.AdminUserPolicyName);

        return app;
    }

    ////[ServiceFilter<ApiKeyAuthFilter>]
    public static async Task<IResult> CreateMovieAsync(
        CreateMovieRequest request,
        IMovieService movieService,
        IOutputCacheStore outputCacheStore,
        CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie();

        await movieService.CreateAsync(movie, cancellationToken);
        await outputCacheStore.EvictByTagAsync("movies", cancellationToken);

        return TypedResults.CreatedAtRoute(movie.MapToResponse(), nameof(GetMovieAsync), new { idOrSlug = movie.Id });
    }

    // QNA: ResponseCache в minimal api?
    //[ResponseCache(Duration = 60, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
    public static async Task<IResult> GetMovieAsync(
        string idOrSlug,
        IMovieService movieService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userId = context.GetUserId();

        Movie? movie = Guid.TryParse(idOrSlug, out var id)
            ? await movieService.GetByIdAsync(id, userId, cancellationToken)
            : await movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);

        if (movie is null)
            return Results.NotFound();

        var response = movie.MapToResponse();
        // QNA: TypedResults vs Results
        return TypedResults.Ok(response);
    }

    //[ResponseCache(Duration = 60, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
    public static async Task<IResult> GetMovieV2Async(
        string idOrSlug,
        IMovieService movieService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userId = context.GetUserId();

        Movie? movie = Guid.TryParse(idOrSlug, out var id)
            ? await movieService.GetByIdAsync(id, userId, cancellationToken)
            : await movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);

        if (movie is null)
            return Results.NotFound();

        var response = movie.MapToResponse();
        // QNA: TypedResults vs Results
        return TypedResults.Ok(response);
    }

    //[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any,
    //    VaryByQueryKeys = ["title", "year", "sortBy", "page", "pageSize"], VaryByHeader = "Accept, Accept-Encoding")]
    public static async Task<IResult> GetAllMoviesAsync(
        // QNA: AsParameters vs FromQuery
        [AsParameters] GetAllMoviesRequest request,
        IMovieService movieService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userId = context.GetUserId();

        var options = request.MapToOptions()
            .WithUser(userId);

        var movies = await movieService.GetAllAsync(options, cancellationToken);
        var moviesCount = await movieService.GetCountAsync(options.Title, options.YearOfRelease, cancellationToken);

        return Results.Ok(movies.MapToResponse(
            request.Page.GetValueOrDefault(PagedRequest.DefaultPage),
            request.PageSize.GetValueOrDefault(PagedRequest.DefaultPageSize),
            moviesCount));
    }

    public static async Task<IResult> UpdateMovieAsync(
        Guid id,
        UpdateMovieRequest request,
        IMovieService movieService,
        IOutputCacheStore outputCacheStore,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userId = context.GetUserId();
        var movie = await movieService.UpdateAsync(request.MapToMovie(id), userId, cancellationToken);

        if (movie is null)
        {
            return Results.NotFound();
        }

        await outputCacheStore.EvictByTagAsync("movies", cancellationToken);

        return TypedResults.Ok(movie.MapToResponse());
    }

    public static async Task<IResult> DeleteMovieAsync(
        Guid id,
        IMovieService movieService,
        IOutputCacheStore outputCacheStore,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userId = context.GetUserId();
        var result = await movieService.DeleteByIdAsync(id, cancellationToken);

        if (!result)
        {
            return Results.NotFound();
        }

        await outputCacheStore.EvictByTagAsync("movies", cancellationToken);

        return Results.NoContent();
    }
}

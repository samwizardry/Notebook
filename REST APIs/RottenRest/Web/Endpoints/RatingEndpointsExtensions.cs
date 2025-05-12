using Microsoft.AspNetCore.Mvc;

using RottenRest.Application.Models;
using RottenRest.Application.Services;
using RottenRest.Contracts.Requests;
using RottenRest.Web.Auth;
using RottenRest.Web.Mapping;

namespace RottenRest.Web.Endpoints;

public static class RatingEndpointsExtensions
{
    public static IEndpointRouteBuilder MapRatingEndpoints(this IEndpointRouteBuilder app)
    {
        var moviesGroup = app.MapGroup(ApiEndpoints.Movies.Prefix)
            .WithApiVersionSet(ApiVersioning.VersionSet)
            .HasApiVersion(1.0d);

        var ratingsGroup = app.MapGroup(ApiEndpoints.Ratings.Prefix)
            .WithApiVersionSet(ApiVersioning.VersionSet)
            .HasApiVersion(1.0d);

        moviesGroup.MapPut(ApiEndpoints.Movies.Rate, RateMovieAsync)
            .WithName(nameof(RateMovieAsync))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        moviesGroup.MapDelete(ApiEndpoints.Movies.DeleteRating, DeleteRatingAsync)
            .WithName(nameof(DeleteRatingAsync))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        ratingsGroup.MapGet(ApiEndpoints.Ratings.GetUserRatings, GetUserRatingsAsync)
            .WithName(nameof(GetUserRatingsAsync))
            .Produces<IEnumerable<MovieRating>>(StatusCodes.Status200OK)
            .RequireAuthorization();

        return app;
    }

    public static async Task<IResult> RateMovieAsync(
        [FromRoute] Guid id,
        [FromBody] RateMovieRequest request,
        IRatingService ratingService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userId = context.GetUserId();

        return await ratingService.RateMovieAsync(id, userId!.Value, request.Rating, cancellationToken)
            ? Results.NoContent()
            : Results.NotFound();
    }

    public static async Task<IResult> DeleteRatingAsync(
        [FromRoute] Guid id,
        IRatingService ratingService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userId = context.GetUserId();

        return await ratingService.DeleteRatingAsync(id, userId!.Value, cancellationToken)
            ? Results.NoContent()
            : Results.NotFound();
    }

    public static async Task<IResult> GetUserRatingsAsync(
        IRatingService ratingService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userId = context.GetUserId();
        var ratings = await ratingService.GetRatingsForUserAsync(userId!.Value, cancellationToken);
        return TypedResults.Ok(ratings.MapToResponse());
    }
}

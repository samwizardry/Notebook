//using Asp.Versioning;

//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//using RottenRest.Application.Models;
//using RottenRest.Application.Services;
//using RottenRest.Contracts.Requests;
//using RottenRest.Web.Auth;
//using RottenRest.Web.Mapping;

//namespace RottenRest.Web.Controllers.V1;

//[ApiController]
//[ApiVersion(1.0d)]
//public class RatingsController : ControllerBase
//{
//    private readonly IRatingService _ratingService;

//    public RatingsController(IRatingService ratingService)
//    {
//        _ratingService = ratingService;
//    }

//    [Authorize]
//    [HttpPut(ApiEndpoints.Movies.Rate)]
//    [ProducesResponseType(StatusCodes.Status204NoContent)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    public async Task<IActionResult> RateMovie(
//        [FromRoute] Guid id,
//        [FromBody] RateMovieRequest request,
//        CancellationToken cancellationToken)
//    {
//        var userId = HttpContext.GetUserId();

//        return await _ratingService.RateMovieAsync(id, userId!.Value, request.Rating, cancellationToken)
//            ? NoContent()
//            : NotFound();
//    }

//    [Authorize]
//    [HttpDelete(ApiEndpoints.Movies.DeleteRating)]
//    [ProducesResponseType(StatusCodes.Status204NoContent)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    public async Task<IActionResult> DeleteRating([FromRoute] Guid id, CancellationToken cancellationToken)
//    {
//        var userId = HttpContext.GetUserId();

//        return await _ratingService.DeleteRatingAsync(id, userId!.Value, cancellationToken)
//            ? NoContent()
//            : NotFound();
//    }

//    [Authorize]
//    [HttpGet(ApiEndpoints.Ratings.GetUserRatings)]
//    [ProducesResponseType<IEnumerable<MovieRating>>(StatusCodes.Status200OK)]
//    public async Task<IActionResult> GetUserRatings(CancellationToken cancellationToken)
//    {
//        var userId = HttpContext.GetUserId();
//        var ratings = await _ratingService.GetRatingsForUserAsync(userId!.Value, cancellationToken);
//        return Ok(ratings.MapToResponse());
//    }
//}

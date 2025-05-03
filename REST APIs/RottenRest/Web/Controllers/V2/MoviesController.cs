using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;

using RottenRest.Application.Models;
using RottenRest.Application.Services;
using RottenRest.Contracts.Responses;
using RottenRest.Web.Auth;
using RottenRest.Web.Mapping;

namespace RottenRest.Web.Controllers;

[ApiController]
[ApiVersion(2.0d)]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    [ProducesResponseType<MovieResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();

        Movie? movie = Guid.TryParse(idOrSlug, out var id)
            ? await _movieService.GetByIdAsync(id, userId, cancellationToken)
            : await _movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);

        if (movie is null)
            return NotFound();

        var response = movie.MapToResponse();
        return Ok(response);
    }
}

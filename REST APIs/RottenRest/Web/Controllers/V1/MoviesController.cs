//using Asp.Versioning;

//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.OutputCaching;

//using RottenRest.Application.Models;
//using RottenRest.Application.Services;
//using RottenRest.Contracts.Requests;
//using RottenRest.Contracts.Responses;
//using RottenRest.Web.Auth;
//using RottenRest.Web.Mapping;

//namespace RottenRest.Web.Controllers.V1;

//[ApiController]
//[ApiVersion(1.0d)]
//public class MoviesController : ControllerBase
//{
//    private readonly IMovieService _movieService;
//    private readonly IOutputCacheStore _outputCacheStore;

//    public MoviesController(IMovieService movieService,
//        IOutputCacheStore outputCacheStore)
//    {
//        _movieService = movieService;
//        _outputCacheStore = outputCacheStore;
//    }

//    [Authorize(AuthConstants.TrustedMemberPolicyName)]
//    //[ServiceFilter<ApiKeyAuthFilter>]
//    [HttpPost(ApiEndpoints.Movies.Create)]
//    [ProducesResponseType<MovieResponse>(StatusCodes.Status201Created)]
//    [ProducesResponseType<ValidationFailureResponse>(StatusCodes.Status400BadRequest)]
//    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request, CancellationToken cancellationToken)
//    {
//        var movie = request.MapToMovie();

//        await _movieService.CreateAsync(movie, cancellationToken);
//        await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);

//        return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movie.MapToResponse());
//    }

//    [HttpGet(ApiEndpoints.Movies.Get)]
//    //[ResponseCache(Duration = 60, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
//    [OutputCache(PolicyName = "MovieCache")]
//    [ProducesResponseType<MovieResponse>(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    public async Task<IActionResult> Get([FromRoute] string idOrSlug, CancellationToken cancellationToken)
//    {
//        var userId = HttpContext.GetUserId();

//        Movie? movie = Guid.TryParse(idOrSlug, out var id)
//            ? await _movieService.GetByIdAsync(id, userId, cancellationToken)
//            : await _movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);

//        if (movie is null)
//            return NotFound();

//        var response = movie.MapToResponse();
//        return Ok(response);
//    }

//    [HttpGet(ApiEndpoints.Movies.GetAll)]
//    //[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any,
//    //    VaryByQueryKeys = ["title", "year", "sortBy", "page", "pageSize"], VaryByHeader = "Accept, Accept-Encoding")]
//    [OutputCache(PolicyName = "MovieCache")]
//    [ProducesResponseType<MoviesResponse>(StatusCodes.Status200OK)]
//    public async Task<IActionResult> GetAll([FromQuery] GetAllMoviesRequest request, CancellationToken cancellationToken)
//    {
//        var userId = HttpContext.GetUserId();

//        var options = request.MapToOptions()
//            .WithUser(userId);

//        var movies = await _movieService.GetAllAsync(options, cancellationToken);
//        var moviesCount = await _movieService.GetCountAsync(options.Title, options.YearOfRelease, cancellationToken);

//        return Ok(movies.MapToResponse(
//            request.Page.GetValueOrDefault(PagedRequest.DefaultPage),
//            request.PageSize.GetValueOrDefault(PagedRequest.DefaultPageSize),
//            moviesCount));
//    }

//    [Authorize(AuthConstants.TrustedMemberPolicyName)]
//    [HttpPut(ApiEndpoints.Movies.Update)]
//    [ProducesResponseType<MovieResponse>(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    [ProducesResponseType<ValidationFailureResponse>(StatusCodes.Status400BadRequest)]
//    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request, CancellationToken cancellationToken)
//    {
//        var userId = HttpContext.GetUserId();
//        var movie = await _movieService.UpdateAsync(request.MapToMovie(id), userId, cancellationToken);

//        if (movie is null)
//        {
//            return NotFound();
//        }

//        await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);

//        return Ok(movie.MapToResponse());
//    }

//    [Authorize(AuthConstants.AdminUserPolicyName)]
//    [HttpDelete(ApiEndpoints.Movies.Delete)]
//    [ProducesResponseType(StatusCodes.Status204NoContent)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
//    {
//        var userId = HttpContext.GetUserId();
//        var result = await _movieService.DeleteByIdAsync(id, cancellationToken);

//        if (!result)
//        {
//            return NotFound();
//        }

//        await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);

//        return NoContent();
//    }
//}


using FluentValidation;
using FluentValidation.Results;

using RottenRest.Application.Models;
using RottenRest.Application.Repositories;

namespace RottenRest.Application.Services;

public class RatingService : IRatingService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IMovieRepository _movieRepository;

    public RatingService(IRatingRepository ratingRepository,
        IMovieRepository movieRepository)
    {
        _ratingRepository = ratingRepository;
        _movieRepository = movieRepository;
    }

    public async Task<bool> RateMovieAsync(Guid movieId, Guid userId, int rating, CancellationToken cancellationToken = default)
    {
        if (rating is <= 1 or > 5)
        {
            throw new ValidationException([
                new ValidationFailure("Rating", "Rating must be between 1 and 5")
                ]);
        }

        if (!await _movieRepository.ExistsByIdAsync(movieId, cancellationToken))
        {
            return false;
        }

        return await _ratingRepository.RateMovieAsync(movieId, userId, rating, cancellationToken);
    }

    public Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        return _ratingRepository.DeleteRatingAsync(movieId, userId, cancellationToken);
    }

    public Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _ratingRepository.GetRatingsForUserAsync(userId, cancellationToken);
    }
}

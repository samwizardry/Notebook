using Dapper;

using RottenRest.Application.Data;
using RottenRest.Application.Models;

namespace RottenRest.Application.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RatingRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> RateMovieAsync(Guid movieId, Guid userId, int rating, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            insert into ratings (movie_id, user_id, rating)
            values (@movieId, @userId, @rating)
            on conflict (movie_id, user_id) do update
                set rating = @rating;
            """, new { movieId, userId, rating }, cancellationToken: cancellationToken));

        return result > 0;
    }

    public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<float?>(new CommandDefinition("""
            select round(avg(r.rating), 1)
            from ratings r
            where r.movie_id = @movieId
            """, new { movieId }, cancellationToken: cancellationToken));
    }

    public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<(float?, int?)>(new CommandDefinition("""
            with
                movie_ratings as (select rating, user_id from ratings where movie_id = @movieId)
            select
                (select round(avg(rating)) from movie_ratings) as rating,
                (select rating from movie_ratings where user_id = @userId) as userrating
            """, new { movieId, userId }, cancellationToken: cancellationToken));
    }

    public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            delete from ratings
            where movie_id = @movieId and user_id = @userId
            """, new { movieId, userId }, cancellationToken: cancellationToken));

        return result > 0;
    }

    public async Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.QueryAsync<MovieRating>(new CommandDefinition("""
            select r.movie_id as movieid, m.slug, r.rating
            from ratings as r
            inner join movies as m on r.movie_id = m.id
            where r.user_id = @userId;
            """, new { userId }, cancellationToken: cancellationToken));
    }
}

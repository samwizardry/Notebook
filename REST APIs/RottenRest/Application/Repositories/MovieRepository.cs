using Dapper;

using Microsoft.Extensions.Options;

using RottenRest.Application.Data;
using RottenRest.Application.Models;

namespace RottenRest.Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MovieRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            insert into movies (id, slug, title, yearofrelease)
            values (@Id, @Slug, @Title, @YearOfRelease);
            """, movie));

        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                    insert into genres (movie_id, name)
                    values (@Id, @Name);
                    """, new { Id = movie.Id, Name = genre }));
            }
        }

        transaction.Commit();

        return result > 0;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        string orderClause = string.Empty;

        if (!string.IsNullOrEmpty(options.SortField))
        {
            orderClause = $"""
                , m.{options.SortField}
                order by m.{options.SortField} {(options.SortOrder == SortOrder.Ascending ? "asc" : "desc")}
                """;
        }

        var result = await connection.QueryAsync(new CommandDefinition($"""
            select m.*,
                string_agg(distinct g.name, ',') as genres,
                round(avg(r.rating), 1) as rating,
                myr.rating as userrating
            from movies as m
            left join genres as g on m.id = g.movie_id
            left join ratings as r on m.id = r.movie_id
            left join ratings as myr on m.id = myr.movie_id and myr.user_id = @UserId
            where (@Title is null or m.title ilike ('%' || @Title || '%'))
                and (@YearOfRelease is null or m.yearofrelease = @YearOfRelease)
            group by m.id, myr.rating
            {orderClause}
            limit @PageSize
            offset (@Page - 1) * @PageSize
            """, options, cancellationToken: cancellationToken));

        return result.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            YearOfRelease = x.yearofrelease,
            Genres = Enumerable.ToList(x.genres.Split(',')),
            Rating = (float?)x.rating,
            UserRating = (int?)x.userrating
        });
    }

    public async Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.QuerySingleAsync<int>(new CommandDefinition($"""
            select count(id)
            from movies
            where (@title is null or title ilike ('%' || @title || '%'))
                and (@yearOfRelease is null or yearofrelease = @yearOfRelease)
            """, new { title, yearOfRelease }, cancellationToken: cancellationToken));
    }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(new CommandDefinition("""
            select m.id, m.slug, m.title, m.yearofrelease, round(avg(r.rating), 1) as rating, myr.rating as userrating
            from movies as m
            left join ratings as r on m.id = r.movie_id
            left join ratings as myr on m.id = myr.movie_id and myr.user_id = @userId
            where id = @id
            group by m.id, myr.rating;
            """, new { id, userId }, cancellationToken: cancellationToken));

        if (movie is null)
            return null;

        var genres = await connection.QueryAsync<string>(new CommandDefinition("""
            select name
            from genres
            where movie_id = @id;
            """, new { id }, cancellationToken: cancellationToken));

        movie.Genres.AddRange(genres);

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(new CommandDefinition("""
            select m.id, m.slug, m.title, m.yearofrelease, round(avg(r.rating), 1) as rating, myr.rating as userrating
            from movies as m
            left join ratings as r on m.id = r.movie_id
            left join ratings as myr on m.id = myr.movie_id and myr.user_id = @userId
            where slug = @slug
            group by m.id, myr.rating;
            """, new { slug, userId }, cancellationToken: cancellationToken));

        if (movie is null)
            return null;

        var genres = await connection.QueryAsync<string>(new CommandDefinition("""
            select name
            from genres
            where movie_id = @id;
            """, new { id = movie.Id }, cancellationToken: cancellationToken));

        movie.Genres.AddRange(genres);

        return movie;
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition("""
            delete from genres where movie_id = @Id
            """, new { movie.Id }, cancellationToken: cancellationToken));

        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                insert into genres (movie_id, name)
                values (@Id, @Name);
                """, new { movie.Id, Name = genre }, cancellationToken: cancellationToken));
        }

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            update movies set
                slug = @Slug,
                title = @Title,
                yearofrelease = @YearOfRelease
            where id = @Id
            """, movie, cancellationToken: cancellationToken));

        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();
        int result = 0;

        try
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                delete from genres where movie_id = @id
                """, new { id = id }, cancellationToken: cancellationToken));

            result = await connection.ExecuteAsync(new CommandDefinition("""
                delete from movies where id = @id
                """, new { id = id }, cancellationToken: cancellationToken));

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
        }

        return result > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var result = await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
            select count(1)
            from movies
            where id = @id
            """, new { id }, cancellationToken: cancellationToken));

        return result;
    }
}

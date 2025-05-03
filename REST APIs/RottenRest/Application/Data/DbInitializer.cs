using Dapper;

namespace RottenRest.Application.Data;

public class DbInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DbInitializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        await connection.ExecuteAsync("""
            create table if not exists movies (
                id uuid primary key,
                slug text not null,
                title text not null,
                yearofrelease integer not null
            );
            """);

        await connection.ExecuteAsync("""
            create unique index concurrently if not exists movies_slug_idx
            on movies
            using btree(slug);
            """);

        await connection.ExecuteAsync("""
            create table if not exists genres (
                movie_id uuid references movies (id),
                name text not null
            );
            """);

        await connection.ExecuteAsync("""
            create table if not exists ratings (
                movie_id uuid references movies (id),
                user_id uuid,
                rating integer not null,
                primary key (movie_id, user_id)
            );
            """);
    }
}

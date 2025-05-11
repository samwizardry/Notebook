using Dapper;

using Microsoft.AspNetCore.Connections;

namespace Library.Web.Data;

public class DatabaseInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DatabaseInitializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        await connection.ExecuteAsync(new CommandDefinition("""
            create table if not exists books (
                isbn text primary key,
                title text not null,
                author text not null,
                short_description text not null,
                page_count integer not null,
                release_date datetime not null
            );
            """));
    }
}

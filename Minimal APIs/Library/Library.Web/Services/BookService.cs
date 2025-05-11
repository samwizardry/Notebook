using System.Threading;

using Dapper;

using Library.Web.Data;
using Library.Web.Models;

namespace Library.Web.Services;

public class BookService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BookService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var existingBook = await GetByIsbnAsync(book.Isbn, cancellationToken);
        if (existingBook is not null)
        {
            return false;
        }

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            insert into books (isbn, title, author, short_description, page_count, release_date)
            values (@Isbn, @Title, @Author, @ShortDescription, @PageCount, @ReleaseDate)
            """, book, cancellationToken: cancellationToken));

        return result > 0;
    }

    public async Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.QueryAsync<Book>(new CommandDefinition("""
            select b.isbn, b.title, b.author, b.short_description as ShortDescription, b.page_count as PageCount, b.release_date as ReleaseDate
            from books b;
            """, cancellationToken: cancellationToken));
    }

    public async Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<Book>(new CommandDefinition("""
            select b.isbn, b.title, b.author, b.short_description as ShortDescription, b.page_count as PageCount, b.release_date as ReleaseDate
            from books b
            where b.isbn = @isbn;
            """, new { isbn }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Book>> SearchByTitleAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.QueryAsync<Book>(new CommandDefinition("""
            select b.isbn, b.title, b.author, b.short_description as ShortDescription, b.page_count as PageCount, b.release_date as ReleaseDate
            from books b
            where b.title like ('%' || @searchTerm || '%');
            """, new { searchTerm }, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(Book book, CancellationToken cancellationToken = default)
    {
        var existingBook = await GetByIsbnAsync(book.Isbn, cancellationToken);
        if (existingBook is null)
        {
            return false;
        }

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            update books
            set title = @Title,
                author = @Author,
                short_description = @ShortDescription,
                page_count = @PageCount,
                release_date = @ReleaseDate
            where isbn = @Isbn;
            """, book, cancellationToken: cancellationToken));

        return result > 0;
    }

    public async Task<bool> DeleteAsync(string isbn, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            delete from books
            where isbn = @isbn;
            """, new { isbn }, cancellationToken: cancellationToken));

        return result > 0;
    }
}

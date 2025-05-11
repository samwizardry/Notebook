using FluentValidation;
using FluentValidation.Results;

using Library.Web.Models;
using Library.Web.Services;

using Microsoft.AspNetCore.Mvc;

namespace Library.Web.Endpoints;

public class LibraryEndpoints : IEndpoints
{
    private const string BaseRoute = "/books";
    private const string ContentType = "application/json";
    private static readonly string[] Tags = ["Books"];

    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<BookService>();
    }

    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(BaseRoute, AddBookAsync)
            .WithName("AddBook")
            .WithTags(Tags)
            .Accepts<Book>(ContentType)
            .Produces<Book>(StatusCodes.Status201Created)
            .Produces<IEnumerable<ValidationFailure>>(StatusCodes.Status400BadRequest);
        //.AllowAnonymous();

        app.MapGet(BaseRoute, GetAllBooksAsync)
            .WithName("GetBooks")
            .WithTags(Tags)
            .Produces<IEnumerable<Book>>(StatusCodes.Status200OK);

        app.MapGet($"{BaseRoute}/{{isbn}}", GetBookAsync)
            .WithName("GetBook")
            .WithTags(Tags)
            .Produces<IEnumerable<Book>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPut($"{BaseRoute}/{{isbn}}", UpdateBookAsync)
            .WithName("UpdateBook")
            .WithTags(Tags)
            .Accepts<Book>(ContentType)
            .Produces<Book>(StatusCodes.Status200OK)
            .Produces<IEnumerable<ValidationFailure>>(StatusCodes.Status400BadRequest);

        app.MapDelete($"{BaseRoute}/{{isbn}}", DeleteBookAsync)
            .WithName("DeleteBook")
            .WithTags(Tags)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("status", Status)
            .ExcludeFromDescription();
        //.RequireCors("AnyOrigin");
    }

    internal static async Task<IResult> AddBookAsync(
        Book book,
        BookService service,
        IValidator<Book> validator,
        LinkGenerator linker,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(book);

        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var result = await service.AddAsync(book, cancellationToken);

        if (result)
        {
            var path = linker.GetUriByName(context, "GetBook", new { isbn = book.Isbn });
            return Results.Created(path, book);
            //return Results.Created($"{BaseRoute}/{book.Isbn}", book);
            //return Results.CreatedAtRoute("GetBook", new { isbn = book.Isbn }, book);
        }
        else
        {
            return Results.BadRequest(new List<ValidationFailure>
            {
                new ValidationFailure(nameof(Book.Isbn), "A book with this ISBN-13 already exists")
            });
        }
    }

    internal static async Task<IResult> GetAllBooksAsync(
        [FromQuery] string? searchTerm,
        BookService service,
        CancellationToken cancellationToken)
    {
        IEnumerable<Book>? books = null;

        if (string.IsNullOrEmpty(searchTerm))
        {
            books = await service.GetAllAsync(cancellationToken);
        }
        else
        {
            books = await service.SearchByTitleAsync(searchTerm, cancellationToken);
        }

        return Results.Ok(books);
    }

    internal static async Task<IResult> GetBookAsync(
        string isbn,
        BookService service,
        CancellationToken cancellationToken)
    {
        var book = await service.GetByIsbnAsync(isbn, cancellationToken);

        if (book is not null)
        {
            return Results.Ok(book);
        }

        return Results.NotFound();
    }

    internal static async Task<IResult> UpdateBookAsync(
        [FromRoute] string isbn,
        [FromBody] Book request,
        [FromServices] BookService service,
        [FromServices] IValidator<Book> validator,
        CancellationToken cancellationToken)
    {
        // FIXME: should act like request map to book
        Book book = new Book
        {
            Isbn = isbn,
            Title = request.Title,
            Author = request.Author,
            ShortDescription = request.ShortDescription,
            PageCount = request.PageCount,
            ReleaseDate = request.ReleaseDate
        };

        var validationResult = await validator.ValidateAsync(book);

        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var result = await service.UpdateAsync(book, cancellationToken);

        if (result)
        {
            return Results.Ok(book);
        }
        else
        {
            return Results.NotFound();
        }
    }

    internal static async Task<IResult> DeleteBookAsync(
        string isbn,
        BookService service,
        CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(isbn, cancellationToken);
        return deleted ? Results.NoContent() : Results.NotFound();
    }

    internal static IResult Status()
    {
        return Results.Extensions.Html("""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Document</title>
            </head>
            <body>
                <h1>Status</h1>
                <p>The server is working fine. Bye bye!</p>
            </body>
            </html>
            """);
    }
}

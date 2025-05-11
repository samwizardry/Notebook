using System.Net.Http.Json;

using FluentAssertions;

using Library.Web.Models;

namespace Library.Api.IntegrationTests;

public class LibraryEndpointsTests : IClassFixture<LibraryApiFactory>, IAsyncLifetime
{
    private readonly LibraryApiFactory _factory;
    private readonly List<string> _createdIsbns = [];

    public LibraryEndpointsTests(LibraryApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateBook_CreatesBook_WhenDataIsCorrect()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();

        // Act
        var response = await httpClient.PostAsJsonAsync("books", book);
        _createdIsbns.Add(book.Isbn);
        var createdBook = await response.Content.ReadFromJsonAsync<Book>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        createdBook.Should().BeEquivalentTo(book);
        response.Headers.Location.Should().Be($"{httpClient.BaseAddress}books/{book.Isbn}");
    }

    [Fact]
    public async Task CreateBook_Fails_WhenIsbnIsInvalid()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook(isbn: "INVALID");

        // Act
        var response = await httpClient.PostAsJsonAsync("books", book);
        _createdIsbns.Add(book.Isbn);
        var errors = await response.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be(nameof(Book.Isbn));
        error.ErrorMessage.Should().Be("Value was not a valid ISBN-13");
    }

    [Fact]
    public async Task CreateBook_Fails_WhenBookExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();

        // Act
        await httpClient.PostAsJsonAsync("books", book);
        _createdIsbns.Add(book.Isbn);
        var response = await httpClient.PostAsJsonAsync("books", book);
        var errors = await response.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be(nameof(Book.Isbn));
        error.ErrorMessage.Should().Be("A book with this ISBN-13 already exists");
    }

    [Fact]
    public async Task GetBook_ReturnsBook_WhenBookExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("books", book);
        _createdIsbns.Add(book.Isbn);

        // Act
        var response = await httpClient.GetAsync($"books/{book.Isbn}");
        var existingBook = await response.Content.ReadFromJsonAsync<Book>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        existingBook.Should().BeEquivalentTo(book);
    }

    [Fact]
    public async Task GetBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var isbn = GenerateIsbn();

        // Act
        var response = await httpClient.GetAsync($"books/{isbn}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllBooks_ReturnsAllBooks_WhenBooksExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("books", book);
        _createdIsbns.Add(book.Isbn);
        List<Book> arrangedBooks = [book];

        // Act
        var response = await httpClient.GetAsync($"books");
        var existingBooks = await response.Content.ReadFromJsonAsync<List<Book>>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        existingBooks.Should().BeEquivalentTo(arrangedBooks);
    }

    [Fact]
    public async Task GetAllBooks_ReturnsNoBooks_WhenNoBooksExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"books");
        var existingBooks = await response.Content.ReadFromJsonAsync<List<Book>>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        existingBooks.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchBooks_ReturnsBooks_WhenTitleMatches()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("books", book);
        _createdIsbns.Add(book.Isbn);
        List<Book> arrangedBooks = [book];

        // Act
        var response = await httpClient.GetAsync($"books?searchTerm=oder");
        var existingBooks = await response.Content.ReadFromJsonAsync<List<Book>>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        existingBooks.Should().BeEquivalentTo(arrangedBooks);
    }

    [Fact]
    public async Task UpdateBook_UpdatesBook_WhenDataIsCorrect()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var originalBook = GenerateBook();
        await httpClient.PostAsJsonAsync("books", originalBook);
        _createdIsbns.Add(originalBook.Isbn);

        // Act
        var modifiedBook = new Book
        {
            Isbn = originalBook.Isbn,
            Title = originalBook.Title,
            Author = originalBook.Author,
            ShortDescription = originalBook.ShortDescription,
            PageCount = 120,
            ReleaseDate = originalBook.ReleaseDate
        };
        var response = await httpClient.PutAsJsonAsync($"books/{originalBook.Isbn}", modifiedBook);
        var updatedBook = await response.Content.ReadFromJsonAsync<Book>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        updatedBook.Should().BeEquivalentTo(modifiedBook);
    }

    [Fact]
    public async Task UpdateBook_Fails_WhenDataIsIncorrect()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var originalBook = GenerateBook();
        await httpClient.PostAsJsonAsync("books", originalBook);
        _createdIsbns.Add(originalBook.Isbn);

        // Act
        var modifiedBook = new Book
        {
            Isbn = originalBook.Isbn,
            Title = string.Empty,
            Author = originalBook.Author,
            ShortDescription = originalBook.ShortDescription,
            PageCount = originalBook.PageCount,
            ReleaseDate = originalBook.ReleaseDate
        };
        var response = await httpClient.PutAsJsonAsync($"books/{originalBook.Isbn}", modifiedBook);
        var errors = await response.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be(nameof(Book.Title));
        error.ErrorMessage.Should().Be("Title must not be empty");
    }

    [Fact]
    public async Task UpdateBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();

        // Act
        var response = await httpClient.PutAsJsonAsync($"books/{book.Isbn}", book);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBook_ReturnsNoContent_WhenBookExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("books", book);
        _createdIsbns.Add(book.Isbn);

        // Act
        var response = await httpClient.DeleteAsync($"books/{book.Isbn}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();

        // Act
        var response = await httpClient.DeleteAsync($"books/{book.Isbn}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    private Book GenerateBook(string title = "The Dirty Coder", string? isbn = null)
    {
        return new Book
        {
            Isbn = isbn ?? GenerateIsbn(),
            Title = title,
            Author = "Nick Chapsas",
            ShortDescription = "All my tricks in one book.",
            PageCount = 322,
            ReleaseDate = new DateTime(2024, 1, 1)
        };
    }

    private string GenerateIsbn()
    {
        return $"{Random.Shared.Next(100, 999)}-{Random.Shared.Next(1_000_000_000, 2_100_999_999)}";
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        var httpClient = _factory.CreateClient();

        foreach (var isbn in _createdIsbns)
        {
            await httpClient.DeleteAsync($"/books/{isbn}");
        }
    }
}

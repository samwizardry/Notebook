using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;

using Refit;

using RottenRest.Contracts.Requests;
using RottenRest.Web.Sdk;
using RottenRest.Web.Sdk.Sandbox;

var services = new ServiceCollection();

services.AddHttpClient();
services.AddSingleton<AuthTokenProvider>();

services
    .AddRefitClient<IMoviesApi>(serviceProvider => new RefitSettings
    {
        AuthorizationHeaderValueGetter = async (_, _) =>
            await serviceProvider.GetRequiredService<AuthTokenProvider>().GetTokenAsync()
    })
    .ConfigureHttpClient(options =>
    {
        options.BaseAddress = new Uri("https://localhost:5001");
    });

var provider = services.BuildServiceProvider();
var moviesApi = provider.GetRequiredService<IMoviesApi>();

Console.WriteLine("Get movie by id or slug");
var movie = await moviesApi.GetMovieAsync("nick-the-greek-2025");
Console.WriteLine(JsonSerializer.Serialize(movie));

Console.WriteLine("Add movie");
movie = await moviesApi.CreateMovieAsync(new CreateMovieRequest
{
    Title = "Spider-Man 5",
    YearOfRelease = 2005,
    Genres = ["Action"]
});
Console.WriteLine(JsonSerializer.Serialize(movie));

Console.WriteLine("Update movie");
movie = await moviesApi.UpdateMovieAsync(movie.Id, new UpdateMovieRequest
{
    Title = movie.Title,
    YearOfRelease = movie.YearOfRelease,
    Genres = ["Action", "Adventure"]
});
Console.WriteLine(JsonSerializer.Serialize(movie));

Console.WriteLine("Delete movie");
await moviesApi.DeleteMovieAsync(movie.Id);

Console.WriteLine("Get all movies");
var movies = await moviesApi.GetMoviesAsync(new GetAllMoviesRequest
{
    Title = null,
    Year = null,
    SortBy = null,
    Page = 1,
    PageSize = 3
});

foreach (var m in movies.Items)
{
    Console.WriteLine(JsonSerializer.Serialize(m));
}

Console.ReadLine();

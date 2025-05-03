using FluentValidation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

using RottenRest.Application;
using RottenRest.Application.Data;
using RottenRest.Application.Repositories;
using RottenRest.Application.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static void AddApplication(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IMovieRepository, MovieRepository>();
        builder.Services.AddSingleton<IRatingRepository, RatingRepository>();
        builder.Services.AddSingleton<IMovieService, MovieService>();
        builder.Services.AddSingleton<IRatingService, RatingService>();

        builder.Services.AddValidatorsFromAssemblyContaining<ApplicationMarker>(ServiceLifetime.Singleton);
    }

    public static void AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
            new NpgsqlConnectionFactory(builder.Configuration.GetConnectionString("Movies")!));

        builder.Services.AddTransient<DbInitializer>();
    }
}

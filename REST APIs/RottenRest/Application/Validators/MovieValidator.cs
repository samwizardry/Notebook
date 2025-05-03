
using FluentValidation;

using RottenRest.Application.Models;
using RottenRest.Application.Repositories;
using RottenRest.Application.Services;

namespace RottenRest.Application.Validators;

public class MovieValidator : AbstractValidator<Movie>
{
    private readonly IMovieRepository _repository;

    public MovieValidator(IMovieRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty();

        RuleFor(x => x.YearOfRelease)
            .LessThanOrEqualTo(DateTime.UtcNow.Year);

        RuleFor(x => x.Genres)
            .NotEmpty();

        RuleFor(x => x.Slug)
            .MustAsync(ValidateSlug)
            .WithMessage("This movie already exists in the system");
    }

    private async Task<bool> ValidateSlug(Movie movie, string slug, CancellationToken token)
    {
        var existingMovie = await _repository.GetBySlugAsync(slug);

        if (existingMovie is not null)
        {
            return movie.Id == existingMovie.Id;
        }

        return existingMovie is null;
    }
}

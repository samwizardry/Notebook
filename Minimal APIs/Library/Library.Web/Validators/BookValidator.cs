using FluentValidation;

using Library.Web.Models;

namespace Library.Web.Validators;

public class BookValidator : AbstractValidator<Book>
{
    public BookValidator()
    {
        RuleFor(x => x.Isbn)
            .Matches(@"^(?=(?:\D*\d){10}(?:(?:\D*\d){3})?$)[\d-]+$")
            .WithMessage("Value was not a valid ISBN-13");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title must not be empty");

        RuleFor(x => x.Author)
            .NotEmpty();

        RuleFor(x => x.ShortDescription)
            .NotEmpty();

        RuleFor(x => x.PageCount)
            .GreaterThan(0);

        RuleFor(x => x.ReleaseDate)
            .LessThan(DateTime.Now);
    }
}

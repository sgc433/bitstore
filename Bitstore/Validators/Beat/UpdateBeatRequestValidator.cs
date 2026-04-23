using Bitstore.DTO.Beat;
using FluentValidation;

namespace Bitstore.Validators.Beat;

public class UpdateBeatRequestValidator: AbstractValidator<UpdateBeatRequest>
{
    public UpdateBeatRequestValidator()
    {
        RuleFor(x => x.Price)
            .NotNull().WithMessage("Price is required")
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.AudioUrl)
            .NotEmpty().WithMessage("AudioUrl cannot be empty");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title cannot be empty")
            .Length(1, 100).WithMessage("Title must be between 1 and 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.CoverUrl)
            .MaximumLength(500).WithMessage("CoverUrl cannot exceed 500 characters")
            .When(x => x.CoverUrl != null);
    }
}
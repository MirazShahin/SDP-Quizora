using FluentValidation;
using Quizora.Application.DTOs.Tests;

namespace Quizora.Application.Validators;

public class CreateTestDtoValidator : AbstractValidator<CreateTestDto>
{
    public CreateTestDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.DurationInMinutes)
            .GreaterThan(0).When(x => x.DurationInMinutes.HasValue)
            .WithMessage("Duration must be greater than 0 minutes");
    }
}
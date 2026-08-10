using FluentValidation;
using Quizora.Application.DTOs.Questions;

namespace Quizora.Application.Validators;

public class CreateQuestionDtoValidator : AbstractValidator<CreateQuestionDto>
{
    public CreateQuestionDtoValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Question text is required")
            .MaximumLength(1000);

        RuleFor(x => x.Options)
            .NotNull().WithMessage("Options are required")
            .Must(o => o != null && o.Count == 4)
            .WithMessage("Exactly 4 options are required");

        RuleFor(x => x.Options)
            .Must(o => o != null && o.Count(opt => opt.IsCorrect) == 1)
            .WithMessage("Exactly one option must be marked as correct");

        RuleForEach(x => x.Options).ChildRules(option =>
        {
            option.RuleFor(o => o.Text)
                .NotEmpty().WithMessage("Option text is required")
                .MaximumLength(500);
        });
    }
}
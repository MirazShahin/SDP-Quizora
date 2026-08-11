using FluentValidation;
using Quizora.Application.DTOs.Questions;

namespace Quizora.Application.Validators;

public class CreateQuestionDtoValidator : AbstractValidator<CreateQuestionDto>
{
    public CreateQuestionDtoValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.QuestionType).NotEmpty();

        When(x => string.Equals(x.QuestionType, "MCQ", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Options).NotNull().Must(o => o.Count >= 2)
                .WithMessage("MCQ needs at least 2 options");
            RuleFor(x => x.Options).Must(o => o.Count(c => c.IsCorrect) == 1)
                .WithMessage("Exactly one correct option required");
        });
    }
}

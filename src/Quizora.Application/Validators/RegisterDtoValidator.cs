using FluentValidation;
using Quizora.Application.DTOs.Auth;
using Quizora.Domain.Enums;
using System.Text.RegularExpressions;

namespace Quizora.Application.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(100).WithMessage("Full name is too long");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .Must(BeValidEmail).WithMessage("Please enter a valid email address (e.g. name@gmail.com)")
            .Must(NotBeCommonTypo).WithMessage("Email domain looks wrong. Did you mean gmail.com / yahoo.com / outlook.com?");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character (!@#$%^&* etc.)");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role");

        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .When(x => x.Role == UserRole.Company);
    }

    private static bool BeValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email.Trim(), pattern);
    }

    private static bool NotBeCommonTypo(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var domain = email.Trim().ToLowerInvariant().Split('@').LastOrDefault() ?? "";

        var typos = new HashSet<string>
        {
            "gmail.cm", "gmail.con", "gmail.co", "gmail.cpm", "gmail.om", "gmail.comm",
            "gmal.com", "gmial.com", "gamil.com", "gmaill.com",
            "yahoo.cm", "yahoo.con", "yaho.com",
            "outlook.cm", "outlook.con",
            "hotmail.cm", "hotmail.con"
        };

        return !typos.Contains(domain);
    }
}
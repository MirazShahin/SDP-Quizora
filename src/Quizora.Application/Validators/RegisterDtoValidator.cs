using FluentValidation;
using Quizora.Application.DTOs.Auth;
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

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.Password).WithMessage("Password and confirm password do not match");

        // Optional profile fields
        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone is too long")
            .Matches(@"^[0-9+\-\s]*$").WithMessage("Phone contains invalid characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Gender)
            .Must(g => g is null || g == "" || g == "Male" || g == "Female" || g == "Other")
            .WithMessage("Invalid gender");

        RuleFor(x => x.BloodGroup)
            .Must(bg => bg is null || bg == "" ||
                        new[] { "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-" }.Contains(bg))
            .WithMessage("Invalid blood group");
    }

    private static bool BeValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var pattern = @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$";
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
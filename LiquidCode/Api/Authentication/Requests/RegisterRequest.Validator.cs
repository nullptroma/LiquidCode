using FluentValidation;

namespace LiquidCode.Api.Authentication.Requests;

/// <summary>
/// Validator for registration requests
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters")
            .Matches(@"^[a-zA-Z0-9_\-\.]+$").WithMessage("Username can only contain letters, numbers, underscore, hyphen, or dot");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .MaximumLength(255).WithMessage("Password must not exceed 255 characters")
            .Must(HasUpperCase).WithMessage("Password must contain at least one uppercase letter")
            .Must(HasLowerCase).WithMessage("Password must contain at least one lowercase letter")
            .Must(HasDigit).WithMessage("Password must contain at least one digit");
    }

    private static bool HasUpperCase(string password) => password.Any(char.IsUpper);
    private static bool HasLowerCase(string password) => password.Any(char.IsLower);
    private static bool HasDigit(string password) => password.Any(char.IsDigit);
}

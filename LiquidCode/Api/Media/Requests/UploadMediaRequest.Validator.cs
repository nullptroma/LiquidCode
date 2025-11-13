using FluentValidation;
using LiquidCode.Shared.Constants;

namespace LiquidCode.Api.Media.Requests;

/// <summary>
/// Валидатор загрузки медиа.
/// </summary>
public class UploadMediaRequestValidator : AbstractValidator<UploadMediaRequest>
{
    public UploadMediaRequestValidator()
    {
        RuleFor(x => x.File)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("File is required")
            .Must(file => file == null || file.Length > 0)
            .WithMessage("File cannot be empty")
            .Must(file => file == null || file.Length <= AppConstants.MaxUploadFileSizeBytes)
            .WithMessage($"File size must not exceed {AppConstants.MaxUploadFileSizeMb}MB");
    }
}

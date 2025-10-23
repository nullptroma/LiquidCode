using FluentValidation;
using LiquidCode.Shared.Constants;

namespace LiquidCode.Api.Missions.Requests;

/// <summary>
/// Валидатор для запросов загрузки миссий
/// </summary>
public class UploadMissionRequestValidator : AbstractValidator<UploadMissionRequest>
{
    public UploadMissionRequestValidator()
    {
        RuleFor(x => x.MissionFile)
            .NotNull()
            .WithMessage("Mission file is required")
            .Custom((file, context) =>
            {
                if (file == null)
                    return;

                // Проверить размер файла
                if (file.Length == 0)
                {
                    context.AddFailure(nameof(UploadMissionRequest.MissionFile), "File cannot be empty");
                    return;
                }

                if (file.Length > AppConstants.MaxUploadFileSizeBytes)
                {
                    context.AddFailure(
                        nameof(UploadMissionRequest.MissionFile),
                        $"File size must not exceed {AppConstants.MaxUploadFileSizeMb}MB");
                    return;
                }

                // Проверить MIME тип
                if (!file.ContentType.Contains("zip", StringComparison.OrdinalIgnoreCase) &&
                    !file.ContentType.Contains("application/x-zip-compressed", StringComparison.OrdinalIgnoreCase) &&
                    !file.ContentType.Contains("application/x-zip", StringComparison.OrdinalIgnoreCase))
                {
                    context.AddFailure(
                        nameof(UploadMissionRequest.MissionFile),
                        "Only ZIP files are allowed");
                    return;
                }

                // Проверить расширение файла
                var fileName = file.FileName.ToLowerInvariant();
                if (!fileName.EndsWith(".zip"))
                {
                    context.AddFailure(
                        nameof(UploadMissionRequest.MissionFile),
                        "File must have .zip extension");
                }
            });

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Mission name is required")
            .Length(3, 255)
            .WithMessage("Mission name must be between 3 and 255 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_.()]+$")
            .WithMessage("Mission name contains invalid characters");

        RuleFor(x => x.Difficulty)
            .GreaterThan(0)
            .WithMessage("Difficulty must be greater than 0")
            .LessThanOrEqualTo(5)
            .WithMessage("Difficulty must be between 1 and 5");
    }
}

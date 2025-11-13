using FluentValidation;
using LiquidCode.Api.Shared;
using LiquidCode.Shared.Constants;
using LiquidCode.Shared.Validation;

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
                var allowedMimeTypes = new[]
                {
                    "application/zip",
                    "application/x-zip-compressed",
                    "application/x-zip"
                };

                if (!allowedMimeTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
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
            .RequiredText("Mission name", ValidationLengths.Mission.Name)
            .Matches(@"^[a-zA-Z0-9\s\-_.()]+$")
            .WithMessage("Mission name contains invalid characters");

        RuleFor(x => x.Difficulty)
            .GreaterThan(0)
            .WithMessage("Difficulty must be greater than 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("Difficulty must be between 1 and 10000");
    }
}

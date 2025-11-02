using System;
using System.Security.Cryptography;
using System.Text;
using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Infrastructure.Database.Entities;
using LiquidCode.Shared.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiquidCode.Domain.Services.Submits;

/// <summary>
/// Реализация сервиса генерации и проверки токенов обратного вызова.
/// </summary>
public sealed class SubmitCallbackTokenService : ISubmitCallbackTokenService
{
    private readonly byte[] _secretKey;
    private readonly ILogger<SubmitCallbackTokenService> _logger;

    public SubmitCallbackTokenService(IOptions<SubmitCallbackTokenOptions> options, ILogger<SubmitCallbackTokenService> logger)
    {
        _logger = logger;
        var secret = options?.Value.Secret;
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("Submit callback secret is not configured.");
        }

        _secretKey = Encoding.UTF8.GetBytes(secret);
    }

    public string GenerateToken(DbSolution solution)
    {
        ValidateSolution(solution);

        var payload = BuildPayload(solution);
        using var hmac = new HMACSHA256(_secretKey);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Base64UrlEncode(hash);
    }

    public bool ValidateToken(DbSolution solution, string providedToken)
    {
        if (string.IsNullOrWhiteSpace(providedToken))
            return false;

        try
        {
            var expected = GenerateToken(solution);
            var expectedBytes = Encoding.UTF8.GetBytes(expected);
            var providedBytes = Encoding.UTF8.GetBytes(NormalizeToken(providedToken));

            if (expectedBytes.Length != providedBytes.Length)
                return false;

            return CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate callback token for solution {SolutionId}", solution.Id);
            return false;
        }
    }

    private static void ValidateSolution(DbSolution solution)
    {
        if (solution == null)
            throw new ArgumentNullException(nameof(solution));

        if (solution.Id <= 0)
            throw new InvalidOperationException("Solution identifier must be assigned before generating token.");
    }

    private static string BuildPayload(DbSolution solution)
    {
        var timestamp = solution.Time.Kind == DateTimeKind.Utc
            ? solution.Time
            : solution.Time.ToUniversalTime();
        return $"{solution.Id}:{timestamp:O}";
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string NormalizeToken(string token) => token.Trim();
}

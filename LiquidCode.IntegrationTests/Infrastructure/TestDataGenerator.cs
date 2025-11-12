using System;

namespace LiquidCode.IntegrationTests.Infrastructure;

internal static class TestDataGenerator
{
    public static string UniqueUsername(string prefix) => $"{prefix}_{Guid.NewGuid():N}";

    public static string ValidPassword() => $"P@ssw0rd{Guid.NewGuid():N}";

    public static string EmailFor(string username) => $"{username}@example.com";

    public static string UniqueGroupName(string prefix = "Test Group") => $"{prefix} {Guid.NewGuid():N}";
}

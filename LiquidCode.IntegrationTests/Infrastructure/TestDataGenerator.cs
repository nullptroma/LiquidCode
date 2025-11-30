using System;

namespace LiquidCode.IntegrationTests.Infrastructure;

internal static class TestDataGenerator
{
    public static string UniqueUsername(string prefix) => $"{prefix}_{Guid.NewGuid():N}";

    public static string ValidPassword() => $"P@ssw0rd{Guid.NewGuid():N}";

    public static string EmailFor(string username) => $"{username}@example.com";

    public static string UniqueGroupName(string prefix = "Test Group") => $"{prefix} {Guid.NewGuid():N}";

    public static string UniqueMissionName(string prefix = "Mission") => $"{prefix} {Guid.NewGuid():N}";

    public static string UniqueArticleName(string prefix = "Article") => $"{prefix} {Guid.NewGuid():N}";

    public static string ArticleContent(string title) => $"# {title}\nGenerated content {Guid.NewGuid():N}";
}

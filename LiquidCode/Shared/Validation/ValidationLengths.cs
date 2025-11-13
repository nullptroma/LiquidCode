using LiquidCode.Shared.Constants;

namespace LiquidCode.Shared.Validation;

/// <summary>
/// Единый справочник ограничений длины строк для API и БД.
/// </summary>
public static class ValidationLengths
{
    public static class User
    {
        public const int UsernameMin = 4;
        public const int UsernameMax = 128;
        public static readonly LengthRange Username = new(UsernameMin, UsernameMax);

        public const int EmailMin = 4;
        public const int EmailMax = 256;
        public static readonly LengthRange Email = new(EmailMin, EmailMax);

        public const int PasswordMin = 8;
        public const int PasswordMax = 255;
        public static readonly LengthRange Password = new(PasswordMin, PasswordMax);

        public const int PasswordHashMax = 256;
    }

    public static class Group
    {
        public const int NameMin = 3;
        public const int NameMax = 128;
        public static readonly LengthRange Name = new(NameMin, NameMax);

        public const int DescriptionMax = 512;

        public const int FeedPostNameMax = 128;
        public const int FeedPostContentMin = 1;
        public const int FeedPostContentMax = 50_000;
        public static readonly LengthRange FeedPostContent = new(FeedPostContentMin, FeedPostContentMax);

        public const int JoinTokenMax = 128;
        public const int InvitationTokenMax = 128;
    }

    public static class Contest
    {
        public const int NameMin = 3;
        public const int NameMax = 128;
        public static readonly LengthRange Name = new(NameMin, NameMax);

        public const int DescriptionMax = 1024;
    }

    public static class Mission
    {
        public const int NameMin = 3;
        public const int NameMax = 128;
        public static readonly LengthRange Name = new(NameMin, NameMax);

        public const int S3KeyMax = 256;
        public const int StatementLanguageMax = 50;
        public const int StatementMediaFileNameMax = 256;
        public const int StatementMediaKeyMax = 512;
        public const int StatementMediaUrlMax = 512;
    }

    public static class Solution
    {
        public const int LanguageMin = 1;
        public const int LanguageMax = 16;
        public static readonly LengthRange Language = new(LanguageMin, LanguageMax);

        public const int LanguageVersionMin = 1;
        public const int LanguageVersionMax = 16;
        public static readonly LengthRange LanguageVersion = new(LanguageVersionMin, LanguageVersionMax);

        public const int SourceCodeMin = 1;
        public const int SourceCodeMax = 10_000;
        public static readonly LengthRange SourceCode = new(SourceCodeMin, SourceCodeMax);
        public const int StatusMax = 256;
        public const int TestingMessageMax = 10_000;
    }

    public static class RefreshToken
    {
        public const int TokenMin = 10;
        public const int TokenMax = 128;
        private const int TokenByteSize = AppConstants.RefreshTokenLength;
        public const int TokenEncodedLength = 4 * ((TokenByteSize + 2) / 3);

        public const int IpAddressMax = 128;
        public const int OsNameMax = 512;
    }

    public static class Article
    {
        public const int NameMin = 3;
        public const int NameMax = 128;
        public static readonly LengthRange Name = new(NameMin, NameMax);

        public const int ContentMin = 1;
        public const int ContentMax = 200_000;
        public static readonly LengthRange Content = new(ContentMin, ContentMax);

        public const int TagsMaxCount = 32;
    }

    public static class Tag
    {
        public const int NameMin = 2;
        public const int NameMax = 64;
        public static readonly LengthRange Name = new(NameMin, NameMax);
    }
}

/// <summary>
/// Диапазон длины строки.
/// </summary>
/// <param name="Min">Минимальная допустимая длина.</param>
/// <param name="Max">Максимальная допустимая длина.</param>
public readonly record struct LengthRange(int Min, int Max);

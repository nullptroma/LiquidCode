namespace LiquidCode.Models.Auth;

public record class AuthTokens(string Jwt, string RefreshToken);
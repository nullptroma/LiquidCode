namespace LiquidCode.Api.Authentication.Requests;

/// <summary>
/// Request model for user registration
/// </summary>
public record RegisterRequest(string Username, string Email, string Password);

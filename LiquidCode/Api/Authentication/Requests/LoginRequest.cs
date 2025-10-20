namespace LiquidCode.Api.Authentication.Requests;

/// <summary>
/// Request model for user login
/// </summary>
public record LoginRequest(string Username, string Password);

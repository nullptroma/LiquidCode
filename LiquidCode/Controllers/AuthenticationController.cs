using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace LiquidCode.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController(IConfiguration configuration) : ControllerBase
{
    [HttpPost]
    [Route("login/{username}")]
    public IActionResult Login(string username)
    {
        Console.WriteLine(Request.Headers.Authorization);
        var claims = new List<Claim> {new Claim(ClaimTypes.Name, username) };
        // создаем JWT-токен
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration[ConfigurationStrings.JwtSigningKey] ?? "0"));
        var jwt = new JwtSecurityToken(
            issuer: configuration[ConfigurationStrings.JwtIssuer],
            audience: configuration[ConfigurationStrings.JwtAudience],
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
            signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256));
            
        return Ok(new JwtSecurityTokenHandler().WriteToken(jwt));
    }

    [Authorize]
    [HttpGet]
    [Route("test")]
    public IActionResult Test()
    {
        return Ok(HttpContext.User.Identity?.Name ?? "noname");
    }
}
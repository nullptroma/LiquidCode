using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LiquidCode.Db;
using LiquidCode.Models.Api.AuthenticationController;
using LiquidCode.Models.Database;
using LiquidCode.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LiquidCode.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthenticationController(IConfiguration configuration, LiquidDbContext dbContext) : ControllerBase
{
    [HttpPost]
    [Route("register")]
    public IActionResult Register(RegisterModel model)
    {
        var userExists = dbContext.Users.Any(u => u.Username == model.Username);
        if (userExists)
            return new BadRequestObjectResult(StringResources.UserAlreadyExistsError);
        var salt = StringTools.RandomBase64(32);
        var passHash = (model.Password + salt).ComputeSha256();
        try
        {
            dbContext.Users.Add(new DbUser
                { Username = model.Username, Email = model.Email, Salt = salt, PassHash = passHash });
            dbContext.SaveChanges();
            return Login(new LoginModel(model.Username, model.Password));
        }
        catch
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    [Route("login")]
    public IActionResult Login(LoginModel model)
    {
        var user = dbContext.Users.FirstOrDefault(u => u.Username == model.Username);
        if (user == null)
            return Unauthorized();

        var passHash = (model.Password + user.Salt).ComputeSha256();
        if (passHash != user.PassHash)
            return Unauthorized();

        return AuthorizeUser(user);
    }

    [HttpPost]
    [Route("refresh")]
    public IActionResult Refresh(RefreshTokenModel model)
    {
        var token = dbContext.RefreshTokens.Include(rf => rf.DbUser)
            .FirstOrDefault(t => t.Token == model.RefreshToken);
        if (token == null)
            return Unauthorized();
        dbContext.RefreshTokens.Remove(token); // remove old token
        dbContext.SaveChanges();

        if (DateTime.UtcNow > token.Expires) // if token has been expired
            return Unauthorized();

        return AuthorizeUser(token.DbUser);
    }

    [HttpGet]
    [Authorize]
    [Route("whoami")]
    public IActionResult WhoAmI()
    {
        var username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        if (username == null)
            return Unauthorized();
        return Ok(username);
    }

    private IActionResult AuthorizeUser(DbUser dbUser)
    {
        var tokens = GenerateTokens(dbUser.Username, dbUser.Id);
        var refreshTokens = dbContext.RefreshTokens.Where(t => t.DbUser == dbUser);
        if (refreshTokens.Count() == 50) // if already 50 tokens, remove the oldest one
        {
            var oldest = refreshTokens.OrderBy(rf => rf.Expires).FirstOrDefault();
            if (oldest != null)
                dbContext.RefreshTokens.Remove(oldest);
        }

        var userAgent = Request.Headers.UserAgent.ToString();
        dbContext.RefreshTokens.Add(new DbRefreshToken
        {
            Token = tokens.RefreshToken,
            DbUser = dbUser,
            Expires = DateTime.UtcNow.Add(TimeSpan.FromDays(7)),
            OsName = userAgent.Substring(0, Math.Min(512, userAgent.Length)),
            IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""
        });
        dbContext.SaveChanges();
        return Ok(tokens);
    }

    private AuthTokens GenerateTokens(string username, int id)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, username), new(ClaimTypes.NameIdentifier, id.ToString()) };
        var securityKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration[ConfigurationStrings.JwtSigningKey] ?? "0"));
        var jwt = new JwtSecurityToken(
            configuration[ConfigurationStrings.JwtIssuer],
            configuration[ConfigurationStrings.JwtAudience],
            claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
            signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256));
        var token = new JwtSecurityTokenHandler().WriteToken(jwt)!;
        var refresh = StringTools.RandomBase64(64);
        return new AuthTokens(token, refresh);
    }
}
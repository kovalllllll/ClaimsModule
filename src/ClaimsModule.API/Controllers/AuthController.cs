using ClaimsModule.API.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(MockJwtTokenService tokenService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("users")]
    public IActionResult GetUsers() =>
        Ok(MockJwtTokenService.Users.Select(u => new { u.UserId, u.Name, u.Role }));

    [AllowAnonymous]
    [HttpPost("token")]
    public IActionResult GetToken([FromBody] AuthTokenRequest request)
    {
        var user = tokenService.FindByRole(request.Role);
        if (user is null)
        {
            return UnprocessableEntity(new
            {
                type = "ValidationError",
                title = "One or more validation errors occurred.",
                status = 422,
                errors = new Dictionary<string, string[]>
                {
                    ["Role"] = ["Role must be handler, supervisor, or manager."]
                }
            });
        }

        return Ok(new AuthTokenResponse(
            tokenService.GenerateToken(user),
            user.UserId,
            user.Name,
            user.Role,
            DateTimeOffset.UtcNow.AddHours(8)));
    }
}

public sealed record AuthTokenRequest(string Role);

public sealed record AuthTokenResponse(
    string Token,
    Guid UserId,
    string Name,
    string Role,
    DateTimeOffset ExpiresAt);

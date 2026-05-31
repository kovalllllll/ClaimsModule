using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClaimsModule.Persistence.Seeding;
using Microsoft.IdentityModel.Tokens;

namespace ClaimsModule.API.Auth;

public sealed class MockJwtTokenService(IConfiguration configuration)
{
    public static IReadOnlyList<MockUserInfo> Users { get; } =
    [
        new(Guid.Parse("11111111-0000-0000-0000-000000000001"), "John Handler", "handler"),
        new(Guid.Parse("22222222-0000-0000-0000-000000000002"), "Sarah Supervisor", "supervisor"),
        new(Guid.Parse("33333333-0000-0000-0000-000000000003"), "Mike Manager", "manager")
    ];

    public string GenerateToken(MockUserInfo user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim("userId", user.UserId.ToString()),
            new Claim("name", user.Name),
            new Claim("role", user.Role),
            new Claim("organisationId", SeedConstants.SeedOrganisationId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public MockUserInfo? FindByRole(string role) =>
        Users.FirstOrDefault(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
}

public sealed record MockUserInfo(Guid UserId, string Name, string Role);

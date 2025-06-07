using System.Security.Claims;

namespace BlazorAutoMode.Client.Models.Entities;

// Add properties to this class and update the server and client AuthenticationStateProviders
// to expose more information about the authenticated user to the client.
public sealed class UserInfo
{
    public required string UserId { get; init; }
    public required string Name { get; init; }
    public required string[] Roles { get; init; }

    public const string UserIdClaimType = "sub";
    public const string NameClaimType = ClaimTypes.Name;
    public const string RoleClaimType = ClaimTypes.Role;

    public static UserInfo FromClaimsPrincipal(ClaimsPrincipal principal) =>
        new()
        {
            UserId = GetRequiredClaim(principal, UserIdClaimType),
            Name = GetRequiredClaim(principal, NameClaimType),
            Roles = GetAllRequiredClaims(principal, RoleClaimType)
        };

    public ClaimsPrincipal ToClaimsPrincipal() =>
        new(new ClaimsIdentity(
            [new(UserIdClaimType, UserId), new(NameClaimType, Name), .. Roles.Select(role => new Claim(RoleClaimType, role))],
            authenticationType: "Cookies",
            nameType: NameClaimType,
            roleType: RoleClaimType));

    private static string GetRequiredClaim(ClaimsPrincipal principal, string claimType) =>
        principal.FindFirst(claimType)?.Value ?? throw new InvalidOperationException($"Could not find required '{claimType}' claim.");

    private static string[] GetAllRequiredClaims(ClaimsPrincipal principal, string claimType)
    {
        var claims = principal.FindAll(claimType).Select(c => c.Value).ToArray();
        if (claims.Length == 0)
        {
            throw new InvalidOperationException($"Could not find required '{claimType}' claims.");
        }

        return claims;        
    }
}

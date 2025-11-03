using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api.Utils;

public static class UserUtil
{
    public static int? GetUserId(HttpContext ctx)
    {
        var sub = ctx.User?.Claims?
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;

        return int.TryParse(sub, out var id) ? id : null;
    }
}

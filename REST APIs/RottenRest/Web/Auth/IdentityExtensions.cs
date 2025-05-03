using System.Security.Claims;

namespace RottenRest.Web.Auth;

public static class IdentityExtensions
{
    public static Guid? GetUserId(this HttpContext httpContext)
    {
        var userId = httpContext.User.Claims.SingleOrDefault(c => c.Type == "userid");

        if (Guid.TryParse(userId?.Value, out var uuid))
            return uuid;

        return null;
    }

    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var userId = user.Claims.SingleOrDefault(c => c.Type == "userid");

        if (Guid.TryParse(userId?.Value, out var uuid))
            return uuid;

        return null;
    }
}

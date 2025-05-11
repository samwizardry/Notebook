using Microsoft.AspNetCore.Authentication;

namespace Library.Web.Auth;

public class ApiKeyAuthSchemeOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; } = "VerySecret";
}

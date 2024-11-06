using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

public class CustomSessionAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public CustomSessionAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if session contains a UserId to authenticate the user
        if (!Context.Session.TryGetValue("UserId", out var userId))
        {
            return Task.FromResult(AuthenticateResult.Fail("User is not authenticated"));
        }

        // Create a claims identity if UserId is present in the session
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, System.Text.Encoding.UTF8.GetString(userId)) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

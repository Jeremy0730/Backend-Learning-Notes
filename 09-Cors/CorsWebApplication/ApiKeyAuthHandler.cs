using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public sealed class ApiKeyAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "ApiKey";

    private readonly string readerKey;
    private readonly string writerKey;
    private readonly string writerBKey;

    public ApiKeyAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<AuthSettings> auth)
        : base(options, logger, encoder)
    {
        readerKey = auth.Value.ReaderKey ?? "";
        writerKey = auth.Value.WriterKey ?? "";
        writerBKey = auth.Value.WriterBKey ?? "";
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Api-Key", out var header))
            return Task.FromResult(AuthenticateResult.NoResult());

        var provided = header.ToString();
        if (provided.Length == 0)
            return Task.FromResult(AuthenticateResult.NoResult());

        if (MatchKey(provided, writerKey))
            return Task.FromResult(Success("writer", "writer"));

        if (MatchKey(provided, writerBKey))
            return Task.FromResult(Success("writer-b", "writer"));

        if (MatchKey(provided, readerKey))
            return Task.FromResult(Success("reader", "reader"));

        return Task.FromResult(AuthenticateResult.Fail("The API key was rejected."));
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        await Response.WriteAsJsonAsync(new { error = "a valid X-Api-Key is required." });
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        await Response.WriteAsJsonAsync(new { error = "this key cannot change forecasts." });
    }

    private AuthenticateResult Success(string name, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, name),
            new Claim("role", role)
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }

    private static bool MatchKey(string provided, string configured)
    {
        if (configured.Length == 0)
            return false;

        var providedHash = SHA256.HashData(Encoding.UTF8.GetBytes(provided));
        var configuredHash = SHA256.HashData(Encoding.UTF8.GetBytes(configured));
        return CryptographicOperations.FixedTimeEquals(providedHash, configuredHash);
    }
}

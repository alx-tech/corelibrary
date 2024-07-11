using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using LeanCode.Kratos.Client.Api;
using LeanCode.Kratos.Client.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static LeanCode.Kratos.Client.Model.KratosSessionAuthenticationMethod;

namespace LeanCode.Kratos;

public partial class KratosAuthenticationHandler<TOptions> : AuthenticationHandler<TOptions>
    where TOptions : KratosAuthenticationOptions, new()
{
    private static readonly FrozenDictionary<MethodEnum, string> AuthenticationMethods = ExtractEnumNames<MethodEnum>();

    private static readonly FrozenDictionary<KratosAuthenticatorAssuranceLevel, string> AssuranceLevels =
        ExtractEnumNames<KratosAuthenticatorAssuranceLevel>();

    private readonly IFrontendApi api;

    public KratosAuthenticationHandler(
        IOptionsMonitor<TOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IFrontendApi api
    )
        : base(options, logger, encoder)
    {
        this.api = api;
    }

    [SuppressMessage("?", "CA1031", Justification = "Exception is returned to the caller, wrapped in Fail result.")]
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var response = await GetSessionAsync();
            var session = response?.Ok();

            if (session is null)
            {
                return AuthenticateResult.NoResult();
            }
            else if (session.Active != true)
            {
                return AuthenticateResult.Fail("Session is not active.");
            }
            else if (session.Identity is null)
            {
                return AuthenticateResult.Fail("Session does not contain an identity.");
            }
            if (!Options.AllowInactiveIdentities && session.Identity.State != KratosIdentity.StateEnum.Active)
            {
                return AuthenticateResult.Fail("Identity is not active.");
            }

            var claims = ExtractClaims(session);

            return AuthenticateResult.Success(
                new(
                    new(new ClaimsIdentity(claims, Scheme.Name, Options.NameClaimType, Options.RoleClaimType)),
                    Scheme.Name
                )
            );
        }
        catch (Exception e)
        {
            return AuthenticateResult.Fail(e);
        }
    }

    protected virtual async Task<IToSessionApiResponse?> GetSessionAsync()
    {
        if (Request.Headers.TryGetValue("X-Session-Token", out var token) && !string.IsNullOrEmpty(token))
        {
            return await api.ToSessionAsync(xSessionToken: token.ToString());
        }
        else if (
            AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out var value)
            && string.Equals(value.Scheme, "bearer", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrEmpty(value.Parameter)
        )
        {
            return await api.ToSessionAsync(xSessionToken: value.Parameter);
        }
        else if (
            Request.Cookies.TryGetValue(Options.SessionCookieName, out var cookie) && !string.IsNullOrEmpty(cookie)
        )
        {
            return await api.ToSessionAsync(cookie: $"{Options.SessionCookieName}={cookie}");
        }
        else
        {
            return null;
        }
    }

    protected virtual List<Claim> ExtractClaims(KratosSession session)
    {
        var claims = new List<Claim>
        {
            new(Options.NameClaimType, session.Identity!.Id),
            new("iss", SchemaUrlPathRegex().Replace(session.Identity.SchemaUrl, string.Empty)),
            new("iat", ToUnixTimeSecondsString(session.IssuedAt!.Value)),
            new("exp", ToUnixTimeSecondsString(session.ExpiresAt!.Value)),
            new("auth_time", ToUnixTimeSecondsString(session.AuthenticatedAt!.Value)),
        };

        if (session.AuthenticatorAssuranceLevel is { } aal && AssuranceLevels.TryGetValue(aal, out var aalName))
        {
            claims.Add(new("acr", aalName));
        }

        foreach (var am in session.AuthenticationMethods ?? [])
        {
            if (am.Method is { } method && AuthenticationMethods.TryGetValue(method, out var methodName))
            {
                claims.Add(new("amr", methodName));
            }
        }

        Options.ClaimsExtractor(session, Options, claims);

        return claims;
    }

    [GeneratedRegex("/schemas/[^/]+$")]
    private static partial Regex SchemaUrlPathRegex();

    private static string ToUnixTimeSecondsString(DateTime value) =>
        new DateTimeOffset(value).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

    private static FrozenDictionary<T, string> ExtractEnumNames<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T
    >()
        where T : struct, Enum
    {
        return Enumerable
            .Zip(Enum.GetNames<T>(), Enum.GetValues<T>())
            .DistinctBy(p => p.Second)
            .ToFrozenDictionary(p => p.Second, p => JsonNamingPolicy.SnakeCaseLower.ConvertName(p.First));
    }
}

public class KratosAuthenticationHandler : KratosAuthenticationHandler<KratosAuthenticationOptions>
{
    public KratosAuthenticationHandler(
        IOptionsMonitor<KratosAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IFrontendApi api
    )
        : base(options, logger, encoder, api) { }
}

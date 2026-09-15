using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using VetSanJose.Client.Services.Api;

namespace VetSanJose.Client.Services.Auth;

public class CustomAuthStateProvider(TokenStorage tokenStorage, AuthApiClient authApiClient) : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonimo = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var accessToken = await tokenStorage.GetAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(accessToken) || EstaExpirado(accessToken))
        {
            var renovado = await IntentarRenovarAsync();
            if (renovado is null)
            {
                return new AuthenticationState(Anonimo);
            }

            accessToken = renovado;
        }

        var claims = JwtParser.ParseClaimsFromJwt(accessToken).ToList();
        var identity = new ClaimsIdentity(claims, authenticationType: "jwt", nameType: ClaimTypes.Name, roleType: ClaimTypes.Role);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotificarSesionIniciada(string accessToken)
    {
        var claims = JwtParser.ParseClaimsFromJwt(accessToken).ToList();
        var identity = new ClaimsIdentity(claims, authenticationType: "jwt", nameType: ClaimTypes.Name, roleType: ClaimTypes.Role);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    public void NotificarSesionCerrada()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonimo)));
    }

    private async Task<string?> IntentarRenovarAsync()
    {
        var refreshToken = await tokenStorage.GetRefreshTokenAsync();
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var resultado = await authApiClient.RefreshAsync(refreshToken);
        if (resultado is null)
        {
            await tokenStorage.LimpiarAsync();
            return null;
        }

        await tokenStorage.GuardarAsync(resultado.AccessToken, resultado.RefreshToken);
        return resultado.AccessToken;
    }

    private static bool EstaExpirado(string accessToken)
    {
        var expClaim = JwtParser.ParseClaimsFromJwt(accessToken).FirstOrDefault(c => c.Type == "exp");
        if (expClaim is null || !long.TryParse(expClaim.Value, out var expUnix))
        {
            return true;
        }

        var expira = DateTimeOffset.FromUnixTimeSeconds(expUnix);
        return expira <= DateTimeOffset.UtcNow.AddSeconds(10);
    }
}

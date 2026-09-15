using System.Net;
using System.Net.Http.Headers;
using VetSanJose.Client.Services.Api;
using VetSanJose.Client.Services.Auth;

namespace VetSanJose.Client.Handlers;

public class AuthorizationMessageHandler(TokenStorage tokenStorage, AuthApiClient authApiClient, CustomAuthStateProvider authStateProvider)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await AdjuntarTokenAsync(request);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        var refreshToken = await tokenStorage.GetRefreshTokenAsync();
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return response;
        }

        var renovado = await authApiClient.RefreshAsync(refreshToken);
        if (renovado is null)
        {
            await tokenStorage.LimpiarAsync();
            authStateProvider.NotificarSesionCerrada();
            return response;
        }

        await tokenStorage.GuardarAsync(renovado.AccessToken, renovado.RefreshToken);
        authStateProvider.NotificarSesionIniciada(renovado.AccessToken);

        var reintento = await ClonarAsync(request);
        reintento.Headers.Authorization = new AuthenticationHeaderValue("Bearer", renovado.AccessToken);

        response.Dispose();
        return await base.SendAsync(reintento, cancellationToken);
    }

    private async Task AdjuntarTokenAsync(HttpRequestMessage request)
    {
        var accessToken = await tokenStorage.GetAccessTokenAsync();
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }

    private static async Task<HttpRequestMessage> ClonarAsync(HttpRequestMessage original)
    {
        var clon = new HttpRequestMessage(original.Method, original.RequestUri);

        if (original.Content is not null)
        {
            var bytes = await original.Content.ReadAsByteArrayAsync();
            clon.Content = new ByteArrayContent(bytes);
            foreach (var header in original.Content.Headers)
            {
                clon.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        foreach (var header in original.Headers)
        {
            clon.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clon;
    }
}

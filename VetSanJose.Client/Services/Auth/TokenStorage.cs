using Blazored.LocalStorage;

namespace VetSanJose.Client.Services.Auth;

public class TokenStorage(ILocalStorageService localStorage)
{
    private const string AccessTokenKey = "vsj_access_token";
    private const string RefreshTokenKey = "vsj_refresh_token";

    public ValueTask<string?> GetAccessTokenAsync() => localStorage.GetItemAsync<string>(AccessTokenKey);

    public ValueTask<string?> GetRefreshTokenAsync() => localStorage.GetItemAsync<string>(RefreshTokenKey);

    public async Task GuardarAsync(string accessToken, string refreshToken)
    {
        await localStorage.SetItemAsync(AccessTokenKey, accessToken);
        await localStorage.SetItemAsync(RefreshTokenKey, refreshToken);
    }

    public async Task LimpiarAsync()
    {
        await localStorage.RemoveItemAsync(AccessTokenKey);
        await localStorage.RemoveItemAsync(RefreshTokenKey);
    }
}

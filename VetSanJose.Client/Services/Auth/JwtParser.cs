using System.Security.Claims;
using System.Text.Json;

namespace VetSanJose.Client.Services.Auth;

public static class JwtParser
{
    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = Convert.FromBase64String(PadBase64(payload));
        using var doc = JsonDocument.Parse(jsonBytes);

        foreach (var property in doc.RootElement.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in property.Value.EnumerateArray())
                {
                    yield return new Claim(property.Name, item.ToString());
                }
            }
            else
            {
                yield return new Claim(property.Name, property.Value.ToString());
            }
        }
    }

    private static string PadBase64(string value)
    {
        value = value.Replace('-', '+').Replace('_', '/');
        return value.PadRight(value.Length + ((4 - (value.Length % 4)) % 4), '=');
    }
}

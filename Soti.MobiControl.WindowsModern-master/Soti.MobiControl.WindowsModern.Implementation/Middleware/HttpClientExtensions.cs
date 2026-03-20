using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Soti.MobiControl.WindowsModern.Implementation.Middleware;

internal static class HttpClientExtensions
{
    private const string AuthenticationSchemeBasic = "Basic";

    public static async Task<string> GetSettingsAsync(
        this HttpClient httpClient,
        AuthenticationSettings authSettings,
        string uri)
    {
        httpClient.AddBasicAuthenticationHeader(authSettings);

        var request = new HttpRequestMessage(HttpMethod.Get, uri);

        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    private static void AddBasicAuthenticationHeader(
        this HttpClient client,
        AuthenticationSettings settings)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(AuthenticationSchemeBasic, GetEncodedCredentials(settings));
    }

    private static string GetEncodedCredentials(AuthenticationSettings authenticationSettings)
    {
        return Convert.ToBase64String(
            Encoding.UTF8.GetBytes(
                $"{authenticationSettings.RegistrationCode}:{authenticationSettings.InstallationId}"));
    }
}

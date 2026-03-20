using System.Net.Http;
using Soti.MobiControl.WindowsModern.Middleware;

namespace Soti.MobiControl.WindowsModern.Implementation.Middleware;

/// <inheritdoc />
/// <seealso cref="IHttpClientProvider" />
internal sealed class HttpClientProvider : IHttpClientProvider
{
    private static readonly HttpClient HttpClient = new();

    /// <inheritdoc />
    public HttpClient Get()
    {
        return HttpClient;
    }
}

using System.Net.Http;

namespace Soti.MobiControl.WindowsModern.Middleware;

/// <summary>
/// Interface for HttpClientProvider.
/// </summary>
public interface IHttpClientProvider
{
    /// <summary>
    /// Creates an instance of HttpClient.
    /// </summary>
    /// <returns>HttpClient</returns>
    HttpClient Get();
}

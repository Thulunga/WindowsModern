using Soti.Api.Metadata;
using System.Collections.Generic;

namespace Soti.MobiControl.WindowsModern.Web;

/// <summary>
/// Interface for Windows Modern Dev Info UI Controller.
/// </summary>
[RoutePrefix("windowsModern/devInfo")]
[Authorize]
public interface IWindowsDevInfoUiController
{
    /// <summary>
    /// Returns models by manufacturer.
    /// </summary>
    /// <param name="manufacturer">The manufacturer.</param>
    /// <returns>List of model.</returns>
    [HttpGet]
    [Route("{manufacturer}/models")]
    IEnumerable<string> GetModelsByManufacturer([FromUri] string manufacturer);
}

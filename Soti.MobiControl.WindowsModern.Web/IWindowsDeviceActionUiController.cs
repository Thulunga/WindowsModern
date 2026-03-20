using System.Collections.Generic;
using Soti.Api.Metadata;

namespace Soti.MobiControl.WindowsModern.Web;

[RoutePrefix("windows/devices")]
public interface IWindowsDeviceActionUiController
{
    /// <summary>
    /// Retrieves Lock Passcode for a specific Windows Modern Device.
    /// </summary>
    /// <remarks>
    /// This API returns the Lock Passcode for a device.
    /// <br/>
    /// <br/>
    /// Requires the caller be granted the "Web Console" permission and "Disable Passcode Lock" Device permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2024.1.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="deviceId">The device identifier.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract Validation Failed.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    [HttpPost]
    [Route("{deviceId}/actions/fetchLockPasscode")]
    string RequestLockPasscode([FromUri] string deviceId);

    /// <summary>
    /// Returns a list of local groups across given devices.
    /// </summary>
    /// <param name="deviceIdentities">List of deviceIdentities.</param>
    /// <returns>List of local groups.</returns>
    [HttpPost]
    [Route("localGroups")]
    IEnumerable<string> GetDevicesLocalGroups([FromBody] IEnumerable<string> deviceIdentities);
}

using System.Collections.Generic;
using Soti.Api.Metadata;
using Soti.MobiControl.WindowsModern.Web.Contracts;

namespace Soti.MobiControl.WindowsModern.Web;

/// <summary>
/// Interface for Windows Modern Device Peripheral Controller.
/// </summary>
[RoutePrefix("windows/devices")]
public interface IWindowsDevicesPeripheralController
{
    /// <summary>
    /// Returns a list of peripheral.
    /// </summary>
    [HttpGet]
    [Route("{deviceId}/peripherals")]
    public IEnumerable<DevicePeripheralSummary> GetDevicePeripherals([FromUri] string deviceId);
}

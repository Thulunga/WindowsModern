using System.Collections.Generic;
using Soti.Api.Metadata;
using Soti.MobiControl.WindowsModern.Web.Contracts;

namespace Soti.MobiControl.WindowsModern.Web;

[RoutePrefix("windows/devices")]
public interface IDeviceHardwareController
{
    /// <summary>
    /// Returns a list of device hardware data
    /// </summary>
    [HttpGet]
    [Route("{deviceId}/deviceHardware")]
    IEnumerable<DeviceHardwareSummary> GetDeviceHardware([FromUri] string deviceId);

    /// <summary>
    /// Updates the device hardware status based on the hardware serial number
    /// </summary>
    [HttpPut]
    [Route("{deviceId}/deviceHardwareStatus")]
    void UpdateDeviceHardwareStatus([FromUri] string deviceId, [FromBody] DeviceHardwareStatusSumary hardwareStatusSummary);
}

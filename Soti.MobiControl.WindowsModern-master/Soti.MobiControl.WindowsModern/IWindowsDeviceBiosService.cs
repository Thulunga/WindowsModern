using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern;

/// <summary>
/// Provides services to process BIOS-related information for Windows devices.
/// </summary>
public interface IWindowsDeviceBiosService
{
    /// <summary>
    /// Retrieves a summary of BIOS boot priority settings for the specified devices.
    /// </summary>
    /// <param name="deviceIds">A collection of device IDs to retrieve BIOS boot priority summaries for.</param>
    /// <returns>A read-only list of <see cref="BiosBootPrioritySummary"/> objects representing the BIOS boot priorities of the specified devices.</returns>
    IReadOnlyList<BiosBootPrioritySummary> GetBiosBootPrioritySummary(IEnumerable<int> deviceIds);

    /// <summary>
    /// Synchronizes the BIOS boot order for the specified device.
    /// </summary>
    /// <param name="deviceId">The ID of the device for which to synchronize the BIOS boot order.</param>
    /// <param name="bootOrder">A string representing the desired BIOS boot order.</param>
    void SynchronizeBiosBootOrder(int deviceId, string bootOrder);

    /// <summary>
    /// Synchronizes the BIOS payload status for the specified device.
    /// </summary>
    /// <param name="deviceId">The ID of the device for which to synchronize the BIOS payload status.</param>
    /// <param name="payloadStatus">A string representing the BIOS payload status.</param>
    void SynchronizeBiosPayloadStatus(int deviceId, string payloadStatus);
}

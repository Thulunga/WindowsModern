using System;

namespace Soti.MobiControl.WindowsModern.Implementation.Models;

/// <summary>
/// Antivirus last scan device data.
/// </summary>
internal sealed class AntivirusLastScanDeviceData
{
    /// <summary>
    /// Gets or sets DevId.
    /// </summary>
    public string DevId { get; set; }

    /// <summary>
    /// Gets or sets LastScanDate.
    /// </summary>
    public DateTime LastScanDate { get; set; }
}

using System;

namespace Soti.MobiControl.WindowsModern.Web.Contracts;

/// <summary>
/// Antivirus last scan device summary.
/// </summary>
public sealed class AntivirusLastScanDeviceSummary
{
    /// <summary>
    /// Gets or sets DeviceId.
    /// </summary>
    public string DeviceId { get; set; }

    /// <summary>
    /// Gets or sets LastScanDate.
    /// </summary>
    public DateTime LastScanDate { get; set; }
}

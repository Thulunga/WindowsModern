using System;

namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Antivirus last scan device summary.
/// </summary>
public sealed class AntivirusLastScanDeviceSummary
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

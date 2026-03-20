using System;

namespace Soti.MobiControl.WindowsModern.Implementation.Models;

internal sealed class AntivirusThreatStatusDeviceData
{
    /// <summary>
    /// Gets or sets DevId.
    /// </summary>
    public string DevId { get; set; }

    /// <summary>
    /// Gets or sets LastStatusChangeTime.
    /// </summary>
    public DateTime LastStatusChangeTime { get; set; }

    /// <summary>
    /// Gets or sets ExternalThreatId.
    /// </summary>
    public long ExternalThreatId { get; set; }

    /// <summary>
    /// Gets or sets TypeId.
    /// </summary>
    public byte TypeId { get; set; }

    /// <summary>
    /// Gets or sets SeverityId.
    /// </summary>
    public byte? SeverityId { get; set; }
}

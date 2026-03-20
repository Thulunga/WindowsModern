using System;
using Soti.MobiControl.WindowsModern.Web.Enums;

namespace Soti.MobiControl.WindowsModern.Web.Contracts;

public sealed class AntivirusThreatStatusDevicesInfo
{
    /// <summary>
    /// Gets or sets DeviceId.
    /// </summary>
    public string DeviceId { get; set; }

    /// <summary>
    /// Gets or sets LastReported.
    /// </summary>
    public DateTime LastReported { get; set; }

    /// <summary>
    /// Gets or sets ThreatId.
    /// </summary>
    public long ThreatId { get; set; }

    /// <summary>
    /// Gets or sets ThreatCategory.
    /// </summary>
    public AntivirusThreatCategory ThreatCategory { get; set; }

    /// <summary>
    /// Gets or sets Severity.
    /// </summary>
    public AntivirusThreatSeverity? Severity { get; set; }
}

using System;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Device threat details model.
/// </summary>
public sealed class DeviceThreatDetails
{
    /// <summary>
    /// Gets or sets external DevId.
    /// </summary>
    public string DevId { get; set; }

    /// <summary>
    /// Gets or sets initital detection time.
    /// </summary>
    public DateTime InitialDetectionTime { get; set; }

    /// <summary>
    /// Gets or sets last status change time.
    /// </summary>
    public DateTime? LastStatusChangeTime { get; set; }

    /// <summary>
    /// Gets or sets antivirus threat status.
    /// </summary>
    public AntivirusThreatStatus ThreatStatus { get; set; }
}

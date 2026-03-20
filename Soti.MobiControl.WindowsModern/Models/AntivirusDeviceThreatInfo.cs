using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Antivirus Device Threat Info.
/// </summary>
public sealed class AntivirusDeviceThreatInfo : AntivirusThreatInfo
{
    /// <summary>
    /// Gets or sets ThreatName.
    /// </summary>
    public string ThreatName { get; set; }

    /// <summary>
    /// Gets or sets CurrentDetectionCount.
    /// </summary>
    public int CurrentDetectionCount { get; set; }

    /// <summary>
    /// Gets or sets LastThreatStatus.
    /// </summary>
    public AntivirusThreatStatus LastThreatStatus { get; set; }
}

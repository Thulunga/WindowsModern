namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Antivirus Group Threat Info.
/// </summary>
public sealed class AntivirusGroupThreatInfo : AntivirusThreatInfo
{
    /// <summary>
    /// Gets or sets CurrentDetectionCount.
    /// </summary>
    public int NoOfDevices { get; set; }
}

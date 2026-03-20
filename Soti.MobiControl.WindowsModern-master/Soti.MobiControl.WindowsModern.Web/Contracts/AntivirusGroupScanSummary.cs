namespace Soti.MobiControl.WindowsModern.Web.Contracts;

/// <summary>
/// Antivirus Group Scan Summary.
/// </summary>
public sealed class AntivirusGroupScanSummary
{
    /// <summary>
    /// Gets or sets devices scanned count within 24 hrs.
    /// </summary>
    public AntivirusScanCounts Within24Hrs { get; set; }

    /// <summary>
    /// Gets or sets devices scanned count within 7 days.
    /// </summary>
    public AntivirusScanCounts Within7Days { get; set; }

    /// <summary>
    /// Gets or sets devices scanned count prior to 30 days.
    /// </summary>
    public AntivirusScanCounts MoreThan30Days { get; set; }

    /// <summary>
    ///  Gets or sets devices scanned count based on the custom date range.
    /// </summary>
    public AntivirusScanCounts Custom { get; set; }
}

using System;
using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Antivirus last scan detail request.
/// </summary>
public sealed class AntivirusLastScanDetailRequest
{
    /// <summary>
    /// Gets of sets List of group Ids.
    /// </summary>
    public IEnumerable<int> GroupIds { get; set; }

    /// <summary>
    /// Gets of sets Device group sync completed datetime.
    /// </summary>
    public DateTime SyncCompletedOn { get; set; }

    /// <summary>
    /// Gets of sets Antivirus scan type.
    /// </summary>
    public AntivirusScanType AntivirusScanType { get; set; }

    /// <summary>
    /// Gets of sets Antivirus scan period.
    /// </summary>
    public AntivirusScanPeriodSubType AntivirusScanPeriod { get; set; }

    /// <summary>
    /// Gets of sets Start date for getting scan summary.
    /// </summary>
    public DateTime? LastScanStartDate { get; set; }

    /// <summary>
    /// Gets of sets End date for getting scan summary.
    /// </summary>
    public DateTime? LastScanEndDate { get; set; }

    /// <summary>
    /// Gets of sets Antivirus last scan records to skip.
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Gets of sets Antivirus last scan records to take.
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// Gets of sets Order of sorting.
    /// </summary>
    public bool Order { get; set; }
}

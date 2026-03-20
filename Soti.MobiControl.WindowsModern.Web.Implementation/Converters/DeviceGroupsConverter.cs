using System;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.MobiControl.WindowsModern.Web.Contracts;
using AntivirusGroupScanSummary = Soti.MobiControl.WindowsModern.Web.Contracts.AntivirusGroupScanSummary;
using AntivirusScanCounts = Soti.MobiControl.WindowsModern.Web.Contracts.AntivirusScanCounts;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Converters;

/// <summary>
/// DeviceGroupsConverter.
/// </summary>
internal static class DeviceGroupsConverter
{
    /// <summary>
    /// Converts an instance of <see cref="DeviceGroupSyncInfo"/> to
    /// <see cref="LastSyncSummary"/>.
    /// </summary>
    /// <param name="deviceGroupSyncInfo">The instance of DeviceGroupSyncInfo.</param>
    /// <returns>LastSyncSummary.</returns>
    public static LastSyncSummary ToContract(this DeviceGroupSyncInfo deviceGroupSyncInfo)
    {
        if (deviceGroupSyncInfo == null)
        {
            throw new ArgumentNullException(nameof(deviceGroupSyncInfo));
        }

        return new LastSyncSummary
        {
            LastSyncedOn = deviceGroupSyncInfo.CompletedOn,
            IsSyncInProgress = deviceGroupSyncInfo.SyncStatus is
                SyncRequestStatus.Queued or SyncRequestStatus.Running
        };
    }

    /// <summary>
    /// Converts an instance of <see cref="AntivirusGroupScanSummary"/> to
    /// <see cref="AntivirusGroupScanSummary"/>.
    /// </summary>
    /// <param name="antivirusGroupScanSummary">The instance of Antivirus Group Scan Summary.</param>
    /// <returns>AntivirusGroupScanSummary.</returns>
    public static AntivirusGroupScanSummary ToContract(this Models.AntivirusGroupScanSummary antivirusGroupScanSummary)
    {
        if (antivirusGroupScanSummary == null)
        {
            throw new ArgumentNullException(nameof(antivirusGroupScanSummary));
        }

        return new AntivirusGroupScanSummary
        {
            Within24Hrs = ToAntivirusScanCountsContract(antivirusGroupScanSummary.Within24Hrs),
            Within7Days = ToAntivirusScanCountsContract(antivirusGroupScanSummary.Within7Days),
            MoreThan30Days = ToAntivirusScanCountsContract(antivirusGroupScanSummary.MoreThan30Days),
            Custom = ToAntivirusScanCountsContract(antivirusGroupScanSummary.Custom)
        };
    }

    private static AntivirusScanCounts ToAntivirusScanCountsContract(Models.AntivirusScanCounts antivirusScanCounts)
    {
        return antivirusScanCounts != null
            ? new AntivirusScanCounts
            {
                QuickScanned = antivirusScanCounts.QuickScanned,
                FullScanned = antivirusScanCounts.FullScanned
            }
            : null;
    }
}

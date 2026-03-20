using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Web.Contracts;
using Soti.MobiControl.WindowsModern.Web.Enums;
using AntivirusLastScanDeviceSummaryContract = Soti.MobiControl.WindowsModern.Web.Contracts.AntivirusLastScanDeviceSummary;
using AntivirusLastScanDeviceSummaryModel = Soti.MobiControl.WindowsModern.Models.AntivirusLastScanDeviceSummary;
using AntivirusThreatCategory = Soti.MobiControl.WindowsModern.Web.Enums.AntivirusThreatCategory;
using AntivirusThreatSeverity = Soti.MobiControl.WindowsModern.Web.Enums.AntivirusThreatSeverity;
using AntivirusThreatStatus = Soti.MobiControl.WindowsModern.Web.Enums.AntivirusThreatStatus;
using DeviceThreatDetails = Soti.MobiControl.WindowsModern.Models.DeviceThreatDetails;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Converters;

internal static class WindowsDefenderConverter
{
    public static IEnumerable<AntivirusDeviceThreatHistory> ToDeviceThreatContract(this IEnumerable<AntivirusDeviceThreatInfo> antivirusThreatInfos)
    {
        return antivirusThreatInfos == null ? throw new ArgumentNullException(nameof(antivirusThreatInfos)) : antivirusThreatInfos.Select(m => m.ToDeviceThreatContract());
    }

    public static IEnumerable<AntivirusGroupThreatHistory> ToGroupThreatContract(this IEnumerable<AntivirusGroupThreatInfo> antivirusThreatInfos)
    {
        return antivirusThreatInfos == null ? throw new ArgumentNullException(nameof(antivirusThreatInfos)) : antivirusThreatInfos.Select(m => m.ToGroupThreatContract());
    }

    public static IEnumerable<Models.Enums.AntivirusThreatType> ToModel(this IEnumerable<AntivirusThreatCategory> antivirusThreatCategories)
    {
        if (antivirusThreatCategories == null)
        {
            throw new ArgumentNullException(nameof(antivirusThreatCategories));
        }

        return antivirusThreatCategories.Select(m => m.ToModel());
    }

    public static IEnumerable<Models.Enums.AntivirusThreatStatus> ToModel(this IEnumerable<AntivirusThreatStatus> antivirusThreatStatuses)
    {
        if (antivirusThreatStatuses == null)
        {
            throw new ArgumentNullException(nameof(antivirusThreatStatuses));
        }

        return antivirusThreatStatuses.Select(m => m.ToModel());
    }

    public static IEnumerable<Models.Enums.AntivirusThreatSeverity> ToModel(this IEnumerable<AntivirusThreatSeverity> antivirusThreatSeverities)
    {
        if (antivirusThreatSeverities == null)
        {
            throw new ArgumentNullException(nameof(antivirusThreatSeverities));
        }

        return antivirusThreatSeverities.Select(m => m.ToModel());
    }

    /// <summary>
    /// Converts to AntivirusScanSummary contract.
    /// </summary>
    /// <param name="antivirusScanSummary">The AntivirusScanSummary contract.</param>
    /// <returns>AntivirusScanSummary.</returns>
    public static Contracts.AntivirusScanSummary ToContract(
        this Models.AntivirusScanSummary antivirusScanSummary)
    {
        return antivirusScanSummary switch
        {
            null => throw new ArgumentNullException(nameof(antivirusScanSummary)),
            _ => new Contracts.AntivirusScanSummary
            {
                LastQuickScanTime = antivirusScanSummary.LastQuickScanTime,
                LastFullScanTime = antivirusScanSummary.LastFullScanTime,
                LastAntivirusSyncTime = antivirusScanSummary.LastAntivirusSyncTime,
                IsThreatsAvailable = antivirusScanSummary.IsThreatsAvailable,
                IsActiveThreatAvailable = antivirusScanSummary.IsActiveThreatAvailable,
                ThreatStatusCountSummary = antivirusScanSummary.ThreatStatusCountSummary?.ToThreatStatusSummary()
            }
        };
    }

    public static Dictionary<string, int> ToThreatStatusSummary(this IDictionary<Models.Enums.AntivirusThreatStatus, int> threatStatusSummary)
    {
        if (threatStatusSummary == null)
        {
            throw new ArgumentNullException(nameof(threatStatusSummary));
        }

        return threatStatusSummary.ToDictionary(
            a => a.Key.ToContract().ToString(),
            a => a.Value
        );
    }

    public static Models.Enums.AntivirusScanPeriodSubType ToModel(this AntivirusScanPeriodSubType antivirusScanPeriod)
    {
        return antivirusScanPeriod switch
        {
            AntivirusScanPeriodSubType.Within24Hrs => Models.Enums.AntivirusScanPeriodSubType.Within24Hrs,
            AntivirusScanPeriodSubType.Within7Days => Models.Enums.AntivirusScanPeriodSubType.Within7Days,
            AntivirusScanPeriodSubType.MoreThan30Days => Models.Enums.AntivirusScanPeriodSubType.MoreThan30Days,
            AntivirusScanPeriodSubType.Custom => Models.Enums.AntivirusScanPeriodSubType.Custom,
            _ => throw new InvalidEnumArgumentException()
        };
    }

    public static Models.Enums.AntivirusScanType ToModel(this AntivirusScanType antivirusScanPeriod)
    {
        return antivirusScanPeriod switch
        {
            AntivirusScanType.FullScan => Models.Enums.AntivirusScanType.FullScan,
            AntivirusScanType.QuickScan => Models.Enums.AntivirusScanType.QuickScan,
            _ => throw new InvalidEnumArgumentException()
        };
    }

    public static IEnumerable<AntivirusLastScanDeviceSummaryContract> ToLastScanSummary(this IEnumerable<AntivirusLastScanDeviceSummaryModel> antivirusLastScanDeviceSummary)
    {
        return antivirusLastScanDeviceSummary == null ? throw new ArgumentNullException(nameof(antivirusLastScanDeviceSummary)) : antivirusLastScanDeviceSummary.Select(e => e.ToLastScanSummary());
    }

    public static Models.Enums.AntivirusThreatStatus ToModel(this AntivirusThreatStatus threatStatus)
    {
        return threatStatus switch
        {
            AntivirusThreatStatus.Active => Models.Enums.AntivirusThreatStatus.Active,
            AntivirusThreatStatus.ActionFailed => Models.Enums.AntivirusThreatStatus.ActionFailed,
            AntivirusThreatStatus.ManualStepsRequired => Models.Enums.AntivirusThreatStatus.ManualStepsRequired,
            AntivirusThreatStatus.FullScanRequired => Models.Enums.AntivirusThreatStatus.FullScanRequired,
            AntivirusThreatStatus.RebootRequired => Models.Enums.AntivirusThreatStatus.RebootRequired,
            AntivirusThreatStatus.RemediatedWithNonCriticalFailures => Models.Enums.AntivirusThreatStatus.RemediatedWithNonCriticalFailures,
            AntivirusThreatStatus.Quarantined => Models.Enums.AntivirusThreatStatus.Quarantined,
            AntivirusThreatStatus.Removed => Models.Enums.AntivirusThreatStatus.Removed,
            AntivirusThreatStatus.Cleaned => Models.Enums.AntivirusThreatStatus.Cleaned,
            AntivirusThreatStatus.Allowed => Models.Enums.AntivirusThreatStatus.Allowed,
            AntivirusThreatStatus.NoStatusCleared => Models.Enums.AntivirusThreatStatus.NoStatusCleared,
            _ => throw new InvalidEnumArgumentException(),
        };
    }

    public static Contracts.DeviceThreatDetails ToContract(this Models.DeviceThreatDetails deviceThreatDetailsModel)
    {
        if (deviceThreatDetailsModel is null)
        {
            throw new ArgumentNullException(nameof(deviceThreatDetailsModel));
        }

        return new Contracts.DeviceThreatDetails
        {
            DeviceId = deviceThreatDetailsModel.DevId,
            ThreatStatus = deviceThreatDetailsModel.ThreatStatus.ToContract(),
            InitialDetectionTime = deviceThreatDetailsModel.InitialDetectionTime,
            LastStatusChangeTime = deviceThreatDetailsModel.LastStatusChangeTime
        };
    }

    public static IEnumerable<AntivirusThreatStatusDevicesInfo> ToContract(this IEnumerable<AntivirusThreatStatusDeviceDetails> antivirusThreatStatusDeviceDetails)
    {
        return antivirusThreatStatusDeviceDetails == null
            ? throw new ArgumentNullException(nameof(antivirusThreatStatusDeviceDetails))
            : antivirusThreatStatusDeviceDetails.Select(e => e.ToContract());
    }

    private static AntivirusLastScanDeviceSummaryContract ToLastScanSummary(this AntivirusLastScanDeviceSummaryModel antivirusLastScanDeviceSummary)
    {
        return new AntivirusLastScanDeviceSummaryContract
        {
            DeviceId = antivirusLastScanDeviceSummary.DevId,
            LastScanDate = antivirusLastScanDeviceSummary.LastScanDate
        };
    }

    public static IDictionary<string, string> ToDictionary(this DeviceThreatDetails threatDetails, int timezoneOffsetInMinutes = 0)
    {
        if (threatDetails == null)
        {
            throw new ArgumentNullException(nameof(threatDetails));
        }

        return new Dictionary<string, string>
        {
            { nameof(DeviceThreatDetails.ThreatStatus), threatDetails.ThreatStatus.ToString() },
            { nameof(DeviceThreatDetails.DevId), threatDetails.DevId },
            { nameof(DeviceThreatDetails.InitialDetectionTime), FormatWithTimezoneOffset(threatDetails.InitialDetectionTime, timezoneOffsetInMinutes) },
            {
                nameof(DeviceThreatDetails.LastStatusChangeTime), threatDetails.LastStatusChangeTime.HasValue
                    ? FormatWithTimezoneOffset(threatDetails.LastStatusChangeTime.Value, timezoneOffsetInMinutes)
                    : string.Empty
            }
        };
    }

    public static IDictionary<string, string> ToExportThreatStatusDictionary(this AntivirusThreatStatusDeviceDetails threatStatus, int timezoneOffsetInMinutes = 0)
    {
        if (threatStatus == null)
        {
            throw new ArgumentNullException(nameof(threatStatus));
        }

        return new Dictionary<string, string>
        {
            { nameof(AntivirusThreatStatusDeviceDetails.DevId), threatStatus.DevId },
            { nameof(AntivirusThreatStatusDeviceDetails.LastStatusChangeTime), FormatWithTimezoneOffset(threatStatus.LastStatusChangeTime, timezoneOffsetInMinutes) },
            { nameof(AntivirusThreatStatusDeviceDetails.ExternalThreatId), threatStatus.ExternalThreatId.ToString() },
            { nameof(AntivirusThreatStatusDeviceDetails.ThreatCategory), threatStatus.ThreatCategory.ToString() },
            { nameof(AntivirusThreatStatusDeviceDetails.Severity), threatStatus.Severity.ToString() },
        };
    }

    public static IDictionary<string, string> ToExportableDictionary(this AntivirusDeviceThreatInfo antivirusThreatInfo, int timeZoneOffset = 0)
    {
        if (antivirusThreatInfo == null)
        {
            throw new ArgumentNullException(nameof(antivirusThreatInfo));
        }

        return new Dictionary<string, string>
        {
            { nameof(antivirusThreatInfo.ExternalThreatId), antivirusThreatInfo.ExternalThreatId.ToString() },
            { nameof(antivirusThreatInfo.Type), antivirusThreatInfo.Type.ToString() },
            { nameof(antivirusThreatInfo.ThreatName), antivirusThreatInfo.ThreatName },
            { nameof(antivirusThreatInfo.Severity), antivirusThreatInfo.Severity.ToString() },
            { nameof(antivirusThreatInfo.InitialDetectionTime), FormatWithTimezoneOffset(antivirusThreatInfo.InitialDetectionTime, timeZoneOffset) },
            { nameof(antivirusThreatInfo.LastStatusChangeTime), FormatWithTimezoneOffset(antivirusThreatInfo.LastStatusChangeTime, timeZoneOffset) },
            { nameof(antivirusThreatInfo.CurrentDetectionCount), antivirusThreatInfo.CurrentDetectionCount.ToString()},
            { nameof(antivirusThreatInfo.LastThreatStatus), antivirusThreatInfo.LastThreatStatus.ToString() }
        };
    }

    public static IDictionary<string, string> ToDictionary(this AntivirusGroupThreatHistoryDetails antivirusGroupThreatHistoryDetails, int timeZoneOffsetInMinutes = 0)
    {
        if (antivirusGroupThreatHistoryDetails == null)
        {
            throw new ArgumentNullException(nameof(antivirusGroupThreatHistoryDetails));
        }

        return new Dictionary<string, string>
        {
            { nameof(AntivirusGroupThreatHistoryDetails.ExternalThreatId), antivirusGroupThreatHistoryDetails.ExternalThreatId.ToString() },
            { nameof(AntivirusGroupThreatHistoryDetails.ThreatCategory), antivirusGroupThreatHistoryDetails.ThreatCategory.ToString() },
            { nameof(AntivirusGroupThreatHistoryDetails.Severity), antivirusGroupThreatHistoryDetails.Severity.ToString() },
            { nameof(AntivirusGroupThreatHistoryDetails.InitialDetectionTime), FormatWithTimezoneOffset(antivirusGroupThreatHistoryDetails.InitialDetectionTime, timeZoneOffsetInMinutes) },
            {
                nameof(AntivirusGroupThreatHistoryDetails.LastStatusChangeTime), antivirusGroupThreatHistoryDetails.LastStatusChangeTime.HasValue
                    ? FormatWithTimezoneOffset(antivirusGroupThreatHistoryDetails.LastStatusChangeTime.Value, timeZoneOffsetInMinutes)
                    : string.Empty
            },
            { nameof(AntivirusGroupThreatHistoryDetails.NumberOfDevices), antivirusGroupThreatHistoryDetails.NumberOfDevices.ToString() }
        };
    }

    public static IDictionary<string, string> ToDictionary(this AntivirusLastScanDeviceSummaryModel antivirusLastScanDeviceSummaryModel, int timeZoneOffsetInMinutes = 0)
    {
        if (antivirusLastScanDeviceSummaryModel == null)
        {
            throw new ArgumentNullException(nameof(antivirusLastScanDeviceSummaryModel));
        }

        return new Dictionary<string, string>
        {
            { nameof(AntivirusLastScanDeviceSummaryModel.DevId), antivirusLastScanDeviceSummaryModel.DevId },
            { nameof(AntivirusLastScanDeviceSummaryModel.LastScanDate), FormatWithTimezoneOffset(antivirusLastScanDeviceSummaryModel.LastScanDate, timeZoneOffsetInMinutes) }
        };
    }

    private static AntivirusGroupThreatHistory ToGroupThreatContract(this AntivirusGroupThreatInfo antivirusThreatInfo)
    {
        return new AntivirusGroupThreatHistory
        {
            ThreatId = antivirusThreatInfo.ExternalThreatId,
            InitialDetectionTime = antivirusThreatInfo.InitialDetectionTime,
            LastStatusChangeTime = antivirusThreatInfo.LastStatusChangeTime,
            NoOfDevices = antivirusThreatInfo.NoOfDevices,
            Category = antivirusThreatInfo.Type.ToContract(),
            Severity = antivirusThreatInfo.Severity.ToContract(),
        };
    }

    private static AntivirusDeviceThreatHistory ToDeviceThreatContract(this AntivirusDeviceThreatInfo antivirusThreatInfo)
    {
        return new AntivirusDeviceThreatHistory
        {
            ThreatId = antivirusThreatInfo.ExternalThreatId,
            Name = antivirusThreatInfo.ThreatName,
            InitialDetectionTime = antivirusThreatInfo.InitialDetectionTime,
            LastStatusChangeTime = antivirusThreatInfo.LastStatusChangeTime,
            NumberOfDetectionsTillDate = antivirusThreatInfo.CurrentDetectionCount,
            Category = antivirusThreatInfo.Type.ToContract(),
            LastThreatStatus = antivirusThreatInfo.LastThreatStatus.ToContract(),
            Severity = antivirusThreatInfo.Severity.ToContract(),
        };
    }

    private static Models.Enums.AntivirusThreatType ToModel(this AntivirusThreatCategory threatCategory)
    {
        return threatCategory switch
        {
            AntivirusThreatCategory.Unknown => Models.Enums.AntivirusThreatType.Unknown,
            AntivirusThreatCategory.Malware => Models.Enums.AntivirusThreatType.Malware,
            AntivirusThreatCategory.Adware => Models.Enums.AntivirusThreatType.Adware,
            AntivirusThreatCategory.Dialer => Models.Enums.AntivirusThreatType.Dialer,
            AntivirusThreatCategory.Spyware => Models.Enums.AntivirusThreatType.Spyware,
            AntivirusThreatCategory.App => Models.Enums.AntivirusThreatType.App,
            AntivirusThreatCategory.Invalid => Models.Enums.AntivirusThreatType.Invalid,
            AntivirusThreatCategory.PasswordStealer => Models.Enums.AntivirusThreatType.PasswordStealer,
            AntivirusThreatCategory.TrojanDownloader => Models.Enums.AntivirusThreatType.TrojanDownloader,
            AntivirusThreatCategory.Worm => Models.Enums.AntivirusThreatType.Worm,
            AntivirusThreatCategory.Backdoor => Models.Enums.AntivirusThreatType.Backdoor,
            AntivirusThreatCategory.RemoteAccessTrojan => Models.Enums.AntivirusThreatType.RemoteAccessTrojan,
            AntivirusThreatCategory.Trojan => Models.Enums.AntivirusThreatType.Trojan,
            AntivirusThreatCategory.EmailFlooder => Models.Enums.AntivirusThreatType.EmailFlooder,
            AntivirusThreatCategory.Keylogger => Models.Enums.AntivirusThreatType.Keylogger,
            AntivirusThreatCategory.MonitoringSoftware => Models.Enums.AntivirusThreatType.MonitoringSoftware,
            AntivirusThreatCategory.BrowserModifier => Models.Enums.AntivirusThreatType.BrowserModifier,
            AntivirusThreatCategory.Cookie => Models.Enums.AntivirusThreatType.Cookie,
            AntivirusThreatCategory.BrowserPlugin => Models.Enums.AntivirusThreatType.BrowserPlugin,
            AntivirusThreatCategory.AOLExploit => Models.Enums.AntivirusThreatType.AOLExploit,
            AntivirusThreatCategory.Nuker => Models.Enums.AntivirusThreatType.Nuker,
            AntivirusThreatCategory.SecurityDisabler => Models.Enums.AntivirusThreatType.SecurityDisabler,
            AntivirusThreatCategory.JokeProgram => Models.Enums.AntivirusThreatType.JokeProgram,
            AntivirusThreatCategory.HostileActiveXControl => Models.Enums.AntivirusThreatType.HostileActiveXControl,
            AntivirusThreatCategory.SoftwareBundler => Models.Enums.AntivirusThreatType.SoftwareBundler,
            AntivirusThreatCategory.StealthModifier => Models.Enums.AntivirusThreatType.StealthModifier,
            AntivirusThreatCategory.SettingsModifier => Models.Enums.AntivirusThreatType.SettingsModifier,
            AntivirusThreatCategory.Toolbar => Models.Enums.AntivirusThreatType.Toolbar,
            AntivirusThreatCategory.RemoteControlSoftware1 => Models.Enums.AntivirusThreatType.RemoteControlSoftware1,
            AntivirusThreatCategory.TrojanFTP => Models.Enums.AntivirusThreatType.TrojanFTP,
            AntivirusThreatCategory.PotentialUnwantedSoftware => Models.Enums.AntivirusThreatType.PotentialUnwantedSoftware,
            AntivirusThreatCategory.ICQExploit => Models.Enums.AntivirusThreatType.ICQExploit,
            AntivirusThreatCategory.TrojanTelnet => Models.Enums.AntivirusThreatType.TrojanTelnet,
            AntivirusThreatCategory.Exploit => Models.Enums.AntivirusThreatType.Exploit,
            AntivirusThreatCategory.FileSharingProgram => Models.Enums.AntivirusThreatType.FileSharingProgram,
            AntivirusThreatCategory.MalwareCreationTool => Models.Enums.AntivirusThreatType.MalwareCreationTool,
            AntivirusThreatCategory.RemoteControlSoftware2 => Models.Enums.AntivirusThreatType.RemoteControlSoftware2,
            AntivirusThreatCategory.Tool => Models.Enums.AntivirusThreatType.Tool,
            AntivirusThreatCategory.TrojanDenialOfService => Models.Enums.AntivirusThreatType.TrojanDenialOfService,
            AntivirusThreatCategory.TrojanDropper => Models.Enums.AntivirusThreatType.TrojanDropper,
            AntivirusThreatCategory.TrojanMassMailer => Models.Enums.AntivirusThreatType.TrojanMassMailer,
            AntivirusThreatCategory.TrojanMonitoringSoftware => Models.Enums.AntivirusThreatType.TrojanMonitoringSoftware,
            AntivirusThreatCategory.TrojanProxyServer => Models.Enums.AntivirusThreatType.TrojanProxyServer,
            AntivirusThreatCategory.Virus => Models.Enums.AntivirusThreatType.Virus,
            AntivirusThreatCategory.Known => Models.Enums.AntivirusThreatType.Known,
            AntivirusThreatCategory.SPP => Models.Enums.AntivirusThreatType.SPP,
            AntivirusThreatCategory.Behavior => Models.Enums.AntivirusThreatType.Behavior,
            AntivirusThreatCategory.Vulnerability => Models.Enums.AntivirusThreatType.Vulnerability,
            AntivirusThreatCategory.Policy => Models.Enums.AntivirusThreatType.Policy,
            AntivirusThreatCategory.EUS => Models.Enums.AntivirusThreatType.EUS,
            AntivirusThreatCategory.Ransomware => Models.Enums.AntivirusThreatType.Ransomware,
            AntivirusThreatCategory.ASRRule => Models.Enums.AntivirusThreatType.ASRRule,
            _ => throw new InvalidEnumArgumentException(),
        };
    }

    private static Models.Enums.AntivirusThreatSeverity ToModel(this AntivirusThreatSeverity threatSeverity)
    {
        return threatSeverity switch
        {
            AntivirusThreatSeverity.Unknown => Models.Enums.AntivirusThreatSeverity.Unknown,
            AntivirusThreatSeverity.Low => Models.Enums.AntivirusThreatSeverity.Low,
            AntivirusThreatSeverity.Moderate => Models.Enums.AntivirusThreatSeverity.Moderate,
            AntivirusThreatSeverity.High => Models.Enums.AntivirusThreatSeverity.High,
            AntivirusThreatSeverity.Severe => Models.Enums.AntivirusThreatSeverity.Severe,
            _ => throw new InvalidEnumArgumentException(),
        };
    }

    private static AntivirusThreatCategory ToContract(this Models.Enums.AntivirusThreatType threatCategory)
    {
        return threatCategory switch
        {
            Models.Enums.AntivirusThreatType.Unknown => AntivirusThreatCategory.Unknown,
            Models.Enums.AntivirusThreatType.Malware => AntivirusThreatCategory.Malware,
            Models.Enums.AntivirusThreatType.Adware => AntivirusThreatCategory.Adware,
            Models.Enums.AntivirusThreatType.Dialer => AntivirusThreatCategory.Dialer,
            Models.Enums.AntivirusThreatType.Spyware => AntivirusThreatCategory.Spyware,
            Models.Enums.AntivirusThreatType.App => AntivirusThreatCategory.App,
            Models.Enums.AntivirusThreatType.Invalid => AntivirusThreatCategory.Invalid,
            Models.Enums.AntivirusThreatType.PasswordStealer => AntivirusThreatCategory.PasswordStealer,
            Models.Enums.AntivirusThreatType.TrojanDownloader => AntivirusThreatCategory.TrojanDownloader,
            Models.Enums.AntivirusThreatType.Worm => AntivirusThreatCategory.Worm,
            Models.Enums.AntivirusThreatType.Backdoor => AntivirusThreatCategory.Backdoor,
            Models.Enums.AntivirusThreatType.RemoteAccessTrojan => AntivirusThreatCategory.RemoteAccessTrojan,
            Models.Enums.AntivirusThreatType.Trojan => AntivirusThreatCategory.Trojan,
            Models.Enums.AntivirusThreatType.EmailFlooder => AntivirusThreatCategory.EmailFlooder,
            Models.Enums.AntivirusThreatType.Keylogger => AntivirusThreatCategory.Keylogger,
            Models.Enums.AntivirusThreatType.MonitoringSoftware => AntivirusThreatCategory.MonitoringSoftware,
            Models.Enums.AntivirusThreatType.BrowserModifier => AntivirusThreatCategory.BrowserModifier,
            Models.Enums.AntivirusThreatType.Cookie => AntivirusThreatCategory.Cookie,
            Models.Enums.AntivirusThreatType.BrowserPlugin => AntivirusThreatCategory.BrowserPlugin,
            Models.Enums.AntivirusThreatType.AOLExploit => AntivirusThreatCategory.AOLExploit,
            Models.Enums.AntivirusThreatType.Nuker => AntivirusThreatCategory.Nuker,
            Models.Enums.AntivirusThreatType.SecurityDisabler => AntivirusThreatCategory.SecurityDisabler,
            Models.Enums.AntivirusThreatType.JokeProgram => AntivirusThreatCategory.JokeProgram,
            Models.Enums.AntivirusThreatType.HostileActiveXControl => AntivirusThreatCategory.HostileActiveXControl,
            Models.Enums.AntivirusThreatType.SoftwareBundler => AntivirusThreatCategory.SoftwareBundler,
            Models.Enums.AntivirusThreatType.StealthModifier => AntivirusThreatCategory.StealthModifier,
            Models.Enums.AntivirusThreatType.SettingsModifier => AntivirusThreatCategory.SettingsModifier,
            Models.Enums.AntivirusThreatType.Toolbar => AntivirusThreatCategory.Toolbar,
            Models.Enums.AntivirusThreatType.RemoteControlSoftware1 => AntivirusThreatCategory.RemoteControlSoftware1,
            Models.Enums.AntivirusThreatType.TrojanFTP => AntivirusThreatCategory.TrojanFTP,
            Models.Enums.AntivirusThreatType.PotentialUnwantedSoftware => AntivirusThreatCategory.PotentialUnwantedSoftware,
            Models.Enums.AntivirusThreatType.ICQExploit => AntivirusThreatCategory.ICQExploit,
            Models.Enums.AntivirusThreatType.TrojanTelnet => AntivirusThreatCategory.TrojanTelnet,
            Models.Enums.AntivirusThreatType.Exploit => AntivirusThreatCategory.Exploit,
            Models.Enums.AntivirusThreatType.FileSharingProgram => AntivirusThreatCategory.FileSharingProgram,
            Models.Enums.AntivirusThreatType.MalwareCreationTool => AntivirusThreatCategory.MalwareCreationTool,
            Models.Enums.AntivirusThreatType.RemoteControlSoftware2 => AntivirusThreatCategory.RemoteControlSoftware2,
            Models.Enums.AntivirusThreatType.Tool => AntivirusThreatCategory.Tool,
            Models.Enums.AntivirusThreatType.TrojanDenialOfService => AntivirusThreatCategory.TrojanDenialOfService,
            Models.Enums.AntivirusThreatType.TrojanDropper => AntivirusThreatCategory.TrojanDropper,
            Models.Enums.AntivirusThreatType.TrojanMassMailer => AntivirusThreatCategory.TrojanMassMailer,
            Models.Enums.AntivirusThreatType.TrojanMonitoringSoftware => AntivirusThreatCategory.TrojanMonitoringSoftware,
            Models.Enums.AntivirusThreatType.TrojanProxyServer => AntivirusThreatCategory.TrojanProxyServer,
            Models.Enums.AntivirusThreatType.Virus => AntivirusThreatCategory.Virus,
            Models.Enums.AntivirusThreatType.Known => AntivirusThreatCategory.Known,
            Models.Enums.AntivirusThreatType.SPP => AntivirusThreatCategory.SPP,
            Models.Enums.AntivirusThreatType.Behavior => AntivirusThreatCategory.Behavior,
            Models.Enums.AntivirusThreatType.Vulnerability => AntivirusThreatCategory.Vulnerability,
            Models.Enums.AntivirusThreatType.Policy => AntivirusThreatCategory.Policy,
            Models.Enums.AntivirusThreatType.EUS => AntivirusThreatCategory.EUS,
            Models.Enums.AntivirusThreatType.Ransomware => AntivirusThreatCategory.Ransomware,
            Models.Enums.AntivirusThreatType.ASRRule => AntivirusThreatCategory.ASRRule,
            _ => throw new InvalidEnumArgumentException(),
        };
    }

    private static AntivirusThreatStatus ToContract(this Models.Enums.AntivirusThreatStatus threatStatus)
    {
        return threatStatus switch
        {
            Models.Enums.AntivirusThreatStatus.Active => AntivirusThreatStatus.Active,
            Models.Enums.AntivirusThreatStatus.ActionFailed => AntivirusThreatStatus.ActionFailed,
            Models.Enums.AntivirusThreatStatus.ManualStepsRequired => AntivirusThreatStatus.ManualStepsRequired,
            Models.Enums.AntivirusThreatStatus.FullScanRequired => AntivirusThreatStatus.FullScanRequired,
            Models.Enums.AntivirusThreatStatus.RebootRequired => AntivirusThreatStatus.RebootRequired,
            Models.Enums.AntivirusThreatStatus.RemediatedWithNonCriticalFailures => AntivirusThreatStatus.RemediatedWithNonCriticalFailures,
            Models.Enums.AntivirusThreatStatus.Quarantined => AntivirusThreatStatus.Quarantined,
            Models.Enums.AntivirusThreatStatus.Removed => AntivirusThreatStatus.Removed,
            Models.Enums.AntivirusThreatStatus.Cleaned => AntivirusThreatStatus.Cleaned,
            Models.Enums.AntivirusThreatStatus.Allowed => AntivirusThreatStatus.Allowed,
            Models.Enums.AntivirusThreatStatus.NoStatusCleared => AntivirusThreatStatus.NoStatusCleared,
            _ => throw new InvalidEnumArgumentException(),
        };
    }

    private static AntivirusThreatSeverity ToContract(this Models.Enums.AntivirusThreatSeverity threatSeverity)
    {
        return threatSeverity switch
        {
            Models.Enums.AntivirusThreatSeverity.Unknown => AntivirusThreatSeverity.Unknown,
            Models.Enums.AntivirusThreatSeverity.Low => AntivirusThreatSeverity.Low,
            Models.Enums.AntivirusThreatSeverity.Moderate => AntivirusThreatSeverity.Moderate,
            Models.Enums.AntivirusThreatSeverity.High => AntivirusThreatSeverity.High,
            Models.Enums.AntivirusThreatSeverity.Severe => AntivirusThreatSeverity.Severe,
            _ => throw new InvalidEnumArgumentException(),
        };
    }

    private static AntivirusThreatStatusDevicesInfo ToContract(this AntivirusThreatStatusDeviceDetails antivirusThreatStatusDeviceDetails)
    {
        return new AntivirusThreatStatusDevicesInfo
        {
            DeviceId = antivirusThreatStatusDeviceDetails.DevId,
            LastReported = antivirusThreatStatusDeviceDetails.LastStatusChangeTime,
            ThreatId = antivirusThreatStatusDeviceDetails.ExternalThreatId,
            ThreatCategory = antivirusThreatStatusDeviceDetails.ThreatCategory.ToContract(),
            Severity = antivirusThreatStatusDeviceDetails.Severity?.ToContract(),
        };
    }

    private static string FormatWithTimezoneOffset(DateTime dateTime, int timezoneOffsetInMinutes)
    {
        return dateTime.AddMinutes(timezoneOffsetInMinutes).ToString(CultureInfo.InvariantCulture);
    }
}

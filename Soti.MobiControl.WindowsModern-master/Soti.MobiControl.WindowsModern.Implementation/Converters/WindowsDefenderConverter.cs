using System;
using System.Collections.Generic;
using System.Linq;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Implementation.Converters;

internal static class WindowsDefenderConverter
{
    public static IEnumerable<AntivirusDeviceThreatInfo> ToDeviceThreatInfoModel(this IEnumerable<AntivirusThreatData> antivirusThreatData)
    {
        return antivirusThreatData == null
            ? throw new ArgumentNullException(nameof(antivirusThreatData))
            : antivirusThreatData.Select(e => e.ToDeviceThreatInfoModel());
    }

    public static IEnumerable<AntivirusThreatStatusDeviceDetails> ToThreatStatusDeviceDetailsModel(this IEnumerable<AntivirusThreatStatusDeviceData> threatStatusDeviceData)
    {
        return threatStatusDeviceData == null
            ? throw new ArgumentNullException(nameof(threatStatusDeviceData))
            : threatStatusDeviceData.Select(e => e.ToThreatStatusDeviceDetailsModel());
    }

    public static IEnumerable<AntivirusGroupThreatInfo> ToGroupThreatInfoModel(this IEnumerable<AntivirusThreatData> antivirusThreatData)
    {
        return antivirusThreatData == null
            ? throw new ArgumentNullException(nameof(antivirusThreatData))
            : antivirusThreatData.Select(e => e.ToGroupThreatInfoModel());
    }

    public static IEnumerable<AntivirusLastScanDeviceSummary> ToLastScanSummaryModel(this IEnumerable<AntivirusLastScanDeviceData> lastScanDeviceData)
    {
        return lastScanDeviceData == null
            ? throw new ArgumentNullException(nameof(lastScanDeviceData))
            : lastScanDeviceData.Select(e => e.ToLastScanSummaryModel());
    }

    public static Dictionary<AntivirusThreatStatus, int> ToThreatStatusSummary(this IDictionary<byte, int> threatStatusCountDictionary)
    {
        return threatStatusCountDictionary == null
            ? throw new ArgumentNullException(nameof(threatStatusCountDictionary))
            : threatStatusCountDictionary.ToDictionary(
            a => a.Key.ToEnum<AntivirusThreatStatus>(),
            a => a.Value
        );
    }

    public static IEnumerable<AntivirusGroupThreatHistoryDetails> ToGroupThreatHistoryDetails(this IEnumerable<DeviceGroupThreatHistoryData> deviceGroupThreatHistoryData)
    {
        return deviceGroupThreatHistoryData == null
            ? throw new ArgumentNullException(nameof(deviceGroupThreatHistoryData))
            : deviceGroupThreatHistoryData.Select(e => e.ToGroupThreatHistoryDetails());
    }

    private static AntivirusLastScanDeviceSummary ToLastScanSummaryModel(this AntivirusLastScanDeviceData lastScanDeviceData)
    {
        return new AntivirusLastScanDeviceSummary
        {
            DevId = lastScanDeviceData.DevId.Trim(),
            LastScanDate = lastScanDeviceData.LastScanDate
        };
    }

    private static AntivirusDeviceThreatInfo ToDeviceThreatInfoModel(this AntivirusThreatData antivirusThreatData)
    {
        return new AntivirusDeviceThreatInfo
        {
            ExternalThreatId = antivirusThreatData.ExternalThreatId,
            Type = antivirusThreatData.TypeId.ToEnum<AntivirusThreatType>(),
            ThreatName = antivirusThreatData.ThreatName,
            Severity = antivirusThreatData.SeverityId.ToEnum<AntivirusThreatSeverity>(),
            InitialDetectionTime = antivirusThreatData.InitialDetectionTime,
            LastStatusChangeTime = antivirusThreatData.LastStatusChangeTime,
            CurrentDetectionCount = antivirusThreatData.CurrentDetectionCount,
            LastThreatStatus = antivirusThreatData.LastThreatStatusId.ToEnum<AntivirusThreatStatus>()
        };
    }

    private static AntivirusThreatStatusDeviceDetails ToThreatStatusDeviceDetailsModel(this AntivirusThreatStatusDeviceData threatStatusDeviceData)
    {
        return new AntivirusThreatStatusDeviceDetails
        {
            DevId = threatStatusDeviceData.DevId,
            LastStatusChangeTime = threatStatusDeviceData.LastStatusChangeTime,
            ExternalThreatId = threatStatusDeviceData.ExternalThreatId,
            ThreatCategory = threatStatusDeviceData.TypeId.ToEnum<AntivirusThreatType>(),
            Severity = threatStatusDeviceData.SeverityId.HasValue ? threatStatusDeviceData.SeverityId.Value.ToEnum<AntivirusThreatSeverity>() : null,
        };
    }

    private static AntivirusGroupThreatInfo ToGroupThreatInfoModel(this AntivirusThreatData antivirusThreatData)
    {
        return new AntivirusGroupThreatInfo
        {
            ExternalThreatId = antivirusThreatData.ExternalThreatId,
            Type = antivirusThreatData.TypeId.ToEnum<AntivirusThreatType>(),
            Severity = antivirusThreatData.SeverityId.ToEnum<AntivirusThreatSeverity>(),
            InitialDetectionTime = antivirusThreatData.InitialDetectionTime,
            LastStatusChangeTime = antivirusThreatData.LastStatusChangeTime,
            NoOfDevices = antivirusThreatData.NoOfDevices,
        };
    }

    private static AntivirusGroupThreatHistoryDetails ToGroupThreatHistoryDetails(this DeviceGroupThreatHistoryData deviceGroupThreatHistoryData)
    {
        return new AntivirusGroupThreatHistoryDetails
        {
            ExternalThreatId = deviceGroupThreatHistoryData.ExternalThreatId,
            ThreatCategory = deviceGroupThreatHistoryData.TypeId.ToEnum<AntivirusThreatType>(),
            Severity = deviceGroupThreatHistoryData.SeverityId.HasValue ? deviceGroupThreatHistoryData.SeverityId.Value.ToEnum<AntivirusThreatSeverity>() : null,
            InitialDetectionTime = deviceGroupThreatHistoryData.InitialDetectionTime,
            LastStatusChangeTime = deviceGroupThreatHistoryData.LastStatusChangeTime,
            NumberOfDevices = deviceGroupThreatHistoryData.NoOfDevices,
        };
    }
}

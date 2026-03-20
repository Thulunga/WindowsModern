using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.Diagnostics;

namespace Soti.MobiControl.WindowsModern.Implementation;

/// <inheritdoc />
internal sealed class WindowsDeviceBiosService(
    IProgramTrace programTrace,
    IWindowsDeviceBootPriorityService bootPriorityService,
    IWindowsDeviceDataProvider windowsDeviceDataProvider)
    : IWindowsDeviceBiosService
{
    private readonly IProgramTrace _programTrace = programTrace ?? throw new ArgumentNullException(nameof(programTrace));
    private readonly IWindowsDeviceBootPriorityService _bootPriorityService = bootPriorityService ?? throw new ArgumentNullException(nameof(bootPriorityService));
    private readonly IWindowsDeviceDataProvider _windowsDeviceDataProvider = windowsDeviceDataProvider ?? throw new ArgumentNullException(nameof(windowsDeviceDataProvider));

    private const string LogName = nameof(WindowsDeviceBiosService);
    private static readonly char[] CommaSeparator = [','];

    /// <inheritdoc />
    public IReadOnlyList<BiosBootPrioritySummary> GetBiosBootPrioritySummary(IEnumerable<int> deviceIds)
    {
        if (deviceIds == null)
        {
            throw new ArgumentNullException(nameof(deviceIds));
        }

        var enumerable = deviceIds.ToList();
        if (!enumerable.Any())
        {
            throw new ArgumentOutOfRangeException(nameof(deviceIds));
        }

        var bootPriorityData = _bootPriorityService.BulkGetByDeviceId(enumerable);
        if (bootPriorityData == null || bootPriorityData.Count == 0)
        {
            return [];
        }

        var bootPriorityIds = bootPriorityData.Select(x => (short)x.BootPriorityId).Distinct().ToList();

        var bootPriorityNames = _bootPriorityService.BulkGetByIds(bootPriorityIds)
                                .ToDictionary(x => x.BootPriorityId, x => x.BootPriorityName);

        var summaries = bootPriorityData
            .GroupBy(x => x.DeviceId)
            .Select(group => new BiosBootPrioritySummary
            {
                DeviceId = group.Key,
                BootPriority = string.Join(", ",
                    group.OrderBy(x => x.BootOrder)
                        .Select(x =>
                        {
                            var name = bootPriorityNames.TryGetValue(x.BootPriorityId, out var value) ? value : "Unknown";
                            return string.IsNullOrWhiteSpace(name) || name.ToUpperInvariant() == "NOTCONFIGURED"
                                ? "Not Configured" : name;
                        }))
            });

        return summaries.ToList();
    }

    /// <inheritdoc />
    public void SynchronizeBiosBootOrder(int deviceId, string bootOrder)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (string.IsNullOrWhiteSpace(bootOrder))
        {
            throw new ArgumentException("Boot Order must not be null or whitespace.", nameof(bootOrder));
        }

        var bootDeviceNames = bootOrder
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(name => name.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.ToUpperInvariant())
                .Distinct()
                .ToList();

        var bootPriorities = _bootPriorityService.BulkInsertAndGet(bootDeviceNames);
        var nameToIdMap = bootPriorities.ToDictionary(x => x.BootPriorityName.ToUpperInvariant(), x => x.BootPriorityId);

        var bootPriorityDataList = bootOrder
            .Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select((name, index) => new WindowsDeviceBootPriority
            {
                DeviceId = deviceId,
                BootPriorityId = nameToIdMap[name.Trim().ToUpperInvariant()],
                BootOrder = index + 1
            }).ToList();

        _bootPriorityService.BulkModifyForDevice(bootPriorityDataList, deviceId);
    }

    /// <inheritdoc />
    public void SynchronizeBiosPayloadStatus(int deviceId, string payloadStatus)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (string.IsNullOrWhiteSpace(payloadStatus))
        {
            throw new ArgumentException("Payload Status must not be null or whitespace.", nameof(payloadStatus));
        }

        if (Enum.TryParse<BiosPasswordStatusType>(payloadStatus.Trim(), out var status))
        {
            _windowsDeviceDataProvider.UpdateBiosPasswordStatusId(deviceId, (byte)status);
        }
        else
        {
            LogError(deviceId, null, $"Unknown PayloadStatus: {payloadStatus}");
        }
    }

    private void LogError(int deviceId, Exception ex, string message)
    {
        _programTrace.Write(TraceLevel.Error, LogName, $"{message} for {deviceId}.");

        if (ex != null)
        {
            _programTrace.Write(TraceLevel.Error, LogName, ex.Message);
        }
    }
}

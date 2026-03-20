using System;
using System.Collections.Generic;
using System.Linq;
using Soti.Diagnostics;
using Soti.MobiControl.Devices;
using Soti.MobiControl.Events;
using Soti.MobiControl.WindowsModern.Implementation.Converters;
using Soti.MobiControl.WindowsModern.Implementation.Events;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.Time;
using Soti.Transactions;
using Soti.Utilities.Extensions;

namespace Soti.MobiControl.WindowsModern.Implementation;

/// <inheritdoc />
internal sealed class WindowsDefenderService : IWindowsDefenderService
{
    private const DeviceFamily WindowsModernDeviceFamily = DeviceFamily.WindowsPhone;
    private const int DaysWithin7Days = -7;
    private const int DaysWithin29Days = -29;
    private const int DaysWithin1Days = -1;

    private readonly IWindowsDefenderDataProvider _windowsDefenderDataProvider;
    private readonly ICurrentTimeSupplier _currentTimeSupplier;
    private readonly IWindowsDeviceService _windowsDeviceService;
    private readonly IProgramTrace _programTrace;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IDeviceIdentityMapper _deviceIdentityMapper;
    private readonly IWindowsDeviceDataProvider _windowsDeviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsDefenderService"/> class.
    /// </summary>
    /// <param name="windowsDefenderDataProvider">The instance of IWindowsDefenderDataProvider.</param>
    /// <param name="currentTimeSupplier">The instance of ICurrentTimeSupplier.</param>
    /// <param name="windowsDeviceService">The instance of IWindowsDeviceService.</param>
    /// <param name="programTrace">The instance of IProgramTrace.</param>
    /// <param name="eventDispatcher">The instance of IEventDispatcher.</param>
    /// <param name="deviceIdentityMapper">The instance of IDeviceIdentityMapper.</param>
    public WindowsDefenderService(IWindowsDefenderDataProvider windowsDefenderDataProvider,
        ICurrentTimeSupplier currentTimeSupplier,
        IWindowsDeviceService windowsDeviceService,
        IProgramTrace programTrace,
        IEventDispatcher eventDispatcher,
        IDeviceIdentityMapper deviceIdentityMapper,
        IWindowsDeviceDataProvider windowsDeviceProvider)
    {
        _windowsDefenderDataProvider = windowsDefenderDataProvider ?? throw new ArgumentNullException(nameof(windowsDefenderDataProvider));
        _currentTimeSupplier = currentTimeSupplier ?? throw new ArgumentNullException(nameof(currentTimeSupplier));
        _windowsDeviceService = windowsDeviceService ?? throw new ArgumentNullException(nameof(windowsDeviceService));
        _programTrace = programTrace ?? throw new ArgumentNullException(nameof(programTrace));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _deviceIdentityMapper = deviceIdentityMapper ?? throw new ArgumentNullException(nameof(deviceIdentityMapper));
        _windowsDeviceProvider = windowsDeviceProvider ?? throw new ArgumentNullException(nameof(windowsDeviceProvider));
    }

    /// <inheritdoc />
    public IEnumerable<AntivirusDeviceThreatInfo> GetAntivirusThreatHistory(
        int deviceId,
        IEnumerable<AntivirusThreatType> types,
        IEnumerable<AntivirusThreatSeverity> severities,
        IEnumerable<AntivirusThreatStatus> statuses,
        DateTime? startDate,
        DateTime? endDate,
        int skip,
        int take,
        AntivirusThreatSortByOption sortBy,
        bool order,
        out int totalCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);

        if (!types.AreAllDefined())
        {
            throw new ArgumentOutOfRangeException(nameof(types));
        }

        if (!severities.AreAllDefined())
        {
            throw new ArgumentOutOfRangeException(nameof(severities));
        }

        if (!statuses.AreAllDefined())
        {
            throw new ArgumentOutOfRangeException(nameof(statuses));
        }

        var threatTypes = types.ToArray();
        var threatSeverities = severities.ToArray();
        var threatStatuses = statuses.ToArray();

        ValidateDateRange(startDate, endDate);
        WindowsDeviceHelperMethods.ValidateDataRetrievalOptions(skip, take);
        sortBy.EnsureDefined();

        var typeIds = threatTypes.Any() ? threatTypes.Cast<byte>() : Enum.GetValues(typeof(AntivirusThreatType)).Cast<byte>();
        var severityIds = threatSeverities.Any() ? threatSeverities.Cast<byte>() : Enum.GetValues(typeof(AntivirusThreatSeverity)).Cast<byte>();
        var statusIds = threatStatuses.Any() ? threatStatuses.Cast<byte>() : Enum.GetValues(typeof(AntivirusThreatStatus)).Cast<byte>();
        var statusChangeStart = startDate ?? DateTime.MinValue;
        var statusChangeEnd = endDate ?? _currentTimeSupplier.GetUtcNow();

        return _windowsDefenderDataProvider.GetAntivirusThreatHistory(deviceId, typeIds, severityIds, statusIds, statusChangeStart, statusChangeEnd, skip, take, sortBy, order, out totalCount).ToDeviceThreatInfoModel();
    }

    /// <inheritdoc />
    public IEnumerable<AntivirusGroupThreatInfo> GetDeviceGroupsAntivirusThreatHistory(
        IEnumerable<int> groupIds,
        IEnumerable<AntivirusThreatType> types,
        IEnumerable<AntivirusThreatSeverity> severities,
        DateTime? startDate,
        DateTime? endDate,
        int skip,
        int take,
        AntivirusThreatHistorySortByOption sortBy,
        bool isDescendingSortOrder,
        out int total)
    {
        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        ValidateGroupIds(deviceGroupIds);
        if (!types.AreAllDefined())
        {
            throw new ArgumentOutOfRangeException(nameof(types));
        }

        if (!severities.AreAllDefined())
        {
            throw new ArgumentOutOfRangeException(nameof(severities));
        }

        sortBy.EnsureDefined();
        var threatTypes = types.ToArray();
        var threatSeverities = severities.ToArray();
        ValidateDateRange(startDate, endDate);
        WindowsDeviceHelperMethods.ValidateDataRetrievalOptions(skip, take);

        var typeIds = threatTypes.Any() ? threatTypes.Cast<byte>() : Enum.GetValues(typeof(AntivirusThreatType)).Cast<byte>();
        var severityIds = threatSeverities.Any() ? threatSeverities.Cast<byte>() : Enum.GetValues(typeof(AntivirusThreatSeverity)).Cast<byte>();
        var statusChangeStart = startDate ?? DateTime.MinValue;
        var statusChangeEnd = endDate ?? _currentTimeSupplier.GetUtcNow();

        return _windowsDefenderDataProvider.GetDeviceGroupsAntivirusThreatHistory(
            deviceGroupIds,
            WindowsModernDeviceFamily,
            typeIds,
            severityIds,
            statusChangeStart,
            statusChangeEnd,
            skip,
            take,
            sortBy,
            isDescendingSortOrder,
            out total).ToGroupThreatInfoModel();
    }

    /// <inheritdoc />
    public AntivirusScanSummary GetAntivirusScanSummary(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var antivirusScanTimeData = _windowsDeviceProvider.GetAntivirusScanTimeData(deviceId);
        var threatStatusCountForDeviceLast24Hours = antivirusScanTimeData is { IsThreatsAvailable: true, LastAntivirusSyncTime: not null }
            ? _windowsDefenderDataProvider.GetThreatStatusIdCountForDevice(deviceId, antivirusScanTimeData.LastAntivirusSyncTime.Value.Date.AddDays(-1),
                antivirusScanTimeData.LastAntivirusSyncTime.Value)
            : new Dictionary<byte, int>();

        return new AntivirusScanSummary
        {
            LastQuickScanTime = antivirusScanTimeData.AntivirusLastQuickScanTime,
            LastFullScanTime = antivirusScanTimeData.AntivirusLastFullScanTime,
            LastAntivirusSyncTime = antivirusScanTimeData.LastAntivirusSyncTime,
            IsThreatsAvailable = antivirusScanTimeData.IsThreatsAvailable,
            IsActiveThreatAvailable = antivirusScanTimeData.IsActiveThreatAvailable,
            ThreatStatusCountSummary = threatStatusCountForDeviceLast24Hours.ToThreatStatusSummary()
        };
    }

    /// <inheritdoc />
    public AntivirusThreatAvailabilityData GetActiveThreatsStatusByDeviceId(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        return _windowsDefenderDataProvider.GetAntivirusThreatAvailabilityByDeviceId(deviceId, AntivirusThreatStatus.Active);
    }

    /// <inheritdoc />
    public IEnumerable<AntivirusThreatAvailabilityData> GetActiveThreatsStatusByDeviceIds(IEnumerable<int> deviceIds)
    {
        var deviceIdentifiers = deviceIds as int[] ?? deviceIds.ToArray();
        if (deviceIdentifiers == null || !deviceIdentifiers.Any())
        {
            throw new ArgumentNullException(nameof(deviceIds));
        }

        return _windowsDefenderDataProvider.GetAntivirusThreatAvailabilityByDeviceIds(deviceIdentifiers, AntivirusThreatStatus.Active);
    }

    /// <inheritdoc />
    public IEnumerable<AntivirusDeviceThreatInfo> GetAllAntivirusThreatHistory(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        return _windowsDefenderDataProvider.GetAllAntivirusThreatHistory(deviceId).ToDeviceThreatInfoModel();
    }

    /// <inheritdoc/>
    public AntivirusGroupScanSummary GetDeviceGroupsDefaultAntivirusScanSummary(IEnumerable<int> groupIds, DateTime syncTime)
    {
        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        ValidateGroupIds(deviceGroupIds);

        return _windowsDeviceProvider.GetDeviceGroupsDefaultAntivirusScanSummary(deviceGroupIds, WindowsModernDeviceFamily, syncTime);
    }

    /// <inheritdoc/>
    public AntivirusGroupScanSummary GetDeviceGroupsCustomAntivirusScanSummary(IEnumerable<int> groupIds, DateTime startDate, DateTime endDate)
    {
        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        ValidateGroupIds(deviceGroupIds);
        ValidateDateRange(startDate, endDate);

        return _windowsDeviceProvider.GetDeviceGroupsCustomAntivirusScanSummary(deviceGroupIds, WindowsModernDeviceFamily, startDate, endDate);
    }

    /// <inheritdoc/>
    public IDictionary<AntivirusThreatStatus, int> GetAntivirusThreatStatusCount(IEnumerable<int> groupIds, DateTime? startDate, DateTime? endDate)
    {
        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        ValidateGroupIds(deviceGroupIds);
        ValidateDateRange(startDate, endDate);

        var statusChangeStart = startDate ?? DateTime.MinValue;
        var statusChangeEnd = endDate ?? _currentTimeSupplier.GetUtcNow();
        return _windowsDefenderDataProvider.GetAntivirusThreatStatusCount(deviceGroupIds, statusChangeStart, statusChangeEnd, (int)WindowsModernDeviceFamily).ToThreatStatusSummary();
    }

    /// <inheritdoc/>
    public IEnumerable<AntivirusLastScanDeviceSummary> GetDeviceGroupsAntivirusLastScanSummary(AntivirusLastScanDetailRequest antivirusLastScanDetailRequest, out int totalCount)
    {
        ArgumentNullException.ThrowIfNull(antivirusLastScanDetailRequest);

        ValidateGroupIds(antivirusLastScanDetailRequest.GroupIds);
        antivirusLastScanDetailRequest.AntivirusScanType.EnsureDefined();
        antivirusLastScanDetailRequest.AntivirusScanPeriod.EnsureDefined();
        WindowsDeviceHelperMethods.ValidateDataRetrievalOptions(antivirusLastScanDetailRequest.Skip, antivirusLastScanDetailRequest.Take);
        ValidateDateRange(antivirusLastScanDetailRequest.LastScanStartDate, antivirusLastScanDetailRequest.LastScanEndDate);

        var (scanStartDate, scanEndDate) = ValidateAndCalculateLastScanTime(antivirusLastScanDetailRequest.AntivirusScanPeriod, antivirusLastScanDetailRequest.LastScanStartDate,
            antivirusLastScanDetailRequest.LastScanEndDate, antivirusLastScanDetailRequest.SyncCompletedOn);

        return antivirusLastScanDetailRequest.AntivirusScanType switch
        {
            AntivirusScanType.FullScan => _windowsDeviceProvider.GetAntivirusLastFullScanSummary(
                    antivirusLastScanDetailRequest.GroupIds,
                    scanStartDate,
                    scanEndDate,
                    WindowsModernDeviceFamily,
                    antivirusLastScanDetailRequest.Skip,
                    antivirusLastScanDetailRequest.Take,
                    antivirusLastScanDetailRequest.Order,
                    out totalCount)
                .ToLastScanSummaryModel(),
            AntivirusScanType.QuickScan => _windowsDeviceProvider.GetAntivirusLastQuickScanSummary(
                    antivirusLastScanDetailRequest.GroupIds,
                    scanStartDate,
                    scanEndDate,
                    WindowsModernDeviceFamily,
                    antivirusLastScanDetailRequest.Skip,
                    antivirusLastScanDetailRequest.Take,
                    antivirusLastScanDetailRequest.Order,
                    out totalCount)
                .ToLastScanSummaryModel(),
            _ => throw new ArgumentOutOfRangeException(nameof(antivirusLastScanDetailRequest.AntivirusScanType))
        };
    }

    /// <inheritdoc />
    public IReadOnlyCollection<DeviceThreatDetails> GetDeviceThreatStatusByGroupIds(
        long threatId,
        IEnumerable<int> groupIds,
        DateTime? startDate,
        DateTime? endDate)
    {
        if (threatId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(threatId));
        }

        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        ValidateGroupIds(deviceGroupIds);
        ValidateDateRange(startDate, endDate);
        var statusChangeStart = startDate ?? DateTime.MinValue;
        var statusChangeEnd = endDate ?? _currentTimeSupplier.GetUtcNow();

        return _windowsDefenderDataProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamily(
            threatId,
            deviceGroupIds,
            WindowsModernDeviceFamily,
            statusChangeStart,
            statusChangeEnd);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<DeviceThreatDetails> GetDeviceThreatStatusByGroupIdsPaginated(
        long threatId,
        IEnumerable<int> groupIds,
        DateTime? startDate,
        DateTime? endDate,
        int skip,
        int take,
        AntivirusThreatHistoryDetailsSortByOption sortBy,
        bool isDescendingOrder,
        out int totalCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(threatId);

        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        ValidateGroupIds(deviceGroupIds);
        ValidateDateRange(startDate, endDate);
        WindowsDeviceHelperMethods.ValidateDataRetrievalOptions(skip, take);
        sortBy.EnsureDefined();

        var statusChangeStart = startDate ?? DateTime.MinValue;
        var statusChangeEnd = endDate ?? _currentTimeSupplier.GetUtcNow();

        return _windowsDefenderDataProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamilyPaginated(
            threatId,
            deviceGroupIds,
            WindowsModernDeviceFamily,
            statusChangeStart,
            statusChangeEnd,
            skip,
            take,
            sortBy,
            isDescendingOrder,
            out totalCount);
    }

    /// <inheritdoc />
    public IEnumerable<AntivirusThreatStatusDeviceDetails> GetDeviceGroupsAntivirusThreatStatus(
        IEnumerable<int> groupIds,
        DateTime lastStatusChangeStartDate,
        DateTime lastStatusChangeEndDate,
        AntivirusThreatStatus threatStatus,
        int skip,
        int take,
        bool isDescendingOrder,
        out int total)
    {
        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        ValidateGroupIds(deviceGroupIds);
        ValidateDateRange(lastStatusChangeStartDate, lastStatusChangeEndDate);
        WindowsDeviceHelperMethods.ValidateDataRetrievalOptions(skip, take);
        threatStatus.EnsureDefined();

        return _windowsDefenderDataProvider.GetAntivirusThreatStatus(
            deviceGroupIds,
            WindowsModernDeviceFamily,
            lastStatusChangeStartDate,
            lastStatusChangeEndDate,
            threatStatus,
            skip,
            take,
            isDescendingOrder,
            out total).ToThreatStatusDeviceDetailsModel();
    }

    /// <inheritdoc/>
    public IEnumerable<AntivirusGroupThreatHistoryDetails> GetAntivirusThreatHistoryDetails(IEnumerable<int> groupIds)
    {
        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        ValidateGroupIds(deviceGroupIds);
        return _windowsDefenderDataProvider.GetAntivirusGroupThreatHistory(deviceGroupIds, WindowsModernDeviceFamily).ToGroupThreatHistoryDetails();
    }

    /// <inheritdoc/>
    public IEnumerable<AntivirusLastScanDeviceSummary> GetDeviceGroupsAntivirusLastScanSummary(IEnumerable<int> groupIds, DateTime syncCompletedOn, AntivirusScanType antivirusScanType,
        AntivirusScanPeriodSubType antivirusScanPeriod, DateTime? lastScanStartDate, DateTime? lastScanEndDate)
    {
        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        ValidateGroupIds(deviceGroupIds);
        antivirusScanType.EnsureDefined();
        antivirusScanPeriod.EnsureDefined();
        ValidateDateRange(lastScanStartDate, lastScanEndDate);

        var (scanStartDate, scanEndDate) = ValidateAndCalculateLastScanTime(antivirusScanPeriod, lastScanStartDate, lastScanEndDate, syncCompletedOn);

        return antivirusScanType switch
        {
            AntivirusScanType.FullScan => _windowsDeviceProvider.GetAntivirusLastFullScanSummary(
                    deviceGroupIds,
                    scanStartDate, scanEndDate,
                    WindowsModernDeviceFamily)
                .ToLastScanSummaryModel(),
            AntivirusScanType.QuickScan => _windowsDeviceProvider.GetAntivirusLastQuickScanSummary(
                    deviceGroupIds,
                    scanStartDate,
                    scanEndDate,
                    WindowsModernDeviceFamily)
                .ToLastScanSummaryModel(),
            _ => throw new ArgumentOutOfRangeException(nameof(antivirusScanType))
        };
    }

    /// <inheritdoc />
    public IEnumerable<AntivirusThreatStatusDeviceDetails> GetAllAntivirusThreatStatus(
        IEnumerable<int> deviceGroupIds,
        DateTime lastStatusChangeStartDate,
        DateTime lastStatusChangeEndDate,
        AntivirusThreatStatus threatStatus)
    {
        var groupIds = deviceGroupIds as int[] ?? deviceGroupIds.ToArray();
        ValidateGroupIds(groupIds);
        ValidateDateRange(lastStatusChangeStartDate, lastStatusChangeEndDate);
        threatStatus.EnsureDefined();

        return _windowsDefenderDataProvider.GetDeviceDataByAntivirusThreatStatus(groupIds, WindowsModernDeviceFamily, lastStatusChangeStartDate, lastStatusChangeEndDate, threatStatus).ToThreatStatusDeviceDetailsModel();
    }

    /// <inheritdoc />
    public void DeleteWindowsDefenderData(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var devId = _deviceIdentityMapper.GetStringDeviceId(deviceId);

        TransactionHelper.RunInTransaction(() =>
        {
            _windowsDeviceService.UpdateDefenderScanInfo(deviceId, null, null, null);
            _windowsDefenderDataProvider.DeleteDeviceAntivirusThreatData(deviceId);
        });

        _programTrace.Write(TraceLevel.Verbose, "General", $"Windows Device Defender Data Cleaned Up for device : {deviceId}");
        _eventDispatcher.DispatchEvent(new DefenderDataDeletedForDeviceEvent(deviceId, devId));
    }

    /// <inheritdoc />
    public void DeleteDeviceAntivirusThreatData(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        _windowsDefenderDataProvider.DeleteDeviceAntivirusThreatData(deviceId);
        _programTrace.Write(TraceLevel.Verbose, "General", $"Windows Device Antivirus Threat Data Cleaned Up for device : {deviceId}");
    }

    private static (DateTime startTime, DateTime endTime) ValidateAndCalculateLastScanTime(AntivirusScanPeriodSubType antivirusScanPeriod, DateTime? startDate, DateTime? endDate, DateTime? completedOnDate)
    {
        return completedOnDate == null
            ? throw new ArgumentNullException(nameof(completedOnDate))
            : ((DateTime startTime, DateTime endTime))(antivirusScanPeriod switch
            {
                AntivirusScanPeriodSubType.Custom when startDate.HasValue && endDate.HasValue => (startDate, endDate),
                AntivirusScanPeriodSubType.Custom => throw new ArgumentException(nameof(antivirusScanPeriod)),
                AntivirusScanPeriodSubType.MoreThan30Days => (DateTime.MinValue, completedOnDate.Value.Date.AddDays(DaysWithin29Days).AddTicks(-1)),
                AntivirusScanPeriodSubType.Within7Days => (completedOnDate.Value.Date.AddDays(DaysWithin7Days), completedOnDate.Value.Date.AddDays(DaysWithin1Days).AddTicks(-1)),
                AntivirusScanPeriodSubType.Within24Hrs => (completedOnDate.Value.Date.AddDays(DaysWithin1Days), completedOnDate.Value.Date.AddDays(1).AddTicks(-1)),
                _ => throw new ArgumentOutOfRangeException(nameof(antivirusScanPeriod))
            });
    }

    private static void ValidateDateRange(DateTime? start, DateTime? end)
    {
        if (!start.HasValue && !end.HasValue)
        {
            return;
        }

        if (!start.HasValue)
        {
            throw new ArgumentNullException(nameof(start));
        }

        if (!end.HasValue)
        {
            throw new ArgumentNullException(nameof(end));
        }

        if (start > end)
        {
            throw new ArgumentException("Start date should not be greater than end date.");
        }
    }

    private static void ValidateGroupIds(IEnumerable<int> groupIds)
    {
        if (groupIds is null || !groupIds.Any())
        {
            throw new ArgumentNullException(nameof(groupIds));
        }
    }
};

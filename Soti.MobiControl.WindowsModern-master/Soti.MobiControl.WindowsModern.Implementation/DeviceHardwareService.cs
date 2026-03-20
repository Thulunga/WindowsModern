using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Soti.Diagnostics;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Microsoft.Extensions.Caching.Memory;
using Soti.MobiControl.WindowsModern.Implementation.Converters;
using Soti.MobiControl.WindowsModern.Implementation.Exceptions;
using Soti.MobiControl.Security.Identity;
using Soti.MobiControl.Events;
using Soti.MobiControl.WindowsModern.Implementation.Events;
using Soti.MobiControl.Search.Database;
using Soti.MobiControl.Search.Database.Enums;
using Soti.MobiControl.Search.Database.Identities;
using Soti.Time;

namespace Soti.MobiControl.WindowsModern.Implementation;

/// <summary>
/// Device Hardware Service.
/// </summary>
internal sealed class DeviceHardwareService : IDeviceHardwareService
{
    private const string LogName = "DeviceHardwareService";
    private const string LogErrorMessage = "Unable to process Windows Device Hardware key information as the input string was not in the proper format.";
    private const string LogErrorMessageSynchronizeWindowsDeviceHardwareData = "Unable to synchronize Windows Device Hardware Data with snapshot.";
    private const string CacheKeyDeviceHardware = "DeviceHardwareCache";
    private const string CacheKeyDeviceHardwareManufacturer = "DeviceHardwareManufacturerCache";
    private const string Acknowledged = "acknowledged";
    private const string Rejected = "rejected";
    private const string Added = "added";
    private const string NewHardware = "A new hardware ";
    private const string PhysicalMemory = "Win32_PhysicalMemory";
    private const string MotherBoard = "Win32_BaseBoard";
    private const string Processor = "Win32_Processor";
    private const string GraphicsCard = "Win32_VideoController";
    private const string HardDisk = "Win32_DiskDrive";
    private const string SoundCard = "Win32_SoundDevice";
    private const string LanCard = "Win32_NetworkAdapter";

    private DeviceHardwareType _deviceHardwareType;
    private readonly IProgramTrace _programTrace;
    private readonly IDeviceHardwareProvider _deviceHardwareProvider;
    private readonly IUserIdentityProvider _userIdentityProvider;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IDeviceSearchInfoService _deviceSearchInfoService;
    private readonly ICurrentTimeSupplier _currentTimeSupplier;
    private static readonly object DeviceHardwareManufacturerCacheLock = new();
    private static IMemoryCache _memoryCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceHardwareService"/> class.
    /// </summary>
    /// <param name="programTrace">The program trace.</param>
    /// <param name="deviceHardwareProvider">The windows device user provider.</param>
    /// <param name="userIdentityProvider">User Identity Provider.</param>
    /// <param name="eventDispatcher">Event Dispatcher.</param>
    /// <param name="deviceSearchInfoService">The device search info service used for continuous sync</param>
    /// <param name="currentTimeSupplier">Current UTC time supplier</param>
    /// <param name="memoryCache">Memory Cache.</param>
    public DeviceHardwareService(
        IProgramTrace programTrace,
        IDeviceHardwareProvider deviceHardwareProvider,
        IUserIdentityProvider userIdentityProvider,
        IEventDispatcher eventDispatcher,
        IDeviceSearchInfoService deviceSearchInfoService,
        ICurrentTimeSupplier currentTimeSupplier,
        IMemoryCache memoryCache)
    {
        _programTrace = programTrace ?? throw new ArgumentNullException(nameof(programTrace));
        _deviceHardwareProvider = deviceHardwareProvider ?? throw new ArgumentNullException(nameof(deviceHardwareProvider));
        _userIdentityProvider = userIdentityProvider ?? throw new ArgumentNullException(nameof(userIdentityProvider));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _deviceSearchInfoService = deviceSearchInfoService ?? throw new ArgumentNullException(nameof(deviceSearchInfoService));
        _currentTimeSupplier = currentTimeSupplier ?? throw new ArgumentNullException(nameof(currentTimeSupplier));

        var memoryCacheOptions = new MemoryCacheOptions
        {
            ExpirationScanFrequency = TimeSpan.MaxValue,
        };
        _memoryCache = memoryCache ?? new MemoryCache(memoryCacheOptions);
    }

    /// <inheritdoc />
    public void SynchronizeWindowsDeviceHardwareDataWithSnapshot(int deviceId, string devId, string windowsDeviceHardwareKeyData)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (string.IsNullOrWhiteSpace(devId))
        {
            throw new ArgumentException(nameof(devId));
        }

        if (string.IsNullOrWhiteSpace(windowsDeviceHardwareKeyData))
        {
            throw new ArgumentException(nameof(windowsDeviceHardwareKeyData));
        }

        try
        {
            var snapshotData = ParseWindowsDeviceHardwareKeySnapshot(deviceId, windowsDeviceHardwareKeyData)
                                .Where(snapshot => !string.IsNullOrWhiteSpace(snapshot.Manufacturer)
                                                    && !string.IsNullOrWhiteSpace(snapshot.Name)
                                                    && !string.IsNullOrWhiteSpace(snapshot.SerialNumber)).ToList();

            if (snapshotData.Count <= 0)
            {
                return;
            }

            var allHardwareIds = new List<int>();
            var deviceHardwareStatuses = new List<DeviceHardwareStatus>();

            foreach (var snapshot in snapshotData)
            {
                var deviceHardwareStatus = CreateDeviceHardwareStatus(snapshot, out var hardwareIds);
                allHardwareIds.AddRange(hardwareIds);
                deviceHardwareStatuses.Add(deviceHardwareStatus);
            }

            if (allHardwareIds.Count > 0)
            {
                _deviceSearchInfoService.AddOrUpdateRecords(SearchTable.Hardware, ToIntegerIdentifier(allHardwareIds));
            }

            var updatedHardwareData = _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatuses);

            _deviceSearchInfoService.AddOrUpdateRecords(SearchTable.HardwareStatus, DeviceHardwareStatusIdToIntegerIdentifier(updatedHardwareData));
            _deviceSearchInfoService.AddOrUpdateRecords(SearchTable.HardwareStatusHardware, GetIdentitiesForDeletionAndUpdate(updatedHardwareData));

            foreach (var deviceHardware in updatedHardwareData)
            {
                var hardwareStatusId = (HardwareStatus)deviceHardware.HardwareStatusId;
                if (hardwareStatusId is HardwareStatus.New or HardwareStatus.Removed)
                {
                    DispatchHardwareStatusEvent(
                        deviceId,
                        devId,
                        (DeviceHardwareType)deviceHardware.DeviceHardwareTypeId,
                        hardwareStatusId,
                        deviceHardware.DeviceHardwareSerialNumber,
                        string.Empty);
                }
            }
        }
        catch (ArgumentException ex)
        {
            LogError(deviceId, ex, LogErrorMessageSynchronizeWindowsDeviceHardwareData);
        }
    }

    /// <inheritdoc />
    public IEnumerable<DeviceHardwareSummary> GetAllDeviceHardwareStatusSummaryByDeviceId(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var deviceHardwareStatusSummary = _deviceHardwareProvider.GetAllDeviceHardwareStatusSummaryByDeviceId(deviceId);

        return deviceHardwareStatusSummary
                .Select(DeviceHardwareDataConverter.ToDeviceHardwareModel);
    }

    /// <inheritdoc />
    public void CleanUpDeviceHardwareStatus(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var deleteHardwareStatusData = _deviceHardwareProvider.DeleteDeviceHardwareStatusByDeviceId(deviceId);
        _deviceSearchInfoService.DeleteRecords(SearchTable.HardwareStatusHardware, GetIdentitiesForDeletionAndUpdate(deleteHardwareStatusData));
    }

    /// <inheritdoc />
    public void UpdateDeviceHardwareStatus(int deviceId, string devId, HardwareStatus hardwareStatus, string deviceHardwareSerialNumber)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (string.IsNullOrWhiteSpace(devId))
        {
            throw new ArgumentException(nameof(devId));
        }

        if (!Enum.IsDefined(typeof(HardwareStatus), hardwareStatus))
        {
            throw new ArgumentOutOfRangeException(nameof(hardwareStatus));
        }

        if (string.IsNullOrWhiteSpace(deviceHardwareSerialNumber))
        {
            throw new ArgumentException(nameof(deviceHardwareSerialNumber));
        }

        ValidateDeviceHardwareStatusForUpdate(hardwareStatus, deviceHardwareSerialNumber, out _deviceHardwareType);
        var deviceHardwareStatusData = _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(hardwareStatus, deviceHardwareSerialNumber);
        _deviceSearchInfoService.AddOrUpdateRecords(SearchTable.HardwareStatusHardware, GetIdentitiesForDeletionAndUpdate(deviceHardwareStatusData));

        var user = _userIdentityProvider.GetUserIdentity();
        if (user != null)
        {
            DispatchHardwareStatusEvent(deviceId, devId, _deviceHardwareType, hardwareStatus, deviceHardwareSerialNumber, user.UserName);
        }
    }

    /// <inheritdoc />
    public int CleanUpDeviceHardwareAsset()
    {
        var utcNow = _currentTimeSupplier.GetUtcNow();
        var removedAcknowledgedCutOffDate = utcNow - TimeSpan.FromHours(24);
        var removedRejectedCutoffDate = utcNow - TimeSpan.FromDays(30);
        var deletedRecords = _deviceHardwareProvider.DeviceHardwareAssetCleanUp(removedAcknowledgedCutOffDate, removedRejectedCutoffDate, out var deviceHardwareStatusData);
        _deviceSearchInfoService.DeleteRecords(SearchTable.HardwareStatusHardware, GetIdentitiesForDeletionAndUpdate(deviceHardwareStatusData));
        return deletedRecords;
    }

    /// <inheritdoc />
    [Obsolete("This is moved to Soti.MobiControl.Devices.Implementation(DeviceHardwareManufacturerService). Any further changes should be done there.")]
    public IEnumerable<DeviceHardwareManufacturer> GetDeviceHardwareManufacturersByIds(int[] deviceHardwareManufacturerIds)
    {
        var manufacturersCache = GetOrCreateManufacturersCache();
        var getCacheMissedManufacturers = () => new HashSet<int>(deviceHardwareManufacturerIds.Except(manufacturersCache.Values));

        var cacheMissedManufacturers = getCacheMissedManufacturers();
        if (cacheMissedManufacturers.Count > 0)
        {
            lock (DeviceHardwareManufacturerCacheLock)
            {
                // Double-check inside the lock to avoid race conditions
                manufacturersCache = GetOrCreateManufacturersCache();
                cacheMissedManufacturers = getCacheMissedManufacturers();

                if (cacheMissedManufacturers.Count > 0)
                {
                    var manufacturerLookup = _deviceHardwareProvider.GetAllDeviceHardwareManufacturer()
                        .ToDictionary(m => m.DeviceHardwareManufacturerId, m => m.DeviceHardwareManufacturerName);

                    foreach (var manufacturerId in cacheMissedManufacturers)
                    {
                        if (manufacturerLookup.TryGetValue(manufacturerId, out var manufacturerName))
                        {
                            manufacturersCache[manufacturerName] = manufacturerId;
                        }
                    }

                    _memoryCache.Set(CacheKeyDeviceHardwareManufacturer, manufacturersCache);
                }
            }
        }

        return manufacturersCache
                .Where(m => deviceHardwareManufacturerIds.Contains(m.Value))
                .Select(m => new DeviceHardwareManufacturer
                {
                    DeviceHardwareManufacturerId = m.Value,
                    DeviceHardwareManufacturerName = m.Key
                });
    }

    [Obsolete("This is moved to Soti.MobiControl.Devices.Implementation(DeviceHardwareManufacturerService). Any further changes should be done there.")]
    public int FetchOrCreateDeviceHardwareManufacturer(string manufacturerName)
    {
        if (string.IsNullOrWhiteSpace(manufacturerName))
        {
            throw new ArgumentOutOfRangeException(nameof(manufacturerName));
        }

        var manufacturersCache = GetOrCreateManufacturersCache();
        if (manufacturersCache.TryGetValue(manufacturerName, out var manufacturerId))
        {
            return manufacturerId;
        }

        var manufacturer = new DeviceHardwareManufacturer { DeviceHardwareManufacturerName = manufacturerName };
        _deviceHardwareProvider.InsertDeviceHardwareManufacturer(manufacturer);
        manufacturersCache[manufacturerName] = manufacturer.DeviceHardwareManufacturerId;
        _memoryCache.Set(CacheKeyDeviceHardwareManufacturer, manufacturersCache);
        return manufacturer.DeviceHardwareManufacturerId;
    }

    private static IEnumerable<Identifier> ToIntegerIdentifier(IEnumerable<int> hardwareIds)
    {
        return hardwareIds.Select(hardwareId => new IntegerIdentifier(hardwareId));
    }

    private static IEnumerable<Identifier> DeviceHardwareStatusIdToIntegerIdentifier(IReadOnlyList<DeviceHardwareData> deviceHardwareStatusData)
    {
        return deviceHardwareStatusData.Select(deviceHardwareData => new IntegerIdentifier(deviceHardwareData.DeviceHardwareStatusId));
    }

    private static IEnumerable<Identifier> GetIdentitiesForDeletionAndUpdate(IEnumerable<DeviceHardwareData> deviceHardwareStatusData)
    {
        return deviceHardwareStatusData.Select(deviceHardwareData => new LinkedIdentifier(new IntegerIdentifier(deviceHardwareData.DeviceHardwareStatusId), new IntegerIdentifier(deviceHardwareData.DeviceHardwareId)));
    }

    private void DispatchHardwareStatusEvent(int deviceId, string devId, DeviceHardwareType deviceHardwareType, HardwareStatus hardwareStatusId, string deviceHardwareSerialNumber, string userName)
    {
        var action = string.Empty;
        var syncAction = string.Empty;
        switch (hardwareStatusId)
        {
            case HardwareStatus.New:
                syncAction = NewHardware + deviceHardwareType;
                action = Added;
                break;
            case HardwareStatus.Removed:
                syncAction = deviceHardwareType.ToString();
                break;
            case HardwareStatus.Installed:
            case HardwareStatus.RemovedAcknowledged:
                action = Acknowledged;
                break;
            case HardwareStatus.InstalledRejected:
            case HardwareStatus.RemovedRejected:
                action = Rejected;
                break;
        }

        if (hardwareStatusId == HardwareStatus.New)
        {
            _eventDispatcher.DispatchEvent(new DeviceHardwareSynchronizationEvent(
                    deviceId,
                    devId,
                    syncAction,
                    deviceHardwareSerialNumber,
                    action
                ));
        }
        else if (hardwareStatusId == HardwareStatus.Removed)
        {
            _eventDispatcher.DispatchEvent(new DeviceHardwareRemovedSynchronizationEvent(
                deviceId,
                devId,
                syncAction,
                deviceHardwareSerialNumber
            ));
        }
        else
        {
            _eventDispatcher.DispatchEvent(
                new DeviceHardwareStatusUpdateEvent(
                    deviceId,
                    devId,
                    userName,
                    action,
                    deviceHardwareType.ToString(),
                    deviceHardwareSerialNumber
                )
            );
        }
    }

    private void ValidateDeviceHardwareStatusForUpdate(HardwareStatus hardwareStatus, string deviceHardwareSerialNumber, out DeviceHardwareType deviceHardwareType)
    {
        var existingDeviceHardwareDetails = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(deviceHardwareSerialNumber);

        if (existingDeviceHardwareDetails == null || existingDeviceHardwareDetails.DeviceHardwareSerialNumber == null)
        {
            throw DeviceHardwareException.DeviceHardwareNotFoundException(deviceHardwareSerialNumber);
        }

        var allowedTransitions = new Dictionary<HardwareStatus, List<HardwareStatus>>
        {
            { HardwareStatus.New, [HardwareStatus.Installed, HardwareStatus.InstalledRejected] },
            { HardwareStatus.Removed, [HardwareStatus.RemovedAcknowledged, HardwareStatus.RemovedRejected] },
        };

        var currentHardwareStatus = (HardwareStatus)existingDeviceHardwareDetails.HardwareStatusId;

        if ((allowedTransitions.TryGetValue(currentHardwareStatus, out var value) && !value.Contains(hardwareStatus)) || value == null)
        {
            throw DeviceHardwareException.DeviceHardwareStatusException(currentHardwareStatus.ToString(), hardwareStatus.ToString(), deviceHardwareSerialNumber);
        }

        deviceHardwareType = (DeviceHardwareType)existingDeviceHardwareDetails.DeviceHardwareTypeId;
    }

    private DeviceHardwareStatus CreateDeviceHardwareStatus(DeviceHardwareSnapshot deviceHardwareSnapshot, out List<int> hardwareId)
    {
        hardwareId = new List<int>();
        return deviceHardwareSnapshot == null
            ? throw new ArgumentException(nameof(deviceHardwareSnapshot))
            : new DeviceHardwareStatus
            {
                DeviceHardwareSerialNumber = deviceHardwareSnapshot.SerialNumber,
                DeviceHardwareId = FetchOrCreateDeviceHardwareId(deviceHardwareSnapshot, out hardwareId)
            };
    }

    private int FetchOrCreateDeviceHardwareId(DeviceHardwareSnapshot snapshot, out List<int> hardwareIds)
    {
        var deviceHardwareManufacturerCache = GetOrCreateManufacturersCache();
        hardwareIds = new List<int>();
        var deviceHardwareCache = _memoryCache.GetOrCreate(CacheKeyDeviceHardware, _ =>
        {
            var cache = new Dictionary<string, int>();
            var deviceHardwareList = _deviceHardwareProvider.GetAllDeviceHardware();
            foreach (var deviceHardware in deviceHardwareList)
            {
                var manufacturerName = deviceHardwareManufacturerCache.FirstOrDefault(x => x.Value == deviceHardware.DeviceHardwareManufacturer.DeviceHardwareManufacturerId).Key;
                var key = $"{deviceHardware.DeviceHardwareName}_{manufacturerName}";

                if (!cache.ContainsKey(key))
                {
                    cache[key] = deviceHardware.DeviceHardwareId;
                }
            }
            return cache;
        });

        var cacheKey = $"{snapshot.Name}_{snapshot.Manufacturer}";
        if (deviceHardwareCache.TryGetValue(cacheKey, out var deviceHardwareId))
        {
            return deviceHardwareId;
        }

        var deviceHardwareManufacturerId = FetchOrCreateDeviceHardwareManufacturerId(snapshot.Manufacturer, deviceHardwareManufacturerCache);
        deviceHardwareId = CreateDeviceHardware(snapshot, deviceHardwareManufacturerId);

        if (!deviceHardwareCache.ContainsKey(cacheKey))
        {
            deviceHardwareCache[cacheKey] = deviceHardwareId;
            _memoryCache.Set(CacheKeyDeviceHardware, deviceHardwareCache);
            hardwareIds.Add(deviceHardwareId);
        }

        return deviceHardwareId;
    }

    private Dictionary<string, int> GetOrCreateManufacturersCache()
    {
        return _memoryCache.GetOrCreate(CacheKeyDeviceHardwareManufacturer, _ =>
        {
            var cache = new Dictionary<string, int>();
            foreach (var manufacturer in _deviceHardwareProvider.GetAllDeviceHardwareManufacturer())
            {
                if (!cache.ContainsKey(manufacturer.DeviceHardwareManufacturerName))
                {
                    cache[manufacturer.DeviceHardwareManufacturerName] = manufacturer.DeviceHardwareManufacturerId;
                }
            }

            return cache;
        });
    }

    private int FetchOrCreateDeviceHardwareManufacturerId(string manufacturerName, Dictionary<string, int> deviceHardwareManufacturerCache)
    {
        if (deviceHardwareManufacturerCache.TryGetValue(manufacturerName, out var manufacturerId))
        {
            return manufacturerId;
        }

        manufacturerId = CreateDeviceHardwareManufacturer(manufacturerName);

        if (!deviceHardwareManufacturerCache.ContainsKey(manufacturerName))
        {
            deviceHardwareManufacturerCache.Add(manufacturerName, manufacturerId);
        }

        return manufacturerId;
    }

    private int CreateDeviceHardware(DeviceHardwareSnapshot snapshot, int manufacturerId)
    {
        var deviceHardware = new DeviceHardware
        {
            DeviceHardwareName = snapshot.Name,
            DeviceHardwareTypeId = (int)GetDeviceHardwareType(snapshot.CreationClassName),
            DeviceHardwareManufacturer = new DeviceHardwareManufacturer
            {
                DeviceHardwareManufacturerName = snapshot.Manufacturer,
                DeviceHardwareManufacturerId = manufacturerId
            }
        };

        _deviceHardwareProvider.InsertDeviceHardware(deviceHardware);
        return deviceHardware.DeviceHardwareId;
    }

    private int CreateDeviceHardwareManufacturer(string manufacturerName)
    {
        var manufacturer = new DeviceHardwareManufacturer { DeviceHardwareManufacturerName = manufacturerName };
        _deviceHardwareProvider.InsertDeviceHardwareManufacturer(manufacturer);
        return manufacturer.DeviceHardwareManufacturerId;
    }

    private IEnumerable<DeviceHardwareSnapshot> ParseWindowsDeviceHardwareKeySnapshot(int deviceId, string windowsDeviceHardwareKeyData)
    {
        try
        {
            return JsonSerializer.Deserialize<List<DeviceHardwareSnapshot>>(windowsDeviceHardwareKeyData) ?? Enumerable.Empty<DeviceHardwareSnapshot>();
        }
        catch (JsonException ex)
        {
            LogError(deviceId, ex, LogErrorMessage);
            return [];
        }
    }

    private static DeviceHardwareType GetDeviceHardwareType(string creationClassName)
    {
        switch (creationClassName)
        {
            case PhysicalMemory:
                return DeviceHardwareType.RAM;
            case MotherBoard:
                return DeviceHardwareType.Motherboard;
            case Processor:
                return DeviceHardwareType.Processor;
            case GraphicsCard:
                return DeviceHardwareType.GraphicsCard;
            case HardDisk:
                return DeviceHardwareType.HardDisk;
            case SoundCard:
                return DeviceHardwareType.SoundCard;
            case LanCard:
                return DeviceHardwareType.LanCard;
            default:
                throw new ArgumentOutOfRangeException(nameof(creationClassName));
        }
    }

    private void LogError(int deviceId, Exception ex, string message)
    {
        _programTrace.Write(TraceLevel.Error, LogName, $"{message} for {deviceId}.");
        _programTrace.Write(TraceLevel.Error, LogName, ex.Message);
    }
}

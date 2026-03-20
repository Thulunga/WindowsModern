using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.MobiControl.WindowsModern.Implementation.Converters;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.Diagnostics;
using Soti.MobiControl.Events;
using Soti.MobiControl.WindowsModern.Implementation.Events;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using Soti.MobiControl.Search.Database;
using Soti.MobiControl.Search.Database.Enums;
using Soti.MobiControl.Search.Database.Identities;

namespace Soti.MobiControl.WindowsModern.Implementation;

/// <summary>
/// Windows device peripheral service class.
/// </summary>
internal sealed class WindowsDevicePeripheralService : IWindowsDevicePeripheralService, IDisposable
{
    private const string Delimiter = "#";
    private const string RegexPattern = "^Name:([^,]+),Manufacturer:([^,]+),DeviceID:(.+?),Version:(.+),PnPClass:(.*?)$";
    private const string LogName = "Peripheral";
    private const string PeripheralCacheKey = "Peripherals";
    private const string PeripheralManufacturerCacheKey = "PeripheralManufacturer";
    private const string ManufacturerNameCacheKey = "ManufacturerName";
    private const string PeripheralNameAndManufacturerIdCacheKey = "PeripheralNameAndManufacturerId";

    private static readonly char[] DelimiterArray = Delimiter.ToArray();
    private readonly IProgramTrace _programTrace;
    private readonly IWindowsDevicePeripheralDataProvider _windowsDevicePeripheralProvider;
    private readonly IPeripheralDataProvider _peripheralProvider;
    private readonly IPeripheralManufacturerProvider _manufacturerProvider;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IDeviceSearchInfoService _deviceSearchInfoService;

    private MemoryCache _memoryCache;
    private readonly Regex _regexPattern;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsDevicePeripheralService"/> class.
    /// </summary>
    /// <param name="programTrace">The program trace.</param>
    /// <param name="windowsDevicePeripheralProvider">The windows device peripheral provider.</param>
    /// /// <param name="peripheralProvider">The peripheral provider</param>
    /// <param name="manufacturerProvider">The manufacturer provider.</param>
    /// <param name="deviceSearchInfoService">The device search info service used for continuous sync</param>
    /// /// <param name="eventDispatcher">The Event Dispatched Service.</param>
    public WindowsDevicePeripheralService(
        IProgramTrace programTrace,
        IWindowsDevicePeripheralDataProvider windowsDevicePeripheralProvider,
        IPeripheralDataProvider peripheralProvider,
        IPeripheralManufacturerProvider manufacturerProvider,
        IDeviceSearchInfoService deviceSearchInfoService,
        IEventDispatcher eventDispatcher)
    {
        _programTrace = programTrace ?? throw new ArgumentNullException(nameof(programTrace));
        _windowsDevicePeripheralProvider = windowsDevicePeripheralProvider ?? throw new ArgumentNullException(nameof(windowsDevicePeripheralProvider));
        _peripheralProvider = peripheralProvider ?? throw new ArgumentNullException(nameof(peripheralProvider));
        _manufacturerProvider = manufacturerProvider ?? throw new ArgumentNullException(nameof(manufacturerProvider));
        _deviceSearchInfoService = deviceSearchInfoService ?? throw new ArgumentNullException(nameof(deviceSearchInfoService));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        var memoryCacheOptions = new MemoryCacheOptions
        {
            ExpirationScanFrequency = TimeSpan.MaxValue,
        };
        _memoryCache = new MemoryCache(memoryCacheOptions);
        _regexPattern = new Regex(RegexPattern, RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
    }

    /// <inheritdoc />
    public IEnumerable<DevicePeripheralSummary> GetDevicePeripheralsSummaryInfo(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var devicePeripheralsData =
            _windowsDevicePeripheralProvider.GetDevicePeripheralSummaryByDeviceId(deviceId);

        var devicePeripherals = new List<DevicePeripheralSummary>();

        if (devicePeripheralsData == null)
        {
            return devicePeripherals;
        }

        foreach (var peripheralData in devicePeripheralsData)
        {
            var peripheral = _peripheralProvider.GetPeripheralData(peripheralData.PeripheralId);
            var manufacturerName = GetPeripheralManufacturerName(peripheral.ManufacturerId);
            devicePeripherals.Add(peripheralData.ToDevicePeripheralsModel(peripheral.Name, manufacturerName, peripheral.PeripheralTypeId));
        }

        return devicePeripherals;
    }

    /// <inheritdoc />
    public void SynchronizePeripheralDataWithSnapshot(int deviceId, string devId, string peripheralKeysData)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (string.IsNullOrWhiteSpace(devId))
        {
            throw new ArgumentException(nameof(devId));
        }

        IReadOnlyDictionary<int, (byte OldStatus, byte NewStatus)> modifiedPeripherals;
        var devicePeripheralIds = new List<int>();
        var peripheralIdsList = new List<int>();

        // When all peripheral disconnected from device which was initially connected.
        if (string.IsNullOrWhiteSpace(peripheralKeysData))
        {
            modifiedPeripherals = _windowsDevicePeripheralProvider.BulkModify(deviceId, []);
            devicePeripheralIds.AddRange(modifiedPeripherals.Keys);
            _deviceSearchInfoService.AddOrUpdateRecords(SearchTable.DevicePeripherals, GetIdentities(deviceId, devicePeripheralIds));
            EventForPeripheralConnectedOrDisconnected(modifiedPeripherals, deviceId, devId);
            return;
        }

        var snapshotData = ParsePeripheralsInfoKeys(deviceId, devId, peripheralKeysData);

        // Create record of peripherals from snapshotData &  to be filled into DB
        var devicePeripherals = snapshotData.Select(snapshot =>
        {
            var manufacturer = GetManufacturer(snapshot.PeripheralDeviceId, snapshot.Manufacturer);

            // Checking Manufacturer code of peripheral is present in Db or not.
            snapshot.Manufacturer = manufacturer.ManufacturerName;
            snapshot.ManufacturerId = manufacturer.ManufacturerId;
            var newPeripheralId = GetPeripheralId(snapshot.Name, snapshot.Category, snapshot.ManufacturerId, out var insertedPeripheralId);
            if (insertedPeripheralId > 0)
            {
                peripheralIdsList.Add(insertedPeripheralId);
            }

            return snapshot.ToDevicePeripheralData(newPeripheralId);
        });

        _deviceSearchInfoService.AddOrUpdateRecords(SearchTable.Peripherals, ToIntegerIdentifier(peripheralIdsList));

        modifiedPeripherals = _windowsDevicePeripheralProvider.BulkModify(deviceId, devicePeripherals);
        devicePeripheralIds.AddRange(modifiedPeripherals.Keys);
        _deviceSearchInfoService.AddOrUpdateRecords(SearchTable.DevicePeripherals, GetIdentities(deviceId, devicePeripheralIds));
        _programTrace.Write(TraceLevel.Verbose, LogName, $"Windows Peripheral key(s) information updated for {deviceId}");
        EventForPeripheralConnectedOrDisconnected(modifiedPeripherals, deviceId, devId);
    }

    /// <inheritdoc />
    public int CleanUpObsoleteWindowsPeripheralData()
    {
        var deletedRowCount = _windowsDevicePeripheralProvider.CleanUpObsoleteWindowsPeripheralData(out var obsoletePeripherals);
        var identitiesToDelete = GetIdentitiesForDeletion(obsoletePeripherals);
        _deviceSearchInfoService.DeleteRecords(SearchTable.DevicePeripherals, identitiesToDelete);

        return deletedRowCount;
    }

    /// <inheritdoc />
    public void CleanUpDevicePeripherals(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var deleteHardwareStatusData = _windowsDevicePeripheralProvider.DeleteDevicePeripheralByDeviceIdAndGetPeripheralData(deviceId);
        _deviceSearchInfoService.DeleteRecords(SearchTable.DevicePeripherals, GetIdentitiesForDeletion(deleteHardwareStatusData));
    }

    private static IEnumerable<Identifier> GetIdentities(int deviceId, IEnumerable<int> peripheralIds)
    {
        return peripheralIds.Select(peripheralId => new LinkedIdentifier(new IntegerIdentifier(peripheralId), new IntegerIdentifier(deviceId)));
    }

    private static IEnumerable<Identifier> GetIdentitiesForDeletion(IReadOnlyList<WindowsDevicePeripheralData> windowsDevicePeripheralData)
    {
        return windowsDevicePeripheralData.Select(peripheralData => new LinkedIdentifier(new IntegerIdentifier(peripheralData.PeripheralId), new IntegerIdentifier(peripheralData.DeviceId)));
    }

    private static IEnumerable<Identifier> ToIntegerIdentifier(IEnumerable<int> peripheralIds)
    {
        return peripheralIds.Select(peripheralId => new IntegerIdentifier(peripheralId));
    }

    private void EventForPeripheralConnectedOrDisconnected(IReadOnlyDictionary<int, (byte OldStatus, byte NewStatus)> modifiedPeripherals, int deviceId, string devId)
    {
        foreach (var entry in modifiedPeripherals)
        {
            var peripheralName = GetPeripheralName(entry.Key);
            GetExistingPeripherals().TryGetValue(entry.Key, out var peripheral);
            var manufacturer = GetPeripheralManufacturerName(peripheral!.ManufacturerId);
            if (entry.Value.NewStatus == (byte)DevicePeripheralStatus.Disconnected)
            {
                _eventDispatcher.DispatchEvent(new PeripheralDisconnectedFromDeviceEvent(deviceId, devId, peripheralName, manufacturer));
            }
            else
            {
                _eventDispatcher.DispatchEvent(new PeripheralConnectedToDeviceEvent(deviceId, devId, peripheralName, manufacturer));
            }
        }
    }

    private List<WindowsDevicePeripheralSnapShot> ParsePeripheralsInfoKeys(int deviceId, string devId, string peripheralData)
    {
        const int nameIndex = 0;
        const int manufacturerIndex = 1;
        const int peripheralDeviceIdIndex = 2;
        const int versionIndex = 3;
        const int categoryIndex = 4;
        try
        {
            var peripheralKeyArray = peripheralData.Split(DelimiterArray, StringSplitOptions.RemoveEmptyEntries);
            var peripheralInfoKeys = new List<WindowsDevicePeripheralSnapShot>(capacity: peripheralKeyArray.Length);

            foreach (var peripheralKeysString in peripheralKeyArray)
            {
                var matchResult = _regexPattern.Match(peripheralKeysString);

                if (!matchResult.Success)
                {
                    throw new FormatException();
                }

                var result = matchResult.Groups.OfType<Group>().Skip(1)
                    .Select(m => m.Value).ToArray();

                peripheralInfoKeys.Add(new WindowsDevicePeripheralSnapShot
                {
                    DeviceId = deviceId,
                    Name = result[nameIndex],
                    Manufacturer = result[manufacturerIndex],
                    PeripheralDeviceId = result[peripheralDeviceIdIndex],
                    Version = result[versionIndex],
                    Category = result[categoryIndex]
                });
            }

            return peripheralInfoKeys;
        }
        catch (FormatException ex)
        {
            _programTrace.Write(TraceLevel.Error, LogName, $"Unable to process Windows peripheral key information for {devId} as the input string was not in the proper format.");
            _programTrace.Write(TraceLevel.Error, LogName, ex.Message);
            return [];
        }
    }

    private string GetPeripheralManufacturerName(short manufacturerId)
    {
        var manufacturers = GetExistingPeripheralManufacturers();

        if (manufacturers.TryGetValue(manufacturerId, out var manufacturer))
        {
            return manufacturer?.ManufacturerName;
        }

        _memoryCache.Remove(PeripheralManufacturerCacheKey);
        _memoryCache.Remove(ManufacturerNameCacheKey);

        GetExistingPeripheralManufacturers().TryGetValue(manufacturerId, out manufacturer);
        return manufacturer?.ManufacturerName;
    }

    private PeripheralManufacturerData GetManufacturer(string manufacturerCode, string manufacturerName)
    {
        var peripheralManufacturers = GetExistingPeripheralManufacturers();
        _memoryCache.TryGetValue(ManufacturerNameCacheKey, out ConcurrentDictionary<string, PeripheralManufacturerData> manufacturers);
        if (manufacturers.TryGetValue(manufacturerCode, out var manufacturer))
        {
            return manufacturer;
        }

        var newManufacturer = new PeripheralManufacturerData
        {
            ManufacturerName = manufacturerName,
            ManufacturerCode = manufacturerCode,
        };
        _manufacturerProvider.InsertPeripheralManufacturerData(newManufacturer);
        peripheralManufacturers.TryAdd(newManufacturer.ManufacturerId, newManufacturer);
        manufacturers.TryAdd(manufacturerCode, newManufacturer);

        return newManufacturer;
    }

    private string GetPeripheralName(int peripheralId)
    {
        GetExistingPeripherals().TryGetValue(peripheralId, out var peripheral);
        return peripheral?.Name;
    }

    private int GetPeripheralId(string peripheralName, string category, short manufacturerId, out int insertedPeripheralId)
    {
        var peripherals = GetExistingPeripherals();
        insertedPeripheralId = 0;
        var peripheralType = (PeripheralType)Enum.Parse(typeof(PeripheralType), category, ignoreCase: true);
        _memoryCache.TryGetValue(PeripheralNameAndManufacturerIdCacheKey, out ConcurrentDictionary<string, PeripheralData> peripheralData);
        if (peripheralData.TryGetValue(peripheralName + manufacturerId, out var peripheral))
        {
            if (peripheral.PeripheralTypeId == 0)
            {
                _peripheralProvider.UpdatePeripheralType(peripheral.PeripheralId, (short)peripheralType);
            }
            return peripheral.PeripheralId;
        }

        var newPeripheral = new PeripheralData
        {
            ManufacturerId = manufacturerId,
            Name = peripheralName,
            PeripheralTypeId = (short)peripheralType
        };
        newPeripheral.PeripheralId = _peripheralProvider.InsertPeripheralData(newPeripheral);
        peripherals.TryAdd(newPeripheral.PeripheralId, newPeripheral);
        peripheralData.TryAdd(peripheralName + manufacturerId, newPeripheral);
        insertedPeripheralId = newPeripheral.PeripheralId;

        return newPeripheral.PeripheralId;
    }

    private ConcurrentDictionary<int, PeripheralData> GetExistingPeripherals()
    {
        if (_memoryCache.TryGetValue(PeripheralCacheKey, out ConcurrentDictionary<int, PeripheralData> peripherals))
        {
            return peripherals;
        }

        var result = _peripheralProvider.GetAllPeripheralData();

        var concurrentResult = new ConcurrentDictionary<int, PeripheralData>(result.ToDictionary(p => p.PeripheralId));
        _memoryCache.Set(PeripheralCacheKey, concurrentResult);

        var peripheralIds = new ConcurrentDictionary<string, PeripheralData>(result.ToDictionary(p => p.Name + p.ManufacturerId));
        _memoryCache.Set(PeripheralNameAndManufacturerIdCacheKey, peripheralIds);

        return concurrentResult;
    }

    private ConcurrentDictionary<short, PeripheralManufacturerData> GetExistingPeripheralManufacturers()
    {
        if (_memoryCache.TryGetValue(PeripheralManufacturerCacheKey, out ConcurrentDictionary<short, PeripheralManufacturerData> peripheralManufacturers))
        {
            return peripheralManufacturers;
        }

        var result = _manufacturerProvider.GetAllPeripheralManufacturerData();

        var peripheralManufacturersResult = new ConcurrentDictionary<short, PeripheralManufacturerData>(result.ToDictionary(p => p.ManufacturerId));
        _memoryCache.Set(PeripheralManufacturerCacheKey, peripheralManufacturersResult);

        var manufacturerIds = new ConcurrentDictionary<string, PeripheralManufacturerData>(result.ToDictionary(p => p.ManufacturerCode));
        _memoryCache.Set(ManufacturerNameCacheKey, manufacturerIds);

        return peripheralManufacturersResult;
    }

    public void Dispose()
    {
        _memoryCache?.Dispose();
        _memoryCache = null;
    }
}

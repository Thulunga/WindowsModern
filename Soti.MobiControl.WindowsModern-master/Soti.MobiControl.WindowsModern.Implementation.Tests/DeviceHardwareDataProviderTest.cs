using NUnit.Framework;
using Soti.MobiControl.Test.Common.Database;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;
using System;
using Soti.MobiControl.WindowsModern.Models.Enums;
using System.Collections.Generic;
using System.Linq;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
[IntegrationTest]
internal sealed class DeviceHardwareDataProviderTest : ProviderTestsBase
{
    private const int DeviceId = 1;
    private const string DeviceHardwareManufacturerName = "Microsoft";
    private const string DeviceHardwareName = "Physical RAM";
    private const string DeviceHardwareSerialNumber1 = "M0001";
    private const string DeviceHardwareSerialNumber2 = "M0002";
    private const string DeviceHardwareSerialNumber3 = "M0003";
    private const string DeviceHardwareSerialNumber4 = "M0004";
    private const string DeviceHardwareSerialNumber5 = "M0005";
    private const string DeviceHardwareSerialNumber6 = "M0006";

    private readonly string _devId = Guid.NewGuid().ToString();
    private readonly string _devId2 = Guid.NewGuid().ToString();
    private static readonly TestDeviceProvider TestDeviceProvider = new();
    private DeviceHardwareDataProvider _deviceHardwareProvider;

    [SetUp]
    public void Setup()
    {
        _deviceHardwareProvider = new DeviceHardwareDataProvider(Database);
    }

    [Test]
    public void Constructor_Test()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _ = new DeviceHardwareDataProvider(null));
    }

    [Test]
    public void DeviceHardwareManufacturerInsert_Validation_Test()
    {
        Assert.Throws<ArgumentNullException>(() => _deviceHardwareProvider.InsertDeviceHardwareManufacturer(null));
        Assert.Throws<ArgumentNullException>(() => _deviceHardwareProvider.InsertDeviceHardware(null));
    }

    [Test]
    public void DeviceHardwareManufacturerInsert_Test()
    {
        var deviceManufacturer = new DeviceHardwareManufacturer
        {
            DeviceHardwareManufacturerName = DeviceHardwareManufacturerName
        };

        _deviceHardwareProvider.InsertDeviceHardwareManufacturer(deviceManufacturer);
        Assert.NotNull(deviceManufacturer.DeviceHardwareManufacturerId);
        var deletedRecord = _deviceHardwareProvider.DeviceHardwareAssetCleanUp(DateTime.UtcNow, DateTime.UtcNow, out var deviceHardwareStatusData);
        Assert.IsNotNull(deletedRecord);
        var deviceHardwareManufacturers = _deviceHardwareProvider.GetAllDeviceHardwareManufacturer();
        Assert.IsNotNull(deviceHardwareManufacturers);
    }

    [Test]
    public void DeviceHardwareInsert_Test()
    {
        var deviceManufacturer = new DeviceHardwareManufacturer
        {
            DeviceHardwareManufacturerName = DeviceHardwareManufacturerName
        };

        _deviceHardwareProvider.InsertDeviceHardwareManufacturer(deviceManufacturer);
        Assert.NotNull(deviceManufacturer.DeviceHardwareManufacturerId);

        var deviceHardware = new DeviceHardware
        {
            DeviceHardwareName = DeviceHardwareName,
            DeviceHardwareTypeId = (int)DeviceHardwareType.RAM,
            DeviceHardwareManufacturer = new DeviceHardwareManufacturer
            {
                DeviceHardwareManufacturerName = DeviceHardwareManufacturerName,
                DeviceHardwareManufacturerId = deviceManufacturer.DeviceHardwareManufacturerId
            }
        };
        _deviceHardwareProvider.InsertDeviceHardware(deviceHardware);
        Assert.IsNotNull(deviceHardware.DeviceHardwareId);
        var deletedRecord = _deviceHardwareProvider.DeviceHardwareAssetCleanUp(DateTime.UtcNow, DateTime.UtcNow, out var deviceStatusData);
        Assert.IsNotNull(deletedRecord);
        var deviceHardwareHardwareList = _deviceHardwareProvider.GetAllDeviceHardware();
        Assert.AreEqual(0, deviceHardwareHardwareList.Count());
        var deviceHardwareManufacturers = _deviceHardwareProvider.GetAllDeviceHardwareManufacturer();
        Assert.IsNotNull(deviceHardwareManufacturers);
    }

    [Test]
    public void GetAllDeviceHardware_Test()
    {
        var deviceManufacturer = new DeviceHardwareManufacturer
        {
            DeviceHardwareManufacturerName = DeviceHardwareManufacturerName
        };

        _deviceHardwareProvider.InsertDeviceHardwareManufacturer(deviceManufacturer);
        Assert.NotNull(deviceManufacturer.DeviceHardwareManufacturerId);

        var deviceHardware = new DeviceHardware
        {
            DeviceHardwareName = DeviceHardwareName,
            DeviceHardwareTypeId = (int)DeviceHardwareType.RAM,
            DeviceHardwareManufacturer = new DeviceHardwareManufacturer
            {
                DeviceHardwareManufacturerName = DeviceHardwareManufacturerName,
                DeviceHardwareManufacturerId = deviceManufacturer.DeviceHardwareManufacturerId
            }
        };
        _deviceHardwareProvider.InsertDeviceHardware(deviceHardware);
        Assert.IsNotNull(deviceHardware.DeviceHardwareId);
        var deviceHardwareHardwareList = _deviceHardwareProvider.GetAllDeviceHardware();
        Assert.IsNotNull(deviceHardwareHardwareList);
        var deletedRecord = _deviceHardwareProvider.DeviceHardwareAssetCleanUp(DateTime.UtcNow, DateTime.UtcNow, out var deviceStatusData);
        Assert.IsNotNull(deletedRecord);
        var deviceHardwareHardwareList1 = _deviceHardwareProvider.GetAllDeviceHardware();
        Assert.AreEqual(0, deviceHardwareHardwareList1.Count());
        var deviceHardwareManufacturers = _deviceHardwareProvider.GetAllDeviceHardwareManufacturer();
        Assert.IsNotNull(deviceHardwareManufacturers);
    }

    [Test]
    public void GetAllDeviceHardwareManufacturer_Test()
    {
        var deviceManufacturer = new DeviceHardwareManufacturer
        {
            DeviceHardwareManufacturerName = DeviceHardwareManufacturerName
        };

        _deviceHardwareProvider.InsertDeviceHardwareManufacturer(deviceManufacturer);
        Assert.NotNull(deviceManufacturer.DeviceHardwareManufacturerId);
        var manufacturers = _deviceHardwareProvider.GetAllDeviceHardwareManufacturer();
        Assert.IsNotNull(manufacturers);
        var deletedRecord = _deviceHardwareProvider.DeviceHardwareAssetCleanUp(DateTime.UtcNow, DateTime.UtcNow, out var deviceStatusData);
        Assert.IsNotNull(deletedRecord);
        var deviceHardwareHardwareList = _deviceHardwareProvider.GetAllDeviceHardware();
        Assert.AreEqual(0, deviceHardwareHardwareList.Count());
        var deviceHardwareManufacturers = _deviceHardwareProvider.GetAllDeviceHardwareManufacturer();
        Assert.IsNotNull(deviceHardwareManufacturers);
    }

    [Test]
    public void GetAllDeviceHardwareStatusSummaryByDeviceId_ThrowsArgumentNullException_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _deviceHardwareProvider.GetAllDeviceHardwareStatusSummaryByDeviceId(-1));
    }

    [Test]
    public void DeviceHardwareStatus_RemovedAcknowledged_CompleteFlow_Test()
    {
        var deviceId = GetDeviceId(_devId);
        var deviceManufacturer = new DeviceHardwareManufacturer
        {
            DeviceHardwareManufacturerName = DeviceHardwareManufacturerName
        };

        _deviceHardwareProvider.InsertDeviceHardwareManufacturer(deviceManufacturer);
        Assert.NotNull(deviceManufacturer.DeviceHardwareManufacturerId);

        var deviceHardware = new DeviceHardware
        {
            DeviceHardwareName = DeviceHardwareName,
            DeviceHardwareTypeId = (int)DeviceHardwareType.RAM,
            DeviceHardwareManufacturer = new DeviceHardwareManufacturer
            {
                DeviceHardwareManufacturerName = DeviceHardwareManufacturerName,
                DeviceHardwareManufacturerId = deviceManufacturer.DeviceHardwareManufacturerId
            }
        };
        _deviceHardwareProvider.InsertDeviceHardware(deviceHardware);
        Assert.IsNotNull(deviceHardware.DeviceHardwareId);

        var deviceHardwareStatus = new List<DeviceHardwareStatus>
        {
            new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber1 },
            new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber2 }
        };

        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus);
        var deviceHardwareHardwareList = _deviceHardwareProvider.GetAllDeviceHardwareStatusSummaryByDeviceId(deviceId);
        Assert.IsNotNull(deviceHardwareHardwareList);
        IEnumerable<DeviceHardwareData> deviceStatusData = new List<DeviceHardwareData>();
        foreach (var hardware in deviceHardwareHardwareList)
        {
            _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.RemovedAcknowledged, hardware.DeviceHardwareSerialNumber);
        }

        var updatedDeviceHardwareList = _deviceHardwareProvider.GetAllDeviceHardwareStatusSummaryByDeviceId(deviceId).ToList();
        Assert.IsNotNull(deviceHardwareHardwareList);

        var lastModifiedDate = updatedDeviceHardwareList.SingleOrDefault(x => x.DeviceHardwareSerialNumber == DeviceHardwareSerialNumber1)!.LastModifiedDate;
        var removalAcknowledgedDate = lastModifiedDate + TimeSpan.FromHours(25);
        var removalRejected = lastModifiedDate + TimeSpan.FromHours(25);
        var deletedRecords = _deviceHardwareProvider.DeviceHardwareAssetCleanUp(removalAcknowledgedDate, removalRejected, out var deviceStatusData1);
        Assert.AreNotEqual(0, deletedRecords);
        _deviceHardwareProvider.DeleteDeviceHardwareStatusByDeviceId(deviceId);
        _deviceHardwareProvider.DeviceHardwareAssetCleanUp(DateTime.UtcNow, DateTime.UtcNow, out var deviceHardwareStatusData);
        var deviceHardwareListData = _deviceHardwareProvider.GetAllDeviceHardware();
        Assert.AreEqual(0, deviceHardwareListData.Count());
        var deviceHardwareManufacturers = _deviceHardwareProvider.GetAllDeviceHardwareManufacturer();
        Assert.IsNotNull(deviceHardwareManufacturers);
        DeleteTestData(deviceId);
    }

    [Test]
    public void DeleteDeviceHardwareStatus_Test()
    {
        var deviceId = GetDeviceId(_devId);
        var deviceManufacturer = new DeviceHardwareManufacturer
        {
            DeviceHardwareManufacturerName = DeviceHardwareManufacturerName
        };

        _deviceHardwareProvider.InsertDeviceHardwareManufacturer(deviceManufacturer);
        Assert.NotNull(deviceManufacturer.DeviceHardwareManufacturerId);

        var deviceHardware = new DeviceHardware
        {
            DeviceHardwareName = DeviceHardwareName,
            DeviceHardwareTypeId = (int)DeviceHardwareType.RAM,
            DeviceHardwareManufacturer = new DeviceHardwareManufacturer
            {
                DeviceHardwareManufacturerName = DeviceHardwareManufacturerName,
                DeviceHardwareManufacturerId = deviceManufacturer.DeviceHardwareManufacturerId
            }
        };
        _deviceHardwareProvider.InsertDeviceHardware(deviceHardware);
        Assert.IsNotNull(deviceHardware.DeviceHardwareId);

        var deviceHardwareStatus = new List<DeviceHardwareStatus>
        {
            new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber1 },
            new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber2 }
        };

        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus);
        var deviceHardwareHardwareList = _deviceHardwareProvider.GetAllDeviceHardwareStatusSummaryByDeviceId(deviceId);
        Assert.IsNotNull(deviceHardwareHardwareList);

        _deviceHardwareProvider.DeleteDeviceHardwareStatusByDeviceId(deviceId);
        var hardwareStatus = _deviceHardwareProvider.GetAllDeviceHardwareStatusSummaryByDeviceId(deviceId);
        Assert.AreEqual(0, hardwareStatus.Count());

        var deletedRecords = _deviceHardwareProvider.DeviceHardwareAssetCleanUp(DateTime.UtcNow, DateTime.UtcNow, out var deviceStatusData);
        Assert.AreNotEqual(0, deletedRecords);
        var deviceHardwareListData = _deviceHardwareProvider.GetAllDeviceHardware();
        Assert.AreEqual(0, deviceHardwareListData.Count());
        var deviceHardwareManufacturers = _deviceHardwareProvider.GetAllDeviceHardwareManufacturer();
        Assert.IsNotNull(deviceHardwareManufacturers);
        DeleteTestData(deviceId);
    }

    [Test]
    public void GetDeviceHardwareStatusSummaryByDeviceHardwareSerialNumber_ThrowsArgumentNullException_Test()
    {
        Assert.Throws<ArgumentException>(() =>
            _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(null));
    }

    [Test]
    public void GetDeviceHardwareStatusSummaryByDeviceHardwareSerialNumber_Test()
    {
        var deviceId = GetDeviceId(_devId);
        var deviceManufacturer = new DeviceHardwareManufacturer
        {
            DeviceHardwareManufacturerName = DeviceHardwareManufacturerName
        };

        _deviceHardwareProvider.InsertDeviceHardwareManufacturer(deviceManufacturer);
        Assert.NotNull(deviceManufacturer.DeviceHardwareManufacturerId);

        var deviceHardware = new DeviceHardware
        {
            DeviceHardwareName = DeviceHardwareName,
            DeviceHardwareTypeId = (int)DeviceHardwareType.RAM,
            DeviceHardwareManufacturer = new DeviceHardwareManufacturer
            {
                DeviceHardwareManufacturerName = DeviceHardwareManufacturerName,
                DeviceHardwareManufacturerId = deviceManufacturer.DeviceHardwareManufacturerId
            }
        };
        _deviceHardwareProvider.InsertDeviceHardware(deviceHardware);
        Assert.IsNotNull(deviceHardware.DeviceHardwareId);

        var deviceHardwareStatus = new List<DeviceHardwareStatus>
        {
            new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber1 },
        };

        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus);

        var deviceHardwareSerialNumber1Summary = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber1Summary);
        Assert.AreEqual(deviceHardwareSerialNumber1Summary.DeviceHardwareName, deviceHardware.DeviceHardwareName);
        Assert.AreEqual(deviceHardwareSerialNumber1Summary.DeviceHardwareManufacturerName, deviceHardware.DeviceHardwareManufacturer.DeviceHardwareManufacturerName);
        Assert.AreEqual(deviceHardwareSerialNumber1Summary.DeviceHardwareTypeId, deviceHardware.DeviceHardwareTypeId);

        _deviceHardwareProvider.DeleteDeviceHardwareStatusByDeviceId(deviceId);
        var deletedRecord = _deviceHardwareProvider.DeviceHardwareAssetCleanUp(DateTime.UtcNow, DateTime.UtcNow, out var deviceStatusData);
        Assert.IsNotNull(deletedRecord);
        var deviceHardwareHardwareList = _deviceHardwareProvider.GetAllDeviceHardware();
        Assert.AreEqual(0, deviceHardwareHardwareList.Count());
        var deviceHardwareManufacturers = _deviceHardwareProvider.GetAllDeviceHardwareManufacturer();
        Assert.IsNotNull(deviceHardwareManufacturers);
        DeleteTestData(deviceId);
    }

    [Test]
    public void BulkModify_Test()
    {
        var deviceId = GetDeviceId(_devId);
        var deviceManufacturer = new DeviceHardwareManufacturer
        {
            DeviceHardwareManufacturerName = DeviceHardwareManufacturerName
        };

        _deviceHardwareProvider.InsertDeviceHardwareManufacturer(deviceManufacturer);
        Assert.NotNull(deviceManufacturer.DeviceHardwareManufacturerId);

        var deviceHardware = new DeviceHardware
        {
            DeviceHardwareName = DeviceHardwareName,
            DeviceHardwareTypeId = (int)DeviceHardwareType.RAM,
            DeviceHardwareManufacturer = new DeviceHardwareManufacturer
            {
                DeviceHardwareManufacturerName = DeviceHardwareManufacturerName,
                DeviceHardwareManufacturerId = deviceManufacturer.DeviceHardwareManufacturerId
            }
        };
        _deviceHardwareProvider.InsertDeviceHardware(deviceHardware);
        Assert.IsNotNull(deviceHardware.DeviceHardwareId);

        var deviceHardwareStatus = new List<DeviceHardwareStatus>
        {
            new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber1 },
            new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber2 },
        };

        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus);
        var deviceHardwareHardwareList = _deviceHardwareProvider.GetAllDeviceHardwareStatusSummaryByDeviceId(deviceId);
        Assert.IsNotNull(deviceHardwareHardwareList);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus);
        var deviceHardwareListData = _deviceHardwareProvider.GetAllDeviceHardwareStatusSummaryByDeviceId(deviceId);
        Assert.IsNotNull(deviceHardwareListData);
        foreach (var hardware in deviceHardwareListData)
        {
            Assert.AreEqual(hardware.HardwareStatusId, (int)HardwareStatus.Installed);
        }

        deviceHardwareStatus.Add(new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber3 });
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus);
        var deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.New);

        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.Installed, DeviceHardwareSerialNumber3);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.Installed);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.Installed);

        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.InstalledRejected, DeviceHardwareSerialNumber3);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.InstalledRejected);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.InstalledRejected);

        deviceHardwareStatus.RemoveAll(d => d.DeviceHardwareSerialNumber == DeviceHardwareSerialNumber3);
        Assert.AreEqual(2, deviceHardwareStatus.Count);

        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.Removed);

        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.Removed);

        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.RemovedAcknowledged, DeviceHardwareSerialNumber3);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.RemovedAcknowledged);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.RemovedAcknowledged);

        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.RemovedRejected, DeviceHardwareSerialNumber3);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.RemovedRejected);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.RemovedRejected);

        deviceHardwareStatus.Add(new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber3 });
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.New);

        _deviceHardwareProvider.DeleteDeviceHardwareStatusByDeviceId(deviceId);
        var deletedRecord = _deviceHardwareProvider.DeviceHardwareAssetCleanUp(DateTime.UtcNow, DateTime.UtcNow, out var deviceHardwareStatusData2);
        Assert.IsNotNull(deletedRecord);
        var deviceHardwareHardwareList1 = _deviceHardwareProvider.GetAllDeviceHardware();
        Assert.AreEqual(0, deviceHardwareHardwareList1.Count());
        var deviceHardwareManufacturers = _deviceHardwareProvider.GetAllDeviceHardwareManufacturer();
        Assert.IsNotNull(deviceHardwareManufacturers);
        DeleteTestData(deviceId);
    }

    [Test]
    public void BulkModifyMultipleDevice_Test()
    {
        var deviceId = GetDeviceId(_devId);
        var deviceId2 = GetDeviceId(_devId2);

        var deviceManufacturer = new DeviceHardwareManufacturer
        {
            DeviceHardwareManufacturerName = DeviceHardwareManufacturerName
        };

        _deviceHardwareProvider.InsertDeviceHardwareManufacturer(deviceManufacturer);
        Assert.NotNull(deviceManufacturer.DeviceHardwareManufacturerId);

        var deviceHardware = new DeviceHardware
        {
            DeviceHardwareName = DeviceHardwareName,
            DeviceHardwareTypeId = (int)DeviceHardwareType.RAM,
            DeviceHardwareManufacturer = new DeviceHardwareManufacturer
            {
                DeviceHardwareManufacturerName = DeviceHardwareManufacturerName,
                DeviceHardwareManufacturerId = deviceManufacturer.DeviceHardwareManufacturerId
            }
        };
        _deviceHardwareProvider.InsertDeviceHardware(deviceHardware);
        Assert.IsNotNull(deviceHardware.DeviceHardwareId);

        var deviceHardwareStatus1 = new List<DeviceHardwareStatus>
        {
            new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber1 },
            new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber2 },
        };

        var deviceHardwareStatus2 = new List<DeviceHardwareStatus>
        {
            new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber3 },
            new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber4 },
        };

        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus1);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId2, deviceHardwareStatus2);

        var updatedDeviceHardwareListDeviceId1 = _deviceHardwareProvider.GetAllDeviceHardwareStatusSummaryByDeviceId(deviceId);
        Assert.IsNotNull(updatedDeviceHardwareListDeviceId1);

        var deviceHardwareListDataDeviceId2 = _deviceHardwareProvider.GetAllDeviceHardwareStatusSummaryByDeviceId(deviceId2);
        Assert.IsNotNull(deviceHardwareListDataDeviceId2);

        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus1);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId2, deviceHardwareStatus2);

        var deviceHardwareDeviceId1InstalledHardwareData = _deviceHardwareProvider.GetAllDeviceHardwareStatusSummaryByDeviceId(deviceId);
        Assert.IsNotNull(deviceHardwareDeviceId1InstalledHardwareData);
        foreach (var hardware in deviceHardwareDeviceId1InstalledHardwareData)
        {
            Assert.AreEqual(hardware.HardwareStatusId, (int)HardwareStatus.Installed);
        }

        var deviceHardwareDeviceId2InstalledHardwareData = _deviceHardwareProvider.GetAllDeviceHardwareStatusSummaryByDeviceId(deviceId2);
        Assert.IsNotNull(deviceHardwareDeviceId2InstalledHardwareData);
        foreach (var hardware in deviceHardwareDeviceId2InstalledHardwareData)
        {
            Assert.AreEqual(hardware.HardwareStatusId, (int)HardwareStatus.Installed);
        }

        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.InstalledRejected, DeviceHardwareSerialNumber1);
        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.InstalledRejected, DeviceHardwareSerialNumber3);

        var deviceHardwareSerialNumber1 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        var deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber1.HardwareStatusId, (int)HardwareStatus.InstalledRejected);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.InstalledRejected);

        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus1);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId2, deviceHardwareStatus2);
        deviceHardwareSerialNumber1 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber1.HardwareStatusId, (int)HardwareStatus.InstalledRejected);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.InstalledRejected);

        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.New, DeviceHardwareSerialNumber1);
        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.New, DeviceHardwareSerialNumber3);

        deviceHardwareSerialNumber1 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber1.HardwareStatusId, (int)HardwareStatus.New);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.New);

        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus1);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId2, deviceHardwareStatus2);
        deviceHardwareSerialNumber1 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber1.HardwareStatusId, (int)HardwareStatus.New);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.New);

        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.Removed, DeviceHardwareSerialNumber1);
        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.Removed, DeviceHardwareSerialNumber3);

        deviceHardwareSerialNumber1 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber1.HardwareStatusId, (int)HardwareStatus.Removed);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.Removed);

        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus1);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId2, deviceHardwareStatus2);
        deviceHardwareSerialNumber1 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber1.HardwareStatusId, (int)HardwareStatus.New);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.New);

        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.RemovedRejected, DeviceHardwareSerialNumber1);
        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.RemovedRejected, DeviceHardwareSerialNumber3);

        deviceHardwareSerialNumber1 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber1.HardwareStatusId, (int)HardwareStatus.RemovedRejected);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.RemovedRejected);

        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus1);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId2, deviceHardwareStatus2);
        deviceHardwareSerialNumber1 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber1.HardwareStatusId, (int)HardwareStatus.New);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.New);

        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.RemovedAcknowledged, DeviceHardwareSerialNumber1);
        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.RemovedAcknowledged, DeviceHardwareSerialNumber3);

        deviceHardwareSerialNumber1 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber1.HardwareStatusId, (int)HardwareStatus.RemovedAcknowledged);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.RemovedAcknowledged);

        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus1);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId2, deviceHardwareStatus2);
        deviceHardwareSerialNumber1 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber1.HardwareStatusId, (int)HardwareStatus.New);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.New);

        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.Installed, DeviceHardwareSerialNumber1);
        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.Installed, DeviceHardwareSerialNumber3);

        deviceHardwareSerialNumber1 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber1.HardwareStatusId, (int)HardwareStatus.Installed);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.Installed);

        deviceHardwareStatus1.RemoveAll(d => d.DeviceHardwareSerialNumber == DeviceHardwareSerialNumber1);
        deviceHardwareStatus2.RemoveAll(d => d.DeviceHardwareSerialNumber == DeviceHardwareSerialNumber3);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus1);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId2, deviceHardwareStatus2);
        deviceHardwareSerialNumber1 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber1.HardwareStatusId, (int)HardwareStatus.Removed);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.Removed);

        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.InstalledRejected, DeviceHardwareSerialNumber1);
        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.InstalledRejected, DeviceHardwareSerialNumber3);

        deviceHardwareSerialNumber1 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber1.HardwareStatusId, (int)HardwareStatus.InstalledRejected);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.InstalledRejected);

        deviceHardwareStatus1.RemoveAll(d => d.DeviceHardwareSerialNumber == DeviceHardwareSerialNumber1);
        deviceHardwareStatus2.RemoveAll(d => d.DeviceHardwareSerialNumber == DeviceHardwareSerialNumber3);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus1);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId2, deviceHardwareStatus2);
        deviceHardwareSerialNumber1 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber1.HardwareStatusId, (int)HardwareStatus.Removed);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.Removed);

        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.Removed, DeviceHardwareSerialNumber1);
        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.RemovedAcknowledged, DeviceHardwareSerialNumber3);

        deviceHardwareSerialNumber1 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber1.HardwareStatusId, (int)HardwareStatus.Removed);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.RemovedAcknowledged);

        deviceHardwareStatus1.RemoveAll(d => d.DeviceHardwareSerialNumber == DeviceHardwareSerialNumber1);
        deviceHardwareStatus2.RemoveAll(d => d.DeviceHardwareSerialNumber == DeviceHardwareSerialNumber3);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus1);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId2, deviceHardwareStatus2);
        deviceHardwareSerialNumber1 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber1.HardwareStatusId, (int)HardwareStatus.Removed);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.RemovedAcknowledged);

        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.Removed, DeviceHardwareSerialNumber1);
        _deviceHardwareProvider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus.Installed, DeviceHardwareSerialNumber3);

        deviceHardwareSerialNumber1 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.AreEqual(deviceHardwareSerialNumber1.HardwareStatusId, (int)HardwareStatus.Removed);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.Installed);

        deviceHardwareStatus1.RemoveAll(d => d.DeviceHardwareSerialNumber == DeviceHardwareSerialNumber1);
        deviceHardwareStatus1.Add(new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber5 });
        deviceHardwareStatus2.Add(new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber6 });

        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus1);
        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId2, deviceHardwareStatus2);
        deviceHardwareSerialNumber1 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber1);
        deviceHardwareSerialNumber3 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber3);
        var deviceHardwareSerialNumber5 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber5);
        var deviceHardwareSerialNumber6 = _deviceHardwareProvider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber6);
        Assert.IsNotNull(deviceHardwareSerialNumber1);
        Assert.IsNotNull(deviceHardwareSerialNumber3);
        Assert.IsNotNull(deviceHardwareSerialNumber5);
        Assert.IsNotNull(deviceHardwareSerialNumber6);
        Assert.AreEqual(deviceHardwareSerialNumber1.HardwareStatusId, (int)HardwareStatus.Removed);
        Assert.AreEqual(deviceHardwareSerialNumber3.HardwareStatusId, (int)HardwareStatus.Removed);
        Assert.AreEqual(deviceHardwareSerialNumber5.HardwareStatusId, (int)HardwareStatus.New);
        Assert.AreEqual(deviceHardwareSerialNumber6.HardwareStatusId, (int)HardwareStatus.New);

        _deviceHardwareProvider.DeleteDeviceHardwareStatusByDeviceId(deviceId);
        _deviceHardwareProvider.DeleteDeviceHardwareStatusByDeviceId(deviceId2);

        var deletedRecord = _deviceHardwareProvider.DeviceHardwareAssetCleanUp(DateTime.UtcNow, DateTime.UtcNow, out var deviceHarStatusData);
        Assert.IsNotNull(deletedRecord);

        var deviceHardwareList = _deviceHardwareProvider.GetAllDeviceHardware();
        Assert.AreEqual(0, deviceHardwareList.Count());

        var deviceHardwareManufacturers = _deviceHardwareProvider.GetAllDeviceHardwareManufacturer();
        Assert.IsNotNull(deviceHardwareManufacturers);

        DeleteTestData(deviceId);
        DeleteTestData(deviceId2);
    }

    [Test]
    public void BulkModifyArgumentOutOfRangeException_Validation_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(-1, null));
        Assert.Throws<ArgumentNullException>(() => _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(DeviceId, null));
    }

    [Test]
    public void BulkModifyMultipleHardware_Test()
    {
        var deviceId = GetDeviceId(_devId);

        var deviceManufacturer = new DeviceHardwareManufacturer
        {
            DeviceHardwareManufacturerName = DeviceHardwareManufacturerName
        };

        _deviceHardwareProvider.InsertDeviceHardwareManufacturer(deviceManufacturer);
        Assert.NotNull(deviceManufacturer.DeviceHardwareManufacturerId);

        var deviceHardware = new DeviceHardware
        {
            DeviceHardwareName = DeviceHardwareName,
            DeviceHardwareTypeId = (int)DeviceHardwareType.RAM,
            DeviceHardwareManufacturer = new DeviceHardwareManufacturer
            {
                DeviceHardwareManufacturerName = DeviceHardwareManufacturerName,
                DeviceHardwareManufacturerId = deviceManufacturer.DeviceHardwareManufacturerId
            }
        };
        _deviceHardwareProvider.InsertDeviceHardware(deviceHardware);
        Assert.IsNotNull(deviceHardware.DeviceHardwareId);

        var deviceHardwareStatus1 = new List<DeviceHardwareStatus>
        {
            new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber1 },
            new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber2 },
        };

        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus1);
        deviceHardwareStatus1 =
        [
            new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber3 },
            new() { DeviceHardwareId = deviceHardware.DeviceHardwareId, DeviceHardwareSerialNumber = DeviceHardwareSerialNumber4 }
        ];

        _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatus1);
        var deviceHardwareListDeviceId1 = _deviceHardwareProvider.GetAllDeviceHardwareStatusSummaryByDeviceId(deviceId);
        Assert.IsNotNull(deviceHardwareListDeviceId1);

        _deviceHardwareProvider.DeleteDeviceHardwareStatusByDeviceId(deviceId);

        var deletedRecord = _deviceHardwareProvider.DeviceHardwareAssetCleanUp(DateTime.UtcNow, DateTime.UtcNow, out var deviceStatusData);
        Assert.IsNotNull(deletedRecord);

        var deviceHardwareList = _deviceHardwareProvider.GetAllDeviceHardware();
        Assert.AreEqual(0, deviceHardwareList.Count());

        var deviceHardwareManufacturers = _deviceHardwareProvider.GetAllDeviceHardwareManufacturer();
        Assert.IsNotNull(deviceHardwareManufacturers);

        DeleteTestData(deviceId);
    }

    private static int GetDeviceId(string devId)
    {
        return TestDeviceProvider.EnsureDeviceRecord(devId, 1004);
    }

    private void DeleteTestData(int deviceId)
    {
        if (deviceId <= 0)
        {
            return;
        }

        TestDeviceProvider.DeleteDevice(deviceId);
    }
}

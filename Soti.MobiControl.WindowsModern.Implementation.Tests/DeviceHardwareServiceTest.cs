using Moq;
using NUnit.Framework;
using Soti.Diagnostics;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.MobiControl.WindowsModern.Implementation.Exceptions;
using Soti.MobiControl.Events;
using Soti.MobiControl.Security.Identity;
using Soti.MobiControl.WindowsModern.Implementation.Events;
using Soti.MobiControl.WindowsModern.Models;
using Microsoft.Extensions.Caching.Memory;
using Soti.MobiControl.Security.Identity.Model;
using Soti.Time;
using Soti.Utilities.Collections;
using Soti.MobiControl.Search.Database;
using Soti.MobiControl.Search.Database.Enums;
using Soti.MobiControl.Search.Database.Identities;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
internal sealed class DeviceHardwareServiceTest
{
    private const string WindowsDeviceHardwareSnapshotKeyData =
        "[{\"CreationClassName\":\"Win32_PhysicalMemory\",\"Name\":\"Physical Memory\",\"Manufacturer\":\"Microsoft Corporation\",\"SerialNumber\":\"DEV11-19217_M0001\"},{\"CreationClassName\":\"Win32_BaseBoard\",\"Name\":\"Base Board\",\"Manufacturer\":\"Microsoft Corporation\",\"SerialNumber\":\"3890-0186-7969-3512-9510-6950-26\"},{\"CreationClassName\":\"Win32_Processor\",\"Name\":\"Intel(R) Xeon(R) Silver 4210 CPU @ 2.20GHz\",\"Manufacturer\":\"GenuineIntel\",\"SerialNumber\":\"DEV11-19217_0000000000000000\"},{\"CreationClassName\":\"Win32_VideoController\",\"Name\":\"Microsoft Hyper-V Video\",\"Manufacturer\":\"Microsoft\",\"SerialNumber\":\"DEV11-19217_VMBUS#{DA0A7802-E377-4AAC-8E77-0558EB1073F8}#{5620E0C7-8062-4DCE-AEB7-520C7EF76171}\"},{\"CreationClassName\":\"Win32_DiskDrive\",\"Name\":\"Microsoft Virtual Disk\",\"Manufacturer\":\"(Standard disk drives)\",\"SerialNumber\":\"DEV11-19217_SCSI#DISK&VEN_MSFT&PROD_VIRTUAL_DISK#5&C5CC8EB&0&000000\"},{\"CreationClassName\":\"Win32_SoundDevice\",\"Name\":\"Realtek High Definition Audio(SST)\",\"Manufacturer\":\"Realtek\",\"SerialNumber\":\"DEV11-19217_INTELAUDIO#FUNC_01&amp;VEN_10EC&amp;DEV_0623&amp;SUBSYS_17AA315F&amp;REV_1000#5&amp;2A95FFC9&amp;0&amp;0001\"},{\"CreationClassName\":\"Win32_NetworkAdapter\",\"Name\":\"Microsoft Kernel Debug Network Adapter\",\"Manufacturer\":\"Microsoft\",\"SerialNumber\":\"DEV11-19217_ROOT#KDNIC#0000\"}]";

    private const string WindowsDeviceHardwareInvalidSnapshotKeyData =
        "[{\"CreationClassName\":\"Win32_PhysicalMemory\",\"Name\":\"Physical Memory\",\"Manufacturer\":\"Microsoft Corporation\",\"SerialNumber\":rs\"DEV11-19217_M0001\"},{\"CreationClassName\":\"Win32_BaseBoard\",\"Name\":\"Base Board\",\"Manufacturer\":\"Microsoft Corporation\",\"SerialNumber\":\"3890-0186-7969-3512-9510-6950-26\"},{\"CreationClassName\":\"Win32_Processor\",\"Name\":\"Intel(R) Xeon(R) Silver 4210 CPU @ 2.20GHz\",\"Manufacturer\":\"GenuineIntel\",\"SerialNumber\":\"DEV11-19217_0000000000000000\"},{\"CreationClassName\":\"Win32_VideoController\",\"Name\":\"Microsoft Hyper-V Video\",\"Manufacturer\":\"Microsoft\",\"SerialNumber\":\"DEV11-19217_VMBUS#{DA0A7802-E377-4AAC-8E77-0558EB1073F8}#{5620E0C7-8062-4DCE-AEB7-520C7EF76171}\"},{\"CreationClassName\":\"Win32_DiskDrive\",\"Name\":\"Microsoft Virtual Disk\",\"Manufacturer\":\"(Standard disk drives)\",\"SerialNumber\":\"DEV11-19217_SCSI#DISK&VEN_MSFT&PROD_VIRTUAL_DISK#5&C5CC8EB&0&000000\"},{\"CreationClassName\":\"Win32_SoundDevice\",\"Name\":\"Realtek High Definition Audio(SST)\",\"Manufacturer\":\"Realtek\",\"SerialNumber\":\"DEV11-19217_INTELAUDIO#FUNC_01&amp;VEN_10EC&amp;DEV_0623&amp;SUBSYS_17AA315F&amp;REV_1000#5&amp;2A95FFC9&amp;0&amp;0001\"},{\"CreationClassName\":\"Win32_NetworkAdapter\",\"Name\":\"Microsoft Kernel Debug Network Adapter\",\"Manufacturer\":\"Microsoft\",\"SerialNumber\":\"DEV11-19217_ROOT#KDNIC#0000\"}]";

    private const string WindowsDeviceUnKnownHardwareSnapshotKeyData =
        "[{\"CreationClassName\":\"Win32_PhysicalMemory\",\"Name\":\"Physical Memory\",\"Manufacturer\":\"Microsoft Corporation\",\"SerialNumber\":\"DEV11-19217_M0001\"},{\"CreationClassName\":\"Win32_BaseBoard\",\"Name\":\"Base Board\",\"Manufacturer\":\"Microsoft Corporation\",\"SerialNumber\":\"3890-0186-7969-3512-9510-6950-26\"},{\"CreationClassName\":\"Win32_Processor\",\"Name\":\"Intel(R) Xeon(R) Silver 4210 CPU @ 2.20GHz\",\"Manufacturer\":\"GenuineIntel\",\"SerialNumber\":\"DEV11-19217_0000000000000000\"},{\"CreationClassName\":\"Win32_VideoController\",\"Name\":\"Microsoft Hyper-V Video\",\"Manufacturer\":\"Microsoft\",\"SerialNumber\":\"DEV11-19217_VMBUS#{DA0A7802-E377-4AAC-8E77-0558EB1073F8}#{5620E0C7-8062-4DCE-AEB7-520C7EF76171}\"},{\"CreationClassName\":\"Win32_DiskDrive\",\"Name\":\"Microsoft Virtual Disk\",\"Manufacturer\":\"(Standard disk drives)\",\"SerialNumber\":\"DEV11-19217_SCSI#DISK&VEN_MSFT&PROD_VIRTUAL_DISK#5&C5CC8EB&0&000000\"},{\"CreationClassName\":\"Win32_SoundDevice\",\"Name\":\"Realtek High Definition Audio(SST)\",\"Manufacturer\":\"Realtek\",\"SerialNumber\":\"DEV11-19217_INTELAUDIO#FUNC_01&amp;VEN_10EC&amp;DEV_0623&amp;SUBSYS_17AA315F&amp;REV_1000#5&amp;2A95FFC9&amp;0&amp;0001\"},{\"CreationClassName\":\"Win32_Battery\",\"Name\":\"Microsoft Kernel Debug Network Adapter\",\"Manufacturer\":\"Microsoft\",\"SerialNumber\":\"DEV11-19217_ROOT#KDNIC#0000\"}]";

    private const string WindowsDeviceHardwareEmptyProperties =
         "[{\"CreationClassName\":\"Win32_PhysicalMemory\",\"Name\":\"Physical Memory\",\"Manufacturer\":\"\",\"SerialNumber\":\"DEV11-19217_M0001\"},{\"CreationClassName\":\"Win32_PhysicalMemory\",\"Name\":\"\",\"Manufacturer\":\"Microsoft Corporation\",\"SerialNumber\":\"DEV11-19217_M0001\"},{\"CreationClassName\":\"Win32_PhysicalMemory\",\"Name\":\"Physical Memory\",\"Manufacturer\":\"Microsoft Corporation\",\"SerialNumber\":\"\"},{\"CreationClassName\":\"Win32_PhysicalMemory\",\"Name\":\"Physical Memory\",\"Manufacturer\":\"Microsoft Corporation\",\"SerialNumber\":\"DEV11-19217_M0002\"}]";

    private const string WindowsDeviceSnapshotData =
        "[{\"CreationClassName\":\"Win32_PhysicalMemory\",\"Name\":\"Physical Memory\",\"Manufacturer\":\"Samsung\",\"SerialNumber\":\"DEVTESTIND231_39F7F8A7\"},{\"CreationClassName\":\"Win32_PhysicalMemory\",\"Name\":\"Physical Memory\",\"Manufacturer\":\"Micron\",\"SerialNumber\":\"DEVTESTIND231_F09BD052\"},{\"CreationClassName\":\"Win32_BaseBoard\",\"Name\":\"Base Board\",\"Manufacturer\":\"LENOVO\",\"SerialNumber\":\"DEVTESTIND231_SDK0J40697 WIN 3305339573733\"},{\"CreationClassName\":\"Win32_Processor\",\"Name\":\"Intel(R) Core(TM) i5-10500 CPU @ 3.10GHz\",\"Manufacturer\":\"GenuineIntel\",\"SerialNumber\":\"DEVTESTIND231_BFEBFBFF000A0653\"},{\"CreationClassName\":\"Win32_VideoController\",\"Name\":\"Intel(R) UHD Graphics 630\",\"Manufacturer\":\"Intel Corporation\",\"SerialNumber\":\"DEVTESTIND231_PCI_VEN_8086&DEV_9BC8&SUBSYS_316817AA&REV_03_3&11583659&0&10\"},{\"CreationClassName\":\"Win32_DiskDrive\",\"Name\":\"SAMSUNG MZVLB256HAHQ-000L7\",\"Manufacturer\":\"(Standard disk drives)\",\"SerialNumber\":\"DEVTESTIND231_0025_3881_91C1_6DB0.\"},{\"CreationClassName\":\"Win32_SoundDevice\",\"Name\":\"Realtek High Definition Audio\",\"Manufacturer\":\"Microsoft\",\"SerialNumber\":\"DEVTESTIND231_HDAUDIO_FUNC_01&VEN_10EC&DEV_0623&SUBSYS_17AA3168&REV_1000_4&2A02E807&0&0001\"},{\"CreationClassName\":\"Win32_SoundDevice\",\"Name\":\"Intel(R) Display Audio\",\"Manufacturer\":\"Intel(R) Corporation\",\"SerialNumber\":\"DEVTESTIND231_HDAUDIO_FUNC_01&VEN_8086&DEV_280B&SUBSYS_80860101&REV_1000_4&2A02E807&0&0201\"},{\"CreationClassName\":\"Win32_NetworkAdapter\",\"Name\":\"Intel(R) Ethernet Connection (11) I219-V\",\"Manufacturer\":\"Intel\",\"SerialNumber\":\"DEVTESTIND231_PCI_VEN_8086&DEV_0D4D&SUBSYS_316817AA&REV_00_3&11583659&0&FE\"}]";

    private const string DeviceHardwareManufacturer = "Microsoft Corporation";
    private const string DeviceHardwareName = "Physical Memory";
    private const string DeviceHardwareSerialNumber = "M0001";
    private const string DeviceHardwareSerialNumber2 = "M0002";
    private readonly string _devId = Guid.NewGuid().ToString();
    private const int DeviceHardwareExceptionNoDeviceHardwareSerialNumberFoundErrorCode = 12051;
    private const int UnableToUpdateDeviceHardwareStatusExceptionMsg = 12052;
    private const int DeviceId = 100000001;

    private Mock<IProgramTrace> _programTraceMock;
    private Mock<IDeviceHardwareProvider> _deviceHardwareProviderMock;
    private Mock<IUserIdentityProvider> _userIdentityProviderMock;
    private Mock<IEventDispatcher> _eventDispatcherMock;
    private Mock<IDeviceSearchInfoService> _deviceSearchInfoService;
    private Mock<ICurrentTimeSupplier> _currentTimeSupplier;
    private Mock<IMemoryCache> _memoryCacheMock;
    private IDeviceHardwareService _deviceHardwareService;

    [SetUp]
    public void Setup()
    {
        _programTraceMock = new Mock<IProgramTrace>();
        _deviceHardwareProviderMock = new Mock<IDeviceHardwareProvider>();
        _userIdentityProviderMock = new Mock<IUserIdentityProvider>();
        _eventDispatcherMock = new Mock<IEventDispatcher>();
        _deviceSearchInfoService = new Mock<IDeviceSearchInfoService>();
        _currentTimeSupplier = new Mock<ICurrentTimeSupplier>();
        _memoryCacheMock = new Mock<IMemoryCache>();
        _memoryCacheMock
        .Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(new Mock<ICacheEntry>().Object);

        _memoryCacheMock
        .Setup(m => m.TryGetValue("DeviceHardwareManufacturerCache", out It.Ref<object>.IsAny))
        .Returns((string _, out object value) =>
        {
            value = new Dictionary<string, int>();
            return false;
        });

        _memoryCacheMock
        .Setup(m => m.TryGetValue("DeviceHardwareCache", out It.Ref<object>.IsAny))
        .Returns((string _, out object value) =>
        {
            value = new Dictionary<string, int>();
            return false;
        });

        _deviceHardwareService = new DeviceHardwareService(_programTraceMock.Object, _deviceHardwareProviderMock.Object, _userIdentityProviderMock.Object, _eventDispatcherMock.Object, _deviceSearchInfoService.Object, _currentTimeSupplier.Object, _memoryCacheMock.Object);
    }

    [Test]
    public void Constructor_Test()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _ = new DeviceHardwareService(null, _deviceHardwareProviderMock.Object, _userIdentityProviderMock.Object, _eventDispatcherMock.Object, _deviceSearchInfoService.Object, _currentTimeSupplier.Object, _memoryCacheMock.Object));
        Assert.Throws<ArgumentNullException>(() =>
            _ = new DeviceHardwareService(_programTraceMock.Object, null, _userIdentityProviderMock.Object, _eventDispatcherMock.Object, _deviceSearchInfoService.Object, _currentTimeSupplier.Object, _memoryCacheMock.Object));
        Assert.Throws<ArgumentNullException>(() =>
            _ = new DeviceHardwareService(_programTraceMock.Object, _deviceHardwareProviderMock.Object, null, _eventDispatcherMock.Object, _deviceSearchInfoService.Object, _currentTimeSupplier.Object, _memoryCacheMock.Object));
        Assert.Throws<ArgumentNullException>(() =>
            _ = new DeviceHardwareService(_programTraceMock.Object, _deviceHardwareProviderMock.Object, _userIdentityProviderMock.Object, null, _deviceSearchInfoService.Object, _currentTimeSupplier.Object, _memoryCacheMock.Object));
        Assert.Throws<ArgumentNullException>(() =>
            _ = new DeviceHardwareService(_programTraceMock.Object, _deviceHardwareProviderMock.Object, _userIdentityProviderMock.Object, _eventDispatcherMock.Object, null, _currentTimeSupplier.Object, _memoryCacheMock.Object));
        Assert.Throws<ArgumentNullException>(() =>
            _ = new DeviceHardwareService(_programTraceMock.Object, _deviceHardwareProviderMock.Object, _userIdentityProviderMock.Object, _eventDispatcherMock.Object, _deviceSearchInfoService.Object, null, _memoryCacheMock.Object));
    }

    [Test]
    public void SynchronizeWindowsDeviceHardwareDataWithSnapshot_Validation_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _deviceHardwareService.SynchronizeWindowsDeviceHardwareDataWithSnapshot(-1, _devId, null));
        Assert.Throws<ArgumentException>(() => _deviceHardwareService.SynchronizeWindowsDeviceHardwareDataWithSnapshot(DeviceId, null, null));
        Assert.Throws<ArgumentException>(() => _deviceHardwareService.SynchronizeWindowsDeviceHardwareDataWithSnapshot(DeviceId, _devId, string.Empty));
        Assert.Throws<ArgumentException>(() => _deviceHardwareService.SynchronizeWindowsDeviceHardwareDataWithSnapshot(DeviceId, null, string.Empty));
    }

    [TestCase(WindowsDeviceHardwareSnapshotKeyData, 3, 3, 1, 0, 0, 1, 1, 1, 1)]
    [TestCase(WindowsDeviceHardwareInvalidSnapshotKeyData, 0, 0, 0, 0, 0, 0, 0, 0, 0)]
    [TestCase(WindowsDeviceHardwareEmptyProperties, 0, 0, 1, 0, 0, 1, 0, 1, 1)]
    [TestCase(WindowsDeviceSnapshotData, 0, 0, 1, 0, 0, 1, 0, 1, 1)]
    public void SynchronizeWindowsDeviceHardwareDataWithSnapshot_Test(string snapshotKeyData, int insertDeviceHardwareCall, int insertManufactuererCall, int bulkModifyCall, int getAllDeviceHardwareCall, int getAllDeviceHardwareManufacturerCall, int eventDispatcherCall, int hardwareUpdateCall, int hardwareStatusUpdateCall, int hardwareStatusHardwareUpdateCall)
    {
        SetDeviceHardwareManufacturerCache();
        SetDeviceHardwareCache();

        _userIdentityProviderMock.Setup(a => a.GetUserIdentity()).Returns(new UserIdentity());
        _deviceHardwareProviderMock.
            Setup(x => x.GetAllDeviceHardware()).Returns(DeviceHardwareTestData);
        _deviceHardwareProviderMock.
            Setup(x => x.GetAllDeviceHardwareManufacturer()).Returns(DeviceHardwareManufacturerTestData);
        _deviceHardwareProviderMock.
            Setup(x => x.InsertDeviceHardwareManufacturer(It.IsAny<DeviceHardwareManufacturer>()));
        _deviceHardwareProviderMock.
            Setup(x => x.InsertDeviceHardware(It.IsAny<DeviceHardware>()));
        _deviceHardwareProviderMock.
            Setup(x => x.BulkModifyDeviceHardwareStatus(DeviceId, It.IsAny<List<DeviceHardwareStatus>>())).Returns(DeviceHardwareDataTestData());

        _deviceHardwareService.SynchronizeWindowsDeviceHardwareDataWithSnapshot(DeviceId, _devId, snapshotKeyData);
        _deviceHardwareProviderMock.Verify(provider => provider.InsertDeviceHardware(It.IsAny<DeviceHardware>()), Times.Exactly(insertDeviceHardwareCall));
        _deviceHardwareProviderMock.Verify(provider => provider.InsertDeviceHardwareManufacturer(It.IsAny<DeviceHardwareManufacturer>()), Times.Exactly(insertManufactuererCall));
        _deviceHardwareProviderMock.Verify(provider => provider.BulkModifyDeviceHardwareStatus(DeviceId, It.IsAny<List<DeviceHardwareStatus>>()), Times.Exactly(bulkModifyCall));
        _deviceHardwareProviderMock.Verify(provider => provider.GetAllDeviceHardwareManufacturer(), Times.Exactly(getAllDeviceHardwareManufacturerCall));
        _deviceHardwareProviderMock.Verify(provider => provider.GetAllDeviceHardware(), Times.Exactly(getAllDeviceHardwareCall));
        _deviceSearchInfoService.Verify(s => s.AddOrUpdateRecords(SearchTable.Hardware, It.IsAny<IEnumerable<Identifier>>()), Times.Exactly(hardwareUpdateCall));
        _deviceSearchInfoService.Verify(s => s.AddOrUpdateRecords(SearchTable.HardwareStatus, It.IsAny<IEnumerable<Identifier>>()), Times.Exactly(hardwareStatusUpdateCall));
        _deviceSearchInfoService.Verify(s => s.AddOrUpdateRecords(SearchTable.HardwareStatusHardware, It.IsAny<IEnumerable<Identifier>>()), Times.Exactly(hardwareStatusHardwareUpdateCall));
        _eventDispatcherMock.Verify(m => m.DispatchEvent(It.IsAny<DeviceHardwareSynchronizationEvent>()), Times.Exactly(eventDispatcherCall));
    }

    [Test]
    public void SynchronizeWindowsDeviceUnknownHardwareDataSnapshot_ThrowsArgumentException_Test()
    {
        _deviceHardwareProviderMock.
            Setup(x => x.InsertDeviceHardwareManufacturer(It.IsAny<DeviceHardwareManufacturer>()));
        _deviceHardwareProviderMock.
            Setup(x => x.InsertDeviceHardware(It.IsAny<DeviceHardware>()));
        _deviceHardwareProviderMock.Setup(x => x.GetAllDeviceHardwareManufacturer())
            .Returns(new List<DeviceHardwareManufacturer>
            {
                new DeviceHardwareManufacturer
                {
                    DeviceHardwareManufacturerId = 1,
                    DeviceHardwareManufacturerName = "TestManufacturer"
                }
            });
        _deviceHardwareProviderMock.Setup(x => x.GetAllDeviceHardware())
            .Returns(new List<DeviceHardware>
            {
                new DeviceHardware
                {
                    DeviceHardwareId = 1,
                    DeviceHardwareName = "TestDeviceHardware",
                    DeviceHardwareTypeId = (int)DeviceHardwareType.RAM,
                    DeviceHardwareManufacturer = new DeviceHardwareManufacturer
                    {
                        DeviceHardwareManufacturerId = 11,
                        DeviceHardwareManufacturerName = "Microsoft"
                    }
                }
            });

        _deviceHardwareService.SynchronizeWindowsDeviceHardwareDataWithSnapshot(DeviceId, _devId, WindowsDeviceUnKnownHardwareSnapshotKeyData);
        _deviceHardwareProviderMock.Verify(provider => provider.InsertDeviceHardware(It.IsAny<DeviceHardware>()), Times.Exactly(6));
        _deviceHardwareProviderMock.Verify(provider => provider.InsertDeviceHardwareManufacturer(It.IsAny<DeviceHardwareManufacturer>()), Times.Exactly(7));
        _deviceHardwareProviderMock.Verify(provider => provider.GetAllDeviceHardwareManufacturer(), Times.Exactly(7));
        _deviceHardwareProviderMock.Verify(provider => provider.GetAllDeviceHardware(), Times.Exactly(7));
    }

    [Test]
    [Obsolete]
    public void FetchOrCreateDeviceHardwareManufacturer_ThrowsArgumentNullException_Test()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _deviceHardwareService.FetchOrCreateDeviceHardwareManufacturer(null));
        Assert.AreEqual("Specified argument was out of the range of valid values. (Parameter 'manufacturerName')", ex?.Message);
    }

    [Test]
    [Obsolete]
    public void FetchOrCreateDeviceHardwareManufacturer_Success()
    {
        SetDeviceHardwareManufacturerCache();
        _deviceHardwareProviderMock.
            Setup(x => x.InsertDeviceHardwareManufacturer(It.IsAny<DeviceHardwareManufacturer>()));
        _deviceHardwareService.FetchOrCreateDeviceHardwareManufacturer(DeviceHardwareManufacturer);
        _deviceHardwareProviderMock.Verify(provider => provider.InsertDeviceHardwareManufacturer(It.IsAny<DeviceHardwareManufacturer>()), Times.Never);

        _memoryCacheMock
            .Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(new Mock<ICacheEntry>().Object);
        _deviceHardwareService.FetchOrCreateDeviceHardwareManufacturer("Test Manufacturer");
        _deviceHardwareProviderMock.Verify(provider => provider.InsertDeviceHardwareManufacturer(It.IsAny<DeviceHardwareManufacturer>()), Times.Once);
        _memoryCacheMock.Verify(m => m.CreateEntry(It.IsAny<object>()), Times.Once);
    }

    [Test]
    public void GetAllDeviceHardwareStatusSummaryByDeviceId_ThrowsArgumentOutOfRangeException_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _deviceHardwareService.GetAllDeviceHardwareStatusSummaryByDeviceId(-1));
    }

    [Test]
    public void GetAllDeviceHardwareStatusSummaryByDeviceId_Test()
    {
        _deviceHardwareProviderMock.
            Setup(x => x.GetAllDeviceHardwareStatusSummaryByDeviceId(DeviceId)).Returns(DeviceHardwareDataTestData);
        var deviceHardwares = _deviceHardwareService.GetAllDeviceHardwareStatusSummaryByDeviceId(DeviceId);
        Assert.IsNotNull(deviceHardwares);
        Assert.AreEqual(2, deviceHardwares.Count());
        _deviceHardwareProviderMock.Verify(provider => provider.GetAllDeviceHardwareStatusSummaryByDeviceId(It.IsAny<int>()), Times.Exactly(1));
    }

    [Test]
    public void GetAllDeviceHardwareStatusSummaryByDeviceIdNoDataExist_Test()
    {
        _deviceHardwareProviderMock.
            Setup(x => x.GetAllDeviceHardwareStatusSummaryByDeviceId(DeviceId)).Returns(new List<DeviceHardwareData>());
        var deviceHardwares = _deviceHardwareService.GetAllDeviceHardwareStatusSummaryByDeviceId(DeviceId);
        Assert.IsNotNull(deviceHardwares);
        Assert.AreEqual(0, deviceHardwares.Count());
        _deviceHardwareProviderMock.Verify(provider => provider.GetAllDeviceHardwareStatusSummaryByDeviceId(It.IsAny<int>()), Times.Exactly(1));
    }

    [Test]
    public void DeleteDeviceHardwareStatusByDeviceId_Test()
    {
        var dummyData = new List<DeviceHardwareData>();
        _deviceHardwareProviderMock.Setup(x => x.DeleteDeviceHardwareStatusByDeviceId(DeviceId)).Returns(dummyData);
        _deviceHardwareService.CleanUpDeviceHardwareStatus(DeviceId);
        _deviceHardwareProviderMock.Verify(provider => provider.DeleteDeviceHardwareStatusByDeviceId(It.IsAny<int>()), Times.Exactly(1));
        _deviceSearchInfoService.Verify(s => s.DeleteRecords(SearchTable.HardwareStatusHardware, It.IsAny<IEnumerable<Identifier>>()), Times.Once);
    }

    [Test]
    public void DeleteDeviceHardwareStatusByDeviceId_ThrowsArgumentOutOfRangeException_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _deviceHardwareService.CleanUpDeviceHardwareStatus(-1));
    }

    [Test]
    public void UpdateDeviceHardwareStatus_Validation_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _deviceHardwareService.UpdateDeviceHardwareStatus(DeviceId, _devId, (HardwareStatus)7, DeviceHardwareSerialNumber));
        Assert.Throws<ArgumentOutOfRangeException>(() => _deviceHardwareService.UpdateDeviceHardwareStatus(-1, _devId, (HardwareStatus)7, DeviceHardwareSerialNumber));
        Assert.Throws<ArgumentException>(() => _deviceHardwareService.UpdateDeviceHardwareStatus(DeviceId, _devId, HardwareStatus.New, null));
        Assert.Throws<ArgumentException>(() => _deviceHardwareService.UpdateDeviceHardwareStatus(DeviceId, null, HardwareStatus.New, null));
    }

    [Test]
    public void UpdateDeviceHardwareStatus_Test()
    {
        _userIdentityProviderMock.Setup(a => a.GetUserIdentity()).Returns(new UserIdentity());
        _deviceHardwareProviderMock.Setup(x => x.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber)).Returns(
            new DeviceHardwareData
            {
                DeviceHardwareManufacturerName = DeviceHardwareManufacturer,
                DeviceHardwareName = DeviceHardwareName,
                DeviceHardwareSerialNumber = DeviceHardwareSerialNumber,
                DeviceHardwareTypeId = (int)DeviceHardwareType.RAM,
                HardwareStatusId = (int)HardwareStatus.New,
                LastModifiedDate = DateTime.UtcNow,
            });
        _deviceHardwareProviderMock
            .Setup(a => a.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(It.IsAny<HardwareStatus>(), It.IsAny<string>()))
            .Returns(new List<DeviceHardwareData>
            {
                new DeviceHardwareData
                {
                    DeviceHardwareStatusId = 12,
                    DeviceHardwareId = 1
                }
            });

        _deviceHardwareService.UpdateDeviceHardwareStatus(
            DeviceId,
            _devId,
            HardwareStatus.Installed,
            DeviceHardwareSerialNumber);
        _deviceHardwareProviderMock.Verify(provider => provider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(It.IsAny<string>()), Times.Exactly(1));
        _deviceHardwareProviderMock.Verify(provider => provider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(It.IsAny<HardwareStatus>(), It.IsAny<string>()), Times.Exactly(1));
        _userIdentityProviderMock.Verify(provider => provider.GetUserIdentity(), Times.Exactly(1));
        _deviceSearchInfoService.Verify(s => s.AddOrUpdateRecords(SearchTable.HardwareStatusHardware, It.IsAny<IEnumerable<Identifier>>()), Times.Once);
        _eventDispatcherMock.Verify(m => m.DispatchEvent(It.IsAny<DeviceHardwareStatusUpdateEvent>()), Times.Exactly(1));
    }

    [Test]
    public void UpdateDeviceHardwareStatus_DeviceHardwareStatusNull_Test()
    {
        _userIdentityProviderMock.Setup(a => a.GetUserIdentity()).Returns(new UserIdentity());
        _deviceHardwareProviderMock.Setup(x => x.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber)).Returns((DeviceHardwareData)null);
        var exception = Assert.Throws<DeviceHardwareException>(() => _deviceHardwareService.UpdateDeviceHardwareStatus(
            DeviceId,
            _devId,
            HardwareStatus.RemovedAcknowledged,
            DeviceHardwareSerialNumber));

        Assert.IsNotNull(exception);
        Assert.IsNotNull(exception.Error);
        Assert.AreEqual(DeviceHardwareExceptionNoDeviceHardwareSerialNumberFoundErrorCode, exception.Error.ErrorCode);
        _deviceHardwareProviderMock.Verify(provider => provider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(It.IsAny<string>()), Times.Exactly(1));
    }

    [Test]
    public void UpdateDeviceHardwareStatus_DeviceHardwareSerialNumberNull_Test()
    {
        _userIdentityProviderMock.Setup(a => a.GetUserIdentity()).Returns(new UserIdentity());
        _deviceHardwareProviderMock.Setup(x => x.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber)).Returns(new DeviceHardwareData
        {
            DeviceHardwareManufacturerName = null,
            DeviceHardwareName = null,
            DeviceHardwareSerialNumber = null,
            DeviceHardwareTypeId = 0,
            HardwareStatusId = 0,
            LastModifiedDate = DateTime.Now,
        });
        var exception = Assert.Throws<DeviceHardwareException>(() => _deviceHardwareService.UpdateDeviceHardwareStatus(
            DeviceId,
            _devId,
            HardwareStatus.RemovedAcknowledged,
            DeviceHardwareSerialNumber));

        Assert.IsNotNull(exception);
        Assert.IsNotNull(exception.Error);
        Assert.AreEqual(DeviceHardwareExceptionNoDeviceHardwareSerialNumberFoundErrorCode, exception.Error.ErrorCode);
        _deviceHardwareProviderMock.Verify(provider => provider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(It.IsAny<string>()), Times.Exactly(1));
    }

    [Test]
    public void UpdateDeviceHardwareStatus_UserIdentityNull_Test()
    {
        _userIdentityProviderMock.Setup(a => a.GetUserIdentity()).Returns((UserIdentity)null);
        _deviceHardwareProviderMock.Setup(x => x.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber)).Returns(new DeviceHardwareData
        {
            DeviceHardwareManufacturerName = null,
            DeviceHardwareName = null,
            DeviceHardwareSerialNumber = DeviceHardwareSerialNumber,
            DeviceHardwareTypeId = 0,
            HardwareStatusId = 1,
            LastModifiedDate = DateTime.Now,
        });
        _deviceHardwareProviderMock
            .Setup(a => a.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(It.IsAny<HardwareStatus>(), It.IsAny<string>()))
            .Returns(new List<DeviceHardwareData>
            {
                new DeviceHardwareData
                {
                    DeviceHardwareStatusId = 12,
                    DeviceHardwareId = 1
                }
            });

        _deviceHardwareService.UpdateDeviceHardwareStatus(
            DeviceId,
            _devId,
            HardwareStatus.Installed,
            DeviceHardwareSerialNumber);

        _deviceHardwareProviderMock.Verify(provider => provider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(It.IsAny<string>()), Times.Exactly(1));
        _deviceHardwareProviderMock.Verify(provider => provider.UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(It.IsAny<HardwareStatus>(), It.IsAny<string>()), Times.Exactly(1));
        _userIdentityProviderMock.Verify(provider => provider.GetUserIdentity(), Times.Exactly(1));
        _eventDispatcherMock.Verify(m => m.DispatchEvent(It.IsAny<DeviceHardwareStatusUpdateEvent>()), Times.Exactly(0));
    }

    [Test]
    public void UpdateDeviceHardwareStatus_DeviceHardwareStatusException_Test()
    {
        _deviceHardwareProviderMock.Setup(x => x.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(DeviceHardwareSerialNumber)).Returns(new DeviceHardwareData
        {
            DeviceHardwareManufacturerName = DeviceHardwareManufacturer,
            DeviceHardwareName = DeviceHardwareName,
            DeviceHardwareSerialNumber = DeviceHardwareSerialNumber,
            DeviceHardwareTypeId = (int)DeviceHardwareType.RAM,
            HardwareStatusId = (int)HardwareStatus.New,
            LastModifiedDate = DateTime.UtcNow,
        });

        var exception = Assert.Throws<DeviceHardwareException>(() => _deviceHardwareService.UpdateDeviceHardwareStatus(
            DeviceId,
            _devId,
            HardwareStatus.RemovedAcknowledged,
            DeviceHardwareSerialNumber));

        Assert.IsNotNull(exception);
        Assert.IsNotNull(exception.Error);
        Assert.AreEqual(UnableToUpdateDeviceHardwareStatusExceptionMsg, exception.Error.ErrorCode);
        _deviceHardwareProviderMock.Verify(provider => provider.GetDeviceHardwareStatusByDeviceHardwareSerialNumber(It.IsAny<string>()), Times.Exactly(1));
    }

    [Test]
    public void CleanUpDeviceHardwareAsset()
    {
        var now = DateTime.UtcNow;
        IReadOnlyList<DeviceHardwareData> dummyData = new List<DeviceHardwareData>();
        _deviceHardwareProviderMock.Setup(x => x.DeviceHardwareAssetCleanUp(It.IsAny<DateTime>(), It.IsAny<DateTime>(), out dummyData));
        _currentTimeSupplier
            .Setup(x => x.GetUtcNow())
            .Returns(now);
        _deviceHardwareService.CleanUpDeviceHardwareAsset();
        _deviceHardwareProviderMock.Verify(provider => provider.DeviceHardwareAssetCleanUp(It.IsAny<DateTime>(), It.IsAny<DateTime>(), out dummyData), Times.Exactly(1));
        _deviceSearchInfoService.Verify(s => s.DeleteRecords(SearchTable.HardwareStatusHardware, It.IsAny<IEnumerable<Identifier>>()), Times.Once);
    }

    [Test]
    [Obsolete]
    public void GetDeviceHardwareManufacturersByIds_WithValidIds_ReturnsCorrectManufacturers()
    {
        // Arrange
        var ids = new[] { 1, 2, 3 };
        var expectedManufacturers = new List<DeviceHardwareManufacturer>
        {
            new DeviceHardwareManufacturer { DeviceHardwareManufacturerId = 1, DeviceHardwareManufacturerName = "Manufacturer1" },
            new DeviceHardwareManufacturer { DeviceHardwareManufacturerId = 2, DeviceHardwareManufacturerName = "Manufacturer2" },
            new DeviceHardwareManufacturer { DeviceHardwareManufacturerId = 3, DeviceHardwareManufacturerName = "Manufacturer3" }
        };

        _memoryCacheMock
        .Setup(m => m.TryGetValue(It.IsAny<string>(), out It.Ref<object>.IsAny))
        .Returns((string _, out object value) =>
        {
            // Setting the out parameter to the expected dictionary.
            value = expectedManufacturers.ToDictionary(
                m => m.DeviceHardwareManufacturerName,
                m => m.DeviceHardwareManufacturerId
            );
            return true;  // Return true to indicate the value was found in cache.
        });

        _deviceHardwareProviderMock.Setup(m => m.GetAllDeviceHardwareManufacturer()).Returns(expectedManufacturers);

        // Act
        var result = _deviceHardwareService.GetDeviceHardwareManufacturersByIds(ids).ToArray();

        // Assert
        Assert.AreEqual(expectedManufacturers.Count, result.Length);
        for (int i = 0; i < result.Length; i++)
        {
            Assert.AreEqual(expectedManufacturers[i].DeviceHardwareManufacturerId, result[i].DeviceHardwareManufacturerId);
            Assert.AreEqual(expectedManufacturers[i].DeviceHardwareManufacturerName, result[i].DeviceHardwareManufacturerName);
        }
    }

    [Test]
    [Obsolete]
    public void GetDeviceHardwareManufacturersByIds_WhenCacheMisses_PopulatesCache()
    {
        // Arrange
        var ids = new[] { 2 };
        var manufacturerCache = new[]
        {
            new DeviceHardwareManufacturer { DeviceHardwareManufacturerId = 1, DeviceHardwareManufacturerName = "Manufacturer1" }
        };

        var manufacturerCache1 = new[]
        {
            new DeviceHardwareManufacturer { DeviceHardwareManufacturerId = 1, DeviceHardwareManufacturerName = "Manufacturer1" }
        };

        var manufacturer = new DeviceHardwareManufacturer { DeviceHardwareManufacturerId = 2, DeviceHardwareManufacturerName = "Manufacturer2" };

        _memoryCacheMock
        .Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(new Mock<ICacheEntry>().Object);

        _memoryCacheMock
        .Setup(m => m.TryGetValue(It.IsAny<string>(), out It.Ref<object>.IsAny))
        .Returns((string _, out object value) =>
        {
            value = manufacturerCache1.ToDictionary(
                m => m.DeviceHardwareManufacturerName,
                m => m.DeviceHardwareManufacturerId
            );
            return true;
        });  // Cache miss

        _deviceHardwareProviderMock.Setup(m => m.GetAllDeviceHardwareManufacturer()).Returns((manufacturerCache.Concat(new[] { manufacturer }).AsArray()));

        // Act
        var result = _deviceHardwareService.GetDeviceHardwareManufacturersByIds(ids).ToArray();

        // Assert
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual(manufacturer.DeviceHardwareManufacturerId, result[0].DeviceHardwareManufacturerId);
        Assert.AreEqual(manufacturer.DeviceHardwareManufacturerName, result[0].DeviceHardwareManufacturerName);
        _memoryCacheMock.Verify(m => m.CreateEntry(It.IsAny<object>()), Times.Once);
    }

    [Test]
    [Obsolete]
    public void GetDeviceHardwareManufacturersByIds_WithNonExistentIds_ReturnsEmptyList()
    {
        // Arrange
        var ids = new[] { 999 };

        _deviceHardwareProviderMock.Setup(m => m.GetAllDeviceHardwareManufacturer()).Returns(new List<DeviceHardwareManufacturer>());

        // Act
        var result = _deviceHardwareService.GetDeviceHardwareManufacturersByIds(ids);

        // Assert
        Assert.AreEqual(0, result.Count());
        _deviceHardwareProviderMock.Verify(m => m.GetAllDeviceHardwareManufacturer(), Times.Exactly(3));
        _memoryCacheMock.Verify(m => m.TryGetValue(It.IsAny<string>(), out It.Ref<object>.IsAny), Times.Exactly(2));
    }

    private List<DeviceHardwareManufacturer> DeviceHardwareManufacturerTestData()
    {
        return
        [
            new DeviceHardwareManufacturer
            {
                DeviceHardwareManufacturerId = 1,
                DeviceHardwareManufacturerName = DeviceHardwareManufacturer
            }
        ];
    }

    private List<DeviceHardware> DeviceHardwareTestData()
    {
        return
        [
            new DeviceHardware
            {
                DeviceHardwareId = 1,
                DeviceHardwareName = DeviceHardwareName,
                DeviceHardwareTypeId = 1,
                DeviceHardwareManufacturer = new DeviceHardwareManufacturer
                {
                    DeviceHardwareManufacturerId = 1,
                    DeviceHardwareManufacturerName = DeviceHardwareManufacturer
                }
            }
        ];
    }

    private List<DeviceHardwareData> DeviceHardwareDataTestData()
    {
        return
        [
            new DeviceHardwareData
            {
                DeviceHardwareManufacturerName = DeviceHardwareManufacturer,
                DeviceHardwareName = DeviceHardwareName,
                HardwareStatusId = (int)HardwareStatus.New,
                DeviceHardwareTypeId = (int)DeviceHardwareType.RAM,
                DeviceHardwareSerialNumber = DeviceHardwareSerialNumber
            },
            new DeviceHardwareData
            {
                DeviceHardwareManufacturerName = DeviceHardwareManufacturer,
                DeviceHardwareName = DeviceHardwareName,
                HardwareStatusId = (int)HardwareStatus.Removed,
                DeviceHardwareTypeId = (int)DeviceHardwareType.RAM,
                DeviceHardwareSerialNumber = DeviceHardwareSerialNumber2
            }
        ];
    }

    private List<DeviceHardware> DeviceHardwareTest()
    {
        return [
            new DeviceHardware
            {
                DeviceHardwareId = 2,
                DeviceHardwareName = "Base Board",
                DeviceHardwareTypeId = 2,
                DeviceHardwareManufacturer = new DeviceHardwareManufacturer
                {
                    DeviceHardwareManufacturerId = 1,
                    DeviceHardwareManufacturerName = "Microsoft Corporation"
                }
            },
            new DeviceHardware
            {
                DeviceHardwareId = 8,
                DeviceHardwareName = "Base Board",
                DeviceHardwareTypeId = 2,
                DeviceHardwareManufacturer = new DeviceHardwareManufacturer
                {
                    DeviceHardwareManufacturerId = 6,
                    DeviceHardwareManufacturerName = "LENOVO"
                }
            },
            new DeviceHardware
            {
                DeviceHardwareId = 9,
                DeviceHardwareName = "Intel(R) Core(TM) i5-10500 CPU @ 3.10GHz",
                DeviceHardwareTypeId = 3,
                DeviceHardwareManufacturer = new DeviceHardwareManufacturer
                {
                    DeviceHardwareManufacturerId = 2,
                    DeviceHardwareManufacturerName = "GenuineIntel"
                }
            },
            new DeviceHardware
            {
                DeviceHardwareId = 13,
                DeviceHardwareName = "Intel(R) Display Audio",
                DeviceHardwareTypeId = 6,
                DeviceHardwareManufacturer = new DeviceHardwareManufacturer
                {
                    DeviceHardwareManufacturerId = 9,
                    DeviceHardwareManufacturerName = "Intel(R) Corporation"
                }
            },
            new DeviceHardware
            {
                DeviceHardwareId = 14,
                DeviceHardwareName = "Intel(R) Ethernet Connection (11) I219-V",
                DeviceHardwareTypeId = 7,
                DeviceHardwareManufacturer = new DeviceHardwareManufacturer
                {
                    DeviceHardwareManufacturerId = 10,
                    DeviceHardwareManufacturerName = "Intel"
                }
            },
            new DeviceHardware
            {
                DeviceHardwareId = 10,
                DeviceHardwareName = "Intel(R) UHD Graphics 630",
                DeviceHardwareTypeId = 4,
                DeviceHardwareManufacturer = new DeviceHardwareManufacturer
                {
                    DeviceHardwareManufacturerId = 7,
                    DeviceHardwareManufacturerName = "Intel Corporation"
                }
            },
            new DeviceHardware
            {
                DeviceHardwareId = 5,
                DeviceHardwareName = "Intel(R) Xeon(R) CPU E5-2660 v3 @ 2.60GHz",
                DeviceHardwareTypeId = 3,
                DeviceHardwareManufacturer = new DeviceHardwareManufacturer
                {
                    DeviceHardwareManufacturerId = 2,
                    DeviceHardwareManufacturerName = "GenuineIntel"
                }
            },
            new DeviceHardware
            {
                DeviceHardwareId = 3,
                DeviceHardwareName = "Intel(R) Xeon(R) Silver 4210 CPU @ 2.20GHz",
                DeviceHardwareTypeId = 3,
                DeviceHardwareManufacturer = new DeviceHardwareManufacturer
                {
                    DeviceHardwareManufacturerId = 2,
                    DeviceHardwareManufacturerName = "GenuineIntel"
                }
            },
            new DeviceHardware
            {
                DeviceHardwareId = 4,
                DeviceHardwareName = "Microsoft Virtual Disk",
                DeviceHardwareTypeId = 5,
                DeviceHardwareManufacturer = new DeviceHardwareManufacturer
                {
                    DeviceHardwareManufacturerId = 3,
                    DeviceHardwareManufacturerName = "(Standard disk drives)"
                }
            },
            new DeviceHardware
            {
                DeviceHardwareId = 1,
                DeviceHardwareName = "Physical Memory",
                DeviceHardwareTypeId = 1,
                DeviceHardwareManufacturer = new DeviceHardwareManufacturer
                {
                    DeviceHardwareManufacturerId = 1,
                    DeviceHardwareManufacturerName = "Microsoft Corporation"
                }
            },
            new DeviceHardware
            {
                DeviceHardwareId = 6,
                DeviceHardwareName = "Physical Memory",
                DeviceHardwareTypeId = 1,
                DeviceHardwareManufacturer = new DeviceHardwareManufacturer
                {
                    DeviceHardwareManufacturerId = 4,
                    DeviceHardwareManufacturerName = "Samsung"
                }
            },
            new DeviceHardware
            {
                DeviceHardwareId = 7,
                DeviceHardwareName = "Physical Memory",
                DeviceHardwareTypeId = 1,
                DeviceHardwareManufacturer = new DeviceHardwareManufacturer
                {
                    DeviceHardwareManufacturerId = 5,
                    DeviceHardwareManufacturerName = "Micron"
                }
            },
            new DeviceHardware
            {
                DeviceHardwareId = 12,
                DeviceHardwareName = "Realtek High Definition Audio",
                DeviceHardwareTypeId = 6,
                DeviceHardwareManufacturer = new DeviceHardwareManufacturer
                {
                    DeviceHardwareManufacturerId = 8,
                    DeviceHardwareManufacturerName = "Microsoft"
                }
            },
            new DeviceHardware
            {
                DeviceHardwareId = 11,
                DeviceHardwareName = "SAMSUNG MZVLB256HAHQ-000L7",
                DeviceHardwareTypeId = 5,
                DeviceHardwareManufacturer = new DeviceHardwareManufacturer
                {
                    DeviceHardwareManufacturerId = 3,
                    DeviceHardwareManufacturerName = "(Standard disk drives)"
                }
            }
        ];
    }

    private void SetDeviceHardwareManufacturerCache()
    {
        _memoryCacheMock
        .Setup(m => m.TryGetValue("DeviceHardwareManufacturerCache", out It.Ref<object>.IsAny))
        .Returns((string _, out object value) =>
        {
            value = DeviceHardwareManufacturerTestData().ToDictionary(m => m.DeviceHardwareManufacturerName, m => m.DeviceHardwareManufacturerId);
            return true;
        });
    }

    private void SetDeviceHardwareCache()
    {
        _memoryCacheMock
        .Setup(m => m.TryGetValue("DeviceHardwareCache", out It.Ref<object>.IsAny))
        .Returns((string _, out object value) =>
        {
            value = DeviceHardwareTest().ToDictionary(h => $"{h.DeviceHardwareName}_{h.DeviceHardwareManufacturer.DeviceHardwareManufacturerName}", h => h.DeviceHardwareId);
            return true;
        });
    }
}

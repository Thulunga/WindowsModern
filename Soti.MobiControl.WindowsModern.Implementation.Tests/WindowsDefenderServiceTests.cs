using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Soti.Diagnostics;
using Soti.MobiControl.Devices;
using Soti.MobiControl.Events;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.Time;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
internal sealed class WindowsDefenderServiceTests
{
    private const int DeviceId = 12345;
    private const string StringDeviceId = "TestDevice";
    private const AntivirusThreatStatus ActionFailedThreatStatus = AntivirusThreatStatus.ActionFailed;
    private const DeviceFamily WindowsModernDeviceFamily = DeviceFamily.WindowsPhone;

    private Mock<IWindowsDefenderDataProvider> _windowsDefenderProviderMock;
    private Mock<IWindowsDeviceService> _windowsDeviceServiceMock;
    private Mock<ICurrentTimeSupplier> _currentTimeSupplierMock;
    private Mock<IProgramTrace> _programTraceMock;
    private Mock<IEventDispatcher> _eventDispatcherMock;
    private Mock<IDeviceIdentityMapper> _deviceIdentityMapperMock;
    private Mock<IWindowsDeviceDataProvider> _windowsDeviceProviderMock;
    private IWindowsDefenderService _windowsDefenderService;

    [SetUp]
    public void Setup()
    {
        _windowsDefenderProviderMock = new Mock<IWindowsDefenderDataProvider>(MockBehavior.Strict);
        _currentTimeSupplierMock = new Mock<ICurrentTimeSupplier>();
        _windowsDeviceServiceMock = new Mock<IWindowsDeviceService>();
        _programTraceMock = new Mock<IProgramTrace>();
        _eventDispatcherMock = new Mock<IEventDispatcher>();
        _deviceIdentityMapperMock = new Mock<IDeviceIdentityMapper>();
        _windowsDeviceProviderMock = new Mock<IWindowsDeviceDataProvider>(MockBehavior.Strict);
        _windowsDefenderService = new WindowsDefenderService(_windowsDefenderProviderMock.Object,
            _currentTimeSupplierMock.Object,
            _windowsDeviceServiceMock.Object,
            _programTraceMock.Object,
            _eventDispatcherMock.Object,
            _deviceIdentityMapperMock.Object,
            _windowsDeviceProviderMock.Object);
    }

    [Test]
    public void ConstructorTests()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new WindowsDefenderService(null,
            _currentTimeSupplierMock.Object,
            _windowsDeviceServiceMock.Object,
            _programTraceMock.Object,
            _eventDispatcherMock.Object,
            _deviceIdentityMapperMock.Object,
            _windowsDeviceProviderMock.Object));
        Assert.Throws<ArgumentNullException>(() => _ = new WindowsDefenderService(_windowsDefenderProviderMock.Object,
            null,
            _windowsDeviceServiceMock.Object,
            _programTraceMock.Object,
            _eventDispatcherMock.Object,
            _deviceIdentityMapperMock.Object,
            _windowsDeviceProviderMock.Object));
        Assert.Throws<ArgumentNullException>(() => _ = new WindowsDefenderService(_windowsDefenderProviderMock.Object,
            _currentTimeSupplierMock.Object,
            null,
            _programTraceMock.Object,
            _eventDispatcherMock.Object,
            _deviceIdentityMapperMock.Object,
            _windowsDeviceProviderMock.Object));
        Assert.Throws<ArgumentNullException>(() => _ = new WindowsDefenderService(_windowsDefenderProviderMock.Object,
            _currentTimeSupplierMock.Object,
            _windowsDeviceServiceMock.Object,
            null,
            _eventDispatcherMock.Object,
            _deviceIdentityMapperMock.Object,
            _windowsDeviceProviderMock.Object));
        Assert.Throws<ArgumentNullException>(() => _ = new WindowsDefenderService(_windowsDefenderProviderMock.Object,
            _currentTimeSupplierMock.Object,
            _windowsDeviceServiceMock.Object,
            _programTraceMock.Object,
            null,
            _deviceIdentityMapperMock.Object,
            _windowsDeviceProviderMock.Object));
        Assert.Throws<ArgumentNullException>(() => _ = new WindowsDefenderService(_windowsDefenderProviderMock.Object,
            _currentTimeSupplierMock.Object,
            _windowsDeviceServiceMock.Object,
            _programTraceMock.Object,
            _eventDispatcherMock.Object,
            null,
            _windowsDeviceProviderMock.Object));
        Assert.Throws<ArgumentNullException>(() => _ = new WindowsDefenderService(_windowsDefenderProviderMock.Object,
            _currentTimeSupplierMock.Object,
            _windowsDeviceServiceMock.Object,
            _programTraceMock.Object,
            _eventDispatcherMock.Object,
            _deviceIdentityMapperMock.Object,
            null));
    }

    [Test]
    public void GetAntivirusThreatHistory_ValidInput_ReturnsData()
    {
        var deviceId = 100001;
        var lastStatusChangeStartTime = DateTime.Now.AddDays(-5);
        var lastStatusChangeEndTime = DateTime.Now;
        var threatTypeIds = new List<AntivirusThreatType> { AntivirusThreatType.Malware };
        var threatSeverityIds = new List<AntivirusThreatSeverity> { AntivirusThreatSeverity.Moderate };
        var lastThreatStatusIds = new List<AntivirusThreatStatus> { AntivirusThreatStatus.FullScanRequired };
        var skip = 0;
        var take = 10;
        var totalCount = 1;

        var expectedData = new List<AntivirusThreatData>
        {
            new()
            {
                ExternalThreatId = 1,
                TypeId = 1,
                ThreatName = "Test Threat",
                SeverityId = 4,
                InitialDetectionTime = DateTime.Now.AddDays(-6),
                LastStatusChangeTime = DateTime.Now.AddDays(-2),
                CurrentDetectionCount = 3,
                LastThreatStatusId = 2
            }
        };

        _windowsDefenderProviderMock
            .Setup(x => x.GetAntivirusThreatHistory(
                It.IsAny<int>(),
                It.IsAny<IEnumerable<byte>>(),
                It.IsAny<IEnumerable<byte>>(),
                It.IsAny<IEnumerable<byte>>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<AntivirusThreatSortByOption>(),
                It.IsAny<bool>(),
                out totalCount))
            .Returns(expectedData.AsEnumerable());

        var result = _windowsDefenderService.GetAntivirusThreatHistory(
            deviceId,
            threatTypeIds,
            threatSeverityIds,
            lastThreatStatusIds,
            lastStatusChangeStartTime,
            lastStatusChangeEndTime,
            skip,
            take,
            AntivirusThreatSortByOption.ThreatName,
            false,
            out totalCount);

        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());

        _windowsDefenderProviderMock.Verify(x => x.GetAntivirusThreatHistory(
            It.IsAny<int>(),
            It.IsAny<IEnumerable<byte>>(),
            It.IsAny<IEnumerable<byte>>(),
            It.IsAny<IEnumerable<byte>>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<AntivirusThreatSortByOption>(),
            It.IsAny<bool>(),
            out totalCount), Times.Once);
    }

    [Test]
    public void GetAntivirusThreatHistory_Throws_Exception()
    {
        var threatTypes = new List<AntivirusThreatType> { AntivirusThreatType.Malware };
        var threatSeverities = new List<AntivirusThreatSeverity> { AntivirusThreatSeverity.Moderate };
        var lastThreatStatuses = new List<AntivirusThreatStatus> { AntivirusThreatStatus.FullScanRequired };

        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderService.GetAntivirusThreatHistory(0, threatTypes, threatSeverities, lastThreatStatuses, DateTime.UtcNow, DateTime.UtcNow, 0, 10, AntivirusThreatSortByOption.AntivirusThreatInitialDetectionTime, false, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderService.GetAntivirusThreatHistory(1, threatTypes, threatSeverities, lastThreatStatuses, null, null, -1, 10, AntivirusThreatSortByOption.AntivirusThreatInitialDetectionTime, false, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderService.GetAntivirusThreatHistory(1, threatTypes, threatSeverities, lastThreatStatuses, null, null, 0, 0, AntivirusThreatSortByOption.AntivirusThreatInitialDetectionTime, false, out _));
        Assert.Throws<ArgumentException>(() => _windowsDefenderService.GetAntivirusThreatHistory(1, threatTypes, threatSeverities, lastThreatStatuses, DateTime.UtcNow, DateTime.UtcNow.AddDays(-1), 0, 10, AntivirusThreatSortByOption.AntivirusThreatInitialDetectionTime, false, out _));
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetAntivirusThreatHistory(1, threatTypes, threatSeverities, lastThreatStatuses, null, DateTime.UtcNow, 0, 10, AntivirusThreatSortByOption.AntivirusThreatInitialDetectionTime, false, out _));
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetAntivirusThreatHistory(1, threatTypes, threatSeverities, lastThreatStatuses, DateTime.UtcNow, null, 0, 10, AntivirusThreatSortByOption.AntivirusThreatInitialDetectionTime, false, out _));
    }

    [Test]
    public void GetAllAntivirusThreatHistory_ThrowsArgumentOutOfRangeException_WhenDeviceIdIsInvalid()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDefenderService.GetAllAntivirusThreatHistory(default));
        Assert.AreEqual("deviceId", ex?.ParamName);
    }

    [Test]
    public void GetAllAntivirusThreatHistory_ReturnsCorrectData()
    {
        var deviceId = 100001;
        var expectedData = new List<AntivirusThreatData>
        {
            new()
            {
                ExternalThreatId = 1,
                TypeId = 1,
                ThreatName = "Test Threat",
                SeverityId = 4,
                InitialDetectionTime = DateTime.Now.AddDays(-6),
                LastStatusChangeTime = DateTime.Now.AddDays(-2),
                CurrentDetectionCount = 3,
                LastThreatStatusId = 2
            }
        };

        _windowsDefenderProviderMock
            .Setup(x => x.GetAllAntivirusThreatHistory(
                It.IsAny<int>()))
            .Returns(expectedData.AsEnumerable());

        var result = _windowsDefenderService.GetAllAntivirusThreatHistory(deviceId);

        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());

        _windowsDefenderProviderMock.Verify(x => x.GetAllAntivirusThreatHistory(It.IsAny<int>()));
    }

    [Test]
    public void GetAntivirusScanSummary_ThrowsArgumentOutOfRangeException_WhenDeviceIdIsInvalid()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDefenderService.GetAntivirusScanSummary(default));

        Assert.AreEqual("deviceId", ex?.ParamName);
    }

    [Test]
    public void GetAntivirusScanSummary_ReturnsAntivirusSummaryWithThreatStatusCountNull()
    {
        var antivirusScanTimeData = new AntivirusScanTimeData
        {
            AntivirusLastQuickScanTime = DateTime.Now.AddDays(-1),
            AntivirusLastFullScanTime = DateTime.Now.AddDays(-2),
            LastAntivirusSyncTime = DateTime.Now.AddDays(-1),
            IsThreatsAvailable = true
        };

        _windowsDeviceProviderMock
            .Setup(x => x.GetAntivirusScanTimeData(DeviceId))
            .Returns(antivirusScanTimeData);

        _windowsDefenderProviderMock
            .Setup(x => x.GetThreatStatusIdCountForDevice(DeviceId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns((Dictionary<byte, int>)null);

        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetAntivirusScanSummary(DeviceId));

        _windowsDeviceProviderMock.Verify(x => x.GetAntivirusScanTimeData(DeviceId), Times.Once);
        _windowsDefenderProviderMock.Verify(x => x.GetThreatStatusIdCountForDevice(DeviceId, It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
    }

    [Test]
    public void GetAntivirusScanSummary_ReturnsCorrectAntivirusSummary()
    {
        var antivirusScanTimeData = new AntivirusScanTimeData
        {
            AntivirusLastQuickScanTime = DateTime.Now.AddDays(-1),
            AntivirusLastFullScanTime = DateTime.Now.AddDays(-2),
            LastAntivirusSyncTime = DateTime.Now.AddDays(-1),
            IsThreatsAvailable = true
        };

        var threatStatusIdCount = new Dictionary<byte, int>()
            {
                {(byte)AntivirusThreatStatus.Active, 1 },
                {(byte)AntivirusThreatStatus.ActionFailed, 0 },
                {(byte)AntivirusThreatStatus.ManualStepsRequired, 1 },
                {(byte)AntivirusThreatStatus.FullScanRequired, 1 },
                {(byte)AntivirusThreatStatus.RebootRequired, 0 },
                {(byte)AntivirusThreatStatus.RemediatedWithNonCriticalFailures, 0 },
                {(byte)AntivirusThreatStatus.Quarantined, 1 },
                {(byte)AntivirusThreatStatus.Removed, 1 },
                {(byte)AntivirusThreatStatus.Cleaned, 0 },
                {(byte)AntivirusThreatStatus.Allowed, 0 },
                {(byte)AntivirusThreatStatus.NoStatusCleared, 1 }
            };

        _windowsDeviceProviderMock
            .Setup(x => x.GetAntivirusScanTimeData(DeviceId))
            .Returns(antivirusScanTimeData);

        _windowsDefenderProviderMock
            .Setup(x => x.GetThreatStatusIdCountForDevice(DeviceId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(threatStatusIdCount);

        var result = _windowsDefenderService.GetAntivirusScanSummary(DeviceId);

        Assert.IsNotNull(result);
        Assert.AreEqual(antivirusScanTimeData.AntivirusLastQuickScanTime, result.LastQuickScanTime);
        Assert.AreEqual(antivirusScanTimeData.AntivirusLastFullScanTime, result.LastFullScanTime);
        Assert.IsTrue(result.IsThreatsAvailable);
        Assert.AreEqual(1, result.ThreatStatusCountSummary[AntivirusThreatStatus.Active]);
        Assert.AreEqual(0, result.ThreatStatusCountSummary[AntivirusThreatStatus.ActionFailed]);

        _windowsDeviceProviderMock.Verify(x => x.GetAntivirusScanTimeData(DeviceId), Times.Once);
        _windowsDefenderProviderMock.Verify(x => x.GetThreatStatusIdCountForDevice(DeviceId, It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
    }

    [Test]
    public void GetAntivirusScanSummary_ReturnsCorrectAntivirusSummary_WhenNoThreatsAvailable()
    {
        var antivirusScanTimeData = new AntivirusScanTimeData
        {
            AntivirusLastQuickScanTime = DateTime.Now.AddDays(-1),
            AntivirusLastFullScanTime = DateTime.Now.AddDays(-2),
            IsThreatsAvailable = false
        };

        _windowsDeviceProviderMock
            .Setup(x => x.GetAntivirusScanTimeData(DeviceId))
            .Returns(antivirusScanTimeData);

        var result = _windowsDefenderService.GetAntivirusScanSummary(DeviceId);

        Assert.IsNotNull(result);
        Assert.AreEqual(antivirusScanTimeData.AntivirusLastQuickScanTime, result.LastQuickScanTime);
        Assert.AreEqual(antivirusScanTimeData.AntivirusLastFullScanTime, result.LastFullScanTime);
        Assert.IsFalse(result.IsThreatsAvailable);

        _windowsDeviceProviderMock.Verify(x => x.GetAntivirusScanTimeData(DeviceId), Times.Once);
        _windowsDefenderProviderMock.Verify(x => x.GetThreatStatusIdCountForDevice(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    public void GetDeviceGroupAntivirusThreatHistory_NullInput_ThrowsArgumentNullException()
    {
        var groupIds = new[] { 1 };
        var threatTypes = new List<AntivirusThreatType> { AntivirusThreatType.Malware };
        var threatSeverities = new List<AntivirusThreatSeverity> { AntivirusThreatSeverity.Moderate };

        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusThreatHistory(
            null,
            threatTypes,
            threatSeverities,
            DateTime.UtcNow,
            DateTime.UtcNow,
            0,
            10,
            AntivirusThreatHistorySortByOption.LastStatusChangeTime,
            true,
            out _));
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusThreatHistory(
            groupIds,
            null,
            threatSeverities,
            DateTime.UtcNow,
            DateTime.UtcNow,
            0,
            10,
            AntivirusThreatHistorySortByOption.LastStatusChangeTime,
            true,
            out _));
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusThreatHistory(
            groupIds,
            threatTypes,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow,
            0,
            10,
            AntivirusThreatHistorySortByOption.LastStatusChangeTime,
            true,
            out _));
        Assert.Throws<ArgumentException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusThreatHistory(groupIds,
            threatTypes,
            threatSeverities,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(-1),
            0,
            10,
            AntivirusThreatHistorySortByOption.LastStatusChangeTime,
            true,
            out _));
    }

    [Test]
    public void GetActiveThreatsStatusByDeviceId_Throws_ExceptionTests()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderService.GetActiveThreatsStatusByDeviceId(default));
    }

    [Test]
    public void GetActiveThreatsStatusByDeviceId_Tests()
    {
        var antivirusThreatAvailabilityData = new AntivirusThreatAvailabilityData()
        {
            LastAntivirusSyncTime = DateTime.Now,
            IsThreatAvailable = true
        };

        _windowsDefenderProviderMock.Setup(
            x => x.GetAntivirusThreatAvailabilityByDeviceId(DeviceId, AntivirusThreatStatus.Active))
            .Returns(antivirusThreatAvailabilityData);

        var result = _windowsDefenderService.GetActiveThreatsStatusByDeviceId(DeviceId);

        Assert.IsNotNull(result);
        Assert.IsNotNull(antivirusThreatAvailabilityData.LastAntivirusSyncTime);
        Assert.IsTrue(antivirusThreatAvailabilityData.IsThreatAvailable);

        _windowsDefenderProviderMock.Verify(
            x => x.GetAntivirusThreatAvailabilityByDeviceId(DeviceId, AntivirusThreatStatus.Active),
            Times.Once);
    }

    [Test]
    public void NoActiveThreatsAvailable_Tests()
    {
        var antivirusThreatAvailabilityData = new AntivirusThreatAvailabilityData()
        {
            LastAntivirusSyncTime = DateTime.Now,
            IsThreatAvailable = false
        };

        _windowsDefenderProviderMock.Setup(
                x => x.GetAntivirusThreatAvailabilityByDeviceId(DeviceId, AntivirusThreatStatus.Active))
            .Returns(antivirusThreatAvailabilityData);

        var result = _windowsDefenderService.GetActiveThreatsStatusByDeviceId(DeviceId);

        Assert.IsNotNull(result);
        Assert.IsNotNull(antivirusThreatAvailabilityData.LastAntivirusSyncTime);
        Assert.IsFalse(antivirusThreatAvailabilityData.IsThreatAvailable);

        _windowsDefenderProviderMock.Verify(
            x => x.GetAntivirusThreatAvailabilityByDeviceId(DeviceId, AntivirusThreatStatus.Active),
            Times.Once);
    }

    [Test]
    public void NoSyncInitiated_Tests()
    {
        var antivirusThreatAvailabilityData = new AntivirusThreatAvailabilityData()
        {
            LastAntivirusSyncTime = null,
            IsThreatAvailable = false
        };

        _windowsDefenderProviderMock.Setup(
                x => x.GetAntivirusThreatAvailabilityByDeviceId(DeviceId, AntivirusThreatStatus.Active))
            .Returns(antivirusThreatAvailabilityData);

        var result = _windowsDefenderService.GetActiveThreatsStatusByDeviceId(DeviceId);

        Assert.IsNotNull(result);
        Assert.IsNull(antivirusThreatAvailabilityData.LastAntivirusSyncTime);
        Assert.IsFalse(antivirusThreatAvailabilityData.IsThreatAvailable);

        _windowsDefenderProviderMock.Verify(
            x => x.GetAntivirusThreatAvailabilityByDeviceId(DeviceId, AntivirusThreatStatus.Active),
            Times.Once);
    }

    [Test]
    public void GetActiveThreatsStatusByDeviceIds_Throws_ExceptionTests()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetActiveThreatsStatusByDeviceIds(null));
    }

    [Test]
    public void GetAntivirusThreatAvailabilityByDeviceIds_Tests()
    {
        var antivirusThreatAvailabilityData = new AntivirusThreatAvailabilityData()
        {
            DeviceId = 1,
            LastAntivirusSyncTime = DateTime.Now,
            IsThreatAvailable = true
        };

        var deviceIds = new[] { 1, 2 };

        _windowsDefenderProviderMock.Setup(
                x => x.GetAntivirusThreatAvailabilityByDeviceIds(deviceIds, AntivirusThreatStatus.Active))
            .Returns(new[] { antivirusThreatAvailabilityData });

        var result = _windowsDefenderService.GetActiveThreatsStatusByDeviceIds(deviceIds).FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(true, result.IsThreatAvailable);
        Assert.IsNotNull(result.LastAntivirusSyncTime);
        Assert.AreEqual(1, result.DeviceId);

        _windowsDefenderProviderMock.Verify(
            x => x.GetAntivirusThreatAvailabilityByDeviceIds(deviceIds, AntivirusThreatStatus.Active),
            Times.Once);
    }

    [Test]
    public void GetDeviceGroupAntivirusThreatHistory_ValidInput_ReturnsData()
    {
        var groupIds = new[] { 1 };
        var lastStatusChangeStartTime = DateTime.Now.AddDays(-5);
        var lastStatusChangeEndTime = DateTime.Now;
        var threatTypeIds = new List<AntivirusThreatType> { AntivirusThreatType.Malware };
        var threatSeverityIds = new List<AntivirusThreatSeverity> { AntivirusThreatSeverity.Moderate };
        var skip = 0;
        var take = 10;
        var total = 0;

        var expectedData = new List<AntivirusThreatData>
        {
            new()
            {
                ExternalThreatId = 1,
                TypeId = 1,
                ThreatName = "Test Threat",
                SeverityId = 4,
                InitialDetectionTime = DateTime.Now.AddDays(-6),
                LastStatusChangeTime = DateTime.Now.AddDays(-2),
                CurrentDetectionCount = 3,
                LastThreatStatusId = 2
            }
        };

        _windowsDefenderProviderMock
            .Setup(x => x.GetDeviceGroupsAntivirusThreatHistory(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<DeviceFamily>(),
                It.IsAny<IEnumerable<byte>>(),
                It.IsAny<IEnumerable<byte>>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                AntivirusThreatHistorySortByOption.LastStatusChangeTime,
                true,
                out total))
            .Returns(expectedData.AsEnumerable());

        var result = _windowsDefenderService.GetDeviceGroupsAntivirusThreatHistory(
            groupIds,
            threatTypeIds,
            threatSeverityIds,
            lastStatusChangeStartTime,
            lastStatusChangeEndTime,
            skip,
            take,
            AntivirusThreatHistorySortByOption.LastStatusChangeTime,
            true,
            out var totalCount);

        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());

        _windowsDefenderProviderMock.Verify(x => x.GetDeviceGroupsAntivirusThreatHistory(
            It.IsAny<IEnumerable<int>>(),
            It.IsAny<DeviceFamily>(),
            It.IsAny<IEnumerable<byte>>(),
            It.IsAny<IEnumerable<byte>>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            AntivirusThreatHistorySortByOption.LastStatusChangeTime,
            true,
            out total), Times.Once);
    }

    [Test]
    public void GetAntivirusThreatStatusIdCount_Success()
    {
        var startTime = new DateTime(2021, 3, 13);
        var endTime = new DateTime(2021, 3, 15);
        var groupIds = new[] { 1, 2 };

        var threatStatusIdCount = new Dictionary<byte, int>()
            {
                {(byte)AntivirusThreatStatus.Active, 1 },
                {(byte)AntivirusThreatStatus.ActionFailed, 0 },
                {(byte)AntivirusThreatStatus.ManualStepsRequired, 1 },
                {(byte)AntivirusThreatStatus.FullScanRequired, 1 },
                {(byte)AntivirusThreatStatus.RebootRequired, 0 },
                {(byte)AntivirusThreatStatus.RemediatedWithNonCriticalFailures, 0 },
                {(byte)AntivirusThreatStatus.Quarantined, 1 },
                {(byte)AntivirusThreatStatus.Removed, 1 },
                {(byte)AntivirusThreatStatus.Cleaned, 0 },
                {(byte)AntivirusThreatStatus.Allowed, 0 },
                {(byte)AntivirusThreatStatus.NoStatusCleared, 1 }
            };

        _windowsDefenderProviderMock.Setup(provider => provider.GetAntivirusThreatStatusCount(groupIds, startTime, endTime, (int)WindowsModernDeviceFamily)).Returns(threatStatusIdCount);
        var result = _windowsDefenderService.GetAntivirusThreatStatusCount(groupIds, startTime, endTime);
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result[AntivirusThreatStatus.Active]);
        Assert.AreEqual(0, result[AntivirusThreatStatus.ActionFailed]);
        _windowsDefenderProviderMock.Verify(provider => provider.GetAntivirusThreatStatusCount(groupIds, startTime, endTime, (int)WindowsModernDeviceFamily), Times.Once);
    }

    [Test]
    public void GetAntivirusThreatStatusIdCount_Throws_DateRangeException()
    {
        var startTime = new DateTime(2021, 3, 13);
        var endTime = new DateTime(2021, 3, 11);
        var groupIds = new[] { 1, 2 };

        var ex = Assert.Throws<ArgumentException>(() => _windowsDefenderService.GetAntivirusThreatStatusCount(groupIds, startTime, endTime));
        Assert.That(ex?.Message, Is.EqualTo("Start date should not be greater than end date."));
    }

    [Test]
    public void GetAntivirusThreatStatusIdCount_Throws_ArgumentNullException()
    {
        var groupIds = new[] { 1, 2 };

        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetAntivirusThreatStatusCount(groupIds, null, DateTime.Now));
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetAntivirusThreatStatusCount(groupIds, new DateTime(2021, 3, 13), null));
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetAntivirusThreatStatusCount(null, new DateTime(2021, 3, 13), DateTime.Now));
    }

    [Test]
    public void GetDeviceGroupsDefaultAntivirusScanSummary_Throws_Exception()
    {
        var groupIds = new[] { 1, 2 };

        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetDeviceGroupsDefaultAntivirusScanSummary(null, DateTime.MinValue));
    }

    [Test]
    public void GetDeviceGroupsDefaultAntivirusScanSummary_Test()
    {
        var groupIds = new[] { 1, 2 };
        var data = new AntivirusGroupScanSummary()
        {
            Within7Days = new AntivirusScanCounts()
            {
                FullScanned = 1,
                QuickScanned = 1
            },
            Within24Hrs = new AntivirusScanCounts()
            {
                FullScanned = 1,
                QuickScanned = 2
            },
            MoreThan30Days = new AntivirusScanCounts()
            {
                FullScanned = 3,
                QuickScanned = 1
            }
        };
        var synCompletedOn = new DateTime(2024, 7, 1);

        _windowsDeviceProviderMock.Setup(x => x.GetDeviceGroupsDefaultAntivirusScanSummary(
                groupIds,
                WindowsModernDeviceFamily,
                synCompletedOn))
            .Returns(data);

        var result = _windowsDefenderService.GetDeviceGroupsDefaultAntivirusScanSummary(groupIds, synCompletedOn);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Within7Days.FullScanned);
        Assert.AreEqual(1, result.Within7Days.QuickScanned);
        Assert.AreEqual(1, result.Within24Hrs.FullScanned);
        Assert.AreEqual(2, result.Within24Hrs.QuickScanned);
        Assert.AreEqual(1, result.MoreThan30Days.QuickScanned);
        Assert.AreEqual(3, result.MoreThan30Days.FullScanned);

        _windowsDeviceProviderMock.Verify(x => x.GetDeviceGroupsDefaultAntivirusScanSummary(
                groupIds,
                WindowsModernDeviceFamily,
                synCompletedOn),
            Times.Once());
    }

    [Test]
    public void GetDeviceGroupsCustomAntivirusScanSummary_Throws_Exception()
    {
        var groupIds = new[] { 1, 2 };

        Assert.Throws<ArgumentException>(() => _windowsDefenderService.GetDeviceGroupsCustomAntivirusScanSummary(groupIds, new DateTime(2024, 07, 10), new DateTime(2024, 07, 09)));
    }

    [Test]
    public void GetDeviceGroupsCustomAntivirusScanSummary_Test()
    {
        var groupIds = new[] { 1, 2 };
        var data = new AntivirusGroupScanSummary()
        {
            Custom = new AntivirusScanCounts()
            {
                FullScanned = 3,
                QuickScanned = 1
            },
        };

        _windowsDeviceProviderMock.Setup(x => x.GetDeviceGroupsCustomAntivirusScanSummary(
                groupIds,
                WindowsModernDeviceFamily,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .Returns(data);

        var result = _windowsDefenderService.GetDeviceGroupsCustomAntivirusScanSummary(groupIds, new DateTime(2024, 7, 1), new DateTime(2024, 7, 10));

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Custom.FullScanned);
        Assert.AreEqual(1, result.Custom.QuickScanned);
        Assert.IsNull(result.Within7Days);

        _windowsDeviceProviderMock.Verify(x => x.GetDeviceGroupsCustomAntivirusScanSummary(groupIds,
                WindowsModernDeviceFamily,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()),
            Times.Once());
    }

    [Test]
    public void GetDeviceGroupsAntivirusLastScanSummaryPaginated_Throws_ArgumentOutOfRangeException()
    {
        var antivirusLastScanInfo = new AntivirusLastScanDetailRequest
        {
            GroupIds = new List<int> { 1, 2 },
            SyncCompletedOn = new DateTime(2021, 3, 19),
            AntivirusScanType = AntivirusScanType.FullScan,
            AntivirusScanPeriod = (AntivirusScanPeriodSubType)255,
            LastScanStartDate = new DateTime(2021, 3, 13),
            LastScanEndDate = new DateTime(2021, 3, 19),
            Skip = 0,
            Take = 50,
            Order = true
        };
        Assert.Throws<InvalidCastException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(antivirusLastScanInfo, out _));

        antivirusLastScanInfo = new AntivirusLastScanDetailRequest
        {
            GroupIds = new List<int> { 1, 2 },
            SyncCompletedOn = new DateTime(2021, 3, 19),
            AntivirusScanType = (AntivirusScanType)255,
            AntivirusScanPeriod = AntivirusScanPeriodSubType.Custom,
            LastScanStartDate = new DateTime(2021, 3, 13),
            LastScanEndDate = new DateTime(2021, 3, 19),
            Skip = 0,
            Take = 50,
            Order = true
        };
        Assert.Throws<InvalidCastException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(antivirusLastScanInfo, out _));

        antivirusLastScanInfo = new AntivirusLastScanDetailRequest
        {
            GroupIds = new List<int> { 1, 2 },
            SyncCompletedOn = new DateTime(2021, 3, 19),
            AntivirusScanType = AntivirusScanType.FullScan,
            AntivirusScanPeriod = AntivirusScanPeriodSubType.Custom,
            LastScanStartDate = new DateTime(2021, 3, 20),
            LastScanEndDate = new DateTime(2021, 3, 19),
            Skip = 0,
            Take = 50,
            Order = true
        };
        Assert.Throws<ArgumentException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(antivirusLastScanInfo, out _));

        antivirusLastScanInfo = new AntivirusLastScanDetailRequest
        {
            GroupIds = new List<int> { 1, 2 },
            SyncCompletedOn = new DateTime(2021, 3, 19),
            AntivirusScanType = AntivirusScanType.FullScan,
            AntivirusScanPeriod = AntivirusScanPeriodSubType.Custom,
            LastScanStartDate = new DateTime(2021, 3, 13),
            LastScanEndDate = new DateTime(2021, 3, 19),
            Skip = -1,
            Take = 50,
            Order = true
        };
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(antivirusLastScanInfo, out _));

        antivirusLastScanInfo = new AntivirusLastScanDetailRequest
        {
            GroupIds = new List<int> { 1, 2 },
            SyncCompletedOn = new DateTime(2021, 3, 19),
            AntivirusScanType = AntivirusScanType.FullScan,
            AntivirusScanPeriod = AntivirusScanPeriodSubType.Custom,
            LastScanStartDate = new DateTime(2021, 3, 13),
            LastScanEndDate = new DateTime(2021, 3, 19),
            Skip = 1,
            Take = 0,
            Order = true
        };
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(antivirusLastScanInfo, out _));
    }

    [Test]
    public void GetDeviceGroupsAntivirusLastScanSummaryPaginated_Throws_ArgumentNullException()
    {
        var groupIds = new[] { 1, 2 };
        var antivirusLastScanInfo = new AntivirusLastScanDetailRequest
        {
            GroupIds = null,
            SyncCompletedOn = new DateTime(2021, 3, 19),
            AntivirusScanType = AntivirusScanType.FullScan,
            AntivirusScanPeriod = AntivirusScanPeriodSubType.Custom,
            LastScanStartDate = new DateTime(2021, 3, 13),
            LastScanEndDate = new DateTime(2021, 3, 18),
            Skip = 0,
            Take = 50,
            Order = true
        };
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(antivirusLastScanInfo, out _));

        antivirusLastScanInfo = new AntivirusLastScanDetailRequest
        {
            GroupIds = groupIds,
            SyncCompletedOn = new DateTime(2021, 3, 19),
            AntivirusScanType = AntivirusScanType.FullScan,
            AntivirusScanPeriod = AntivirusScanPeriodSubType.Custom,
            LastScanStartDate = null,
            LastScanEndDate = new DateTime(2021, 3, 18),
            Skip = 0,
            Take = 50,
            Order = true
        };
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(antivirusLastScanInfo, out _));

        antivirusLastScanInfo = new AntivirusLastScanDetailRequest
        {
            GroupIds = groupIds,
            SyncCompletedOn = new DateTime(2021, 3, 19),
            AntivirusScanType = AntivirusScanType.FullScan,
            AntivirusScanPeriod = AntivirusScanPeriodSubType.Custom,
            LastScanStartDate = new DateTime(2021, 3, 13),
            LastScanEndDate = null,
            Skip = 0,
            Take = 50,
            Order = true
        };
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(antivirusLastScanInfo, out _));

        antivirusLastScanInfo = new AntivirusLastScanDetailRequest
        {
            GroupIds = groupIds,
            SyncCompletedOn = new DateTime(2021, 3, 19),
            AntivirusScanType = AntivirusScanType.FullScan,
            AntivirusScanPeriod = AntivirusScanPeriodSubType.Custom,
            LastScanStartDate = new DateTime(2021, 3, 19),
            LastScanEndDate = new DateTime(2021, 3, 18),
            Skip = 0,
            Take = 50,
            Order = true
        };
        Assert.Throws<ArgumentException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(antivirusLastScanInfo, out _));
    }

    [Test]
    public void GetDeviceGroupsAntivirusLastFullScanSummaryPaginated_Tests()
    {
        var total = 0;
        var deviceId = Guid.NewGuid();

        var antivirusLastScanInfo = new AntivirusLastScanDetailRequest
        {
            GroupIds = new[] { 1, 2 },
            SyncCompletedOn = new DateTime(2021, 3, 19),
            AntivirusScanType = AntivirusScanType.FullScan,
            AntivirusScanPeriod = AntivirusScanPeriodSubType.Custom,
            LastScanStartDate = new DateTime(2021, 3, 13),
            LastScanEndDate = new DateTime(2021, 3, 18),
            Skip = 0,
            Take = 50,
            Order = true
        };

        var antivirusLastScanDeviceData = new AntivirusLastScanDeviceData
        {
            DevId = deviceId.ToString(),
            LastScanDate = DateTime.UtcNow,
        };

        _windowsDeviceProviderMock
            .Setup(provider => provider.GetAntivirusLastFullScanSummary(
            It.IsAny<IEnumerable<int>>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<DeviceFamily>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            true,
            out total))
            .Returns(new List<AntivirusLastScanDeviceData>() { antivirusLastScanDeviceData });

        var actual = _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(
            antivirusLastScanInfo,
            out total);

        var result = actual.FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(result.LastScanDate, antivirusLastScanDeviceData.LastScanDate);
        Assert.AreEqual(result.DevId, antivirusLastScanDeviceData.DevId);

        _windowsDeviceProviderMock.Verify(provider => provider.GetAntivirusLastFullScanSummary(It.IsAny<IEnumerable<int>>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<DeviceFamily>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            true,
            out total), Times.Once);
    }

    [Test]
    public void GetDeviceGroupsAntivirusLastQuickScanSummaryPaginated_Tests()
    {
        var total = 0;
        var deviceId = Guid.NewGuid();
        var antivirusLastScanInfo = new AntivirusLastScanDetailRequest
        {
            GroupIds = new List<int> { 1, 2 },
            SyncCompletedOn = new DateTime(2021, 3, 19),
            AntivirusScanType = AntivirusScanType.QuickScan,
            AntivirusScanPeriod = AntivirusScanPeriodSubType.Custom,
            LastScanStartDate = new DateTime(2021, 3, 13),
            LastScanEndDate = new DateTime(2021, 3, 18),
            Skip = 0,
            Take = 50,
            Order = true
        };

        var antivirusLastScanDeviceData = new AntivirusLastScanDeviceData
        {
            DevId = deviceId.ToString(),
            LastScanDate = DateTime.UtcNow,
        };

        _windowsDeviceProviderMock
            .Setup(provider => provider.GetAntivirusLastQuickScanSummary(
            It.IsAny<IEnumerable<int>>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<DeviceFamily>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            true,
            out total))
            .Returns(new List<AntivirusLastScanDeviceData>() { antivirusLastScanDeviceData });

        var actual = _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(
            antivirusLastScanInfo,
            out total);

        var result = actual.FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(result.LastScanDate, antivirusLastScanDeviceData.LastScanDate);
        Assert.AreEqual(result.DevId, antivirusLastScanDeviceData.DevId);

        _windowsDeviceProviderMock.Verify(provider => provider.GetAntivirusLastQuickScanSummary(It.IsAny<IEnumerable<int>>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<DeviceFamily>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            true,
            out total), Times.Once);
    }

    [Test]
    public void GetDeviceGroupsAntivirusThreatStatus_Throws_Exception()
    {
        var deviceGroupIds = new[] { 1 };
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusThreatStatus(null, DateTime.UtcNow, DateTime.UtcNow, ActionFailedThreatStatus, 0, 10, true, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusThreatStatus(deviceGroupIds, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), ActionFailedThreatStatus, -1, 10, true, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusThreatStatus(deviceGroupIds, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), ActionFailedThreatStatus, 0, 0, true, out _));
        var ex = Assert.Throws<ArgumentException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusThreatStatus(deviceGroupIds, DateTime.UtcNow, DateTime.UtcNow.AddDays(-1), ActionFailedThreatStatus, 0, 10, true, out _));
        Assert.AreEqual("Start date should not be greater than end date.", ex?.Message);
    }

    [Test]
    public void GetDeviceGroupsAntivirusThreatStatus_ValidInput_ReturnsData()
    {
        var deviceId = "1";
        var deviceGroupIds = new[] { 1 };
        var lastStatusChangeStartTime = DateTime.Now.AddDays(-5);
        var lastStatusChangeEndTime = DateTime.Now;
        var skip = 0;
        var take = 10;
        var expectedTotalCount = 20;

        var expectedData = new List<AntivirusThreatStatusDeviceData>
        {
            new()
            {
                DevId = deviceId,
                LastStatusChangeTime = DateTime.Now.AddDays(-2),
                ExternalThreatId = 1,
                TypeId = (int)AntivirusThreatType.Adware,
                SeverityId = (int)AntivirusThreatSeverity.Moderate,
            }
        };

        _windowsDefenderProviderMock
            .Setup(x => x.GetAntivirusThreatStatus(
                It.IsAny<IEnumerable<int>>(),
                DeviceFamily.WindowsPhone,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                ActionFailedThreatStatus,
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                out expectedTotalCount))
            .Returns(expectedData.AsEnumerable());

        var result = _windowsDefenderService.GetDeviceGroupsAntivirusThreatStatus(deviceGroupIds, lastStatusChangeStartTime, lastStatusChangeEndTime, ActionFailedThreatStatus, skip, take, true, out var totalCount);

        Assert.NotNull(result);
        Assert.AreEqual(expectedTotalCount, totalCount);
        Assert.AreEqual(1, result.Count());

        _windowsDefenderProviderMock.Verify(x => x.GetAntivirusThreatStatus(
            It.IsAny<IEnumerable<int>>(),
            DeviceFamily.WindowsPhone,
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            ActionFailedThreatStatus,
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            out expectedTotalCount), Times.Once);
    }

    [Test]
    public void GetDeviceThreatStatusPaginated_Throws_Exception()
    {
        var deviceGroupIds = new[] { 1 };
        var lastStatusChangeStartTime = DateTime.Now.AddDays(-5);
        var lastStatusChangeEndTime = DateTime.Now;
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderService.GetDeviceThreatStatusByGroupIdsPaginated(-1, deviceGroupIds, lastStatusChangeStartTime, lastStatusChangeEndTime, 0, 50, AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime, true, out _));
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetDeviceThreatStatusByGroupIdsPaginated(123123, null, lastStatusChangeStartTime, lastStatusChangeEndTime, 0, 50, AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime, true, out _));
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetDeviceThreatStatusByGroupIdsPaginated(123123, Enumerable.Empty<int>(), lastStatusChangeStartTime, lastStatusChangeEndTime, 0, 50, AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime, true, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderService.GetDeviceThreatStatusByGroupIdsPaginated(123123, deviceGroupIds, lastStatusChangeStartTime, lastStatusChangeEndTime, -1, 50, AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime, true, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderService.GetDeviceThreatStatusByGroupIdsPaginated(123123, deviceGroupIds, lastStatusChangeStartTime, lastStatusChangeEndTime, 0, 0, AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime, true, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderService.GetDeviceThreatStatusByGroupIdsPaginated(123123, deviceGroupIds, lastStatusChangeStartTime, lastStatusChangeEndTime, 0, -1, AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime, true, out _));
    }

    [Test]
    public void GetDeviceThreatStatusByGroupIds_Success()
    {
        var lastStatusChangeStartTime = DateTime.Now.AddDays(-5);
        var lastStatusChangeEndTime = DateTime.Now;
        var threatId = 1;
        var groupIds = new List<int> { 1, 2, 3 };
        var expectedThreatDetails = new List<DeviceThreatDetails>
        {
            new() { DevId = "Device1", InitialDetectionTime = DateTime.UtcNow, ThreatStatus = AntivirusThreatStatus.Active },
            new() { DevId = "Device2", InitialDetectionTime = DateTime.UtcNow, ThreatStatus = AntivirusThreatStatus.Cleaned }
        };

        _windowsDefenderProviderMock
            .Setup(provider => provider.GetDeviceThreatStatusByGroupIdsAndDeviceFamily(threatId, groupIds, WindowsModernDeviceFamily, lastStatusChangeStartTime, lastStatusChangeEndTime))
            .Returns(expectedThreatDetails);

        var result = _windowsDefenderService.GetDeviceThreatStatusByGroupIds(threatId, groupIds, lastStatusChangeStartTime, lastStatusChangeEndTime);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(expectedThreatDetails, result);

        _windowsDefenderProviderMock.Verify(provider => provider.GetDeviceThreatStatusByGroupIdsAndDeviceFamily(threatId, groupIds, WindowsModernDeviceFamily, lastStatusChangeStartTime, lastStatusChangeEndTime), Times.Once);
    }

    [Test]
    public void GetDeviceThreatStatusPaginated_Success()
    {
        const long threatId = 1;
        const int skip = 0;
        const int take = 10;
        var groupIds = new[] { 1, 2, 3 };
        var expectedTotalCount = 20;
        var lastStatusChangeStartTime = DateTime.Now.AddDays(-5);
        var lastStatusChangeEndTime = DateTime.Now;

        var expectedThreatDetails = new List<DeviceThreatDetails>
        {
            new() { DevId = "Device1", InitialDetectionTime = DateTime.UtcNow, ThreatStatus = AntivirusThreatStatus.Active },
            new() { DevId = "Device2", InitialDetectionTime = DateTime.UtcNow, ThreatStatus = AntivirusThreatStatus.Cleaned }
        };

        _windowsDefenderProviderMock
            .Setup(provider => provider.GetDeviceThreatStatusByGroupIdsAndDeviceFamilyPaginated(threatId, groupIds, WindowsModernDeviceFamily, lastStatusChangeStartTime, lastStatusChangeEndTime, skip, take, AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime, true, out expectedTotalCount))
            .Returns(expectedThreatDetails);

        var result = _windowsDefenderService.GetDeviceThreatStatusByGroupIdsPaginated(threatId, groupIds, lastStatusChangeStartTime, lastStatusChangeEndTime, skip, take, AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime, true, out var totalCount);

        Assert.IsNotNull(result);
        Assert.AreEqual(expectedThreatDetails.Count, result.Count);
        Assert.AreEqual(expectedThreatDetails, result);
        Assert.AreEqual(expectedTotalCount, totalCount);

        _windowsDefenderProviderMock.Verify(provider => provider.GetDeviceThreatStatusByGroupIdsAndDeviceFamilyPaginated(
            threatId,
            groupIds,
            WindowsModernDeviceFamily,
            lastStatusChangeStartTime,
            lastStatusChangeEndTime,
            skip,
            take,
            AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime,
            true,
            out expectedTotalCount), Times.Once);
    }

    [Test]
    public void GetAntivirusThreatHistoryDetails_Success()
    {
        var groupIds = new[] { 1 };
        var expectedData = new List<DeviceGroupThreatHistoryData>
        {
            new()
            {
                ExternalThreatId = 1,
                TypeId = (byte)AntivirusThreatType.Dialer,
                SeverityId = (byte)AntivirusThreatSeverity.Severe,
                InitialDetectionTime = DateTime.Now.AddDays(-6),
                LastStatusChangeTime = DateTime.Now.AddDays(-2),
                NoOfDevices = 2,
            }
        };

        _windowsDefenderProviderMock.Setup(provider => provider.GetAntivirusGroupThreatHistory(groupIds, WindowsModernDeviceFamily)).Returns(expectedData);

        var result = _windowsDefenderService.GetAntivirusThreatHistoryDetails(groupIds).FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(expectedData[0].ExternalThreatId, result.ExternalThreatId);
        Assert.AreEqual(expectedData[0].TypeId, (byte)result.ThreatCategory);
        Assert.AreEqual(expectedData[0].SeverityId, (byte?)result.Severity);
        Assert.AreEqual(expectedData[0].InitialDetectionTime, result.InitialDetectionTime);
        Assert.AreEqual(expectedData[0].LastStatusChangeTime, result.LastStatusChangeTime);
        Assert.AreEqual(expectedData[0].NoOfDevices, result.NumberOfDevices);

        _windowsDefenderProviderMock.Verify(provider => provider.GetAntivirusGroupThreatHistory(groupIds, WindowsModernDeviceFamily), Times.Once);
    }

    [Test]
    public void GetAntivirusThreatHistoryDetails_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetAntivirusThreatHistoryDetails(null));
    }

    [Test]
    public void GetDeviceGroupsAntivirusLastScanSummary_Throws_ArgumentOutOfRangeException()
    {
        var groupIds = new[] { 1, 2 };
        var antivirusScanType = AntivirusScanType.FullScan;
        var antivirusScanPeriod = AntivirusScanPeriodSubType.Custom;
        var lastScanStartDate = new DateTime(2021, 3, 13);
        var lastScanEndDate = new DateTime(2021, 3, 19);
        var syncCompletedOn = new DateTime(2021, 3, 18);

        antivirusScanPeriod = (AntivirusScanPeriodSubType)255;

        Assert.Throws<InvalidCastException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(groupIds, syncCompletedOn, antivirusScanType, antivirusScanPeriod, lastScanStartDate, lastScanEndDate));

        antivirusScanType = (AntivirusScanType)255;
        antivirusScanPeriod = AntivirusScanPeriodSubType.Custom;

        Assert.Throws<InvalidCastException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(groupIds, syncCompletedOn, antivirusScanType, antivirusScanPeriod, lastScanStartDate, lastScanEndDate));

        antivirusScanType = AntivirusScanType.FullScan;
        lastScanStartDate = new DateTime(2021, 3, 20);

        Assert.Throws<ArgumentException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(groupIds, syncCompletedOn, antivirusScanType, antivirusScanPeriod, lastScanStartDate, lastScanEndDate));
    }

    [Test]
    public void GetDeviceGroupsAntivirusLastScanSummary_Throws_ArgumentNullException()
    {
        List<int> groupIds = null;
        var antivirusScanType = AntivirusScanType.FullScan;
        var antivirusScanPeriod = AntivirusScanPeriodSubType.Custom;
        var lastScanStartDate = new DateTime(2021, 3, 13);
        var lastScanEndDate = new DateTime(2021, 3, 18);
        var syncCompletedOn = new DateTime(2021, 3, 18);

        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(groupIds, syncCompletedOn, antivirusScanType, antivirusScanPeriod, lastScanStartDate, lastScanEndDate));

        groupIds = new List<int> { 1, 2 };
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(groupIds, syncCompletedOn, antivirusScanType, antivirusScanPeriod, null, lastScanEndDate));

        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(groupIds, syncCompletedOn, antivirusScanType, antivirusScanPeriod, lastScanStartDate, null));
        lastScanStartDate = new DateTime(2021, 3, 19);
        lastScanEndDate = new DateTime(2021, 3, 18);

        Assert.Throws<ArgumentException>(() => _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(groupIds, syncCompletedOn, antivirusScanType, antivirusScanPeriod, lastScanStartDate, lastScanEndDate));
    }

    [Test]
    public void GetDeviceGroupsAntivirusLastFullScanSummary_Tests()
    {
        var groupIds = new[] { 1, 2 };
        var antivirusScanType = AntivirusScanType.FullScan;
        var antivirusScanPeriod = AntivirusScanPeriodSubType.Custom;
        var lastScanStartDate = new DateTime(2021, 3, 13);
        var lastScanEndDate = new DateTime(2021, 3, 18);
        var syncCompletedOn = new DateTime(2021, 3, 18);
        var deviceId = Guid.NewGuid();

        var antivirusLastScanDeviceData = new AntivirusLastScanDeviceData
        {
            DevId = deviceId.ToString(),
            LastScanDate = DateTime.UtcNow,
        };

        _windowsDeviceProviderMock
            .Setup(provider => provider.GetAntivirusLastFullScanSummary(
            It.IsAny<IEnumerable<int>>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<DeviceFamily>()))
            .Returns(new List<AntivirusLastScanDeviceData>() { antivirusLastScanDeviceData });

        var actual = _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(groupIds, syncCompletedOn, antivirusScanType, antivirusScanPeriod, lastScanStartDate, lastScanEndDate);

        var result = actual.FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(result.LastScanDate, antivirusLastScanDeviceData.LastScanDate);
        Assert.AreEqual(result.DevId, antivirusLastScanDeviceData.DevId);

        _windowsDeviceProviderMock.Verify(provider => provider.GetAntivirusLastFullScanSummary(It.IsAny<IEnumerable<int>>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<DeviceFamily>()), Times.Once);
    }

    [Test]
    public void GetDeviceGroupsAntivirusLastQuickScanSummary_Tests()
    {
        var deviceId = Guid.NewGuid();
        var groupIds = new[] { 1, 2 };
        var antivirusScanType = AntivirusScanType.QuickScan;
        var antivirusScanPeriod = AntivirusScanPeriodSubType.Custom;
        var lastScanStartDate = new DateTime(2021, 3, 13);
        var lastScanEndDate = new DateTime(2021, 3, 18);

        var antivirusLastScanDeviceData = new AntivirusLastScanDeviceData
        {
            DevId = deviceId.ToString(),
            LastScanDate = DateTime.UtcNow,
        };

        _windowsDeviceProviderMock
            .Setup(provider => provider.GetAntivirusLastQuickScanSummary(
            It.IsAny<IEnumerable<int>>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<DeviceFamily>()))
            .Returns(new List<AntivirusLastScanDeviceData>() { antivirusLastScanDeviceData });

        var actual = _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(groupIds, DateTime.Now, antivirusScanType, antivirusScanPeriod, lastScanStartDate, lastScanEndDate);

        var result = actual.FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(result.LastScanDate, antivirusLastScanDeviceData.LastScanDate);
        Assert.AreEqual(result.DevId, antivirusLastScanDeviceData.DevId);

        _windowsDeviceProviderMock.Verify(provider => provider.GetAntivirusLastQuickScanSummary(It.IsAny<IEnumerable<int>>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<DeviceFamily>()), Times.Once);
    }

    [Test]
    public void GetDeviceThreatStatus_Throws_Exception()
    {
        var groupIds = new[] { 1 };
        var lastStatusChangeStartTime = DateTime.Now.AddDays(-5);
        var lastStatusChangeEndTime = DateTime.Now;

        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderService.GetDeviceThreatStatusByGroupIds(-1, groupIds, lastStatusChangeStartTime, lastStatusChangeEndTime));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderService.GetDeviceThreatStatusByGroupIds(0, groupIds, lastStatusChangeStartTime, lastStatusChangeEndTime));
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetDeviceThreatStatusByGroupIds(123123, null, lastStatusChangeStartTime, lastStatusChangeEndTime));
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetDeviceThreatStatusByGroupIds(123123, Enumerable.Empty<int>(), lastStatusChangeStartTime, lastStatusChangeEndTime));
    }

    [Test]
    public void GetAllAntivirusThreatStatus_Throws_Exception()
    {
        var groupIds = new[] { 1 };

        Assert.Throws<ArgumentNullException>(() => _windowsDefenderService.GetAllAntivirusThreatStatus(null, DateTime.UtcNow, DateTime.UtcNow, ActionFailedThreatStatus));
        var ex = Assert.Throws<ArgumentException>(() => _windowsDefenderService.GetAllAntivirusThreatStatus(groupIds, DateTime.UtcNow, DateTime.UtcNow.AddDays(-1), ActionFailedThreatStatus));
        Assert.AreEqual("Start date should not be greater than end date.", ex?.Message);
    }

    [Test]
    public void GetAllAntivirusThreatStatus_ValidInput_ReturnsData()
    {
        var deviceId = "1";
        var deviceGroupIds = new[] { 1 };
        var lastStatusChangeStartTime = DateTime.Now.AddDays(-5);
        var lastStatusChangeEndTime = DateTime.Now;

        var expectedData = new List<AntivirusThreatStatusDeviceData>
        {
            new()
            {
                DevId = deviceId,
                LastStatusChangeTime = DateTime.Now.AddDays(-2),
                ExternalThreatId = 1,
                TypeId = (int)AntivirusThreatType.Adware,
                SeverityId = (int)AntivirusThreatSeverity.Moderate,
            }
        };

        _windowsDefenderProviderMock
            .Setup(x => x.GetDeviceDataByAntivirusThreatStatus(
                It.IsAny<IEnumerable<int>>(),
                DeviceFamily.WindowsPhone,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                ActionFailedThreatStatus))
            .Returns(expectedData.AsEnumerable());

        var result = _windowsDefenderService.GetAllAntivirusThreatStatus(deviceGroupIds, lastStatusChangeStartTime, lastStatusChangeEndTime, ActionFailedThreatStatus);

        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());

        _windowsDefenderProviderMock.Verify(x => x.GetDeviceDataByAntivirusThreatStatus(
            It.IsAny<IEnumerable<int>>(),
            DeviceFamily.WindowsPhone,
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            ActionFailedThreatStatus));
    }

    [Test]
    public void DeleteWindowsDefenderData_Throws_ArgumentOutOfRangeException_WhenDeviceIdIsInvalid()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDefenderService.DeleteWindowsDefenderData(default));
        Assert.AreEqual("deviceId", ex?.ParamName);
    }

    [Test]
    public void DeleteWindowsDefenderData_Test()
    {
        _windowsDeviceServiceMock.Setup(x => x.UpdateDefenderScanInfo(DeviceId, null, null, null));
        _windowsDefenderProviderMock.Setup(x => x.DeleteDeviceAntivirusThreatData(DeviceId));
        _programTraceMock.Setup(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()));
        _eventDispatcherMock.Setup(x => x.DispatchEvent(It.IsAny<IEvent>()));
        _deviceIdentityMapperMock.Setup(x => x.GetStringDeviceId(It.IsAny<int>())).Returns(StringDeviceId);

        _windowsDefenderService.DeleteWindowsDefenderData(DeviceId);

        _windowsDefenderProviderMock.Verify(x => x.DeleteDeviceAntivirusThreatData(DeviceId), Times.Once);
        _windowsDeviceServiceMock.Verify(x => x.UpdateDefenderScanInfo(DeviceId, null, null, null), Times.Once);
        _programTraceMock.Verify(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _eventDispatcherMock.Verify(x => x.DispatchEvent(It.IsAny<IEvent>()), Times.Once);
        _deviceIdentityMapperMock.Verify(x => x.GetStringDeviceId(It.IsAny<int>()), Times.Once);
    }

    [Test]
    public void DeleteDeviceAntivirusThreatData_Throws_ArgumentOutOfRangeException_WhenDeviceIdIsInvalid()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDefenderService.DeleteDeviceAntivirusThreatData(0));
        Assert.AreEqual("deviceId", ex?.ParamName);
    }

    [Test]
    public void DeleteDeviceAntivirusThreatData_Test()
    {
        _windowsDefenderProviderMock.Setup(x => x.DeleteDeviceAntivirusThreatData(DeviceId));
        _programTraceMock.Setup(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()));

        _windowsDefenderService.DeleteDeviceAntivirusThreatData(DeviceId);

        _windowsDefenderProviderMock.Verify(x => x.DeleteDeviceAntivirusThreatData(DeviceId), Times.Once);
        _programTraceMock.Verify(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}

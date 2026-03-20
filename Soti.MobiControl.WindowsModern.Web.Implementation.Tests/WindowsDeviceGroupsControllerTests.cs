using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Soti.MobiControl.WebApi.Foundation.Mvc;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Soti.Api.Metadata.DataRetrieval;
using Soti.MobiControl.DeviceGroupManagement;
using Soti.MobiControl.Security.Authorization;
using Soti.MobiControl.Security.Authorization.Permissions;
using Soti.MobiControl.Security.Management.Services;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.MobiControl.WindowsModern.Web.Enums;
using Soti.MobiControl.WindowsModern.Web.Implementation.Exceptions;
using AntivirusScanCounts = Soti.MobiControl.WindowsModern.Models.AntivirusScanCounts;
using AntivirusGroupScanSummary = Soti.MobiControl.WindowsModern.Models.AntivirusGroupScanSummary;
using AntivirusScanPeriod = Soti.MobiControl.WindowsModern.Web.Enums.AntivirusScanPeriod;
using AntivirusThreatSeverity = Soti.MobiControl.WindowsModern.Models.Enums.AntivirusThreatSeverity;
using AntivirusThreatStatus = Soti.MobiControl.WindowsModern.Models.Enums.AntivirusThreatStatus;
using AntivirusLastScanDeviceSummaryModel = Soti.MobiControl.WindowsModern.Models.AntivirusLastScanDeviceSummary;
using AntivirusScanTypeModel = Soti.MobiControl.WindowsModern.Models.Enums.AntivirusScanType;
using AntivirusScanPeriodSubTypeModel = Soti.MobiControl.WindowsModern.Models.Enums.AntivirusScanPeriodSubType;
using DeviceThreatDetails = Soti.MobiControl.WindowsModern.Models.DeviceThreatDetails;
using ICsvConverter = Soti.Csv.ICsvConverter;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests;

[TestFixture]
internal sealed class WindowsDeviceGroupsControllerTests
{
    private const string GroupPath = "groupPath";
    private const int GroupId = 1;
    private const long ThreatId = 123123;
    private const string ApplicationCsv = "application/csv";
    private static readonly DateTime DeviceGroupSyncCompletedOn = DateTime.UtcNow;

    private Mock<IAccessControlManager> _accessControlManagerMock;
    private Mock<IDeviceGroupIdentityMapper> _deviceGroupIdentityMapperMock;
    private Mock<IWindowsDeviceGroupsService> _windowsDeviceGroupsServiceMock;
    private Mock<IAccessibleDeviceGroupService> _accessibleDeviceGroupServiceMock;
    private Mock<IWindowsDefenderService> _windowsDefenderServiceMock;
    private Mock<IWindowsDeviceLocalGroupsService> _windowsDeviceLocalGroupsServiceMock;
    private Mock<ICsvConverter> _csvConverterMock;

    private WindowsDeviceGroupsController _controller;

    [SetUp]
    public void Setup()
    {
        _accessControlManagerMock = new Mock<IAccessControlManager>(MockBehavior.Strict);
        _deviceGroupIdentityMapperMock = new Mock<IDeviceGroupIdentityMapper>();
        _windowsDeviceGroupsServiceMock = new Mock<IWindowsDeviceGroupsService>();
        _accessibleDeviceGroupServiceMock = new Mock<IAccessibleDeviceGroupService>();
        _windowsDefenderServiceMock = new Mock<IWindowsDefenderService>();
        _windowsDeviceLocalGroupsServiceMock = new Mock<IWindowsDeviceLocalGroupsService>(MockBehavior.Strict);
        _csvConverterMock = new Mock<ICsvConverter>();
        _controller = new WindowsDeviceGroupsController(
            new Lazy<IDeviceGroupIdentityMapper>(() => _deviceGroupIdentityMapperMock.Object),
            _accessControlManagerMock.Object,
            _windowsDeviceGroupsServiceMock.Object,
            _windowsDefenderServiceMock.Object,
            new Lazy<IAccessibleDeviceGroupService>(() => _accessibleDeviceGroupServiceMock.Object),
            new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object),
            new Lazy<ICsvConverter>(() => _csvConverterMock.Object)
        )
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public void Constructor_Throws_Exception()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDeviceGroupsController(
                null,
                _accessControlManagerMock.Object,
                _windowsDeviceGroupsServiceMock.Object,
                _windowsDefenderServiceMock.Object,
                new Lazy<IAccessibleDeviceGroupService>(() => _accessibleDeviceGroupServiceMock.Object),
                new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object),
                new Lazy<ICsvConverter>(() => _csvConverterMock.Object)));

        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDeviceGroupsController(
                new Lazy<IDeviceGroupIdentityMapper>(() => _deviceGroupIdentityMapperMock.Object),
                null,
                _windowsDeviceGroupsServiceMock.Object,
                _windowsDefenderServiceMock.Object,
                new Lazy<IAccessibleDeviceGroupService>(() => _accessibleDeviceGroupServiceMock.Object),
                new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object),
                new Lazy<ICsvConverter>(() => _csvConverterMock.Object)));

        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDeviceGroupsController(
                new Lazy<IDeviceGroupIdentityMapper>(() => _deviceGroupIdentityMapperMock.Object),
                _accessControlManagerMock.Object,
                null,
                _windowsDefenderServiceMock.Object,
                new Lazy<IAccessibleDeviceGroupService>(() => _accessibleDeviceGroupServiceMock.Object),
                new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object),
                new Lazy<ICsvConverter>(() => _csvConverterMock.Object)));

        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDeviceGroupsController(
                new Lazy<IDeviceGroupIdentityMapper>(() => _deviceGroupIdentityMapperMock.Object),
                _accessControlManagerMock.Object,
                _windowsDeviceGroupsServiceMock.Object,
                null,
                new Lazy<IAccessibleDeviceGroupService>(() => _accessibleDeviceGroupServiceMock.Object),
                new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object),
                new Lazy<ICsvConverter>(() => _csvConverterMock.Object)));

        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDeviceGroupsController(
                new Lazy<IDeviceGroupIdentityMapper>(() => _deviceGroupIdentityMapperMock.Object),
                _accessControlManagerMock.Object,
                _windowsDeviceGroupsServiceMock.Object,
                _windowsDefenderServiceMock.Object,
                null,
                new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object),
                new Lazy<ICsvConverter>(() => _csvConverterMock.Object)));

        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDeviceGroupsController(
                new Lazy<IDeviceGroupIdentityMapper>(() => _deviceGroupIdentityMapperMock.Object),
                _accessControlManagerMock.Object,
                _windowsDeviceGroupsServiceMock.Object,
                _windowsDefenderServiceMock.Object,
                new Lazy<IAccessibleDeviceGroupService>(() => _accessibleDeviceGroupServiceMock.Object),
                null,
                new Lazy<ICsvConverter>(() => _csvConverterMock.Object)));

        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDeviceGroupsController(
                new Lazy<IDeviceGroupIdentityMapper>(() => _deviceGroupIdentityMapperMock.Object),
                _accessControlManagerMock.Object,
                _windowsDeviceGroupsServiceMock.Object,
                _windowsDefenderServiceMock.Object,
                new Lazy<IAccessibleDeviceGroupService>(() => _accessibleDeviceGroupServiceMock.Object),
                new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object),
                null));
    }

    [Test]
    public void GetAntivirusLastSyncedStatus_Tests()
    {
        var endTime = new DateTime(2021, 3, 11);
        var lastSyncSummaryModel = new DeviceGroupSyncInfo
        {
            SyncStatus = SyncRequestStatus.Running,
            CompletedOn = endTime
        };
        EnsureDeviceGroupRights();
        _windowsDeviceGroupsServiceMock.Setup(m => m.GetGroupSyncStatus(It.IsAny<int>(), It.IsAny<SyncRequestType>())).Returns(lastSyncSummaryModel);

        var result = _controller.GetAntivirusLastSyncedStatus(GroupPath);

        Assert.IsNotNull(result);
        Assert.AreEqual(endTime, result.LastSyncedOn);
        Assert.IsTrue(result.IsSyncInProgress);

        _windowsDeviceGroupsServiceMock.Verify(m => m.GetGroupSyncStatus(It.IsAny<int>(), It.IsAny<SyncRequestType>()), Times.Once);
        _deviceGroupIdentityMapperMock.Verify(v => v.GetId(It.IsAny<string>()), Times.Once);
        _accessControlManagerMock.Verify(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.DeviceGroup, 1), Times.Once);
    }

    [Test]
    public void GetAntivirusLastSyncedStatus_Throws_Exception()
    {
        var path = string.Empty;
        var ex = Assert.Throws<ValidationException>(() => _controller.GetAntivirusLastSyncedStatus(path));
        Assert.That(ex?.Message, Is.EqualTo("path"));
    }

    [Test]
    public void GetAntivirusThreatStatusCount_Success()
    {
        var startTime = new DateTime(2021, 3, 10, 10, 0, 0, DateTimeKind.Utc);
        var endTime = new DateTime(2021, 3, 11, 10, 0, 0, DateTimeKind.Utc);
        var groupIds = new List<int> { 1, 2, 3 };

        var threatStatusCount = new Dictionary<AntivirusThreatStatus, int>()
        {
            { AntivirusThreatStatus.Active, 1 },
            { AntivirusThreatStatus.ActionFailed, 0 },
            { AntivirusThreatStatus.ManualStepsRequired, 1 },
            { AntivirusThreatStatus.FullScanRequired, 1 },
            { AntivirusThreatStatus.RebootRequired, 0 },
            { AntivirusThreatStatus.RemediatedWithNonCriticalFailures, 0 },
            { AntivirusThreatStatus.Quarantined, 1 },
            { AntivirusThreatStatus.Removed, 1 },
            { AntivirusThreatStatus.Cleaned, 0 },
            { AntivirusThreatStatus.Allowed, 0 },
            { AntivirusThreatStatus.NoStatusCleared, 1 }
        };

        EnsureDeviceGroupRights();
        EnsureSyncCompleted();
        _accessibleDeviceGroupServiceMock.Setup(s => s.GetPermittedDeviceGroupIds(It.IsAny<string>(), true)).Returns(new HashSet<int> { 1, 2, 3 });
        _windowsDefenderServiceMock.Setup(m => m.GetAntivirusThreatStatusCount(groupIds, startTime, endTime)).Returns(threatStatusCount);

        var result = _controller.GetAntivirusThreatStatusCount(GroupPath, startTime, endTime);

        Assert.IsNotNull(result);
        Assert.AreEqual(threatStatusCount[AntivirusThreatStatus.Active], result[AntivirusThreatStatus.Active.ToString()]);
        Assert.AreEqual(threatStatusCount[AntivirusThreatStatus.ActionFailed], result[AntivirusThreatStatus.ActionFailed.ToString()]);
        Assert.AreEqual(threatStatusCount[AntivirusThreatStatus.ManualStepsRequired], result[AntivirusThreatStatus.ManualStepsRequired.ToString()]);
        Assert.AreEqual(threatStatusCount[AntivirusThreatStatus.FullScanRequired], result[AntivirusThreatStatus.FullScanRequired.ToString()]);
        Assert.AreEqual(threatStatusCount[AntivirusThreatStatus.RebootRequired], result[AntivirusThreatStatus.RebootRequired.ToString()]);
        Assert.AreEqual(threatStatusCount[AntivirusThreatStatus.RemediatedWithNonCriticalFailures], result[AntivirusThreatStatus.RemediatedWithNonCriticalFailures.ToString()]);
        Assert.AreEqual(threatStatusCount[AntivirusThreatStatus.Quarantined], result[AntivirusThreatStatus.Quarantined.ToString()]);
        Assert.AreEqual(threatStatusCount[AntivirusThreatStatus.Removed], result[AntivirusThreatStatus.Removed.ToString()]);
        Assert.AreEqual(threatStatusCount[AntivirusThreatStatus.Cleaned], result[AntivirusThreatStatus.Cleaned.ToString()]);
        Assert.AreEqual(threatStatusCount[AntivirusThreatStatus.Allowed], result[AntivirusThreatStatus.Allowed.ToString()]);
        Assert.AreEqual(threatStatusCount[AntivirusThreatStatus.NoStatusCleared], result[AntivirusThreatStatus.NoStatusCleared.ToString()]);

        _windowsDefenderServiceMock.Verify(a => a.GetAntivirusThreatStatusCount(groupIds, startTime, endTime), Times.Once);
    }

    [Test]
    public void GetAntivirusThreatStatusIdCount_Throws_PathException()
    {
        var path = "";
        var ex = Assert.Throws<ValidationException>(() => _controller.GetAntivirusThreatStatusCount(path, new DateTime(2021, 3, 10), new DateTime(2021, 3, 11)));
        Assert.That(ex.Message, Is.EqualTo("path"));
    }

    [Test]
    public void GetDeviceGroupAntivirusThreatHistory_Throws_Exception()
    {
        var ex = Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupsAntivirusThreatHistory(null));
        Assert.That(ex.Message, Is.EqualTo("path"));
    }

    [Test]
    public void GetDeviceGroupAntivirusThreatHistory_InvalidSeverity_Throws_Exception()
    {
        var invalidSeverity = new List<Enums.AntivirusThreatSeverity> { (Enums.AntivirusThreatSeverity)1007 };

        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Skip = 0,
            Take = 10
        };

        var exception = Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupsAntivirusThreatHistory(
            GroupPath,
            DateTime.Now,
            DateTime.Now.AddDays(-5),
            new List<AntivirusThreatCategory>(),
            invalidSeverity,
            dataRetrievalOptions));

        Assert.AreEqual("One or more values in the 'severity' parameter are invalid.", exception.Message);
    }

    [Test]
    public void GetDeviceGroupAntivirusThreatHistory_InvalidSortBy_Throws_Exception()
    {
        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Skip = 0,
            Take = 10,
            Order = new[]
            {
                new DataRetrievalOrder
                {
                    By = "TestField",
                    Descending = true
                }
            }
        };

        var exception = Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupsAntivirusThreatHistory(
            GroupPath,
            DateTime.Now.AddDays(-5),
            DateTime.Now,
            new List<AntivirusThreatCategory>(),
            new List<Enums.AntivirusThreatSeverity>(),
            dataRetrievalOptions));

        Assert.AreEqual("Invalid sort value: 'TestField'.", exception.Message);
    }

    [Test]
    public void GetAntivirusThreatHistory_DataRetrievalOption_Skip_Throws_Exception()
    {
        var endDateTime = DateTime.Now;
        var startDateTime = DateTime.Now.AddDays(-5);

        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Skip = -1,
            Take = 10
        };

        var ex = Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupsAntivirusThreatHistory(
            GroupPath,
            startDateTime,
            endDateTime,
            new List<AntivirusThreatCategory>(),
            new List<Enums.AntivirusThreatSeverity>(),
            dataRetrievalOptions));

        Assert.AreEqual("Skip value should be greater than or equal to zero.", ex.Message);
        dataRetrievalOptions = new DataRetrievalOptions
        {
            Order = new DataRetrievalOrder[]
            {
                new DataRetrievalOrder { By = "LastStatusChangeTime", Descending = true },
                new DataRetrievalOrder { By = "LastStatusChangeTime", Descending = true }
            },
            Skip = 0,
            Take = 10
        };

        Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupsAntivirusThreatHistory(
            GroupPath,
            startDateTime,
            endDateTime,
            new List<AntivirusThreatCategory>(),
            new List<Enums.AntivirusThreatSeverity>(),
            dataRetrievalOptions));
    }

    [Test]
    public void GetAntivirusThreatHistory_DataRetrievalOption_Take_Throws_Exception()
    {
        var endDateTime = DateTime.Now;
        var startDateTime = DateTime.Now.AddDays(-5);

        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Skip = 0,
            Take = -1
        };

        var ex = Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupsAntivirusThreatHistory(
            GroupPath,
            startDateTime,
            endDateTime,
            new List<AntivirusThreatCategory>(),
            new List<Enums.AntivirusThreatSeverity>(),
            dataRetrievalOptions));

        Assert.AreEqual("Take value should be greater than zero", ex.Message);
    }

    [Test]
    public void GetDeviceGroupsAntivirusScanSummary_PathThrowsException()
    {
        var ex = Assert.Throws<ValidationException>(() =>
            _controller.GetDeviceGroupsAntivirusScanSummary(
                null,
                AntivirusScanPeriod.Custom,
                DateTime.Now,
                DateTime.Now.AddDays(-5)));

        Assert.AreEqual("path", ex.Message);
    }

    [Test]
    public void GetDeviceGroupsAntivirusScanSummary_ScanPeriodThrowsException()
    {
        var ex = Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupsAntivirusScanSummary(GroupPath, (AntivirusScanPeriod)100, DateTime.Now.AddDays(-5), DateTime.Now));
        Assert.AreEqual("The 'scanPeriod' parameter is invalid.", ex.Message);
    }

    [Test]
    public void GetDeviceGroupsCustomAntivirusScanSummary_ScanPeriodThrowsException()
    {
        EnsureDeviceGroupRights();
        var ex = Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupsAntivirusScanSummary(GroupPath, AntivirusScanPeriod.Custom, null, DateTime.Now));
        Assert.AreEqual("Start time or end time cannot be null.", ex.Message);
    }

    [Test]
    public void GetDeviceGroupsCustomAntivirusScanSummaryEndDate_ScanPeriodThrowsException()
    {
        EnsureDeviceGroupRights();
        var ex = Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupsAntivirusScanSummary(GroupPath, AntivirusScanPeriod.Custom, DateTime.Now, null));
        Assert.AreEqual("Start time or end time cannot be null.", ex.Message);
    }

    [Test]
    public void GetDeviceGroupsAntivirusScanSummary_SyncInProgressThrowsException()
    {
        var lastSyncSummaryModel = new DeviceGroupSyncInfo
        {
            SyncStatus = SyncRequestStatus.Running,
            CompletedOn = new DateTime(2024, 9, 18),
        };

        EnsureDeviceGroupRights();
        _windowsDeviceGroupsServiceMock.Setup(m => m.GetGroupSyncStatus(It.IsAny<int>(), It.IsAny<SyncRequestType>())).Returns(lastSyncSummaryModel);

        var ex = Assert.Throws<WindowsModernWebException>(() => _controller.GetDeviceGroupsAntivirusScanSummary(GroupPath, AntivirusScanPeriod.Default, new DateTime(2024, 7, 10), DateTime.Now));
        Assert.AreEqual("Sync is in progress. Try again later", ex.Message);

        _deviceGroupIdentityMapperMock.Verify(m => m.GetId(GroupPath), Times.Once);
        _windowsDeviceGroupsServiceMock.Verify(m => m.GetGroupSyncStatus(It.IsAny<int>(), It.IsAny<SyncRequestType>()), Times.Once);
    }

    [Test]
    public void GetDeviceGroupsAntivirusScanSummary_Test()
    {
        var groupIds = new List<int>() { 1001, 1002, 1003 };
        var endDateTime = DateTime.Now;
        var startDateTime = new DateTime(2024, 7, 10);

        var lastSyncSummaryModel = new DeviceGroupSyncInfo
        {
            SyncStatus = SyncRequestStatus.PartiallyCompleted,
            CompletedOn = new DateTime(2024, 9, 18),
        };

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

        EnsureDeviceGroupRights();
        _accessibleDeviceGroupServiceMock
            .Setup(s => s.GetPermittedDeviceGroupIds(It.IsAny<string>(), true))
            .Returns(new HashSet<int> { 1001, 1002, 1003 });
        _windowsDeviceGroupsServiceMock.Setup(m => m.GetGroupSyncStatus(It.IsAny<int>(), It.IsAny<SyncRequestType>())).Returns(lastSyncSummaryModel);
        _windowsDefenderServiceMock.Setup(m => m.GetDeviceGroupsDefaultAntivirusScanSummary(groupIds, It.IsAny<DateTime>())).Returns(data);

        var result = _controller.GetDeviceGroupsAntivirusScanSummary(GroupPath, AntivirusScanPeriod.Default, startDateTime, endDateTime);
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Within7Days.FullScanned);
        Assert.AreEqual(1, result.Within7Days.QuickScanned);
        Assert.AreEqual(1, result.Within24Hrs.FullScanned);
        Assert.AreEqual(2, result.Within24Hrs.QuickScanned);
        Assert.AreEqual(1, result.MoreThan30Days.QuickScanned);
        Assert.AreEqual(3, result.MoreThan30Days.FullScanned);

        _accessibleDeviceGroupServiceMock.Verify(x => x.GetPermittedDeviceGroupIds(It.IsAny<string>(), true), Times.Once);
        _deviceGroupIdentityMapperMock.Verify(m => m.GetId(GroupPath), Times.Once);
        _windowsDeviceGroupsServiceMock.Verify(m => m.GetGroupSyncStatus(It.IsAny<int>(), It.IsAny<SyncRequestType>()), Times.Once);
        _windowsDefenderServiceMock.Verify(m => m.GetDeviceGroupsDefaultAntivirusScanSummary(groupIds, It.IsAny<DateTime>()), Times.Once);
    }

    [Test]
    public void GetDeviceGroupsCustomAntivirusScanSummary_Test()
    {
        var groupIds = new List<int>() { 1001, 1002, 1003 };
        var endDateTime = DateTime.Now;
        var startDateTime = new DateTime(2024, 07, 10);

        var data = new AntivirusGroupScanSummary()
        {
            Custom = new AntivirusScanCounts()
            {
                FullScanned = 3,
                QuickScanned = 1
            },
        };

        EnsureDeviceGroupRights();
        _accessibleDeviceGroupServiceMock
            .Setup(s => s.GetPermittedDeviceGroupIds(It.IsAny<string>(), true))
            .Returns(new HashSet<int> { 1001, 1002, 1003 });
        _windowsDefenderServiceMock.Setup(m => m.GetDeviceGroupsCustomAntivirusScanSummary(groupIds, It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(data);

        var result = _controller.GetDeviceGroupsAntivirusScanSummary(GroupPath, AntivirusScanPeriod.Custom, startDateTime, endDateTime);
        Assert.IsNotNull(result);
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Custom.FullScanned);
        Assert.AreEqual(1, result.Custom.QuickScanned);
        Assert.IsNull(result.Within7Days);

        _accessibleDeviceGroupServiceMock.Verify(x => x.GetPermittedDeviceGroupIds(It.IsAny<string>(), true), Times.Once);
        _windowsDefenderServiceMock.Verify(m => m.GetDeviceGroupsCustomAntivirusScanSummary(groupIds, It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
    }

    [Test]
    public void GetDeviceGroupAntivirusThreatHistory_NullInput_ReturnsCorrectResult()
    {
        var totalCount = 1;

        var data = new AntivirusGroupThreatInfo()
        {
            Type = AntivirusThreatType.ASRRule,
            InitialDetectionTime = new DateTime(2024, 10, 1, 10, 0, 0),
            LastStatusChangeTime = new DateTime(2024, 10, 2),
            NoOfDevices = 3,
            Severity = AntivirusThreatSeverity.High,
            ExternalThreatId = 82012
        };

        EnsureDeviceGroupRights();
        EnsureSyncCompleted();
        _accessibleDeviceGroupServiceMock
            .Setup(s => s.GetPermittedDeviceGroupIds(It.IsAny<string>(), true))
            .Returns(new HashSet<int> { 1001, 1002, 1003 });

        _windowsDefenderServiceMock
            .Setup(w => w.GetDeviceGroupsAntivirusThreatHistory(
                new List<int> { 1001, 1002, 1003 },
                It.IsAny<IEnumerable<AntivirusThreatType>>(),
                It.IsAny<IEnumerable<AntivirusThreatSeverity>>(),
                null,
                null,
                0,
                50,
                AntivirusThreatHistorySortByOption.LastStatusChangeTime,
                true,
                out totalCount))
            .Returns(new List<AntivirusGroupThreatInfo>() { data });

        var actual = _controller.GetDeviceGroupsAntivirusThreatHistory(
            GroupPath,
            null,
            null,
            null,
            null
        );

        var result = actual.FirstOrDefault();
        Assert.IsNotNull(result);
        Assert.AreEqual(result.Category, (AntivirusThreatCategory)data.Type);
        Assert.AreEqual(result.InitialDetectionTime, data.InitialDetectionTime);
        Assert.AreEqual(result.LastStatusChangeTime, data.LastStatusChangeTime);
        Assert.AreEqual(result.Severity, (Enums.AntivirusThreatSeverity)data.Severity);
        Assert.AreEqual(result.NoOfDevices, data.NoOfDevices);
        Assert.AreEqual(result.ThreatId, data.ExternalThreatId);

        _accessibleDeviceGroupServiceMock.Verify(x => x.GetPermittedDeviceGroupIds(It.IsAny<string>(), true), Times.Once);

        _windowsDefenderServiceMock
            .Verify(w => w.GetDeviceGroupsAntivirusThreatHistory(
                new List<int> { 1001, 1002, 1003 },
                It.IsAny<IEnumerable<AntivirusThreatType>>(),
                It.IsAny<IEnumerable<AntivirusThreatSeverity>>(),
                null,
                null,
                0,
                50,
                AntivirusThreatHistorySortByOption.LastStatusChangeTime,
                true,
                out totalCount), Times.Once);
    }

    [Test]
    public void GetDeviceGroupAntivirusThreatHistory_InvalidCategory_Throws_Exception()
    {
        var invalidCategories = new List<AntivirusThreatCategory> { (AntivirusThreatCategory)2801 };
        var endDateTime = DateTime.Now;
        var startDateTime = DateTime.Now.AddDays(-5);

        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Skip = 0,
            Take = 10
        };

        var exception = Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupsAntivirusThreatHistory(
            GroupPath,
            startDateTime,
            endDateTime,
            invalidCategories,
            new List<Enums.AntivirusThreatSeverity>(),
            dataRetrievalOptions));

        Assert.AreEqual("One or more values in the 'category' parameter are invalid.", exception.Message);
    }

    [Test]
    public void GetDeviceGroupAntivirusThreatHistory_Tests()
    {
        var total = 0;
        var data = new AntivirusGroupThreatInfo()
        {
            Type = AntivirusThreatType.ASRRule,
            InitialDetectionTime = new DateTime(2024, 10, 1, 10, 0, 0),
            LastStatusChangeTime = new DateTime(2024, 10, 2),
            NoOfDevices = 3,
            Severity = AntivirusThreatSeverity.High,
            ExternalThreatId = 82012
        };

        var startDateTime = new DateTime(2024, 10, 1, 10, 0, 0);
        var endDateTime = new DateTime(2024, 10, 2, 10, 0, 0);

        EnsureDeviceGroupRights();
        EnsureSyncCompleted();
        _accessibleDeviceGroupServiceMock
            .Setup(s => s.GetPermittedDeviceGroupIds(It.IsAny<string>(), true))
            .Returns(new HashSet<int> { 1001, 1002, 1003 });

        _windowsDefenderServiceMock
            .Setup(w => w.GetDeviceGroupsAntivirusThreatHistory(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<IEnumerable<AntivirusThreatType>>(),
                It.IsAny<IEnumerable<AntivirusThreatSeverity>>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<int>(), AntivirusThreatHistorySortByOption.LastStatusChangeTime,
                true,
                out total))
            .Returns(new List<AntivirusGroupThreatInfo>() { data });

        var category = new List<AntivirusThreatCategory>() { AntivirusThreatCategory.ASRRule };
        var severities = new List<Enums.AntivirusThreatSeverity>() { Enums.AntivirusThreatSeverity.High };
        var actual = _controller.GetDeviceGroupsAntivirusThreatHistory(GroupPath, startDateTime, endDateTime, category, severities, null);

        var result = actual.FirstOrDefault();
        Assert.IsNotNull(result);
        Assert.AreEqual(result.Category, (AntivirusThreatCategory)data.Type);
        Assert.AreEqual(result.InitialDetectionTime, data.InitialDetectionTime);
        Assert.AreEqual(result.LastStatusChangeTime, data.LastStatusChangeTime);
        Assert.AreEqual(result.Severity, (Enums.AntivirusThreatSeverity)data.Severity);
        Assert.AreEqual(result.NoOfDevices, data.NoOfDevices);
        Assert.AreEqual(result.ThreatId, data.ExternalThreatId);

        _accessibleDeviceGroupServiceMock.Verify(x => x.GetPermittedDeviceGroupIds(It.IsAny<string>(), true), Times.Once);

        _windowsDefenderServiceMock
            .Verify(w => w.GetDeviceGroupsAntivirusThreatHistory(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<IEnumerable<AntivirusThreatType>>(),
                It.IsAny<IEnumerable<AntivirusThreatSeverity>>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<int>(), AntivirusThreatHistorySortByOption.LastStatusChangeTime,
                true,
                out total), Times.Once);
    }

    [Test]
    public void GetDeviceGroupThreatDetails_SortMultipleFields_Throws_Exception()
    {
        var startDateTime = new DateTime(2024, 10, 1, 10, 0, 0);
        var endDateTime = new DateTime(2024, 10, 2, 10, 0, 0);

        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Order = new DataRetrievalOrder[]
            {
                new DataRetrievalOrder { By = "LastStatusChangeTime", Descending = true },
                new DataRetrievalOrder { By = "LastStatusChangeTime", Descending = true }
            },
            Skip = 0,
            Take = 10
        };
        var ex = Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupThreatDetails(GroupPath, ThreatId, startDateTime, endDateTime, dataRetrievalOptions).ToList());
        Assert.AreEqual("Sorting by multiple fields is not supported.", ex.Message);
    }

    [Test]
    public void GetDeviceGroupThreatDetails_Success()
    {
        var startDateTime = new DateTime(2024, 10, 1, 10, 0, 0, DateTimeKind.Utc);
        var endDateTime = new DateTime(2024, 10, 2, 10, 0, 0, DateTimeKind.Utc);

        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Order = new DataRetrievalOrder[]
            {
                new DataRetrievalOrder { By = "InitialDetectionTime", Descending = true }
            },
            Skip = 0,
            Take = 10
        };

        var groupIds = new List<int> { 1001, 1002, 1003 };
        _accessibleDeviceGroupServiceMock
            .Setup(s => s.GetPermittedDeviceGroupIds(It.IsAny<string>(), true))
            .Returns(groupIds.ToHashSet());

        var threatDetails = new List<DeviceThreatDetails>
        {
            new() { DevId = "dev1", InitialDetectionTime = DateTime.Now, ThreatStatus = AntivirusThreatStatus.Quarantined },
            new() { DevId = "dev2", InitialDetectionTime = DateTime.Now, ThreatStatus = AntivirusThreatStatus.Cleaned }
        };

        EnsureDeviceGroupRights();
        EnsureSyncCompleted();
        _windowsDefenderServiceMock
            .Setup(s => s.GetDeviceThreatStatusByGroupIdsPaginated(ThreatId, groupIds, startDateTime, endDateTime, 0, 10, AntivirusThreatHistoryDetailsSortByOption.InitialDetectionTime, true, out It.Ref<int>.IsAny))
            .Returns(threatDetails);

        var result = _controller.GetDeviceGroupThreatDetails(GroupPath, ThreatId, startDateTime, endDateTime, dataRetrievalOptions).ToList();

        Assert.AreEqual(threatDetails.Count, result.Count);
        Assert.AreEqual("dev1", result[0].DeviceId);
        Assert.AreEqual("dev2", result[1].DeviceId);

        _accessibleDeviceGroupServiceMock.Verify(s => s.GetPermittedDeviceGroupIds(GroupPath, true), Times.Once);
        _windowsDefenderServiceMock.Verify(s => s.GetDeviceThreatStatusByGroupIdsPaginated(ThreatId, groupIds, startDateTime, endDateTime, It.IsAny<int>(), It.IsAny<int>(), AntivirusThreatHistoryDetailsSortByOption.InitialDetectionTime, true, out It.Ref<int>.IsAny));
    }

    [TestCase("")]
    [TestCase("  ")]
    [TestCase(null)]
    public void GetDeviceGroupThreatDetails_Invalid_Path_ThrowsException(string path)
    {
        var exception = Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupThreatDetails(path, 123));
        Assert.That(exception?.Message, Is.EqualTo("path"));
    }

    [Test]
    public void GetDeviceGroupThreatDetails_Invalid_ThreatId_ThrowsException()
    {
        var exception = Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupThreatDetails(GroupPath, -1));
        Assert.That(exception?.Message, Is.EqualTo("threatId"));
    }

    [Test]
    public void ExportDeviceGroupThreatDetails_Success()
    {
        var startDateTime = new DateTime(2024, 10, 1, 10, 0, 0, DateTimeKind.Utc);
        var endDateTime = new DateTime(2024, 10, 2, 10, 0, 0, DateTimeKind.Utc);
        var dummyData = "Dummy Data";
        var groupIds = new List<int> { 1001, 1002, 1003 };
        var threatDetails = new List<DeviceThreatDetails>
        {
            new() { DevId = "dev1", InitialDetectionTime = DateTime.Now, LastStatusChangeTime = DateTime.Now, ThreatStatus = AntivirusThreatStatus.Quarantined },
            new() { DevId = "dev2", InitialDetectionTime = DateTime.Now, LastStatusChangeTime = DateTime.Now, ThreatStatus = AntivirusThreatStatus.Cleaned }
        };

        EnsureDeviceGroupRights();
        EnsureSyncCompleted();
        _accessibleDeviceGroupServiceMock
            .Setup(s => s.GetPermittedDeviceGroupIds(It.IsAny<string>(), true))
            .Returns(groupIds.ToHashSet());
        _windowsDefenderServiceMock
            .Setup(s => s.GetDeviceThreatStatusByGroupIds(ThreatId, groupIds, startDateTime, endDateTime))
            .Returns(threatDetails);
        _csvConverterMock.Setup(c => c.GenerateCsvContent(
                It.IsAny<IEnumerable<IDictionary<string, string>>>(),
                It.IsAny<string[]>(),
                It.IsAny<Stream>(),
                It.IsAny<int>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<Func<string, string>>(),
                It.IsAny<bool>()))
            .Callback<IEnumerable<IDictionary<string, string>>, string[], Stream, int, CultureInfo, Func<string, string>, bool>(
                (records, headers, stream, offset, culture, localize, closeStream) =>
                {
                    using var writer = new StreamWriter(stream);
                    writer.WriteLine(localize(dummyData));
                    writer.FlushAsync();
                })
            .Returns(Task.CompletedTask);

        var result = _controller.ExportDeviceGroupThreatDetails(GroupPath, ThreatId, startDateTime, endDateTime);
        var pushStreamResult = result as PushStreamResult;
        Assert.NotNull(pushStreamResult);
        _accessibleDeviceGroupServiceMock.Verify(s => s.GetPermittedDeviceGroupIds(GroupPath, true), Times.Once);
        _windowsDefenderServiceMock.Verify(s => s.GetDeviceThreatStatusByGroupIds(ThreatId, groupIds, startDateTime, endDateTime), Times.Once);
    }

    [Test]
    public void ExportDeviceGroupThreatDetails_Invalid_ThreatId_ThrowsException()
    {
        Assert.Throws<ValidationException>(() => _controller.ExportDeviceGroupThreatDetails(GroupPath, -1));
    }

    [Test]
    public void ExportDeviceGroupsAntivirusThreatHistory_Success()
    {
        var dummyData = "Dummy Data";
        var initialDetectionTime = new DateTime(2024, 10, 1, 10, 0, 0);
        var lastStatusChangeTime = new DateTime(2024, 10, 2, 10, 0, 0);
        var timeZoneOffset = 20;
        var groupIds = new List<int> { 1001, 1002, 1003 };

        var expectedData = new List<AntivirusGroupThreatHistoryDetails>
        {
            new()
            {
                ExternalThreatId = 1,
                ThreatCategory = AntivirusThreatType.Spyware,
                Severity = AntivirusThreatSeverity.Severe,
                InitialDetectionTime = initialDetectionTime,
                LastStatusChangeTime = lastStatusChangeTime,
                NumberOfDevices = 2
            }
        };

        EnsureDeviceGroupRights();
        EnsureSyncCompleted();
        _windowsDefenderServiceMock
            .Setup(x => x.GetAntivirusThreatHistoryDetails(
                It.IsAny<IEnumerable<int>>()
            ))
            .Returns(expectedData);
        _accessibleDeviceGroupServiceMock
            .Setup(s => s.GetPermittedDeviceGroupIds(It.IsAny<string>(), true))
            .Returns(groupIds.ToHashSet());
        _csvConverterMock.Setup(c => c.GenerateCsvContent(
                It.IsAny<IEnumerable<IDictionary<string, string>>>(),
                It.IsAny<string[]>(),
                It.IsAny<Stream>(),
                It.IsAny<int>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<Func<string, string>>(),
                It.IsAny<bool>()))
            .Callback<IEnumerable<IDictionary<string, string>>, string[], Stream, int, CultureInfo, Func<string, string>, bool>(
                (records, headers, stream, offset, culture, localize, closeStream) =>
                {
                    using var writer = new StreamWriter(stream);
                    writer.WriteLine(localize(dummyData));
                    writer.FlushAsync();
                })
            .Returns(Task.CompletedTask);

        var result = _controller.ExportDeviceGroupsAntivirusThreatHistory(GroupPath, timeZoneOffset);
        var pushStreamResult = result as PushStreamResult;
        Assert.NotNull(pushStreamResult);
        using var memoryStream = new MemoryStream();
        pushStreamResult.WriteStreamAsync(memoryStream);
        Assert.AreEqual(ApplicationCsv, pushStreamResult.MediaType.ToString());
        Assert.IsTrue(pushStreamResult.FileDownloadName.Contains("DeviceGroup_ThreatHistory_"));
        _accessibleDeviceGroupServiceMock.Verify(x => x.GetPermittedDeviceGroupIds(It.IsAny<string>(), true), Times.Once);
        _windowsDefenderServiceMock.Verify(x => x.GetAntivirusThreatHistoryDetails(
            It.IsAny<IEnumerable<int>>()), Times.Once);
        _csvConverterMock.Verify(c => c.GenerateCsvContent(
            It.IsAny<IEnumerable<IDictionary<string, string>>>(),
            It.IsAny<string[]>(),
            It.IsAny<Stream>(),
            It.IsAny<int>(),
            It.IsAny<CultureInfo>(),
            It.IsAny<Func<string, string>>(),
            It.IsAny<bool>()), Times.Once);
    }

    [TestCase("")]
    [TestCase("  ")]
    [TestCase(null)]
    public void ExportDeviceGroupsAntivirusThreatHistory_Invalid_Path_ThrowsException(string path)
    {
        Assert.Throws<ValidationException>(() => _controller.ExportDeviceGroupsAntivirusThreatHistory(path, 1));
    }

    [Test]
    public void GetDeviceGroupsAntivirusLastScanSummary_ThrowsValidationException()
    {
        var startDateTime = new DateTime(2024, 10, 1, 10, 0, 0);
        var endDateTime = new DateTime(2024, 10, 2, 10, 0, 0);

        var dataRetrievalOptions = new DataRetrievalOptionsSkipTakeOnly { Skip = 0, Take = 10 };

        Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupsAntivirusLastScanSummary(
            GroupPath,
            (Enums.AntivirusScanPeriodSubType)255,
            Enums.AntivirusScanType.FullScan,
            startDateTime,
            endDateTime,
            true,
            dataRetrievalOptions));

        Assert.Throws<ValidationException>(() =>
            _controller.GetDeviceGroupsAntivirusLastScanSummary(GroupPath, Enums.AntivirusScanPeriodSubType.Custom, (Enums.AntivirusScanType)255, startDateTime, endDateTime, true,
                dataRetrievalOptions));
        Assert.Throws<ValidationException>(() =>
            _controller.GetDeviceGroupsAntivirusLastScanSummary("", Enums.AntivirusScanPeriodSubType.Custom, Enums.AntivirusScanType.FullScan, startDateTime, endDateTime, true, dataRetrievalOptions));

        startDateTime = DateTime.Now;
        endDateTime = DateTime.Now.AddDays(-1);

        Assert.Throws<ValidationException>(() =>
            _controller.GetDeviceGroupsAntivirusLastScanSummary(GroupPath, Enums.AntivirusScanPeriodSubType.Custom, Enums.AntivirusScanType.FullScan, startDateTime, endDateTime, true,
                dataRetrievalOptions));

        startDateTime = new DateTime(2024, 10, 1, 10, 0, 0);
        endDateTime = new DateTime(2024, 10, 2, 10, 0, 0);

        dataRetrievalOptions = new DataRetrievalOptionsSkipTakeOnly { Skip = -1, Take = 10 };
        Assert.Throws<ValidationException>(() =>
            _controller.GetDeviceGroupsAntivirusLastScanSummary(GroupPath, Enums.AntivirusScanPeriodSubType.Custom, Enums.AntivirusScanType.FullScan, startDateTime, endDateTime, true,
                dataRetrievalOptions));

        dataRetrievalOptions = new DataRetrievalOptionsSkipTakeOnly { Skip = 0, Take = 0 };
        Assert.Throws<ValidationException>(() =>
            _controller.GetDeviceGroupsAntivirusLastScanSummary(GroupPath, Enums.AntivirusScanPeriodSubType.Custom, Enums.AntivirusScanType.FullScan, startDateTime, endDateTime, true,
                dataRetrievalOptions));
    }

    [Test]
    public void GetDeviceGroupsAntivirusLastScanSummary_Tests()
    {
        var deviceId = Guid.NewGuid();
        var total = 0;
        var antivirusLastScanDeviceSummary = new AntivirusLastScanDeviceSummaryModel
        {
            DevId = deviceId.ToString(),
            LastScanDate = DateTime.UtcNow,
        };

        var dataRetrievalOptions = new DataRetrievalOptionsSkipTakeOnly { Skip = 0, Take = 10 };

        var startDateTime = new DateTime(2024, 10, 1, 10, 0, 0);
        var endDateTime = new DateTime(2024, 10, 2, 10, 0, 0);

        EnsureDeviceGroupRights();
        EnsureSyncCompleted();
        _accessibleDeviceGroupServiceMock
            .Setup(s => s.GetPermittedDeviceGroupIds(It.IsAny<string>(), true))
            .Returns(new HashSet<int> { 1, 2, 3 });

        _windowsDefenderServiceMock.Setup(m => m.GetDeviceGroupsAntivirusLastScanSummary(
            It.IsAny<AntivirusLastScanDetailRequest>(),
            out total
        )).Returns(new List<AntivirusLastScanDeviceSummaryModel>() { antivirusLastScanDeviceSummary });
        var actual = _controller.GetDeviceGroupsAntivirusLastScanSummary(GroupPath, Enums.AntivirusScanPeriodSubType.Custom, Enums.AntivirusScanType.FullScan, startDateTime, endDateTime, true,
            dataRetrievalOptions);
        var result = actual.FirstOrDefault();
        Assert.IsNotNull(result);
        Assert.AreEqual(result.LastScanDate, antivirusLastScanDeviceSummary.LastScanDate);
        Assert.AreEqual(result.DeviceId, antivirusLastScanDeviceSummary.DevId);

        _windowsDefenderServiceMock.Verify(v => v.GetDeviceGroupsAntivirusLastScanSummary(
            It.IsAny<AntivirusLastScanDetailRequest>(),
            out total
        ), Times.Once());
    }

    [Test]
    public void GetDeviceGroupsAntivirusThreatStatusDevices_ZeroTake_ThrowsValidationException()
    {
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.DeviceGroup, GroupId));
        var endDateTime = DateTime.Now;
        var startDateTime = DateTime.Now.AddDays(-5);

        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Skip = 0,
            Take = 0
        };

        var ex = Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupsAntivirusThreatStatusDevices(
            GroupPath, startDateTime, endDateTime, Enums.AntivirusThreatStatus.Active, true, dataRetrievalOptions));

        Assert.AreEqual("Take value should be greater than zero", ex.Message);
    }

    [Test]
    public void GetDeviceGroupsAntivirusThreatStatusDevices_NegativeSkip_ThrowsValidationException()
    {
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.DeviceGroup, GroupId));
        var endDateTime = DateTime.Now;
        var startDateTime = DateTime.Now.AddDays(-5);

        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Skip = -1,
            Take = 10
        };

        var ex = Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupsAntivirusThreatStatusDevices(
            GroupPath, startDateTime, endDateTime, Enums.AntivirusThreatStatus.Active, true, dataRetrievalOptions));

        Assert.AreEqual("Skip value should be greater than or equal to zero.", ex.Message);
    }

    [Test]
    public void GetDeviceGroupsAntivirusThreatStatusDevices_EnumNotDefined_ValidationException()
    {
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.DeviceGroup, GroupId));
        var endDateTime = DateTime.Now;
        var startDateTime = DateTime.Now.AddDays(-5);

        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Skip = 0,
            Take = 10
        };
        var ex = Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupsAntivirusThreatStatusDevices(
            GroupPath, startDateTime, endDateTime, (Enums.AntivirusThreatStatus)11, true, dataRetrievalOptions));
        Assert.AreEqual("Invalid enum value '11'", ex.Message);
    }

    [Test]
    public void GetDeviceGroupsAntivirusThreatStatusDevices_EmptyPath_ValidationException()
    {
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.DeviceGroup, GroupId));
        var endDateTime = DateTime.Now;
        var startDateTime = DateTime.Now.AddDays(-5);

        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Skip = 0,
            Take = 10
        };
        var ex = Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupsAntivirusThreatStatusDevices(
            "", startDateTime, endDateTime, Enums.AntivirusThreatStatus.Active, true, dataRetrievalOptions));
        Assert.That(ex.Message, Is.EqualTo("path"));
    }

    [Test]
    public void GetDeviceGroupsAntivirusThreatStatusDevices_InvalidStatusChangeTimeRange_ThrowsValidationException()
    {
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.DeviceGroup, GroupId));

        var endDateTime = DateTime.Now.AddDays(-5);
        var startDateTime = DateTime.Now;

        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Skip = 0,
            Take = 10
        };

        var ex = Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupsAntivirusThreatStatusDevices(
            GroupPath, startDateTime, endDateTime, Enums.AntivirusThreatStatus.Active, true, dataRetrievalOptions));

        Assert.AreEqual("Start time cannot be later than the end time.", ex.Message);
    }

    [Test]
    public void GetDeviceGroupsAntivirusThreatStatusDevices_ValidInput_ReturnsCorrectResult()
    {
        EnsureDeviceGroupRights();
        EnsureSyncCompleted();
        _accessibleDeviceGroupServiceMock
            .Setup(s => s.GetPermittedDeviceGroupIds(It.IsAny<string>(), true))
            .Returns(new HashSet<int> { 1001, 1002, 1003 });
        var expectedData = new List<AntivirusThreatStatusDeviceDetails>
        {
            new()
            {
                DevId = "1",
                LastStatusChangeTime = DateTime.Now.AddDays(-5),
            }
        };
        var endDateTime = DateTime.Now;
        var startDateTime = DateTime.Now.AddDays(-5);

        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Skip = 0,
            Take = 10
        };

        _windowsDefenderServiceMock
            .Setup(x => x.GetDeviceGroupsAntivirusThreatStatus(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                AntivirusThreatStatus.Active,
                It.IsAny<int>(),
                It.IsAny<int>(),
                true,
                out It.Ref<int>.IsAny
            ))
            .Returns(expectedData);

        var result = _controller.GetDeviceGroupsAntivirusThreatStatusDevices(
            GroupPath, startDateTime, endDateTime, Enums.AntivirusThreatStatus.Active, true, dataRetrievalOptions);

        Assert.NotNull(result);
        Assert.AreEqual(expectedData.Select(x => x.DevId), result.Select(x => x.DeviceId));
        Assert.AreEqual(expectedData.Select(x => x.LastStatusChangeTime), result.Select(x => x.LastReported));

        _accessibleDeviceGroupServiceMock.Verify(x => x.GetPermittedDeviceGroupIds(It.IsAny<string>(), true), Times.Once);

        _windowsDefenderServiceMock.Verify(x => x.GetDeviceGroupsAntivirusThreatStatus(
            It.IsAny<IEnumerable<int>>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            AntivirusThreatStatus.Active,
            It.IsAny<int>(),
            It.IsAny<int>(),
            true,
            out It.Ref<int>.IsAny), Times.Once);
    }

    [Test]
    public void ExportDeviceGroupsAntivirusScanSummaryDevices_ThrowsValidationException()
    {
        var startDateTime = new DateTime(2024, 10, 1, 10, 0, 0);
        var endDateTime = new DateTime(2024, 10, 2, 10, 0, 0);

        Assert.Throws<ValidationException>(() => _controller.ExportDeviceGroupsAntivirusScanSummaryDevices(GroupPath, Enums.AntivirusScanType.FullScan, (Enums.AntivirusScanPeriodSubType)255, startDateTime, endDateTime, 0));
        Assert.Throws<ValidationException>(() => _controller.ExportDeviceGroupsAntivirusScanSummaryDevices(GroupPath, (Enums.AntivirusScanType)255, Enums.AntivirusScanPeriodSubType.Custom, startDateTime, endDateTime, 0));
        Assert.Throws<ValidationException>(() => _controller.ExportDeviceGroupsAntivirusScanSummaryDevices("", Enums.AntivirusScanType.FullScan, Enums.AntivirusScanPeriodSubType.Custom, startDateTime, endDateTime, 0));

        startDateTime = DateTime.Now;
        endDateTime = DateTime.Now.AddDays(-1);

        Assert.Throws<ValidationException>(() => _controller.ExportDeviceGroupsAntivirusScanSummaryDevices(GroupPath, Enums.AntivirusScanType.FullScan, Enums.AntivirusScanPeriodSubType.Custom, startDateTime, endDateTime, 0));
    }

    [Test]
    public void ExportDeviceGroupsAntivirusScanSummaryDevices_Tests()
    {
        const string dummyData = "Dummy Data";
        var deviceId = Guid.NewGuid();
        var antivirusLastScanDeviceSummary = new AntivirusLastScanDeviceSummaryModel
        {
            DevId = deviceId.ToString(),
            LastScanDate = DateTime.UtcNow,
        };

        var startDateTime = new DateTime(2024, 10, 1, 10, 0, 0);
        var endDateTime = new DateTime(2024, 10, 2, 10, 0, 0);

        EnsureDeviceGroupRights();
        EnsureSyncCompleted();
        _accessibleDeviceGroupServiceMock
        .Setup(s => s.GetPermittedDeviceGroupIds(It.IsAny<string>(), true))
        .Returns(new HashSet<int> { 1, 2, 3 });

        _windowsDefenderServiceMock.Setup(m => m.GetDeviceGroupsAntivirusLastScanSummary(
            It.IsAny<IEnumerable<int>>(),
            It.IsAny<DateTime>(),
            It.IsAny<AntivirusScanTypeModel>(),
            It.IsAny<AntivirusScanPeriodSubTypeModel>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>())).Returns(new List<AntivirusLastScanDeviceSummaryModel>() { antivirusLastScanDeviceSummary });

        _csvConverterMock.Setup(c => c.GenerateCsvContent(
                It.IsAny<IEnumerable<IDictionary<string, string>>>(),
                It.IsAny<string[]>(),
                It.IsAny<Stream>(),
                It.IsAny<int>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<Func<string, string>>(),
                It.IsAny<bool>()))
            .Callback<IEnumerable<IDictionary<string, string>>, string[], Stream, int, CultureInfo, Func<string, string>, bool>(
                (records, headers, stream, offset, culture, localize, closeStream) =>
                {
                    using var writer = new StreamWriter(stream);
                    writer.WriteLine(dummyData);
                    writer.FlushAsync();
                })
            .Returns(Task.CompletedTask);

        var result = _controller.ExportDeviceGroupsAntivirusScanSummaryDevices(GroupPath, Enums.AntivirusScanType.FullScan, Enums.AntivirusScanPeriodSubType.Custom, startDateTime, endDateTime, 0);
        var pushStreamResult = result as PushStreamResult;
        Assert.NotNull(pushStreamResult);
        using var memoryStream = new MemoryStream();
        pushStreamResult.WriteStreamAsync(memoryStream);
        Assert.AreEqual(ApplicationCsv, pushStreamResult.MediaType.ToString());
        Assert.IsTrue(pushStreamResult.FileDownloadName.Contains("DeviceGroup_ScanHistory_FullScan"));
        _accessibleDeviceGroupServiceMock.Verify(s => s.GetPermittedDeviceGroupIds(It.IsAny<string>(), true), Times.Once);
        _windowsDefenderServiceMock.Verify(v => v.GetDeviceGroupsAntivirusLastScanSummary(
           It.IsAny<IEnumerable<int>>(),
            It.IsAny<DateTime>(),
            It.IsAny<AntivirusScanTypeModel>(),
            It.IsAny<AntivirusScanPeriodSubTypeModel>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>()
            ), Times.Once());
        _csvConverterMock.Verify(c => c.GenerateCsvContent(
            It.IsAny<IEnumerable<IDictionary<string, string>>>(),
            It.IsAny<string[]>(),
            It.IsAny<Stream>(),
            It.IsAny<int>(),
            It.IsAny<CultureInfo>(),
            It.IsAny<Func<string, string>>(),
            It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public void ExportDeviceGroupAntivirusThreatStatus_Success()
    {
        const string dummyData = "Dummy Data";

        var endDateTime = DateTime.Now;
        var startDateTime = DateTime.Now.AddDays(-5);
        var threatStatus = AntivirusThreatStatus.Active;
        var groupIds = new List<int> { 1001, 1002, 1003 };

        EnsureDeviceGroupRights();
        EnsureSyncCompleted();
        _accessibleDeviceGroupServiceMock
            .Setup(s => s.GetPermittedDeviceGroupIds(It.IsAny<string>(), true))
                .Returns(groupIds.ToHashSet());

        var threatDetails = new List<AntivirusThreatStatusDeviceDetails>
            {
                new() { DevId = "dev1", LastStatusChangeTime = DateTime.Now, ExternalThreatId = 78757, ThreatCategory = AntivirusThreatType.Cookie, Severity = AntivirusThreatSeverity.Low },
                new() { DevId = "dev2", LastStatusChangeTime = DateTime.Now, ExternalThreatId = 78758, ThreatCategory = AntivirusThreatType.EmailFlooder, Severity = AntivirusThreatSeverity.Unknown }
            };

        _windowsDefenderServiceMock
                .Setup(s => s.GetAllAntivirusThreatStatus(groupIds, startDateTime, endDateTime, threatStatus))
                .Returns(threatDetails);

        _csvConverterMock.Setup(c => c.GenerateCsvContent(
                    It.IsAny<IEnumerable<IDictionary<string, string>>>(),
                    It.IsAny<string[]>(),
                    It.IsAny<Stream>(),
                    It.IsAny<int>(),
                    It.IsAny<CultureInfo>(),
                    It.IsAny<Func<string, string>>(),
                    It.IsAny<bool>()))
                .Callback<IEnumerable<IDictionary<string, string>>, string[], Stream, int, CultureInfo, Func<string, string>, bool>(
                    (records, headers, stream, offset, culture, localize, closeStream) =>
                    {
                        using var writer = new StreamWriter(stream);
                        writer.WriteLine(dummyData);
                        writer.FlushAsync();
                    })
                .Returns(Task.CompletedTask);

        var result = _controller.ExportDeviceGroupAntivirusThreatStatus(GroupPath, startDateTime, endDateTime, Enums.AntivirusThreatStatus.Active, 0);
        var pushStreamResult = result as PushStreamResult;
        Assert.NotNull(pushStreamResult);
    }

    [TestCase("")]
    [TestCase("  ")]
    [TestCase(null)]
    public void ExportDeviceGroupThreatStatus_Invalid_Path_ThrowsException(string path)
    {
        Assert.Throws<ValidationException>(() => _controller.ExportDeviceGroupAntivirusThreatStatus(path, DateTime.Now.AddDays(-5), DateTime.Now, Enums.AntivirusThreatStatus.Active));
    }

    [Test]
    public void ExportDeviceGroupThreatStatus_EnumNotDefined_ValidationException()
    {
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.DeviceGroup, GroupId));
        var endDateTime = DateTime.Now;
        var startDateTime = DateTime.Now.AddDays(-5);
        var ex = Assert.Throws<ValidationException>(() => _controller.ExportDeviceGroupAntivirusThreatStatus(GroupPath, DateTime.Now.AddDays(-5), DateTime.Now, (Enums.AntivirusThreatStatus)11));
        Assert.AreEqual("Invalid enum value '11'", ex.Message);
    }

    [Test]
    public void ExportDeviceGroupThreatStatus_InvalidStatusChangeTimeRange_ThrowsValidationException()
    {
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.DeviceGroup, GroupId));

        var endDateTime = DateTime.Now.AddDays(-5);
        var startDateTime = DateTime.Now;
        var ex = Assert.Throws<ValidationException>(() => _controller.ExportDeviceGroupAntivirusThreatStatus(GroupPath, startDateTime, endDateTime, Enums.AntivirusThreatStatus.Active));
        Assert.AreEqual("Start time cannot be later than the end time.", ex.Message);
        Assert.Throws<ValidationException>(() => _controller.ExportDeviceGroupAntivirusThreatStatus(GroupPath, startDateTime, endDateTime, Enums.AntivirusThreatStatus.Active));
        Assert.Throws<ValidationException>(() => _controller.ExportDeviceGroupAntivirusThreatStatus(GroupPath, startDateTime, endDateTime, Enums.AntivirusThreatStatus.Active));
    }

    private void EnsureDeviceGroupRights()
    {
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.DeviceGroup, GroupId));
        _deviceGroupIdentityMapperMock.Setup(m => m.GetId(GroupPath)).Returns(GroupId);
    }

    private void EnsureSyncCompleted()
    {
        _windowsDeviceGroupsServiceMock.Setup(m => m.GetGroupSyncStatus(It.IsAny<int>(), It.IsAny<SyncRequestType>())).Returns(new DeviceGroupSyncInfo
        {
            SyncStatus = SyncRequestStatus.Completed,
            CompletedOn = DeviceGroupSyncCompletedOn
        });
    }
}

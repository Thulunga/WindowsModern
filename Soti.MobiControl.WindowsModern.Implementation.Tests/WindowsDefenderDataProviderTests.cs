using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using Soti.MobiControl.Devices;
using Soti.MobiControl.Test.Common.Database;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
[IntegrationTest]
internal sealed class WindowsDefenderDataProviderTests : ProviderTestsBase
{
    private const AntivirusThreatStatus ThreatStatus = AntivirusThreatStatus.ActionFailed;
    private const string ThreatName = "TestVirus";
    private static readonly DateTime InitialDetectionTime = new DateTime(2023, 6, 4, 9, 0, 0);
    private static readonly DateTime LastStatusChangeTime = new DateTime(2023, 6, 7, 10, 40, 0);
    private static readonly DateTime LastStatusChangeStartTime = new DateTime(2023, 06, 05);
    private static readonly DateTime LastStatusChangeEndTime = DateTime.Now;
    private const int Skip = 0;
    private const int Take = 10;
    private const int IsFirstTimeDetected = 1;
    private const int CurrentDetectionCount = 3;

    private static readonly TestDeviceProvider TestDeviceProvider = new TestDeviceProvider();
    private static readonly int[] deviceGroupIds = new int[] { 1 };

    private static int _antivirusThreatId;
    private static int _deviceId1;
    private static int _deviceId2;
    private static long _externalThreatId;
    private static string _devId1;
    private static string _devId2;

    private readonly string _devId = Guid.NewGuid().ToString();

    private WindowsDefenderDataProvider _windowsDefenderProvider;
    private WindowsDeviceDataProvider _windowsDeviceProvider;

    [SetUp]
    public void Setup()
    {
        _windowsDefenderProvider = new WindowsDefenderDataProvider(Database);
        _windowsDeviceProvider = new WindowsDeviceDataProvider(Database);

        CleanTestData();
    }

    [TearDown]
    public void TearDown()
    {
        CleanTestData();
    }

    [Test]
    public void GetAntivirusThreatHistory_Throws_Exception()
    {
        var totalCount = 1;
        var threatTypeIds = new List<byte> { 1 };
        var threatSeverityIds = new List<byte> { 1 };
        var threatStatusIds = new List<byte> { 1 };
        const AntivirusThreatSortByOption sortByOption = AntivirusThreatSortByOption.AntivirusThreatInitialDetectionTime;
        const bool order = true;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDefenderProvider.GetAntivirusThreatHistory(0, threatTypeIds, threatSeverityIds, threatStatusIds, LastStatusChangeStartTime, LastStatusChangeEndTime, Skip, Take, sortByOption,
                order, out totalCount));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDefenderProvider.GetAntivirusThreatHistory(1, threatTypeIds, threatSeverityIds, threatStatusIds, LastStatusChangeStartTime, LastStatusChangeEndTime, -1, Take, sortByOption, order,
                out totalCount));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDefenderProvider.GetAntivirusThreatHistory(1, threatTypeIds, threatSeverityIds, threatStatusIds, LastStatusChangeStartTime, LastStatusChangeEndTime, Skip, 0, sortByOption, order,
                out totalCount));
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDefenderProvider.GetAntivirusThreatHistory(1, threatTypeIds, null, threatStatusIds, LastStatusChangeStartTime, LastStatusChangeEndTime, Skip, Take, sortByOption, order,
                out totalCount));
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDefenderProvider.GetAntivirusThreatHistory(1, threatTypeIds, threatSeverityIds, null, LastStatusChangeStartTime, LastStatusChangeEndTime, Skip, Take, sortByOption, order,
                out totalCount));
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDefenderProvider.GetAntivirusThreatHistory(1, null, threatSeverityIds, threatStatusIds, LastStatusChangeStartTime, LastStatusChangeEndTime, Skip, Take, sortByOption, order,
                out totalCount));
        Assert.Throws<InvalidEnumArgumentException>(() =>
        _windowsDefenderProvider.GetAntivirusThreatHistory(1, threatTypeIds, threatSeverityIds, threatStatusIds, LastStatusChangeStartTime, LastStatusChangeEndTime, Skip, Take, (AntivirusThreatSortByOption)999, order,
                out totalCount));
    }

    [Test]
    public void GetAntivirusThreatHistory_Tests()
    {
        EnsureTestData();
        var threatTypeIds = new List<byte> { 1 };
        var threatSeverityIds = new List<byte> { 1 };
        var threatStatusIds = new List<byte> { 1 };
        var count = 1;

        var output = new AntivirusThreatData
        {
            ExternalThreatId = _externalThreatId,
            TypeId = (int)AntivirusThreatType.Malware,
            ThreatName = ThreatName,
            SeverityId = (int)AntivirusThreatSeverity.Low,
            CurrentDetectionCount = 3,
            LastThreatStatusId = 1,
            InitialDetectionTime = new DateTime(2023, 06, 04, 09, 00, 00),
            LastStatusChangeTime = new DateTime(2023, 06, 07, 10, 40, 00)
        };

        var result = _windowsDefenderProvider.GetAntivirusThreatHistory(_deviceId1, threatTypeIds, threatSeverityIds, threatStatusIds, LastStatusChangeStartTime, LastStatusChangeEndTime, Skip, Take, AntivirusThreatSortByOption.AntivirusThreatInitialDetectionTime, true, out var totalCount).FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(result.ExternalThreatId, output.ExternalThreatId);
        Assert.AreEqual(result.ThreatName, output.ThreatName);
        Assert.AreEqual(result.InitialDetectionTime, output.InitialDetectionTime);
        Assert.AreEqual(result.LastStatusChangeTime, output.LastStatusChangeTime);
        Assert.AreEqual(result.TypeId, output.TypeId);
        Assert.AreEqual(result.CurrentDetectionCount, output.CurrentDetectionCount);
        Assert.AreEqual(result.LastThreatStatusId, output.LastThreatStatusId);
        Assert.AreEqual(result.SeverityId, output.SeverityId);
        Assert.AreEqual(totalCount, count);
    }

    [Test]
    public void GetDeviceGroupAntivirusThreatHistory_NullTests()
    {
        var threatTypeIds = new List<byte> { 1 };
        var family = DeviceFamily.WindowsPhone;
        var groupIds = new List<int>() { 1 };
        var threatSeverityIds = new List<byte> { 1 };
        var total = 0;

        Assert.Throws<ArgumentNullException>(() => _windowsDefenderProvider.GetDeviceGroupsAntivirusThreatHistory(
            null,
            family,
            threatTypeIds,
            threatSeverityIds,
            LastStatusChangeStartTime,
            LastStatusChangeEndTime,
            -1,
            Take,
            AntivirusThreatHistorySortByOption.LastStatusChangeTime,
            true,
            out total));
        Assert.Throws<InvalidEnumArgumentException>(() => _windowsDefenderProvider.GetDeviceGroupsAntivirusThreatHistory(
            groupIds,
            (DeviceFamily)23,
            threatTypeIds,
            threatSeverityIds,
            LastStatusChangeStartTime,
            LastStatusChangeEndTime,
            Skip,
            -1,
            AntivirusThreatHistorySortByOption.LastStatusChangeTime,
            true,
            out total));
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderProvider.GetDeviceGroupsAntivirusThreatHistory(
            groupIds,
            family,
            null,
            threatSeverityIds,
            LastStatusChangeStartTime,
            LastStatusChangeEndTime,
            Skip,
            Take,
            AntivirusThreatHistorySortByOption.LastStatusChangeTime,
            true,
            out total));
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderProvider.GetDeviceGroupsAntivirusThreatHistory(
            groupIds,
            family,
            threatTypeIds,
            null,
            LastStatusChangeStartTime,
            LastStatusChangeEndTime,
            Skip,
            Take,
            AntivirusThreatHistorySortByOption.LastStatusChangeTime,
            true,
            out total));
        Assert.Throws<ArgumentException>(() => _windowsDefenderProvider.GetDeviceGroupsAntivirusThreatHistory(
            groupIds,
            family,
            threatTypeIds,
            threatSeverityIds,
            LastStatusChangeStartTime,
            new DateTime(2023, 06, 04),
            Skip,
            Take,
            AntivirusThreatHistorySortByOption.LastStatusChangeTime,
            true, out total));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderProvider.GetDeviceGroupsAntivirusThreatHistory(
            groupIds,
            family,
            threatTypeIds,
            threatSeverityIds,
            LastStatusChangeStartTime,
            new DateTime(2023, 06, 07),
            -1,
            Take,
            AntivirusThreatHistorySortByOption.LastStatusChangeTime,
            true, out total));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderProvider.GetDeviceGroupsAntivirusThreatHistory(
            groupIds,
            family,
            threatTypeIds,
            threatSeverityIds,
            LastStatusChangeStartTime,
            new DateTime(2023, 06, 07),
            0,
            0,
            AntivirusThreatHistorySortByOption.LastStatusChangeTime,
            true, out total));
    }

    [Test]
    public void GetAntivirusThreatAvailabilityByDeviceId_Throws_ExceptionTests()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderProvider.GetAntivirusThreatAvailabilityByDeviceId(
            default,
            AntivirusThreatStatus.Active));

        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderProvider.GetAntivirusThreatAvailabilityByDeviceId(
            _deviceId1,
            (AntivirusThreatStatus)200));
    }

    [Test]
    public void GetAntivirusThreatAvailabilityByDeviceId_Tests()
    {
        var deviceId = GetDeviceId(_devId);
        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = deviceId,
            IsLocked = false
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);
        var threatType = 1;
        var threatStatus = (byte)AntivirusThreatStatus.Active;
        _antivirusThreatId = 1007;
        var time = new DateTime(2024, 07, 10, 15, 50, 30);
        InsertTestDeviceThreatData(_devId, _antivirusThreatId, threatType, threatStatus, time);

        var result = _windowsDefenderProvider.GetAntivirusThreatAvailabilityByDeviceId(_deviceId1, AntivirusThreatStatus.Active);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsThreatAvailable);
        Assert.IsNotNull(result.LastAntivirusSyncTime);
    }

    [Test]
    public void GetAntivirusThreatAvailabilityByDeviceIds_Tests()
    {
        var deviceId = GetDeviceId(_devId);
        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = deviceId,
            IsLocked = false
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);
        var threatType = 1;
        var threatStatus = (byte)AntivirusThreatStatus.Active;
        _antivirusThreatId = 1007;
        var time = new DateTime(2024, 07, 10, 15, 50, 30);
        InsertTestDeviceThreatData(_devId, _antivirusThreatId, threatType, threatStatus, time);

        var result = _windowsDefenderProvider.GetAntivirusThreatAvailabilityByDeviceIds(
            new[] { _deviceId1, _deviceId2 },
            AntivirusThreatStatus.Active).FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(_deviceId1, result.DeviceId);
        Assert.IsTrue(result.IsThreatAvailable);
        Assert.AreEqual(time, result.LastAntivirusSyncTime);
    }

    [Test]
    public void GetAntivirusThreatAvailabilityByDeviceIds_NoActiveThreatExists_Tests()
    {
        var deviceId = GetDeviceId(_devId);
        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = deviceId,
            IsLocked = false
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);
        var threatType = 1;
        var threatStatus = (byte)AntivirusThreatStatus.Quarantined;
        _antivirusThreatId = 1007;
        var time = new DateTime(2024, 07, 10, 15, 50, 30);
        InsertTestDeviceThreatData(_devId, _antivirusThreatId, threatType, threatStatus, time);

        var result = _windowsDefenderProvider.GetAntivirusThreatAvailabilityByDeviceIds(
            new[] { _deviceId1, _deviceId2 },
            AntivirusThreatStatus.Active).FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(_deviceId1, result.DeviceId);
        Assert.IsFalse(result.IsThreatAvailable);
        Assert.AreEqual(time, result.LastAntivirusSyncTime);
    }

    [Test]
    public void GetAntivirusThreatAvailabilityByDeviceIds_NoSyncTime_Tests()
    {
        var deviceId = GetDeviceId(_devId);
        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = deviceId,
            IsLocked = false
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);
        var threatType = 1;
        var threatStatus = (byte)AntivirusThreatStatus.Quarantined;
        _antivirusThreatId = 1007;
        var time = new DateTime(2024, 07, 10, 15, 50, 30);
        InsertTestDeviceThreatData(_devId, _antivirusThreatId, threatType, threatStatus, time, false);

        var result = _windowsDefenderProvider.GetAntivirusThreatAvailabilityByDeviceIds(
            new[] { _deviceId1, _deviceId2 },
            AntivirusThreatStatus.Active).FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(_deviceId1, result.DeviceId);
        Assert.IsFalse(result.IsThreatAvailable);
        Assert.IsNull(result.LastAntivirusSyncTime);
    }

    [Test]
    public void GetAntivirusThreatAvailabilityByDeviceIds_Throws_ExceptionTests()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderProvider.GetAntivirusThreatAvailabilityByDeviceIds(
            null,
            AntivirusThreatStatus.Active));

        Assert.Throws<InvalidCastException>(() => _windowsDefenderProvider.GetAntivirusThreatAvailabilityByDeviceIds(
            new[] { _deviceId1, _deviceId2 },
            (AntivirusThreatStatus)200));
    }

    [Test]
    public void GetAntivirusThreatAvailabilityByDeviceId_NoActiveThreats_Tests()
    {
        var deviceId = GetDeviceId(_devId);
        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = deviceId,
            IsLocked = false
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);
        var threatType = 1;
        var threatStatus = (byte)AntivirusThreatStatus.ActionFailed;
        _antivirusThreatId = 1007;
        var time = new DateTime(2024, 07, 10, 15, 50, 30);
        InsertTestDeviceThreatData(_devId, _antivirusThreatId, threatType, threatStatus, time);

        var result = _windowsDefenderProvider.GetAntivirusThreatAvailabilityByDeviceId(_deviceId1, AntivirusThreatStatus.Active);

        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsThreatAvailable);
        Assert.AreEqual(time, result.LastAntivirusSyncTime);
    }

    [Test]
    public void GetDeviceGroupAntivirusThreatHistory_Tests()
    {
        EnsureTestData();
        var family = DeviceFamily.WindowsPhone;
        var groupIds = new List<int>() { 1 };
        var threatTypeIds = new List<byte> { 1 };
        var threatSeverityIds = new List<byte> { 1 };
        var total = 1;

        var output = new AntivirusThreatData
        {
            ExternalThreatId = _externalThreatId,
            TypeId = (int)AntivirusThreatType.Malware,
            SeverityId = (int)AntivirusThreatSeverity.Low,
            InitialDetectionTime = new DateTime(2023, 06, 04, 09, 00, 00),
            LastStatusChangeTime = new DateTime(2023, 06, 07, 10, 40, 00),
            NoOfDevices = 2
        };

        var result = _windowsDefenderProvider.GetDeviceGroupsAntivirusThreatHistory(
            groupIds,
            family,
            threatTypeIds,
            threatSeverityIds,
            LastStatusChangeStartTime,
            LastStatusChangeEndTime,
            Skip,
            Take,
            AntivirusThreatHistorySortByOption.LastStatusChangeTime,
            true,
            out var totalCount).FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(result.ExternalThreatId, output.ExternalThreatId);
        Assert.AreEqual(result.InitialDetectionTime, output.InitialDetectionTime);
        Assert.AreEqual(result.LastStatusChangeTime, output.LastStatusChangeTime);
        Assert.AreEqual(result.TypeId, output.TypeId);
        Assert.AreEqual(result.NoOfDevices, output.NoOfDevices);
        Assert.AreEqual(result.SeverityId, output.SeverityId);
        Assert.AreEqual(total, totalCount);
    }

    [Test]
    public void GetAllAntivirusThreatHistory_Tests()
    {
        var deviceId = GetDeviceId(_devId);

        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = deviceId,
            IsLocked = false
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);

        var threatType = 1;
        var threatStatus = (byte)AntivirusThreatStatus.Active;
        _antivirusThreatId = 25;
        var time = new DateTime(2024, 10, 3, 15, 50, 30);

        InsertTestDeviceThreatData(_devId, _antivirusThreatId, threatType, threatStatus, time);

        var output = new AntivirusThreatData
        {
            ExternalThreatId = 1,
            TypeId = 1,
            ThreatName = ThreatName,
            SeverityId = 1,
            CurrentDetectionCount = 3,
            LastThreatStatusId = threatStatus,
            InitialDetectionTime = time,
            LastStatusChangeTime = time
        };

        var result = _windowsDefenderProvider.GetAllAntivirusThreatHistory(deviceId).FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(result.ExternalThreatId, output.ExternalThreatId);
        Assert.AreEqual(result.InitialDetectionTime, output.InitialDetectionTime);
        Assert.AreEqual(result.LastStatusChangeTime, output.LastStatusChangeTime);
        Assert.AreEqual(result.TypeId, output.TypeId);
        Assert.AreEqual(result.NoOfDevices, output.NoOfDevices);
        Assert.AreEqual(result.SeverityId, output.SeverityId);
    }

    [Test]
    public void GetAllAntivirusThreatHistory_Throws_Exception()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderProvider.GetAllAntivirusThreatHistory(0));
    }

    [Test]
    public void GetDeviceGroupsDefaultAntivirusScanSummary_ExceptionTest()
    {
        var family = DeviceFamily.WindowsPhone;
        var groupIds = new List<int>() { 1 };
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceProvider.GetDeviceGroupsDefaultAntivirusScanSummary(null, family, DateTime.MinValue));
        Assert.Throws<InvalidEnumArgumentException>(() => _windowsDeviceProvider.GetDeviceGroupsDefaultAntivirusScanSummary(groupIds, (DeviceFamily)200, DateTime.MinValue));
    }

    [Test]
    public void GetDeviceGroupsDefaultAntivirusScanSummary_NoData_ExceptionTest()
    {
        _deviceId1 = GetDeviceId(_devId);
        var groupIds = new List<int>() { 1 };

        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);

        var result = _windowsDeviceProvider.GetDeviceGroupsDefaultAntivirusScanSummary(
            groupIds,
            DeviceFamily.WindowsPhone,
            new DateTime(2024, 06, 10));

        Assert.IsNotNull(result);
        Assert.IsNull(result.Custom);
        Assert.IsNotNull(result.MoreThan30Days);
        Assert.IsNotNull(result.Within24Hrs);
        Assert.IsNotNull(result.Within7Days);
        Assert.AreEqual(0, result.MoreThan30Days.FullScanned);
        Assert.AreEqual(0, result.MoreThan30Days.QuickScanned);
        Assert.AreEqual(0, result.Within24Hrs.FullScanned);
        Assert.AreEqual(0, result.Within24Hrs.QuickScanned);
        Assert.AreEqual(0, result.Within7Days.FullScanned);
        Assert.AreEqual(0, result.Within7Days.QuickScanned);

        TestDeviceProvider.DeleteDevice(_deviceId1);
    }

    [Test]
    public void GetDeviceGroupsDefaultAntivirusScanSummary_Test()
    {
        _deviceId1 = GetDeviceId(_devId);
        _deviceId2 = GetDeviceId(Guid.NewGuid().ToString());
        var groupIds = new List<int>() { 2 };

        var windowsDeviceData1 = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false
        };

        var windowsDeviceData2 = new WindowsDeviceData
        {
            DeviceId = _deviceId2,
            IsLocked = false
        };

        _windowsDeviceProvider.Insert(windowsDeviceData1);
        _windowsDeviceProvider.Insert(windowsDeviceData2);

        InsertWindowsDeviceData(_deviceId1, new DateTime(2024, 10, 01, 23, 59, 00), new DateTime(2024, 09, 24));
        InsertWindowsDeviceData(_deviceId2, new DateTime(2024, 08, 01), new DateTime(2024, 09, 30));

        var result = _windowsDeviceProvider.GetDeviceGroupsDefaultAntivirusScanSummary(
            groupIds,
            DeviceFamily.WindowsPhone,
            new DateTime(2024, 10, 01));

        Assert.IsNotNull(result);
        Assert.IsNull(result.Custom);
        Assert.IsNotNull(result.MoreThan30Days);
        Assert.IsNotNull(result.Within24Hrs);
        Assert.IsNotNull(result.Within7Days);
        Assert.AreEqual(1, result.Within24Hrs.QuickScanned);
        Assert.AreEqual(1, result.Within24Hrs.FullScanned);
        Assert.AreEqual(1, result.MoreThan30Days.QuickScanned);
        Assert.AreEqual(0, result.MoreThan30Days.FullScanned);
        Assert.AreEqual(0, result.Within7Days.QuickScanned);
        Assert.AreEqual(1, result.Within7Days.FullScanned);

        TestDeviceProvider.DeleteDevice(_deviceId1);
        TestDeviceProvider.DeleteDevice(_deviceId2);
    }

    [Test]
    public void GetDeviceGroupsCustomAntivirusScanSummary_NoData_Test()
    {
        _deviceId1 = GetDeviceId(_devId);
        var groupIds = new List<int>() { 1 };

        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);

        var result = _windowsDeviceProvider.GetDeviceGroupsCustomAntivirusScanSummary(
            groupIds,
            DeviceFamily.WindowsPhone,
            new DateTime(2024, 06, 10),
            DateTime.Now);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Custom);
        Assert.IsNull(result.MoreThan30Days);
        Assert.IsNull(result.Within24Hrs);
        Assert.IsNull(result.Within7Days);
        Assert.AreEqual(0, result.Custom.FullScanned);
        Assert.AreEqual(0, result.Custom.QuickScanned);

        TestDeviceProvider.DeleteDevice(_deviceId1);
    }

    [Test]
    public void GetDeviceGroupsCustomAntivirusScanSummary_Test()
    {
        _deviceId1 = GetDeviceId(_devId);
        _deviceId2 = GetDeviceId(Guid.NewGuid().ToString());
        var groupIds = new List<int>() { 2 };

        var windowsDeviceData1 = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false
        };

        var windowsDeviceData2 = new WindowsDeviceData
        {
            DeviceId = _deviceId2,
            IsLocked = false
        };

        _windowsDeviceProvider.Insert(windowsDeviceData1);
        _windowsDeviceProvider.Insert(windowsDeviceData2);

        InsertWindowsDeviceData(_deviceId1, new DateTime(2024, 07, 10), new DateTime(2024, 05, 30));
        InsertWindowsDeviceData(_deviceId2, new DateTime(2024, 06, 10), new DateTime(2024, 07, 11));

        var result = _windowsDeviceProvider.GetDeviceGroupsCustomAntivirusScanSummary(
            groupIds,
            DeviceFamily.WindowsPhone,
            new DateTime(2024, 06, 10),
            DateTime.Now);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Custom);
        Assert.IsNull(result.MoreThan30Days);
        Assert.IsNull(result.Within24Hrs);
        Assert.IsNull(result.Within7Days);
        Assert.AreEqual(1, result.Custom.FullScanned);
        Assert.AreEqual(2, result.Custom.QuickScanned);

        TestDeviceProvider.DeleteDevice(_deviceId1);
        TestDeviceProvider.DeleteDevice(_deviceId2);
    }

    [Test]
    public void GetThreatStatusIdCountForDeviceExceptionTest()
    {
        _deviceId1 = GetDeviceId(_devId);
        Assert.Throws<ArgumentException>(() => _windowsDefenderProvider.GetThreatStatusIdCountForDevice(_deviceId1, DateTime.Now, DateTime.Now.AddDays(-2)));
    }

    [Test]
    public void GetThreatStatusIdCountForDeviceTest()
    {
        _deviceId1 = GetDeviceId(_devId);
        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);

        var threatType = 1;
        var threatStatus = (byte)AntivirusThreatStatus.Active;
        _antivirusThreatId = 25;
        var time = new DateTime(2024, 10, 3, 15, 50, 30);
        InsertTestDeviceThreatData(_devId, _antivirusThreatId, threatType, threatStatus, time);

        var result = _windowsDefenderProvider.GetThreatStatusIdCountForDevice(_deviceId1, time, time.AddDays(1));
        Assert.IsNotNull(result);
    }

    [Test]
    public void GetAntivirusScanTimeDataExceptionTest()
    {
        _deviceId1 = GetDeviceId(_devId);
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceProvider.GetAntivirusScanTimeData(default));
    }

    [Test]
    public void GetAntivirusScanTimeDataTest()
    {
        _deviceId1 = GetDeviceId(_devId);
        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);

        _antivirusThreatId = 25;
        var threatType = 1;
        var threatStatus = (byte)AntivirusThreatStatus.Active;
        var antivirusLastSyncTime = new DateTime(2024, 10, 3, 15, 50, 30);
        InsertTestDeviceThreatData(_devId, _antivirusThreatId, threatType, threatStatus, antivirusLastSyncTime);

        var result = _windowsDeviceProvider.GetAntivirusScanTimeData(_deviceId1);
        Assert.IsNotNull(result);
        Assert.AreEqual(result.LastAntivirusSyncTime, antivirusLastSyncTime);
        Assert.AreEqual(result.IsThreatsAvailable, true);
    }

    [Test]
    public void GetAntivirusLastQuickScanSummaryPaginated_Exceptions()
    {
        var total = 0;
        var groupIds = new List<int> { 1, 2 };
        var startTime = new DateTime(2021, 3, 13);
        var endTime = new DateTime(2021, 3, 17);
        Assert.Throws<InvalidEnumArgumentException>(() => _windowsDeviceProvider.GetAntivirusLastQuickScanSummary(groupIds, startTime, endTime, (DeviceFamily)999, 0, 50, true, out total));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceProvider.GetAntivirusLastQuickScanSummary(groupIds, startTime, endTime, DeviceFamily.WindowsPhone, -1, 50, true, out total));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceProvider.GetAntivirusLastQuickScanSummary(groupIds, startTime, endTime, DeviceFamily.WindowsPhone, 0, 0, true, out total));
        Assert.Throws<ArgumentException>(() => _windowsDeviceProvider.GetAntivirusLastQuickScanSummary(groupIds, new DateTime(2021, 3, 18), endTime, DeviceFamily.WindowsPhone, 0, 50, true, out total));
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceProvider.GetAntivirusLastQuickScanSummary(null, startTime, endTime, DeviceFamily.WindowsPhone, 0, 50, true, out total));
    }

    [Test]
    public void GetAntivirusLastFullScanSummaryPaginated_Exceptions()
    {
        var total = 0;
        var groupIds = new List<int> { 1, 2 };
        var startTime = new DateTime(2021, 3, 13);
        var endTime = new DateTime(2021, 3, 17);
        Assert.Throws<InvalidEnumArgumentException>(() => _windowsDeviceProvider.GetAntivirusLastFullScanSummary(groupIds, startTime, endTime, (DeviceFamily)999, 0, 50, true, out total));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceProvider.GetAntivirusLastFullScanSummary(groupIds, startTime, endTime, DeviceFamily.WindowsPhone, -1, 50, true, out total));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceProvider.GetAntivirusLastFullScanSummary(groupIds, startTime, endTime, DeviceFamily.WindowsPhone, 0, 0, true, out total));
        Assert.Throws<ArgumentException>(() => _windowsDeviceProvider.GetAntivirusLastFullScanSummary(groupIds, new DateTime(2021, 3, 18), endTime, DeviceFamily.WindowsPhone, 0, 50, true, out total));
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceProvider.GetAntivirusLastFullScanSummary(null, startTime, endTime, DeviceFamily.WindowsPhone, 0, 50, true, out total));
    }

    [Test]
    public void GetAntivirusLastFullScanSummaryPaginated_Tests()
    {
        var groupIds = new List<int> { 1, 2 };
        var startTime = new DateTime(2024, 3, 13);
        var endTime = new DateTime(2024, 11, 17);
        EnsureTestData();
        var windowsDeviceData1 = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false
        };

        var windowsDeviceData2 = new WindowsDeviceData
        {
            DeviceId = _deviceId2,
            IsLocked = false
        };

        _windowsDeviceProvider.Insert(windowsDeviceData1);
        _windowsDeviceProvider.Insert(windowsDeviceData2);

        InsertWindowsDeviceData(_deviceId1, new DateTime(2024, 10, 01, 23, 59, 00), new DateTime(2024, 09, 24));
        InsertWindowsDeviceData(_deviceId2, new DateTime(2024, 08, 01), new DateTime(2024, 09, 30));
        var actual = _windowsDeviceProvider.GetAntivirusLastFullScanSummary(groupIds, startTime, endTime, DeviceFamily.WindowsPhone, 0, 50, true, out _);
        var result = actual.FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(result.LastScanDate, new DateTime(2024, 09, 30));
        Assert.AreEqual(_devId2, result.DevId.Trim());
    }

    [Test]
    public void GetAntivirusLastQuickScanSummaryPaginated_Tests()
    {
        var groupIds = new List<int> { 1, 2 };
        var startTime = new DateTime(2024, 3, 13);
        var endTime = new DateTime(2024, 11, 17);
        EnsureTestData();
        var windowsDeviceData1 = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false
        };

        var windowsDeviceData2 = new WindowsDeviceData
        {
            DeviceId = _deviceId2,
            IsLocked = false
        };

        _windowsDeviceProvider.Insert(windowsDeviceData1);
        _windowsDeviceProvider.Insert(windowsDeviceData2);

        InsertWindowsDeviceData(_deviceId1, new DateTime(2024, 10, 01), new DateTime(2024, 09, 24));
        InsertWindowsDeviceData(_deviceId2, new DateTime(2024, 08, 01), new DateTime(2024, 09, 30));
        var actual = _windowsDeviceProvider.GetAntivirusLastQuickScanSummary(groupIds, startTime, endTime, DeviceFamily.WindowsPhone, 0, 50, true, out _);
        var result = actual.FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(result.LastScanDate, new DateTime(2024, 10, 01));
        Assert.AreEqual(_devId1, result.DevId.Trim());
    }

    [Test]
    public void GetAntivirusThreatStatusCount_Throws_Exception()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDefenderProvider.GetAntivirusThreatStatusCount(null, new DateTime(2023, 06, 04), new DateTime(2023, 06, 07), 8));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDefenderProvider.GetAntivirusThreatStatusCount([1], new DateTime(2023, 06, 04), new DateTime(2023, 06, 07), 0));
        var ex = Assert.Throws<ArgumentException>(() => _windowsDefenderProvider.GetAntivirusThreatStatusCount([1], DateTime.Now, DateTime.Now.AddDays(-2), 8));
        Assert.That(ex.Message, Is.EqualTo("Start time cannot be later than the end time."));
    }

    [Test]
    public void GetAntivirusThreatStatusCount_SuccessTests()
    {
        var threatStatusCount = new Dictionary<byte, int>()
        {
            {(byte)AntivirusThreatStatus.Active, 0 },
            {(byte)AntivirusThreatStatus.ActionFailed, 2 },
            {(byte)AntivirusThreatStatus.ManualStepsRequired, 0 },
            {(byte)AntivirusThreatStatus.FullScanRequired, 0 },
            {(byte)AntivirusThreatStatus.RebootRequired, 0 },
            {(byte)AntivirusThreatStatus.RemediatedWithNonCriticalFailures, 0 },
            {(byte)AntivirusThreatStatus.Quarantined, 0 },
            {(byte)AntivirusThreatStatus.Removed, 0 },
            {(byte)AntivirusThreatStatus.Cleaned, 0 },
            {(byte)AntivirusThreatStatus.Allowed, 0 },
            {(byte)AntivirusThreatStatus.NoStatusCleared, 0 }
        };

        var startTime = new DateTime(2023, 1, 4);
        var endTime = new DateTime(2023, 10, 7);
        EnsureTestData();
        var result = _windowsDefenderProvider.GetAntivirusThreatStatusCount([1], startTime, endTime, 8);

        Assert.AreEqual(threatStatusCount[(byte)AntivirusThreatStatus.ActionFailed], result[(byte)AntivirusThreatStatus.ActionFailed]);
    }

    [Test]
    public void GetThreatStatus_Throws_Exception()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamily(-1, [1], DeviceFamily.WindowsPhone, LastStatusChangeStartTime, LastStatusChangeEndTime));
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamily(123, null, DeviceFamily.WindowsPhone, LastStatusChangeStartTime, LastStatusChangeEndTime));
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamily(123, [], DeviceFamily.WindowsPhone, LastStatusChangeStartTime, LastStatusChangeEndTime));
        Assert.Throws<InvalidEnumArgumentException>(() => _windowsDefenderProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamily(123123, [1], (DeviceFamily)9999, LastStatusChangeStartTime, LastStatusChangeEndTime));
        Assert.Throws<ArgumentException>(() => _windowsDefenderProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamily(123123, [1], DeviceFamily.WindowsPhone, LastStatusChangeStartTime, new DateTime(2023, 06, 04)));
    }

    [Test]
    public void GetThreatStatusPaginated_Throws_Exception()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamilyPaginated(-1, [1], DeviceFamily.WindowsPhone, LastStatusChangeStartTime, LastStatusChangeEndTime, 0, 50, AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime, true, out var _));
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamilyPaginated(123, null, DeviceFamily.WindowsPhone, LastStatusChangeStartTime, LastStatusChangeEndTime, 0, 50, AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime, true, out var _));
        Assert.Throws<ArgumentNullException>(() => _windowsDefenderProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamilyPaginated(123, [], DeviceFamily.WindowsPhone, LastStatusChangeStartTime, LastStatusChangeEndTime, 0, 50, AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime, true, out var _));
        Assert.Throws<InvalidEnumArgumentException>(() => _windowsDefenderProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamilyPaginated(123123, [1], (DeviceFamily)999, LastStatusChangeStartTime, LastStatusChangeEndTime, 0, 50, AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime, true, out var _));
        Assert.Throws<ArgumentException>(() => _windowsDefenderProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamilyPaginated(123123, [1], DeviceFamily.WindowsPhone, LastStatusChangeStartTime, new DateTime(2023, 06, 04), 0, 50, AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime, true, out var _));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamilyPaginated(123, [1], DeviceFamily.WindowsPhone, LastStatusChangeStartTime, LastStatusChangeEndTime, -1, 50, AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime, true, out var _));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamilyPaginated(123, [1], DeviceFamily.WindowsPhone, LastStatusChangeStartTime, LastStatusChangeEndTime, 0, 0, AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime, true, out var _));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDefenderProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamilyPaginated(123, [1], DeviceFamily.WindowsPhone, LastStatusChangeStartTime, LastStatusChangeEndTime, 0, -1, AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime, true, out var _));
    }

    [Test]
    public void GetThreatStatusPaginated_TotalCountTest()
    {
        EnsureTestData();

        var expectedCount = _windowsDefenderProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamily(_externalThreatId, [1], DeviceFamily.WindowsPhone, LastStatusChangeStartTime, LastStatusChangeEndTime).Count;
        _windowsDefenderProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamilyPaginated(_externalThreatId, [1], DeviceFamily.WindowsPhone, LastStatusChangeStartTime, LastStatusChangeEndTime, 0, 1, AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime, true, out var actualCount);
        Assert.AreEqual(expectedCount, actualCount);
    }

    [Test]
    public void GetThreatStatus_Success()
    {
        EnsureTestData();
        var deviceTreatStatus = _windowsDefenderProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamily(_externalThreatId, [1], DeviceFamily.WindowsPhone, LastStatusChangeStartTime, LastStatusChangeEndTime).FirstOrDefault();
        Assert.NotNull(deviceTreatStatus);
        Assert.AreEqual(ThreatStatus, deviceTreatStatus.ThreatStatus);
        Assert.AreEqual(InitialDetectionTime, deviceTreatStatus.InitialDetectionTime);
        Assert.AreEqual(LastStatusChangeTime, deviceTreatStatus.LastStatusChangeTime);
    }

    [Test]
    public void GetThreatStatusPaginated_Success()
    {
        EnsureTestData();
        var deviceTreatStatus = _windowsDefenderProvider.GetDeviceThreatStatusByGroupIdsAndDeviceFamilyPaginated(_externalThreatId, [1], DeviceFamily.WindowsPhone, LastStatusChangeStartTime, LastStatusChangeEndTime, 0, 50, AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime, true, out var _).FirstOrDefault();
        Assert.NotNull(deviceTreatStatus);
        Assert.AreEqual(ThreatStatus, deviceTreatStatus.ThreatStatus);
        Assert.AreEqual(InitialDetectionTime, deviceTreatStatus.InitialDetectionTime);
        Assert.AreEqual(LastStatusChangeTime, deviceTreatStatus.LastStatusChangeTime);
    }

    [Test]
    public void GetAntivirusThreatStatus_Throws_Exception()
    {
        // Null deviceGroupIds
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDefenderProvider.GetAntivirusThreatStatus(
                null,
                DeviceFamily.WindowsPhone,
                LastStatusChangeStartTime,
                LastStatusChangeEndTime,
                ThreatStatus,
                Skip,
                Take,
                true,
                out _));

        // Empty deviceGroupIds
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDefenderProvider.GetAntivirusThreatStatus(
                new List<int>(), // empty list
                DeviceFamily.WindowsPhone,
                LastStatusChangeStartTime,
                LastStatusChangeEndTime,
                ThreatStatus,
                Skip,
                Take,
                true,
                out _));

        // Negative skip
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDefenderProvider.GetAntivirusThreatStatus(
                deviceGroupIds,
                DeviceFamily.WindowsPhone,
                LastStatusChangeStartTime,
                LastStatusChangeEndTime,
                ThreatStatus,
                -1,
                Take,
                true,
                out _));

        // Zero take
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDefenderProvider.GetAntivirusThreatStatus(
                deviceGroupIds,
                DeviceFamily.WindowsPhone,
                LastStatusChangeStartTime,
                LastStatusChangeEndTime,
                ThreatStatus,
                Skip,
                0,
                true,
                out _));

        // Invalid enum for DeviceFamily
        Assert.Throws<InvalidEnumArgumentException>(() =>
            _windowsDefenderProvider.GetAntivirusThreatStatus(
                deviceGroupIds,
                (DeviceFamily)999,
                LastStatusChangeStartTime,
                LastStatusChangeEndTime,
                ThreatStatus,
                Skip,
                Take,
                true,
                out _));

        // Invalid enum for ThreatStatus
        Assert.Throws<InvalidCastException>(() =>
            _windowsDefenderProvider.GetAntivirusThreatStatus(
                deviceGroupIds,
                DeviceFamily.WindowsPhone,
                LastStatusChangeStartTime,
                LastStatusChangeEndTime,
                unchecked((AntivirusThreatStatus)999),
                Skip,
                Take,
                true,
                out _));
    }

    [Test]
    public void GetAntivirusThreatStatus_Success()
    {
        EnsureTestData();

        var expected = new AntivirusThreatStatusDeviceData
        {
            DevId = _devId1,
            LastStatusChangeTime = LastStatusChangeTime,
            ExternalThreatId = _externalThreatId,
            TypeId = (int)AntivirusThreatType.Malware,
            SeverityId = (int)AntivirusThreatSeverity.Low,
        };

        var result = _windowsDefenderProvider.GetAntivirusThreatStatus(deviceGroupIds, DeviceFamily.WindowsPhone, LastStatusChangeStartTime, LastStatusChangeEndTime, ThreatStatus, Skip, Take, true, out _).FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(expected.DevId, result.DevId);
        Assert.AreEqual(expected.LastStatusChangeTime, result.LastStatusChangeTime);
        Assert.AreEqual(result.ExternalThreatId, expected.ExternalThreatId);
        Assert.AreEqual(result.TypeId, expected.TypeId);
        Assert.AreEqual(result.SeverityId, expected.SeverityId);
    }

    [Test]
    public void GetAntivirusGroupThreatHistory_Throws_Exception()
    {
        var groupIds = new List<int>() { 1 };
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDefenderProvider.GetAntivirusGroupThreatHistory(null, DeviceFamily.WindowsPhone));

        Assert.Throws<InvalidEnumArgumentException>(() =>
            _windowsDefenderProvider.GetAntivirusGroupThreatHistory(groupIds, (DeviceFamily)200));
    }

    [Test]
    public void GetAntivirusGroupThreatHistory_Tests()
    {
        EnsureTestData();

        var family = DeviceFamily.WindowsPhone;
        var groupIds = new List<int>() { 1 };

        var output = new DeviceGroupThreatHistoryData
        {
            ExternalThreatId = _externalThreatId,
            TypeId = (int)AntivirusThreatType.Malware,
            SeverityId = (int)AntivirusThreatSeverity.Low,
            InitialDetectionTime = new DateTime(2023, 06, 04, 09, 00, 00),
            LastStatusChangeTime = new DateTime(2023, 06, 07, 10, 40, 00),
            NoOfDevices = 2
        };

        var result = _windowsDefenderProvider.GetAntivirusGroupThreatHistory(groupIds, family).FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(result.ExternalThreatId, output.ExternalThreatId);
        Assert.AreEqual(result.InitialDetectionTime, output.InitialDetectionTime);
        Assert.AreEqual(result.LastStatusChangeTime, output.LastStatusChangeTime);
        Assert.AreEqual(result.TypeId, output.TypeId);
        Assert.AreEqual(result.NoOfDevices, output.NoOfDevices);
        Assert.AreEqual(result.SeverityId, output.SeverityId);
    }

    [Test]
    public void GetAntivirusLastQuickScanSummary_Exceptions()
    {
        var groupIds = new List<int> { 1, 2 };
        var startTime = new DateTime(2021, 3, 13);
        var endTime = new DateTime(2021, 3, 17);
        Assert.Throws<InvalidEnumArgumentException>(() => _windowsDeviceProvider.GetAntivirusLastQuickScanSummary(groupIds, startTime, endTime, (DeviceFamily)999));
        Assert.Throws<ArgumentException>(() => _windowsDeviceProvider.GetAntivirusLastQuickScanSummary(groupIds, new DateTime(2021, 3, 18), endTime, DeviceFamily.WindowsPhone));
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceProvider.GetAntivirusLastQuickScanSummary(null, startTime, endTime, DeviceFamily.WindowsPhone));
    }

    [Test]
    public void GetAntivirusLastFullScanSummary_Exceptions()
    {
        var groupIds = new List<int> { 1, 2 };
        var startTime = new DateTime(2021, 3, 13);
        var endTime = new DateTime(2021, 3, 17);
        Assert.Throws<InvalidEnumArgumentException>(() => _windowsDeviceProvider.GetAntivirusLastFullScanSummary(groupIds, startTime, endTime, (DeviceFamily)999));
        Assert.Throws<ArgumentException>(() => _windowsDeviceProvider.GetAntivirusLastFullScanSummary(groupIds, new DateTime(2021, 3, 18), endTime, DeviceFamily.WindowsPhone));
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceProvider.GetAntivirusLastFullScanSummary(null, startTime, endTime, DeviceFamily.WindowsPhone));
    }

    [Test]
    public void GetAntivirusLastFullScanSummary_Tests()
    {
        var groupIds = new List<int> { 1, 2 };
        var startTime = new DateTime(2024, 3, 13);
        var endTime = new DateTime(2024, 11, 17);
        EnsureTestData();
        var windowsDeviceData1 = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false
        };

        var windowsDeviceData2 = new WindowsDeviceData
        {
            DeviceId = _deviceId2,
            IsLocked = false
        };

        _windowsDeviceProvider.Insert(windowsDeviceData1);
        _windowsDeviceProvider.Insert(windowsDeviceData2);

        InsertWindowsDeviceData(_deviceId1, new DateTime(2024, 10, 01, 23, 59, 00), new DateTime(2024, 09, 24));
        InsertWindowsDeviceData(_deviceId2, new DateTime(2024, 08, 01), new DateTime(2024, 09, 30));

        var actual = _windowsDeviceProvider.GetAntivirusLastFullScanSummary(groupIds, startTime, endTime, DeviceFamily.WindowsPhone);
        var result = actual.FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(result.LastScanDate, new DateTime(2024, 09, 24));
        Assert.AreEqual(_devId1, result.DevId.Trim());
    }

    [Test]
    public void GetAntivirusLastQuickScanSummary_Tests()
    {
        var groupIds = new List<int> { 1, 2 };
        var startTime = new DateTime(2024, 3, 13);
        var endTime = new DateTime(2024, 11, 17);
        EnsureTestData();
        var windowsDeviceData1 = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false
        };

        var windowsDeviceData2 = new WindowsDeviceData
        {
            DeviceId = _deviceId2,
            IsLocked = false
        };

        _windowsDeviceProvider.Insert(windowsDeviceData1);
        _windowsDeviceProvider.Insert(windowsDeviceData2);

        InsertWindowsDeviceData(_deviceId1, new DateTime(2024, 10, 01), new DateTime(2024, 09, 24));
        InsertWindowsDeviceData(_deviceId2, new DateTime(2024, 08, 01), new DateTime(2024, 09, 30));

        var actual = _windowsDeviceProvider.GetAntivirusLastQuickScanSummary(groupIds, startTime, endTime, DeviceFamily.WindowsPhone);
        var result = actual.FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(result.LastScanDate, new DateTime(2024, 10, 01));
        Assert.AreEqual(_devId1, result.DevId.Trim());
    }

    [Test]
    public void GetDeviceDataByAntivirusThreatStatus_Throws_Exception()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDefenderProvider.GetDeviceDataByAntivirusThreatStatus(null, DeviceFamily.WindowsPhone, LastStatusChangeStartTime, LastStatusChangeEndTime, ThreatStatus));
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDefenderProvider.GetDeviceDataByAntivirusThreatStatus(new List<int>(), DeviceFamily.WindowsPhone, LastStatusChangeStartTime, LastStatusChangeEndTime, ThreatStatus));
        Assert.Throws<InvalidEnumArgumentException>(() =>
            _windowsDefenderProvider.GetDeviceDataByAntivirusThreatStatus(deviceGroupIds, (DeviceFamily)999, LastStatusChangeStartTime, LastStatusChangeEndTime, ThreatStatus));
        Assert.Throws<InvalidCastException>(() =>
            _windowsDefenderProvider.GetDeviceDataByAntivirusThreatStatus(deviceGroupIds, DeviceFamily.WindowsPhone, LastStatusChangeStartTime, LastStatusChangeEndTime, unchecked((AntivirusThreatStatus)999)));
    }

    [Test]
    public void GetDeviceDataByAntivirusThreatStatus_Success()
    {
        EnsureTestData();

        var expected = new AntivirusThreatStatusDeviceData
        {
            DevId = _devId1,
            LastStatusChangeTime = LastStatusChangeTime,
            ExternalThreatId = _externalThreatId,
            TypeId = (int)AntivirusThreatType.Malware,
            SeverityId = (int)AntivirusThreatSeverity.Low,
        };

        var data = _windowsDefenderProvider.GetDeviceDataByAntivirusThreatStatus(deviceGroupIds, DeviceFamily.WindowsPhone, LastStatusChangeStartTime, LastStatusChangeEndTime, ThreatStatus).FirstOrDefault();

        Assert.IsNotNull(data);
        Assert.AreEqual(expected.DevId, data.DevId);
        Assert.AreEqual(expected.LastStatusChangeTime, data.LastStatusChangeTime);
        Assert.AreEqual(data.ExternalThreatId, expected.ExternalThreatId);
        Assert.AreEqual(data.TypeId, expected.TypeId);
        Assert.AreEqual(data.SeverityId, expected.SeverityId);
    }

    [Test]
    public void DeleteDeviceAntivirusThreatData_Tests()
    {
        var deviceId = GetDeviceId(_devId);
        var threatType = 1;
        var threatStatus = (byte)AntivirusThreatStatus.Active;
        _antivirusThreatId = 1007;
        var time = new DateTime(2024, 07, 10, 15, 50, 30);
        InsertTestDeviceThreatData(_devId, _antivirusThreatId, threatType, threatStatus, time);

        _windowsDefenderProvider.DeleteDeviceAntivirusThreatData(deviceId);

        var result = _windowsDefenderProvider.GetAllAntivirusThreatHistory(_deviceId1);

        Assert.AreEqual(result.Count(), 0);
    }

    private static int GetDeviceId(string devId)
    {
        return TestDeviceProvider.EnsureDeviceRecord(devId, 1004);
    }

    private void InsertTestDeviceThreatData(string devId, int threatId, int threatType, byte threatStatus, DateTime time, bool updateWindowsDeviceTable = true)
    {
        _deviceId1 = GetDeviceId(devId);
        var id = 1;
        var count = 3;
        byte isFirstTimeDetected = 0;
        if (updateWindowsDeviceTable)
        {
            Database.CreateCommand($"Update WindowsDevice set AntivirusLastQuickScanTime = '{time}', AntivirusLastFullScanTime = '{time}', LastAntivirusSyncTime = '{time}' WHERE DeviceId = {_deviceId1};").ExecuteNonQuery();
        }

        Database.CreateCommand($"SET IDENTITY_INSERT dbo.AntivirusThreat ON;" +
                               $"INSERT INTO AntivirusThreat( AntivirusThreatId, AntivirusThreatTypeId, ThreatName, ExternalAntivirusThreatId, AntivirusThreatSeverityId) " +
                               $"VALUES ({threatId}, {threatType}, '{ThreatName}', {id}, {id});" +
                               $"SET IDENTITY_INSERT dbo.AntivirusThreat OFF;").ExecuteNonQuery();
        Database.CreateCommand($"INSERT INTO DeviceAntivirusThreatStatus (DeviceId, AntivirusThreatId, CurrentDetectionCount, AntivirusLastThreatStatusId, IsFirstTimeDetected, AntivirusThreatInitialDetectionTime, AntivirusThreatLastStatusChangeTime) " +
                               $"VALUES ({_deviceId1}, {threatId}, {count}, {threatStatus}, {isFirstTimeDetected}, '{time}', '{time}');").ExecuteNonQuery();
    }

    private void CleanTestData()
    {
        if (_antivirusThreatId > 0 && _deviceId1 > 0)
        {
            Database.CreateCommand(
                $"delete from DeviceAntivirusThreatStatus where AntivirusThreatId = '{_antivirusThreatId}' and DeviceId = '{_deviceId1}'").ExecuteNonQuery();
            Database.CreateCommand($"delete from DeviceGroupDevice where DeviceId = '{_deviceId1}'").ExecuteNonQuery();
            Database.CreateCommand($"delete from DevInfo where DeviceId = '{_deviceId1}'").ExecuteNonQuery();
            if (_deviceId2 > 0)
            {
                Database.CreateCommand(
                    $"delete from DeviceAntivirusThreatStatus where AntivirusThreatId = '{_antivirusThreatId}' and DeviceId = '{_deviceId2}'").ExecuteNonQuery();
                Database.CreateCommand($"delete from DeviceGroupDevice where DeviceId = '{_deviceId2}'").ExecuteNonQuery();
                Database.CreateCommand($"delete from DevInfo where DeviceId = '{_deviceId2}'").ExecuteNonQuery();
            }

            Database.CreateCommand(
                $"Delete from AntivirusThreat where AntivirusThreatId = '{_antivirusThreatId}'").ExecuteNonQuery();

            _antivirusThreatId = 0;
        }

        Database.CreateCommand(
            $"DELETE FROM WindowsDevice WHERE DeviceId IN ('{_deviceId1}', '{_deviceId2}')").ExecuteNonQuery();
        _deviceId1 = 0;
        _deviceId2 = 0;
    }

    private void InsertWindowsDeviceData(int deviceId, DateTime quickScanTIme, DateTime fullScanTime)
    {
        Database.CreateCommand($"Update WindowsDevice set AntivirusLastQuickScanTime = '{quickScanTIme}', AntivirusLastFullScanTime = '{fullScanTime}' WHERE DeviceId = {deviceId};").ExecuteNonQuery();
    }

    private void EnsureTestData(AntivirusThreatStatus threatStatus = AntivirusThreatStatus.ActionFailed)
    {
        var groupId = 1;
        var deviceId1 = Guid.NewGuid().ToString();
        var deviceId2 = Guid.NewGuid().ToString();
        var threat = ThreatName;
        Database.CreateCommand($"INSERT INTO DevInfo (DevId, DevName, TypeId, [Online], Mode) VALUES ('{deviceId1}', 'Test', 1000, 0, 0)").ExecuteNonQuery();
        var id1 = Database.CreateCommand($"Select DeviceId from DevInfo where DevId = '{deviceId1}'").ExecuteCollection(record => record.GetInt32(0)).Single();
        Database.CreateCommand($"INSERT INTO DevInfo (DevId, DevName, TypeId, [Online], Mode) VALUES ('{deviceId2}', 'Test', 1000, 0, 0)").ExecuteNonQuery();
        var id2 = Database.CreateCommand($"Select DeviceId from DevInfo where DevId = '{deviceId2}'").ExecuteCollection(record => record.GetInt32(0)).Single();

        Database.CreateCommand($"INSERT INTO AntivirusThreat (AntivirusThreatTypeId, ThreatName, ExternalAntivirusThreatId, AntivirusThreatSeverityId)VALUES ( 1, '{threat}', 2, 1)").ExecuteNonQuery();
        var threatId = Database.CreateCommand($"Select AntivirusThreatId from AntivirusThreat where ThreatName = '{threat}'").ExecuteCollection(record => record.GetInt16(0)).Single();

        Database.CreateCommand($"INSERT INTO DeviceAntivirusThreatStatus (DeviceId, AntivirusThreatId, CurrentDetectionCount, AntivirusLastThreatStatusId, IsFirstTimeDetected, AntivirusThreatInitialDetectionTime, AntivirusThreatLastStatusChangeTime) VALUES " +
                               $"({id1}, {threatId}, {CurrentDetectionCount}, {(int)threatStatus}, {IsFirstTimeDetected}, '{InitialDetectionTime}', '{LastStatusChangeTime}')").ExecuteNonQuery();
        var externalThreatId = Database.CreateCommand($"Select ExternalAntivirusThreatId from AntivirusThreat where AntivirusThreatId  = {threatId}").ExecuteCollection(record => record.GetInt64(0))
            .Single();

        Database.CreateCommand($"INSERT INTO DeviceAntivirusThreatStatus (DeviceId, AntivirusThreatId, CurrentDetectionCount, AntivirusLastThreatStatusId, IsFirstTimeDetected, AntivirusThreatInitialDetectionTime, AntivirusThreatLastStatusChangeTime) VALUES " +
                               $"({id2}, {threatId}, {CurrentDetectionCount}, {(int)ThreatStatus}, {IsFirstTimeDetected}, '{InitialDetectionTime}', '{LastStatusChangeTime}')").ExecuteNonQuery();

        _deviceId1 = id1;
        _deviceId2 = id2;
        _devId1 = deviceId1;
        _devId2 = deviceId2;
        _antivirusThreatId = threatId;
        _externalThreatId = externalThreatId;
        Database.CreateCommand($"INSERT INTO DeviceGroupDevice (DeviceId, DeviceGroupId) VALUES('{_deviceId1}', '{groupId}')").ExecuteNonQuery();
        Database.CreateCommand($"INSERT INTO DeviceGroupDevice (DeviceId, DeviceGroupId) VALUES('{_deviceId2}', '{groupId}')").ExecuteNonQuery();
    }
}

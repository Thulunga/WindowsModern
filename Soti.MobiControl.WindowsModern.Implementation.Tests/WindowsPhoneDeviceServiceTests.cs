using Microsoft.Extensions.Caching.Memory;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Soti.MobiControl.Settings;
using Soti.MobiControl.Test.Common.Database;
using Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;
using Soti.MobiControl.WindowsModern.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
[IntegrationTest]
public class WindowsPhoneDeviceServiceTests : ProviderTestsBase
{
    private const int InvalidDeviceId = 0;
    private const string Channel = "Channel";
    private const string SessionContext = "SessionContext";
    private const string SessionContext1 = "SessionContext1";
    private const string SessionWatermark = "SessionWatermark";
    private const string SessionWatermark1 = "SessionWatermark1";
    private const string EnrollmentId = "EnrollmentId";
    private const string TpmSpecVersion = "TpmSpecVersion";
    private const string TpmSpecLevel = "TpmSpecLevel";
    private const string TpmSpecRevision = "TpmSpecRevision";
    private const int RetryIntervalDays = 20;
    private const int RenewPeriodDays = 10;
    private const int Id1 = 1;
    private const int Id2 = 2;
    private static readonly TestDeviceProvider TestDeviceProvider = new TestDeviceProvider();
    private readonly string _devId1 = Guid.NewGuid().ToString();
    private WindowsPhoneDeviceProvider _windowsPhoneDeviceProvider;
    private WindowsPhoneDeviceService _testee;
    private Mock<IMemoryCache> _deviceSnapshotCacheMock;
    private Mock<ISettingsManagementService> _settingsManagementServiceMock;
    private int _deviceId1;

    [SetUp]
    public void Setup()
    {
        _windowsPhoneDeviceProvider = new WindowsPhoneDeviceProvider(Database);
        _deviceSnapshotCacheMock = new Mock<IMemoryCache>();
        _settingsManagementServiceMock = new Mock<ISettingsManagementService>();
        _testee = new WindowsPhoneDeviceService(_windowsPhoneDeviceProvider,
            _deviceSnapshotCacheMock.Object,
            _settingsManagementServiceMock.Object);
    }

    [TearDown]
    public void Teardown()
    {
        DeleteTestData(_deviceId1);
    }

    [Test]
    public void Insert_Update_GetByDeviceId_test()
    {
        _deviceId1 = GetDeviceId(_devId1);
        var data = new WindowsPhoneDeviceInfo
        {
            DeviceId = _deviceId1,
            SessionContext = SessionContext,
            Channel = Channel,
            SessionWatermark = SessionWatermark,
            EnrollmentId = EnrollmentId,
            Timestamp = new DateTime(2000, 01, 01).ToUniversalTime(),
            IsRoboSupport = true,
            RenewPeriodDays = RenewPeriodDays,
            RetryIntervalDays = RetryIntervalDays,
            TpmSpecVersion = TpmSpecVersion,
            TpmSpecLevel = TpmSpecLevel,
            TpmSpecRevision = TpmSpecRevision,
            IsSMode = true,
            IsUpdateApprovalRequired = true,
            IsManageUpdates = true,
            EnrollmentInfoId = Id1,
            ChannelStatus = (int)WnsChannelStatus.Success
        };

        //// will do insert since data doesnt exist
        _testee.Update(data, new List<WindowsPhoneInfoExtendedProperties>());
        var dataFromDatabase = _testee.GetByDeviceId(_deviceId1);
        Assert.That(dataFromDatabase, Is.Not.Null);
        var serializedData = JsonConvert.SerializeObject(data);
        var serializedInserted = JsonConvert.SerializeObject(dataFromDatabase);
        Assert.That(serializedData, Is.EqualTo(serializedInserted));
        //// update and get Enrollment ID
        var enrollIdFromDatabase = _testee.GetEnrollmentId(_deviceId1);
        Assert.That(enrollIdFromDatabase, Is.EqualTo(Id1));
        _testee.UpdateEnrollmentId(_deviceId1, Id2);
        enrollIdFromDatabase = _testee.GetEnrollmentId(_deviceId1);
        Assert.That(enrollIdFromDatabase, Is.EqualTo(Id2));
        //// get Channel Info
        var wnsInfo = _testee.GetWnsChannelInfo(_deviceId1);
        Assert.That(wnsInfo.Item1, Is.EqualTo(Channel));
        Assert.That(wnsInfo.Item2, Is.EqualTo((int)WnsChannelStatus.Success));
        var isChannelActive = _testee.CheckChannel(_deviceId1);
        Assert.That(isChannelActive);
        var channelStatus = _testee.GetChannelStatus(_deviceId1);
        Assert.That(channelStatus, Is.EqualTo((int)WnsChannelStatus.Success));
        var wnsListeners = _testee.GetWnsListeners();
        Assert.That(wnsListeners.Count(), Is.EqualTo(1));
        _testee.BlockChannel(_deviceId1);
        channelStatus = _testee.GetChannelStatus(_deviceId1);
        Assert.That(channelStatus, Is.EqualTo((int)WnsChannelStatus.ChannelExpired));
        wnsListeners = _testee.GetWnsListeners();
        Assert.That(wnsListeners.Count(), Is.EqualTo(0));
        //// get session context
        var sessionContext = _testee.GetSessionContextById(_deviceId1);
        Assert.That(sessionContext, Is.EqualTo(SessionContext));
        //// get tmp
        var tmpVersion = _testee.GetTpmVersion(_deviceId1);
        Assert.That(tmpVersion.TpmSpecVersion, Is.EqualTo(TpmSpecVersion));
        Assert.That(tmpVersion.TpmSpecLevel, Is.EqualTo(TpmSpecLevel));
        Assert.That(tmpVersion.TpmSpecRevision, Is.EqualTo(TpmSpecRevision));
        var tmpVersions = _testee.GetTpmVersions(new[] { _deviceId1 });
        Assert.That(tmpVersions.Count, Is.EqualTo(1));
        ////Update with flags
        var data1 = new WindowsPhoneDeviceInfo
        {
            DeviceId = _deviceId1,
            SessionContext = SessionContext1,
            Channel = Channel,
            SessionWatermark = SessionWatermark1,
            EnrollmentId = EnrollmentId,
            Timestamp = new DateTime(2002, 01, 01).ToUniversalTime(),
            IsRoboSupport = false,
            RenewPeriodDays = RenewPeriodDays,
            RetryIntervalDays = RetryIntervalDays,
            TpmSpecVersion = TpmSpecVersion,
            TpmSpecLevel = TpmSpecLevel,
            TpmSpecRevision = TpmSpecRevision,
            IsSMode = false,
            IsUpdateApprovalRequired = false,
            IsManageUpdates = false,
            EnrollmentInfoId = Id1,
            ChannelStatus = (int)WnsChannelStatus.Success,
            Id = dataFromDatabase.Id
        };
        _testee.Update(data1, new[] { WindowsPhoneInfoExtendedProperties.SessionContext, WindowsPhoneInfoExtendedProperties.IsRoboSupport });
        dataFromDatabase = _testee.GetByDeviceId(_deviceId1);
        _testee.UpdateEnrollmentId(_deviceId1, Id1);
        Assert.That(dataFromDatabase.SessionContext, Is.EqualTo(SessionContext1));
        Assert.That(dataFromDatabase.IsRoboSupport, Is.EqualTo(false));
        //// assert other changed fields not affected
        Assert.That(dataFromDatabase.SessionWatermark, Is.EqualTo(SessionWatermark));
        Assert.That(dataFromDatabase.IsManageUpdates, Is.EqualTo(true));
    }

    [Test]
    public void ArgsValidation_test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _testee.GetByDeviceId(InvalidDeviceId));
        Assert.Throws<ArgumentNullException>(
            () => _testee.Update(null, Array.Empty<WindowsPhoneInfoExtendedProperties>()));
        Assert.Throws<ArgumentOutOfRangeException>(() => _testee.UpdateEnrollmentId(InvalidDeviceId, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => _testee.GetEnrollmentId(InvalidDeviceId));
        Assert.Throws<ArgumentOutOfRangeException>(() => _testee.GetWnsChannelInfo(InvalidDeviceId));
        Assert.Throws<ArgumentOutOfRangeException>(() => _testee.GetSessionContextById(InvalidDeviceId));
        Assert.Throws<ArgumentOutOfRangeException>(() => _testee.CheckChannel(InvalidDeviceId));
        Assert.Throws<ArgumentOutOfRangeException>(() => _testee.GetChannelStatus(InvalidDeviceId));
        Assert.Throws<ArgumentOutOfRangeException>(() => _testee.BlockChannel(InvalidDeviceId));
        Assert.Throws<ArgumentOutOfRangeException>(() => _testee.GetTpmVersion(InvalidDeviceId));
        Assert.Throws<ArgumentNullException>(() => _testee.GetTpmVersions(null));

        ////provider
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsPhoneDeviceProvider.GetByDeviceId(InvalidDeviceId));
        Assert.Throws<ArgumentNullException>(
            () => _windowsPhoneDeviceProvider.Update(null, WindowsPhoneInfoUpdateColumns.None));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsPhoneDeviceProvider.UpdateEnrollmentId(InvalidDeviceId, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsPhoneDeviceProvider.GetEnrollmentId(InvalidDeviceId));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsPhoneDeviceProvider.GetWnsChannelInfo(InvalidDeviceId));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsPhoneDeviceProvider.GetSessionContextById(InvalidDeviceId));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsPhoneDeviceProvider.CheckChannel(InvalidDeviceId));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsPhoneDeviceProvider.GetChannelStatus(InvalidDeviceId));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsPhoneDeviceProvider.BlockChannel(InvalidDeviceId));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsPhoneDeviceProvider.GetTpmVersion(InvalidDeviceId));
        Assert.Throws<ArgumentNullException>(() => _windowsPhoneDeviceProvider.GetTpmVersions(null));
    }

    [Test]
    public void InvalidateDeviceSnapshotCache_RemovesCacheEntry()
    {
        _deviceId1 = GetDeviceId(_devId1);

        _settingsManagementServiceMock
            .Setup(s => s.GetSetting<bool>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CachingOptions>(),
                It.IsAny<ObfuscationOption>(),
                It.IsAny<bool>()))
            .Returns(true);

        var service = new WindowsPhoneDeviceService(
            _windowsPhoneDeviceProvider,
            _deviceSnapshotCacheMock.Object,
            _settingsManagementServiceMock.Object);

        service.InvalidateDeviceSnapshotCache(_deviceId1);

        _deviceSnapshotCacheMock.Verify(m => m.Remove(_deviceId1), Times.Once);
    }

    [Test]
    public void InvalidateDeviceSnapshotCache_CacheDisabled_DoesNotRemove()
    {
        _deviceId1 = GetDeviceId(_devId1);

        _settingsManagementServiceMock
            .Setup(s => s.GetSetting<bool>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CachingOptions>(),
                It.IsAny<ObfuscationOption>(),
                It.IsAny<bool>()))
            .Returns(false);

        var service = new WindowsPhoneDeviceService(
            _windowsPhoneDeviceProvider,
            _deviceSnapshotCacheMock.Object,
            _settingsManagementServiceMock.Object);

        service.InvalidateDeviceSnapshotCache(_deviceId1);

        _deviceSnapshotCacheMock.Verify(m => m.Remove(It.IsAny<object>()), Times.Never);
    }

    [Test]
    public void GetSessionContextById_ReturnsValueFromCache_WhenCacheEnabled()
    {
        _deviceId1 = GetDeviceId(_devId1);
        const string cachedValue = "cached-session";

        _settingsManagementServiceMock
            .Setup(s => s.GetSetting<bool>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CachingOptions>(),
                It.IsAny<ObfuscationOption>(),
                It.IsAny<bool>()))
            .Returns(true);

        var cacheEntryMock = new Mock<ICacheEntry>();
        _deviceSnapshotCacheMock
            .Setup(m => m.CreateEntry(It.IsAny<object>()))
            .Returns(cacheEntryMock.Object);

        object boxed = cachedValue;
        _deviceSnapshotCacheMock
            .Setup(m => m.TryGetValue(_deviceId1, out boxed))
            .Returns(true);

        var service = new WindowsPhoneDeviceService(
            _windowsPhoneDeviceProvider,
            _deviceSnapshotCacheMock.Object,
            _settingsManagementServiceMock.Object);

        var result = service.GetSessionContextById(_deviceId1);

        Assert.AreEqual(cachedValue, result);
    }

    [Test]
    public void GetSessionContextById_CacheEnabled_ReturnsProviderValue_WithoutMocks()
    {
        _deviceId1 = GetDeviceId(_devId1);
        const string providerValue = "provider-session";

        var deviceInfo = new WindowsPhoneDeviceInfo
        {
            DeviceId = _deviceId1,
            SessionContext = providerValue,
            ChannelStatus = (int)WnsChannelStatus.Success,
            IsRoboSupport = false,
            RoboStatusId = 0
        };

        _windowsPhoneDeviceProvider.Update(deviceInfo, WindowsPhoneInfoUpdateColumns.SessionContext);

        _settingsManagementServiceMock
            .Setup(s => s.GetSetting<bool>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CachingOptions>(),
                It.IsAny<ObfuscationOption>(),
                It.IsAny<bool>()))
            .Returns(true);

        object boxed = null;
        _deviceSnapshotCacheMock
            .Setup(m => m.TryGetValue(_deviceId1, out boxed))
            .Returns(false);

        var cacheEntryMock = new Mock<ICacheEntry>();
        _deviceSnapshotCacheMock
            .Setup(m => m.CreateEntry(_deviceId1))
            .Returns(cacheEntryMock.Object);

        var service = new WindowsPhoneDeviceService(
            _windowsPhoneDeviceProvider,
            _deviceSnapshotCacheMock.Object,
            _settingsManagementServiceMock.Object);

        var result = service.GetSessionContextById(_deviceId1);

        Assert.AreEqual(providerValue, result, "Provider value should be returned from DB");

        _deviceSnapshotCacheMock.Verify(m => m.CreateEntry(_deviceId1), Times.Once);
        cacheEntryMock.VerifySet(e => e.Value = providerValue, Times.Once);
    }

    [Test]
    public void GetSessionContextById_CacheDisabled_DoesNotUseCache()
    {
        _deviceId1 = GetDeviceId(_devId1);

        _settingsManagementServiceMock
            .Setup(s => s.GetSetting<bool>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CachingOptions>(),
                It.IsAny<ObfuscationOption>(),
                It.IsAny<bool>()))
            .Returns(false);

        object boxed = null;
        _deviceSnapshotCacheMock
            .Setup(m => m.TryGetValue(_deviceId1, out boxed))
            .Returns(false);

        var service = new WindowsPhoneDeviceService(
            _windowsPhoneDeviceProvider,
            _deviceSnapshotCacheMock.Object,
            _settingsManagementServiceMock.Object);

        var result = service.GetSessionContextById(_deviceId1);

        Assert.IsNotNull(result);

        _deviceSnapshotCacheMock.Verify(m => m.CreateEntry(It.IsAny<object>()), Times.Never);
    }

    [Test]
    public void Update_StoresSessionContextToCache_WhenCachingEnabled()
    {
        _deviceId1 = GetDeviceId(_devId1);
        const string session = "session-new";

        _settingsManagementServiceMock
            .Setup(s => s.GetSetting<bool>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CachingOptions>(),
                It.IsAny<ObfuscationOption>(),
                It.IsAny<bool>()))
            .Returns(true);

        var cacheEntryMock = new Mock<ICacheEntry>();
        _deviceSnapshotCacheMock
            .Setup(m => m.CreateEntry(It.IsAny<object>()))
            .Returns(cacheEntryMock.Object);

        var service = new WindowsPhoneDeviceService(
            _windowsPhoneDeviceProvider,
            _deviceSnapshotCacheMock.Object,
            _settingsManagementServiceMock.Object);

        var deviceInfo = new WindowsPhoneDeviceInfo
        {
            DeviceId = _deviceId1,
            SessionContext = session
        };

        service.Update(deviceInfo, Array.Empty<WindowsPhoneInfoExtendedProperties>());

        _deviceSnapshotCacheMock.Verify(
            m => m.CreateEntry(_deviceId1),
            Times.Once);

        cacheEntryMock.VerifySet(e => e.Value = session, Times.Once);
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

        var cmd = Database.StoredProcedures["[dbo].[GEN_WP8Device_Delete]"];
        cmd.Parameters.Add("DeviceId", deviceId);
        cmd.ExecuteNonQuery();
        TestDeviceProvider.DeleteDevice(deviceId);
    }
}

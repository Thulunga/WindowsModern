using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Soti.Diagnostics;
using Soti.MobiControl.DeploymentServerExtensions.Contracts;
using Soti.MobiControl.Events;
using Soti.MobiControl.Messaging;
using Soti.MobiControl.WindowsModern.Implementation.Events;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models;
using Soti.SensitiveDataProtection;
using Soti.SensitiveDataProtection.Model;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
public class WindowsDeviceLoggedInUserServiceTests
{
    private const string TestUserName = "TestUserName";
    private const string TestUserDomainName = "TestUserDomainName";
    private const string TestUserFullName = "TestUserFullName";
    private const string UserSid = "S-100798-280198-220222";
    private const string DevId = "TestDevId";
    private const string PreviousFullName = "PreviousFullName";
    private const string PreviousUserName = "PreviousUserName";
    private const string PreviousUserSid = "S-100798-280198-220224";

    private Mock<IWindowsDeviceUserProvider> _windowsDeviceUserProviderMock;
    private Mock<IWindowsDeviceService> _windowsDeviceServiceMock;
    private Mock<IEventDispatcher> _eventDispatcherMock;
    private Mock<IProgramTrace> _programTraceMock;
    private Mock<IDataKeyService> _dataKeyServiceMock;
    private Mock<IMessagePublisher> _messagePublisherMock;
    private Mock<IDeviceAdministrationCallback> _deviceAdministrationCallbackMock;
    private IWindowsDeviceLoggedInUserService _windowsDeviceLoggedInUserService;

    [SetUp]
    public void Setup()
    {
        _windowsDeviceUserProviderMock = new Mock<IWindowsDeviceUserProvider>(MockBehavior.Strict);
        _windowsDeviceServiceMock = new Mock<IWindowsDeviceService>(MockBehavior.Strict);
        _eventDispatcherMock = new Mock<IEventDispatcher>(MockBehavior.Strict);
        _programTraceMock = new Mock<IProgramTrace>(MockBehavior.Strict);
        _dataKeyServiceMock = new Mock<IDataKeyService>(MockBehavior.Strict);
        _messagePublisherMock = new Mock<IMessagePublisher>();
        _deviceAdministrationCallbackMock = new Mock<IDeviceAdministrationCallback>();
        _windowsDeviceLoggedInUserService = new WindowsDeviceLoggedInUserService(
            _windowsDeviceUserProviderMock.Object,
            _windowsDeviceServiceMock.Object,
            _programTraceMock.Object,
            _eventDispatcherMock.Object,
            _dataKeyServiceMock.Object,
            new Lazy<IMessagePublisher>(() => _messagePublisherMock.Object),
            new Lazy<IDeviceAdministrationCallback>(() => _deviceAdministrationCallbackMock.Object));
    }

    [Test]
    public void ConstructorTests()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new WindowsDeviceLoggedInUserService(null,
            _windowsDeviceServiceMock.Object,
            _programTraceMock.Object,
            _eventDispatcherMock.Object,
            _dataKeyServiceMock.Object,
            new Lazy<IMessagePublisher>(() => _messagePublisherMock.Object),
            new Lazy<IDeviceAdministrationCallback>(() => _deviceAdministrationCallbackMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _ = new WindowsDeviceLoggedInUserService(_windowsDeviceUserProviderMock.Object,
            null,
            _programTraceMock.Object,
            _eventDispatcherMock.Object,
            _dataKeyServiceMock.Object,
            new Lazy<IMessagePublisher>(() => _messagePublisherMock.Object),
            new Lazy<IDeviceAdministrationCallback>(() => _deviceAdministrationCallbackMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _ = new WindowsDeviceLoggedInUserService(_windowsDeviceUserProviderMock.Object,
            _windowsDeviceServiceMock.Object,
            null,
            _eventDispatcherMock.Object,
            _dataKeyServiceMock.Object,
            new Lazy<IMessagePublisher>(() => _messagePublisherMock.Object),
            new Lazy<IDeviceAdministrationCallback>(() => _deviceAdministrationCallbackMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _ = new WindowsDeviceLoggedInUserService(_windowsDeviceUserProviderMock.Object,
            _windowsDeviceServiceMock.Object,
            _programTraceMock.Object,
            null,
            _dataKeyServiceMock.Object,
            new Lazy<IMessagePublisher>(() => _messagePublisherMock.Object),
            new Lazy<IDeviceAdministrationCallback>(() => _deviceAdministrationCallbackMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _ = new WindowsDeviceLoggedInUserService(_windowsDeviceUserProviderMock.Object,
            _windowsDeviceServiceMock.Object,
            _programTraceMock.Object,
            _eventDispatcherMock.Object,
            null,
            new Lazy<IMessagePublisher>(() => _messagePublisherMock.Object),
            new Lazy<IDeviceAdministrationCallback>(() => _deviceAdministrationCallbackMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _ = new WindowsDeviceLoggedInUserService(_windowsDeviceUserProviderMock.Object,
            _windowsDeviceServiceMock.Object,
            _programTraceMock.Object,
            _eventDispatcherMock.Object,
            _dataKeyServiceMock.Object,
            null,
            new Lazy<IDeviceAdministrationCallback>(() => _deviceAdministrationCallbackMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _ = new WindowsDeviceLoggedInUserService(_windowsDeviceUserProviderMock.Object,
            _windowsDeviceServiceMock.Object,
            _programTraceMock.Object,
            _eventDispatcherMock.Object,
            _dataKeyServiceMock.Object,
            new Lazy<IMessagePublisher>(() => _messagePublisherMock.Object),
            null));
    }

    [Test]
    public void GetWindowsDeviceLoggedInUserData_ExceptionTest()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLoggedInUserService.GetWindowsDeviceLoggedInUserData(0));
    }

    [Test]
    public void GetWindowsDeviceLoggedInUserData_SuccessTest()
    {
        var deviceId = 1007;
        var windowsDeviceUserData = new WindowsDeviceUserData()
        {
            DeviceId = deviceId,
            UserDomain = null,
            UserFullName = TestUserFullName,
            UserSID = UserSid,
            UserName = TestUserName,
            LastCheckInDeviceUserTime = DateTime.MinValue,
            DataKeyId = 1
        };
        var dataKey = GetDataKey(1);
        _windowsDeviceUserProviderMock
            .Setup(x => x.GetLoggedInUserDataByDeviceId(deviceId)).Returns(windowsDeviceUserData);
        _programTraceMock.Setup(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()));
        _dataKeyServiceMock.Setup(x => x.GetKey(dataKey.Id)).Returns(dataKey);
        var result = _windowsDeviceLoggedInUserService.GetWindowsDeviceLoggedInUserData(deviceId);
        var resultFromCache = _windowsDeviceLoggedInUserService.GetWindowsDeviceLoggedInUserData(deviceId);

        Assert.IsNotNull(result);
        Assert.AreEqual(result.LastCheckInDeviceUserTime, windowsDeviceUserData.LastCheckInDeviceUserTime);
        Assert.AreEqual(result.DeviceId, windowsDeviceUserData.DeviceId);
        Assert.AreEqual(result.UserDomain, windowsDeviceUserData.UserDomain);
        Assert.AreEqual(result.UserFullName, windowsDeviceUserData.UserFullName);
        Assert.AreEqual(result.UserName, windowsDeviceUserData.UserName);
        Assert.AreEqual(result.UserSID, windowsDeviceUserData.UserSID);
        Assert.AreEqual(result, resultFromCache);

        _windowsDeviceUserProviderMock
            .Verify(x => x.GetLoggedInUserDataByDeviceId(deviceId), Times.Once);

        _programTraceMock.Verify(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));

        _windowsDeviceLoggedInUserService.InvalidateLoggedInUserCache(deviceId, false);
    }

    [Test]
    public void ProcessWindowsDeviceLoggedInUser_ExceptionTest()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLoggedInUserService.ProcessWindowsDeviceLoggedInUser(null));

        var windowsDeviceLoggedInUserModel = new WindowsDeviceLoggedInUserModel()
        {
            DeviceId = 10001,
            UserDomain = null,
            UserFullName = TestUserFullName,
            UserSID = UserSid,
            UserName = TestUserName,
            IsUserLoggedIn = true
        };

        Assert.Throws<ArgumentException>(() =>
            _windowsDeviceLoggedInUserService.ProcessWindowsDeviceLoggedInUser(windowsDeviceLoggedInUserModel));
    }

    [TestCase(null, UserSid)]
    [TestCase(TestUserName, "")]
    [TestCase("  ", null)]
    public void ProcessWindowsDeviceLoggedInUser_FailureTest(string username, string userSid)
    {
        var windowsDeviceLoggedInUserModel1 = new WindowsDeviceLoggedInUserModel()
        {
            DeviceId = 10001,
            DevId = DevId,
            UserDomain = TestUserDomainName,
            UserFullName = TestUserFullName,
            UserSID = userSid,
            UserName = username,
            IsUserLoggedIn = true
        };

        _programTraceMock.Setup(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()));

        _eventDispatcherMock.Setup(m => m.DispatchEvent(It.IsAny<LoggedInUserFailureEvent>()));

        _windowsDeviceLoggedInUserService.ProcessWindowsDeviceLoggedInUser(windowsDeviceLoggedInUserModel1);

        _eventDispatcherMock.Verify(m => m.DispatchEvent(It.IsAny<LoggedInUserFailureEvent>()), Times.Once());

        _programTraceMock.Verify(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void ProcessWindowsDeviceLoggedInUser_UserLoggedInChangeDetectedTest()
    {
        var windowsDeviceLoggedInUserModel = new WindowsDeviceLoggedInUserModel()
        {
            DeviceId = 10001,
            DevId = DevId,
            UserDomain = null,
            UserFullName = TestUserFullName,
            UserSID = UserSid,
            UserName = TestUserName,
            IsUserLoggedIn = true
        };

        var windowsDeviceLoggedInUserData = new WindowsDeviceUserData()
        {
            DeviceId = 10001,
            UserDomain = null,
            UserFullName = PreviousFullName,
            UserSID = PreviousUserSid,
            UserName = PreviousUserName,
            DataKeyId = 1
        };
        var dataKey = GetDataKey(1);

        _windowsDeviceUserProviderMock.Setup(x => x.GetLoggedInUserDataByDeviceId(It.IsAny<int>()))
            .Returns(windowsDeviceLoggedInUserData);

        _dataKeyServiceMock.Setup(x => x.GetKey(dataKey.Id)).Returns(dataKey);

        _eventDispatcherMock.Setup(m => m.DispatchEvent(It.IsAny<LoggedInUserChangeDetectedEvent>()));

        _windowsDeviceServiceMock.Setup(x =>
            x.UpdateLoggedInUserLastCheckInTime(It.IsAny<int>(), It.IsAny<DateTime>()));

        _dataKeyServiceMock.Setup(x => x.GetKey()).Returns(dataKey);

        _windowsDeviceUserProviderMock
            .Setup(x => x.ModifyLoggedInUser(It.IsAny<WindowsDeviceUserData>()));

        _programTraceMock.Setup(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()));

        _windowsDeviceLoggedInUserService.ProcessWindowsDeviceLoggedInUser(windowsDeviceLoggedInUserModel);

        _windowsDeviceUserProviderMock.Verify(x => x.GetLoggedInUserDataByDeviceId(It.IsAny<int>()), Times.Once);

        _eventDispatcherMock.Verify(m => m.DispatchEvent(It.IsAny<LoggedInUserChangeDetectedEvent>()), Times.Once());

        _windowsDeviceUserProviderMock
            .Verify(x => x.ModifyLoggedInUser(It.IsAny<WindowsDeviceUserData>()), Times.Once);

        _windowsDeviceServiceMock.Verify(
            x =>
            x.UpdateLoggedInUserLastCheckInTime(It.IsAny<int>(), It.IsAny<DateTime>()), Times.Once);

        _programTraceMock.Verify(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));

        _windowsDeviceLoggedInUserService.InvalidateLoggedInUserCache(windowsDeviceLoggedInUserModel.DeviceId, false);
    }

    [Test]
    public void ProcessWindowsDeviceLoggedInUser_SameUserLoggedInTest()
    {
        var windowsDeviceLoggedInUserModel = new WindowsDeviceLoggedInUserModel()
        {
            DeviceId = 10001,
            DevId = DevId,
            UserDomain = null,
            UserFullName = TestUserFullName,
            UserSID = UserSid,
            UserName = TestUserName,
            IsUserLoggedIn = true
        };

        var windowsDeviceLoggedInUserData = new WindowsDeviceUserData()
        {
            DeviceId = 10001,
            UserDomain = null,
            UserFullName = TestUserFullName,
            UserSID = UserSid,
            UserName = TestUserName,
            DataKeyId = 1
        };

        var dataKey = GetDataKey(1);

        _windowsDeviceUserProviderMock.Setup(x => x.GetLoggedInUserDataByDeviceId(It.IsAny<int>()))
            .Returns(windowsDeviceLoggedInUserData);

        _programTraceMock.Setup(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()));

        _dataKeyServiceMock.Setup(x => x.GetKey(dataKey.Id)).Returns(dataKey);

        _windowsDeviceLoggedInUserService.ProcessWindowsDeviceLoggedInUser(windowsDeviceLoggedInUserModel);

        _windowsDeviceUserProviderMock.Verify(x => x.GetLoggedInUserDataByDeviceId(It.IsAny<int>()), Times.Once);

        _programTraceMock.Verify(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        _windowsDeviceLoggedInUserService.InvalidateLoggedInUserCache(windowsDeviceLoggedInUserModel.DeviceId, false);
    }

    [Test]
    public void ProcessWindowsDeviceLoggedInUser_NoUserLoggedInTest()
    {
        var windowsDeviceLoggedInUserModel = new WindowsDeviceLoggedInUserModel()
        {
            DeviceId = 10001,
            DevId = DevId,
            UserDomain = null,
            UserFullName = null,
            UserSID = null,
            UserName = null,
            IsUserLoggedIn = false
        };

        var windowsDeviceLoggedInUserData = new WindowsDeviceUserData()
        {
            DeviceId = 10001,
            UserDomain = null,
            UserFullName = PreviousFullName,
            UserSID = UserSid,
            UserName = PreviousUserName,
            DataKeyId = 1
        };

        var dataKey = GetDataKey(1);

        _windowsDeviceUserProviderMock.Setup(x => x.GetLoggedInUserDataByDeviceId(It.IsAny<int>()))
            .Returns(windowsDeviceLoggedInUserData);

        _dataKeyServiceMock.Setup(x => x.GetKey(dataKey.Id)).Returns(dataKey);

        _eventDispatcherMock.Setup(m => m.DispatchEvent(It.IsAny<NoUserLoggedInEvent>()));

        _windowsDeviceUserProviderMock.Setup(x => x.LogOffUserByDeviceId(windowsDeviceLoggedInUserModel.DeviceId));

        _windowsDeviceServiceMock.Setup(x =>
            x.UpdateLoggedInUserLastCheckInTime(It.IsAny<int>(), It.IsAny<DateTime>()));

        _programTraceMock.Setup(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()));

        _windowsDeviceLoggedInUserService.ProcessWindowsDeviceLoggedInUser(windowsDeviceLoggedInUserModel);

        _windowsDeviceUserProviderMock.Verify(x => x.GetLoggedInUserDataByDeviceId(It.IsAny<int>()), Times.Once);

        _eventDispatcherMock.Verify(m => m.DispatchEvent(It.IsAny<NoUserLoggedInEvent>()), Times.Once);

        _windowsDeviceUserProviderMock.Verify(
            x => x.LogOffUserByDeviceId(windowsDeviceLoggedInUserModel.DeviceId),
            Times.Once);

        _windowsDeviceServiceMock.Verify(
            x =>
            x.UpdateLoggedInUserLastCheckInTime(It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);

        _programTraceMock.Verify(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));

        _windowsDeviceLoggedInUserService.InvalidateLoggedInUserCache(windowsDeviceLoggedInUserModel.DeviceId, false);
    }

    [Test]
    public void ProcessWindowsDeviceLoggedInUser_UserLoggedInTest()
    {
        var windowsDeviceLoggedInUserModel = new WindowsDeviceLoggedInUserModel()
        {
            DevId = DevId,
            DeviceId = 10001,
            UserDomain = null,
            UserFullName = TestUserFullName,
            UserSID = UserSid,
            UserName = TestUserName,
            IsUserLoggedIn = true
        };

        var dataKey = GetDataKey(1);

        _windowsDeviceUserProviderMock.Setup(x => x.GetLoggedInUserDataByDeviceId(It.IsAny<int>()))
            .Returns((WindowsDeviceUserData)null);
        _eventDispatcherMock.Setup(m => m.DispatchEvent(It.IsAny<UserLoggedInEvent>()));

        _windowsDeviceServiceMock.Setup(x =>
            x.UpdateLoggedInUserLastCheckInTime(It.IsAny<int>(), It.IsAny<DateTime>()));

        _windowsDeviceUserProviderMock
            .Setup(x => x.ModifyLoggedInUser(It.IsAny<WindowsDeviceUserData>()));

        _dataKeyServiceMock.Setup(x => x.GetKey()).Returns(dataKey);

        _programTraceMock.Setup(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()));

        _windowsDeviceLoggedInUserService.ProcessWindowsDeviceLoggedInUser(windowsDeviceLoggedInUserModel);

        _windowsDeviceUserProviderMock.Verify(x => x.GetLoggedInUserDataByDeviceId(It.IsAny<int>()), Times.Once);

        _eventDispatcherMock.Verify(m => m.DispatchEvent(It.IsAny<UserLoggedInEvent>()), Times.Once);

        _windowsDeviceUserProviderMock
            .Verify(x => x.ModifyLoggedInUser(It.IsAny<WindowsDeviceUserData>()), Times.Once);

        _windowsDeviceServiceMock.Verify(
            x =>
                x.UpdateLoggedInUserLastCheckInTime(It.IsAny<int>(), It.IsAny<DateTime>()), Times.Once);

        _programTraceMock.Verify(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));

        _windowsDeviceLoggedInUserService.InvalidateLoggedInUserCache(windowsDeviceLoggedInUserModel.DeviceId, false);
    }

    [Test]
    public void LogOffWindowsDeviceUser_ExceptionTest()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLoggedInUserService.LogOffWindowsDeviceUser(0));
    }

    [Test]
    public void LogOffWindowsDeviceUser_SuccessTest()
    {
        _windowsDeviceUserProviderMock
            .Setup(x => x.LogOffUserByDeviceId(It.IsAny<int>()));
        _programTraceMock.Setup(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()));

        _windowsDeviceLoggedInUserService.LogOffWindowsDeviceUser(1004);

        _windowsDeviceUserProviderMock
            .Verify(x => x.LogOffUserByDeviceId(It.IsAny<int>()), Times.Once);

        _programTraceMock.Verify(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void GetWindowsDeviceLoggedInUserDataByDeviceIds_ExceptionTest()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLoggedInUserService.GetWindowsDeviceLoggedInUserDataByDeviceIds(null));
        Assert.Throws<ArgumentException>(() =>
            _windowsDeviceLoggedInUserService.GetWindowsDeviceLoggedInUserDataByDeviceIds(new List<int>()));
    }

    [Test]
    public void GetWindowsDeviceLoggedInUserDataByDeviceIds_SuccessTest()
    {
        var deviceIds = new List<int> { 10001, 10002 };
        var windowsDeviceUserData = new List<WindowsDeviceUserData>
            {
                new WindowsDeviceUserData()
                {
                    DeviceId = 10001,
                    UserDomain = null,
                    UserFullName = TestUserFullName,
                    UserSID = UserSid,
                    UserName = TestUserName,
                    LastCheckInDeviceUserTime = DateTime.MinValue,
                    DataKeyId = 1
                }
            };
        var dataKey = GetDataKey(1);
        _windowsDeviceUserProviderMock.Setup(x => x.GetLoggedInUserDataByDeviceIds(deviceIds)).Returns(windowsDeviceUserData);
        _programTraceMock.Setup(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()));
        _dataKeyServiceMock.Setup(x => x.GetKey(dataKey.Id)).Returns(dataKey);
        var result = _windowsDeviceLoggedInUserService.GetWindowsDeviceLoggedInUserDataByDeviceIds(deviceIds).FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(result.LastCheckInDeviceUserTime, windowsDeviceUserData[0].LastCheckInDeviceUserTime);
        Assert.AreEqual(result.DeviceId, windowsDeviceUserData[0].DeviceId);
        Assert.AreEqual(result.UserDomain, windowsDeviceUserData[0].UserDomain);
        Assert.AreEqual(result.UserFullName, TestUserFullName);
        Assert.AreEqual(result.UserName, TestUserName);
        Assert.AreEqual(result.UserSID, windowsDeviceUserData[0].UserSID);

        _windowsDeviceUserProviderMock
            .Verify(x => x.GetLoggedInUserDataByDeviceIds(deviceIds), Times.Once);

        _programTraceMock.Verify(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void InvalidateLoggedInUserCacheTest()
    {
        const int deviceId = 10001;
        _messagePublisherMock.Setup(x => x.Publish(It.IsAny<InvalidateLoggedInUserCacheMessage>(), ApplicableServer.Dse, ApplicableServer.Ms));

        _windowsDeviceLoggedInUserService.InvalidateLoggedInUserCache(deviceId, true);

        _messagePublisherMock.Verify(x => x.Publish(It.IsAny<InvalidateLoggedInUserCacheMessage>(), ApplicableServer.Dse, ApplicableServer.Ms), Times.Once);
    }

    private static DataKey GetDataKey(int dataKeyId)
    {
        return new DataKey
        {
            Id = dataKeyId,
            KeyData = Encoding.UTF8.GetBytes("1234"),
            DataKeyAlgorithm = DataKeyAlgorithm.Aes256,
            CreationTime = DateTime.MinValue,
            MasterKeyId = null
        };
    }
}

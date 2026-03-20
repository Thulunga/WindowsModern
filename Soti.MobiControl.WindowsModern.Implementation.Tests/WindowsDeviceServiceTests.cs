using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.SensitiveDataProtection;
using Soti.SensitiveDataProtection.Model;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
public class WindowsDeviceServiceTests
{
    private const int DeviceId = 1000001;
    private const int DeviceId2 = 1000002;
    private const string HardwareId = "A213456";
    private const string WifiSubnet = "255.255.240.0";

    private Mock<IDataKeyService> _dataKeyServiceMock;
    private Mock<ISensitiveDataEncryptionService> _sensitiveDataEncryptionServiceMock;
    private Mock<IWindowsDeviceDataProvider> _windowsDeviceProviderMock;
    private IWindowsDeviceService _windowsDeviceService;

    [SetUp]
    public void Setup()
    {
        _dataKeyServiceMock = new Mock<IDataKeyService>(MockBehavior.Strict);
        _sensitiveDataEncryptionServiceMock = new Mock<ISensitiveDataEncryptionService>(MockBehavior.Strict);
        _windowsDeviceProviderMock = new Mock<IWindowsDeviceDataProvider>(MockBehavior.Strict);

        _windowsDeviceService = new WindowsDeviceService(
            _dataKeyServiceMock.Object,
            _sensitiveDataEncryptionServiceMock.Object,
            _windowsDeviceProviderMock.Object);
    }

    [Test]
    public void ConstructorTests()
    {
        var argumentCount = 1;
        for (var i = 0; i < argumentCount; i++)
        {
            var j = 0;
            Assert.Throws<ArgumentNullException>(() => _ = new WindowsDeviceService(
                i == j++ ? null : _dataKeyServiceMock.Object,
                i == j++ ? null : _sensitiveDataEncryptionServiceMock.Object,
                i == j++ ? null : _windowsDeviceProviderMock.Object));

            argumentCount = j;
        }
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void UpdateWindowsSandBoxStatus_Throws_ArgumentOutOfRangeException(int deviceId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceService.UpdateWindowsSandBoxStatus(deviceId, true));
    }

    [Test]
    public void UpdateWindowsSandBoxStatus_Success()
    {
        _windowsDeviceProviderMock.Setup(provider => provider.UpdateWindowsSandBoxStatus(DeviceId, true));

        _windowsDeviceService.UpdateWindowsSandBoxStatus(DeviceId, true);

        _windowsDeviceProviderMock.Verify(provider => provider.UpdateWindowsSandBoxStatus(It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void GetWinSandBoxStatusByDeviceId_Throws_ArgumentOutOfRangeException(int deviceId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceService.GetWinSandBoxStatusByDeviceId(deviceId));
    }

    [Test]
    public void GetWinSandBoxStatusByDeviceId_Success()
    {
        var sandBoxDetails = GetWindowsDeviceData(DeviceId, null, false, null, true);
        _windowsDeviceProviderMock.Setup(provider => provider.Get(DeviceId)).Returns(sandBoxDetails);
        var result = _windowsDeviceService.GetWinSandBoxStatusByDeviceId(DeviceId);
        Assert.AreEqual(sandBoxDetails.IsSandBoxEnabled, result.IsSandBoxEnabled);
        _windowsDeviceProviderMock.Verify(provider => provider.Get(DeviceId), Times.Once);

        _windowsDeviceProviderMock.Setup(provider => provider.Get(DeviceId)).Returns((WindowsDeviceData)null);
        result = _windowsDeviceService.GetWinSandBoxStatusByDeviceId(DeviceId);
        Assert.AreEqual(null, result);
    }

    [Test]
    public void GetAllWinSandBoxStatus_Success()
    {
        var testWindowsDevicesList = new List<WindowsDeviceData>
            {
                GetWindowsDeviceData(DeviceId, null, false, null, true),
                GetWindowsDeviceData(DeviceId2, null, false, null, false)
            };

        _windowsDeviceProviderMock.Setup(provider => provider.GetAll()).Returns(testWindowsDevicesList);

        var result = _windowsDeviceService.GetAllWinSandBoxStatus().ToList();
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);

        Assert.AreEqual(testWindowsDevicesList[0].DeviceId, result[0].DeviceId);
        Assert.AreEqual(testWindowsDevicesList[0].IsSandBoxEnabled, result[0].IsSandBoxEnabled);

        Assert.AreEqual(testWindowsDevicesList[1].DeviceId, result[1].DeviceId);
        Assert.AreEqual(testWindowsDevicesList[1].IsSandBoxEnabled, result[1].IsSandBoxEnabled);

        _windowsDeviceProviderMock.Verify(provider => provider.GetAll(), Times.Once);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void IsDeviceLocked_Throws_ArgumentOutOfRangeException(int deviceId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceService.IsDeviceLocked(deviceId));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void IsDeviceLocked_Success(bool lockStatus)
    {
        _windowsDeviceProviderMock.Setup(provider => provider.IsDeviceLocked(DeviceId)).Returns(lockStatus);
        var result = _windowsDeviceService.IsDeviceLocked(DeviceId);
        Assert.AreEqual(lockStatus, result);
        _windowsDeviceProviderMock.Verify(provider => provider.IsDeviceLocked(DeviceId), Times.Once);
    }

    [Test]
    public void GetWinSandBoxStatusByDeviceIds_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceService.GetWinSandBoxStatusByDeviceIds(null));
    }

    [Test]
    public void GetWinSandBoxStatusByDeviceIds_Success()
    {
        var targetedDevices = new List<int> { DeviceId, DeviceId2 };
        var testLockStatus = new Dictionary<int, bool> { { DeviceId, true }, { DeviceId2, false } };
        _windowsDeviceProviderMock.Setup(provider => provider.GetSandBoxStatusByIds(targetedDevices)).Returns(testLockStatus);

        var areDevicesLockedResults = _windowsDeviceService.GetWinSandBoxStatusByDeviceIds(targetedDevices).ToList();
        Assert.IsNotNull(areDevicesLockedResults);
        Assert.AreEqual(2, areDevicesLockedResults.Count);
        Assert.AreEqual(true, areDevicesLockedResults[0].Value);
        Assert.AreEqual(false, areDevicesLockedResults[1].Value);

        _windowsDeviceProviderMock.Verify(provider => provider.GetSandBoxStatusByIds(targetedDevices), Times.Once);
    }

    [Test]
    public void AreDevicesLocked_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceService.AreDevicesLocked(null));
    }

    [Test]
    public void AreDevicesLocked_Success()
    {
        var targetedDevices = new HashSet<int> { DeviceId, DeviceId2 };
        var testLockStatus = new Dictionary<int, bool> { { DeviceId, true }, { DeviceId2, false } };
        _windowsDeviceProviderMock.Setup(provider => provider.AreDevicesLocked(targetedDevices)).Returns(testLockStatus);

        var areDevicesLockedResults = _windowsDeviceService.AreDevicesLocked(targetedDevices).ToList();
        Assert.IsNotNull(areDevicesLockedResults);
        Assert.AreEqual(2, areDevicesLockedResults.Count);
        Assert.AreEqual(true, areDevicesLockedResults[0].Value);
        Assert.AreEqual(false, areDevicesLockedResults[1].Value);

        _windowsDeviceProviderMock.Verify(provider => provider.AreDevicesLocked(targetedDevices), Times.Once);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void GetByDeviceId_Throws_ArgumentOutOfRangeException(int deviceId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceService.GetByDeviceId(deviceId));
    }

    [Test]
    public void GetByDeviceId_IsNull()
    {
        _windowsDeviceProviderMock.Setup(provider => provider.Get(DeviceId)).Returns((WindowsDeviceData)null);
        var result = _windowsDeviceService.GetByDeviceId(DeviceId);
        Assert.IsNull(result);
    }

    [Test]
    public void GetByDeviceId_NullPasscode()
    {
        var testUnlockedWindowsDeviceData = GetWindowsDeviceData(DeviceId, null, true);
        _windowsDeviceProviderMock.Setup(provider => provider.Get(DeviceId)).Returns(testUnlockedWindowsDeviceData);
        Assert.Throws<InvalidOperationException>(() => _windowsDeviceService.GetByDeviceId(DeviceId));
    }

    [Test]
    public void GetByDeviceId_Throws_ArgumentException()
    {
        var testPasscode = "123456";
        var testUnlockedWindowsDeviceData = GetWindowsDeviceData(DeviceId, null, false, testPasscode);
        _windowsDeviceProviderMock.Setup(provider => provider.Get(DeviceId)).Returns(testUnlockedWindowsDeviceData);
        Assert.Throws<InvalidOperationException>(() => _windowsDeviceService.GetByDeviceId(DeviceId));
    }

    [Test]
    public void GetByDeviceId_OsImageData_Success()
    {
        var osImageDetails = GetOsImageData(DeviceId, 1, DateTime.UtcNow);
        _windowsDeviceProviderMock.Setup(provider => provider.Get(DeviceId)).Returns(osImageDetails);
        var result = _windowsDeviceService.GetByDeviceId(DeviceId);
        Assert.AreEqual(osImageDetails.OsImageId, result.OsImageId);
        Assert.AreEqual(osImageDetails.OsImageDeploymentTime, result.OsImageDeploymentTime);
        _windowsDeviceProviderMock.Verify(provider => provider.Get(DeviceId), Times.Once);
    }

    [Test]
    public void BulkGetDeviceDetails_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => _windowsDeviceService.BulkGetDeviceDetails(null));
        Assert.AreEqual("Value cannot be null. (Parameter 'deviceIds')", exception?.Message);
    }

    [Test]
    public void BulkGetDeviceDetails_Success()
    {
        var targetedDeviceIds = new HashSet<int> { DeviceId, DeviceId2 };

        _windowsDeviceProviderMock.Setup(provider => provider.BulkGetDeviceDetails(targetedDeviceIds)).Returns((IReadOnlyDictionary<int, WindowsDeviceData>)null);
        var result = _windowsDeviceService.BulkGetDeviceDetails(targetedDeviceIds);
        Assert.AreEqual(null, result);

        var testData = new Dictionary<int, WindowsDeviceData>
        {
            {
                DeviceId,
                new WindowsDeviceData
                {
                    DeviceId = DeviceId,
                    IsLocked = true,
                    OsImageId = 1,
                    OsImageDeploymentTime = DateTime.Now
                }
            }
        };
        _windowsDeviceProviderMock.Setup(provider => provider.BulkGetDeviceDetails(targetedDeviceIds)).Returns(testData);
        result = _windowsDeviceService.BulkGetDeviceDetails(targetedDeviceIds);
        Assert.AreEqual(testData[DeviceId].OsImageId, result[DeviceId].OsImageId);
        Assert.AreEqual(testData[DeviceId].OsImageDeploymentTime, result[DeviceId].OsImageDeploymentTime);
    }

    [Test]
    public void GetByDeviceId_Unlocked_Success()
    {
        var testUnlockedWindowsDeviceData = GetWindowsDeviceData(DeviceId);
        _windowsDeviceProviderMock.Setup(provider => provider.Get(DeviceId)).Returns(testUnlockedWindowsDeviceData);

        var unlockedResult = _windowsDeviceService.GetByDeviceId(DeviceId);

        Assert.IsNotNull(unlockedResult);
        Assert.AreEqual(testUnlockedWindowsDeviceData.DeviceId, unlockedResult.DeviceId);
        Assert.AreEqual(testUnlockedWindowsDeviceData.IsLocked, unlockedResult.IsLocked);
        Assert.AreEqual(testUnlockedWindowsDeviceData.Passcode, unlockedResult.Passcode);

        _windowsDeviceProviderMock.Verify(provider => provider.Get(DeviceId), Times.Once);
        _dataKeyServiceMock.Verify(service => service.GetKey(It.IsAny<int>()), Times.Never);
        _sensitiveDataEncryptionServiceMock.Verify(service => service.Decrypt(It.IsAny<byte[]>(), It.IsAny<DataKey>()), Times.Never);
    }

    [TestCase("123456")]
    [TestCase("888888")]
    public void GetByDeviceId_Locked_Success(string passcode)
    {
        var testLockedWindowsDeviceData = GetWindowsDeviceData(DeviceId, 1, true, passcode);
        var testDataKey = GetDataKey(DeviceId, passcode);
        _windowsDeviceProviderMock.Setup(provider => provider.Get(DeviceId)).Returns(testLockedWindowsDeviceData);
        _dataKeyServiceMock.Setup(service => service.GetKey((int)testLockedWindowsDeviceData.DataKeyId)).Returns(testDataKey);
        _sensitiveDataEncryptionServiceMock.Setup(service => service.Decrypt(testLockedWindowsDeviceData.Passcode, testDataKey)).Returns(testLockedWindowsDeviceData.Passcode);

        var lockedResult = _windowsDeviceService.GetByDeviceId(DeviceId);

        Assert.IsNotNull(lockedResult);
        Assert.AreEqual(testLockedWindowsDeviceData.DeviceId, lockedResult.DeviceId);
        Assert.AreEqual(testLockedWindowsDeviceData.IsLocked, lockedResult.IsLocked);
        Assert.AreEqual(Encoding.UTF8.GetString(testLockedWindowsDeviceData.Passcode), lockedResult.Passcode);

        _windowsDeviceProviderMock.Verify(provider => provider.Get(DeviceId), Times.Once);
        _dataKeyServiceMock.Verify(service => service.GetKey((int)testLockedWindowsDeviceData.DataKeyId), Times.Once);
        _sensitiveDataEncryptionServiceMock.Verify(service => service.Decrypt(testLockedWindowsDeviceData.Passcode, testDataKey), Times.Once);
    }

    [Test]
    public void Insert_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceService.Insert(null));
    }

    [Test]
    public void BulkInsertWindowsDevices_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceService.BulkInsertWindowsDevices(null));
    }

    [Test]
    public void BulkInsertWindowsDevices_Success()
    {
        var testPasscodeBytes = Encoding.UTF8.GetBytes("123456");
        var testDataKey = GetDataKey(DeviceId, "pass123");
        var devices = new List<WindowsDeviceModel>
        {
            new WindowsDeviceModel
            {
                DeviceId = 101,
                IsLocked = true,
                Passcode = "pass123",
                OsImageId = 1,
                OsImageDeploymentTime = new DateTime(2024, 12, 1, 10, 0, 0),
                BiosPasswordStatus = BiosPasswordStatusType.NotConfigured
            },
            new WindowsDeviceModel
            {
                DeviceId = 102,
                IsLocked = false,
                Passcode = null,
                OsImageId = 2,
                OsImageDeploymentTime = new DateTime(2025, 1, 15, 15, 30, 0),
                BiosPasswordStatus = BiosPasswordStatusType.NotConfigured
            }
        };

        _windowsDeviceProviderMock.Setup(provider => provider.BulkInsertWindowsDevices(It.IsAny<List<WindowsDeviceData>>()));
        _dataKeyServiceMock.Setup(service => service.GetKey()).Returns(testDataKey);
        _sensitiveDataEncryptionServiceMock.Setup(service => service.Encrypt(It.IsAny<byte[]>(), It.IsAny<DataKey>())).Returns(testPasscodeBytes);
        _windowsDeviceService.BulkInsertWindowsDevices(devices);
        _windowsDeviceProviderMock.Verify(provider => provider.BulkInsertWindowsDevices(It.IsAny<List<WindowsDeviceData>>()), Times.Once);
    }

    [Test]
    public void Insert_NullPasscode()
    {
        var testWindowsDeviceModel = GetWindowsDeviceModel(DeviceId, true);
        _windowsDeviceProviderMock.Setup(provider => provider.Insert(It.IsAny<WindowsDeviceData>()));
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceService.Insert(testWindowsDeviceModel));
    }

    [Test]
    public void Insert_NotLocked_Passcode()
    {
        var testPasscode = "123456";
        var testUnlockedWindowsDeviceModel = GetWindowsDeviceModel(DeviceId, false, testPasscode);
        _windowsDeviceProviderMock.Setup(provider => provider.Insert(It.IsAny<WindowsDeviceData>()));
        Assert.Throws<ArgumentException>(() => _windowsDeviceService.Insert(testUnlockedWindowsDeviceModel));
    }

    [Test]
    public void Insert_Success()
    {
        var testWindowsDeviceModel = GetWindowsDeviceModel(DeviceId, false);

        _windowsDeviceProviderMock.Setup(provider => provider.Insert(It.IsAny<WindowsDeviceData>()));

        _windowsDeviceService.Insert(testWindowsDeviceModel);

        _dataKeyServiceMock.Verify(service => service.GetKey(), Times.Never);
        _sensitiveDataEncryptionServiceMock.Verify(service => service.Encrypt(It.IsAny<byte[]>(), It.IsAny<DataKey>()), Times.Never);
        _windowsDeviceProviderMock.Verify(provider => provider.Insert(It.IsAny<WindowsDeviceData>()), Times.Once);

        _dataKeyServiceMock.Invocations.Clear();
        _sensitiveDataEncryptionServiceMock.Invocations.Clear();
        _windowsDeviceProviderMock.Invocations.Clear();

        var testPasscode = "123456";
        var testPasscodeBytes = Encoding.UTF8.GetBytes(testPasscode);
        var testDataKey = GetDataKey(DeviceId, testPasscode);
        var testLockedWindowsDeviceModel = GetWindowsDeviceModel(DeviceId, true, testPasscode);

        _windowsDeviceProviderMock.Setup(provider => provider.Insert(It.IsAny<WindowsDeviceData>()));
        _dataKeyServiceMock.Setup(service => service.GetKey()).Returns(testDataKey);
        _sensitiveDataEncryptionServiceMock.Setup(service => service.Encrypt(testPasscodeBytes, testDataKey)).Returns(testPasscodeBytes);

        _windowsDeviceService.Insert(testLockedWindowsDeviceModel);

        _dataKeyServiceMock.Verify(service => service.GetKey(), Times.Once);
        _sensitiveDataEncryptionServiceMock.Verify(service => service.Encrypt(It.IsAny<byte[]>(), It.IsAny<DataKey>()), Times.Once);
        _windowsDeviceProviderMock.Verify(provider => provider.Insert(It.IsAny<WindowsDeviceData>()), Times.Once);
    }

    [Test]
    public void Update_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceService.Update(null));
    }

    [Test]
    public void Update_Throws_ArgumentException()
    {
        var testPasscode = "123456";
        var testUnlockedWindowsDeviceModel = GetWindowsDeviceModel(DeviceId, false, testPasscode);

        _windowsDeviceProviderMock.Setup(provider => provider.Update(It.IsAny<WindowsDeviceData>()));

        Assert.Throws<ArgumentException>(() => _windowsDeviceService.Update(testUnlockedWindowsDeviceModel));
    }

    [Test]
    public void Update_Passcode_ArgumentNullException()
    {
        var testUnlockedWindowsDeviceModel = GetWindowsDeviceModel(DeviceId, true);

        _windowsDeviceProviderMock.Setup(provider => provider.Update(It.IsAny<WindowsDeviceData>()));

        Assert.Throws<ArgumentNullException>(() => _windowsDeviceService.Update(testUnlockedWindowsDeviceModel));
    }

    [Test]
    public void Update_Success()
    {
        var testUnlockedWindowsDeviceModel = GetWindowsDeviceModel(DeviceId, false);

        _windowsDeviceProviderMock.Setup(provider => provider.Update(It.IsAny<WindowsDeviceData>()));

        _windowsDeviceService.Update(testUnlockedWindowsDeviceModel);

        _dataKeyServiceMock.Verify(service => service.GetKey(), Times.Never);
        _sensitiveDataEncryptionServiceMock.Verify(service => service.Encrypt(It.IsAny<byte[]>(), It.IsAny<DataKey>()), Times.Never);
        _windowsDeviceProviderMock.Verify(provider => provider.Update(It.IsAny<WindowsDeviceData>()), Times.Once);

        _dataKeyServiceMock.Invocations.Clear();
        _sensitiveDataEncryptionServiceMock.Invocations.Clear();
        _windowsDeviceProviderMock.Invocations.Clear();

        var testPasscode = "123456";
        var testPasscodeBytes = Encoding.UTF8.GetBytes(testPasscode);
        var testDataKey = GetDataKey(DeviceId, testPasscode);
        var testLockedWindowsDeviceModel = GetWindowsDeviceModel(DeviceId, true, testPasscode);

        _windowsDeviceProviderMock.Setup(provider => provider.Update(It.IsAny<WindowsDeviceData>()));
        _dataKeyServiceMock.Setup(service => service.GetKey()).Returns(testDataKey);
        _sensitiveDataEncryptionServiceMock.Setup(service => service.Encrypt(testPasscodeBytes, testDataKey)).Returns(testPasscodeBytes);

        _windowsDeviceService.Update(testLockedWindowsDeviceModel);

        _dataKeyServiceMock.Verify(service => service.GetKey(), Times.Once);
        _sensitiveDataEncryptionServiceMock.Verify(service => service.Encrypt(It.IsAny<byte[]>(), It.IsAny<DataKey>()), Times.Once);
        _windowsDeviceProviderMock.Verify(provider => provider.Update(It.IsAny<WindowsDeviceData>()), Times.Once);
    }

    [Test]
    public void UpdateLoggedInUserLastCheckInTime_ExceptionTest()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceService.UpdateLoggedInUserLastCheckInTime(0, DateTime.UtcNow));
    }

    [Test]
    public void UpdateLoggedInUserLastCheckInTime_SuccessTest()
    {
        var deviceId = 1007;
        _windowsDeviceProviderMock
            .Setup(x => x.UpdateLastCheckInDeviceUserTime(It.IsAny<int>(), It.IsAny<DateTime>()));

        _windowsDeviceService.UpdateLoggedInUserLastCheckInTime(deviceId, DateTime.UtcNow);

        _windowsDeviceProviderMock
            .Verify(x => x.UpdateLastCheckInDeviceUserTime(It.IsAny<int>(), It.IsAny<DateTime>()), Times.Once);
    }

    [TestCase(0, "SampleHardwareId")]
    [TestCase(-1, "SampleHardwareId")]
    public void UpdateHardwareId_Throws_ArgumentOutOfRangeException(int deviceId, string hardwareId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceService.UpdateHardwareId(deviceId, hardwareId));
    }

    [TestCase(1000007, null)]
    [TestCase(1000008, "")]
    [TestCase(1000009, " ")]
    public void UpdateHardwareId_Throws_ArgumentException(int deviceId, string hardwareId)
    {
        Assert.Throws<ArgumentException>(() =>
            _windowsDeviceService.UpdateHardwareId(deviceId, hardwareId));
    }

    [Test]
    public void UpdateHardwareId_SuccessTest()
    {
        var deviceId = 1007;
        _windowsDeviceProviderMock
            .Setup(x => x.UpdateHardwareId(It.IsAny<int>(), It.IsAny<string>()));

        _windowsDeviceService.UpdateHardwareId(deviceId, HardwareId);

        _windowsDeviceProviderMock
            .Verify(x => x.UpdateHardwareId(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [TestCase(0, "SampleWifiSubnet", "SampleHardwareId")]
    [TestCase(-1, "SampleWifiSubnet", "SampleHardwareId")]
    public void UpdateHardwareIdWifiSubnet_Throws_ArgumentOutOfRangeException(int deviceId, string wifiSubnet, string hardwareId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceService.UpdateHardwareIdWifiSubnet(deviceId, wifiSubnet, hardwareId));
    }

    [Test]
    public void UpdateHardwareIdWifiSubnet_SuccessTest()
    {
        var deviceId = 1007;
        _windowsDeviceProviderMock
            .Setup(x => x.UpdateHardwareIdWifiSubnet(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));

        _windowsDeviceService.UpdateHardwareIdWifiSubnet(deviceId, WifiSubnet, HardwareId);

        _windowsDeviceProviderMock
            .Verify(x => x.UpdateHardwareIdWifiSubnet(deviceId, WifiSubnet, HardwareId), Times.Once);
    }

    [Test]
    public void UpdateWindowsDeviceDetails_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceService.UpdateWindowsDeviceDetails(null));
    }

    [TestCaseSource(nameof(UpdateWifiDetailsArgumentOutOfRangeExceptionTestData))]
    public void UpdateWindowsDeviceDetails_Throws_ArgumentOutOfRangeException(WindowsModernSnapshot windowsModernSnapshot)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceService.UpdateWindowsDeviceDetails(windowsModernSnapshot));
    }

    [Test]
    public void UpdateWindowsDeviceDetails_SuccessTest()
    {
        var testData = new WindowsModernSnapshot()
        {
            DeviceId = 1007,
            WifiSubnet = WifiSubnet,
            WifiModeId = WifiModeType.ESS,
            NetworkAuthenticationId = NetworkAuthenticationType.Open,
            WirelessLanModeId = WirelessLanModeType.Auto,
            WindowsSandBoxEnabledStatus = false
        };
        _windowsDeviceProviderMock
            .Setup(x => x.UpdateWindowsDeviceDetails(It.IsAny<WindowsModernSnapshot>()));

        _windowsDeviceService.UpdateWindowsDeviceDetails(testData);

        _windowsDeviceProviderMock
            .Verify(x => x.UpdateWindowsDeviceDetails(testData), Times.Once);
    }

    [Test]
    public void UpdateDefenderScanInfo_ExceptionTest()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceService.UpdateDefenderScanInfo(0, null, null, DateTime.UtcNow));
    }

    [Test]
    public void UpdateDefenderScanInfo_SuccessTest()
    {
        _windowsDeviceProviderMock
            .Setup(x => x.UpdateDefenderScanInfo(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()));

        _windowsDeviceService.UpdateDefenderScanInfo(DeviceId, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow);

        _windowsDeviceProviderMock
            .Verify(x => x.UpdateDefenderScanInfo(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
    }

    #region UpdateOsImageInfo

    [TestCase(-1, 1, typeof(ArgumentOutOfRangeException))]
    [TestCase(0, 1, typeof(ArgumentOutOfRangeException))]
    [TestCase(1, -1, typeof(ArgumentOutOfRangeException))]
    [TestCase(1, 0, typeof(ArgumentOutOfRangeException))]
    public void UpdateOsImageInfo_Throws(int deviceId, int osImageId, Type exception)
    {
        Assert.Throws(exception, () => _windowsDeviceService.UpdateOsImageInfo(deviceId, osImageId, DateTime.Now));
    }

    [Test]
    public void UpdateOsImageInfo_Success()
    {
        _windowsDeviceProviderMock.Setup(s => s.UpdateOsImageInfo(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>()));

        _windowsDeviceService.UpdateOsImageInfo(1, 1, DateTime.UtcNow);

        _windowsDeviceProviderMock.Verify(vv => vv.UpdateOsImageInfo(1, 1, It.IsAny<DateTime>()), Times.Once);
    }

    #endregion

    private static IEnumerable<TestCaseData> UpdateWifiDetailsArgumentOutOfRangeExceptionTestData()
    {
        yield return new TestCaseData(new WindowsModernSnapshot
        {
            DeviceId = -1,
            WifiModeId = WifiModeType.ESS,
            NetworkAuthenticationId = NetworkAuthenticationType.OWE,
            WirelessLanModeId = WirelessLanModeType.Auto
        });
        yield return new TestCaseData(new WindowsModernSnapshot
        {
            DeviceId = 1000006,
            WifiModeId = (WifiModeType)11,
            NetworkAuthenticationId = NetworkAuthenticationType.OWE,
            WirelessLanModeId = WirelessLanModeType.Auto
        });
        yield return new TestCaseData(new WindowsModernSnapshot
        {
            DeviceId = 1000007,
            WifiModeId = WifiModeType.ESS,
            NetworkAuthenticationId = (NetworkAuthenticationType)111,
            WirelessLanModeId = WirelessLanModeType.Auto
        });
        yield return new TestCaseData(new WindowsModernSnapshot
        {
            DeviceId = 1000008,
            WifiModeId = WifiModeType.ESS,
            NetworkAuthenticationId = NetworkAuthenticationType.Shared,
            WirelessLanModeId = (WirelessLanModeType)11
        });
    }

    private DataKey GetDataKey(
        int dataKeyId,
        string passcode)
    {
        return new DataKey
        {
            Id = dataKeyId,
            KeyData = Encoding.UTF8.GetBytes(passcode),
            DataKeyAlgorithm = DataKeyAlgorithm.Aes256,
            CreationTime = DateTime.Now,
            MasterKeyId = null
        };
    }

    private WindowsDeviceData GetWindowsDeviceData(
        int deviceId,
        int? dataKeyId = null,
        bool isLocked = false,
        string passcode = null,
        bool isSandBoxEnabled = false)
    {
        return new WindowsDeviceData
        {
            DeviceId = deviceId,
            DataKeyId = dataKeyId,
            IsLocked = isLocked,
            Passcode = !string.IsNullOrEmpty(passcode) ? Encoding.UTF8.GetBytes(passcode) : null,
            IsSandBoxEnabled = isSandBoxEnabled
        };
    }

    private WindowsDeviceModel GetWindowsDeviceModel(
        int deviceId,
        bool isLocked,
        string passcode = null)
    {
        return new WindowsDeviceModel
        {
            DeviceId = deviceId,
            IsLocked = isLocked,
            Passcode = passcode
        };
    }

    private static WindowsDeviceData GetOsImageData(
        int deviceId,
        int osImageId,
        DateTime osImageDeployedTime)
    {
        return new WindowsDeviceData
        {
            DeviceId = deviceId,
            OsImageId = osImageId,
            OsImageDeploymentTime = osImageDeployedTime
        };
    }
}

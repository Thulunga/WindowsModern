using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Soti.MobiControl.Test.Common.Database;
using Soti.MobiControl.Test.Common.SensitiveDataProtection;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.SensitiveDataProtection;
using Soti.SensitiveDataProtection.Model;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
[IntegrationTest]
public class WindowsDeviceProviderTests : ProviderTestsBase
{
    private static readonly TestDeviceProvider TestDeviceProvider = new TestDeviceProvider();
    private readonly string _devId1 = Guid.NewGuid().ToString();
    private readonly string _devId2 = Guid.NewGuid().ToString();
    private readonly string _devId3 = Guid.NewGuid().ToString();
    private readonly string _hardwareId = "A32145";
    private readonly string _wifiSubnet = "255.255.240.0";

    private WindowsDeviceDataProvider _windowsDeviceProvider;
    private IDataKeyService _dataKeyService;
    private int _deviceId1;
    private int _deviceId2;
    private int _deviceId3;

    [SetUp]
    public void Setup()
    {
        _dataKeyService = new TestDataKeyService(Database);
        _windowsDeviceProvider = new WindowsDeviceDataProvider(Database);
    }

    [TearDown]
    public void TearDown()
    {
        DeleteTestData(_deviceId1);
        DeleteTestData(_deviceId2);
        DeleteTestData(_deviceId3);
    }

    [Test]
    public void ConstructorTests()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new WindowsDeviceDataProvider(null));
    }

    [Test]
    public void Update_Windows_SandBox_Status()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceProvider.UpdateWindowsSandBoxStatus(0, true));

        _deviceId1 = GetDeviceId(_devId1);
        var insertWindowsDeviceData = GetWindowsDeviceData(_deviceId1);
        _windowsDeviceProvider.Insert(insertWindowsDeviceData);

        var insertResult = _windowsDeviceProvider.Get(_deviceId1);
        Assert.IsNotNull(insertResult);
        Assert.AreEqual(insertResult.IsSandBoxEnabled, false);

        _windowsDeviceProvider.UpdateWindowsSandBoxStatus(_deviceId1, true);

        var updateResult = _windowsDeviceProvider.Get(_deviceId1);
        Assert.IsNotNull(updateResult);
        Assert.AreEqual(updateResult.IsSandBoxEnabled, true);
    }

    [Test]
    public void Insert_Update_Get_IsDeviceLocked_Tests()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceProvider.Insert(null));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceProvider.Get(0));
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceProvider.Update(null));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceProvider.IsDeviceLocked(0));

        _deviceId1 = GetDeviceId(_devId1);
        var insertWindowsDeviceData = GetWindowsDeviceData(_deviceId1);

        _windowsDeviceProvider.Insert(insertWindowsDeviceData);

        var insertResult = _windowsDeviceProvider.Get(_deviceId1);
        Assert.IsNotNull(insertResult);
        VerifyWindowsDeviceDetails(insertWindowsDeviceData, insertResult);

        var isDeviceLockedResult = _windowsDeviceProvider.IsDeviceLocked(_deviceId1);
        Assert.AreEqual(insertWindowsDeviceData.IsLocked, isDeviceLockedResult);

        var testPasscode = "123456";
        var testDataKey = GenerateDataKey();
        var updateWindowsDeviceData = GetWindowsDeviceData(_deviceId1, testDataKey.Id, true, testPasscode);

        _windowsDeviceProvider.Update(updateWindowsDeviceData);

        var updateResult = _windowsDeviceProvider.Get(_deviceId1);
        Assert.IsNotNull(updateResult);
        VerifyWindowsDeviceDetails(updateWindowsDeviceData, updateResult);

        isDeviceLockedResult = _windowsDeviceProvider.IsDeviceLocked(_deviceId1);
        Assert.AreEqual(updateWindowsDeviceData.IsLocked, isDeviceLockedResult);
    }

    [Test]
    public void BulkInsertWindowsDevices_Throws_Exception()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceProvider.BulkInsertWindowsDevices(null));
    }

    [Test]
    public void BulkInsertWindowsDevices_Success_Multiple_Device_Case()
    {
        _deviceId1 = GetDeviceId(_devId1);
        _deviceId2 = GetDeviceId(_devId2);

        IEnumerable<WindowsDeviceData> windowsDevices = new List<WindowsDeviceData>
        {
            GetWindowsDeviceData(_deviceId1, null, false, null, false),
            GetWindowsDeviceData(_deviceId2, null, false, null, true)
        };

        _windowsDeviceProvider.BulkInsertWindowsDevices(windowsDevices);
        var insertResults = _windowsDeviceProvider.GetAll().ToList();
        List<int> deviceIds = insertResults.Select(d => d.DeviceId).ToList();
        Assert.IsNotNull(insertResults);
        Assert.IsTrue(deviceIds.Contains(_deviceId1) && deviceIds.Contains(_deviceId2));
    }

    [Test]
    public void BulkInsertWindowsDevices_Success_Single_Device_Case()
    {
        _deviceId1 = GetDeviceId(_devId1);
        IEnumerable<WindowsDeviceData> windowsDevices = new List<WindowsDeviceData>
        {
            GetWindowsDeviceData(_deviceId1, null, false, null, false)
        };

        _windowsDeviceProvider.BulkInsertWindowsDevices(windowsDevices);
        var insertResults = _windowsDeviceProvider.GetAll().ToList();
        List<int> deviceIds = insertResults.Select(d => d.DeviceId).ToList();
        Assert.IsNotNull(insertResults);
        Assert.IsTrue(deviceIds.Contains(_deviceId1));
    }

    [Test]
    public void GetWindowsSandBoxByIdsTest()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceProvider.GetSandBoxStatusByIds(null));

        _deviceId1 = GetDeviceId(_devId1);
        _deviceId2 = GetDeviceId(_devId2);
        _deviceId3 = GetDeviceId(_devId3);

        var windowsDeviceData1 = GetWindowsDeviceData(_deviceId1, null, false, null, false);
        var windowsDeviceData2 = GetWindowsDeviceData(_deviceId2, null, false, null, true);
        var windowsDeviceData3 = GetWindowsDeviceData(_deviceId3, null, false, null, false);

        _windowsDeviceProvider.Insert(windowsDeviceData1);
        _windowsDeviceProvider.Insert(windowsDeviceData2);
        _windowsDeviceProvider.Insert(windowsDeviceData3);

        _windowsDeviceProvider.UpdateWindowsSandBoxStatus(_deviceId1, true);

        var insertResults = _windowsDeviceProvider.GetAll().ToList();
        Assert.IsNotNull(insertResults);

        var targetedDevices = new List<int> { _deviceId1, _deviceId3 };
        var isSandBoxEnabledResults = _windowsDeviceProvider.GetSandBoxStatusByIds(targetedDevices).ToList();
        Assert.IsNotNull(isSandBoxEnabledResults);

        Assert.AreEqual(2, isSandBoxEnabledResults.Count);
        Assert.AreEqual(isSandBoxEnabledResults[0].Value, true);
        Assert.AreEqual(isSandBoxEnabledResults[1].Value, false);
    }

    [Test]
    public void BulkMethodsTest()
    {
        var testDataKey = GenerateDataKey();
        var testPasscode = "123456";
        var encryptedPasscode = Encoding.UTF8.GetBytes(testPasscode);

        Assert.Throws<ArgumentNullException>(() => _windowsDeviceProvider.AreDevicesLocked(null));
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceProvider.BulkUpdate(null, true, encryptedPasscode, testDataKey.Id));

        _deviceId1 = GetDeviceId(_devId1);
        _deviceId2 = GetDeviceId(_devId2);
        _deviceId3 = GetDeviceId(_devId3);

        var windowsDeviceData1 = GetWindowsDeviceData(_deviceId1);
        var windowsDeviceData2 = GetWindowsDeviceData(_deviceId2);
        var windowsDeviceData3 = GetWindowsDeviceData(_deviceId3, testDataKey.Id, true, testPasscode);

        _windowsDeviceProvider.Insert(windowsDeviceData1);
        _windowsDeviceProvider.Insert(windowsDeviceData2);
        _windowsDeviceProvider.Insert(windowsDeviceData3);

        var insertResults = _windowsDeviceProvider.GetAll().ToList();
        Assert.IsNotNull(insertResults);
        Assert.AreEqual(3, insertResults.Count);

        VerifyWindowsDeviceDetails(windowsDeviceData1, insertResults[0]);
        VerifyWindowsDeviceDetails(windowsDeviceData2, insertResults[1]);
        VerifyWindowsDeviceDetails(windowsDeviceData3, insertResults[2]);

        var targetedDevices = new HashSet<int> { _deviceId1, _deviceId3 };

        var exception = Assert.Throws<ArgumentNullException>(() => _windowsDeviceProvider.BulkGetDeviceDetails(null));
        Assert.AreEqual("Value cannot be null. (Parameter 'deviceIds')", exception?.Message);
        var bulkResult = _windowsDeviceProvider.BulkGetDeviceDetails(targetedDevices);
        Assert.IsNotNull(bulkResult);
        Assert.AreEqual(2, bulkResult.Count);
        VerifyWindowsDeviceDetails(windowsDeviceData1, bulkResult[_deviceId1]);
        VerifyWindowsDeviceDetails(windowsDeviceData3, bulkResult[_deviceId3]);

        var areDevicesLockedResults = _windowsDeviceProvider.AreDevicesLocked(targetedDevices).ToList();
        Assert.IsNotNull(areDevicesLockedResults);
        Assert.AreEqual(2, areDevicesLockedResults.Count);
        Assert.AreEqual(windowsDeviceData1.IsLocked, areDevicesLockedResults[0].Value);
        Assert.AreEqual(windowsDeviceData3.IsLocked, areDevicesLockedResults[1].Value);

        var updateLockTargets = new HashSet<int> { _deviceId1, _deviceId2 };

        _windowsDeviceProvider.BulkUpdate(updateLockTargets, true, encryptedPasscode, testDataKey.Id);

        var updateLockedDevice1 = _windowsDeviceProvider.Get(_deviceId1);
        var updateLockedDevice2 = _windowsDeviceProvider.Get(_deviceId2);
        Assert.IsNotNull(updateLockedDevice1);
        Assert.IsNotNull(updateLockedDevice2);
        VerifyWindowsDeviceDetails(updateLockedDevice1, _deviceId1, testDataKey.Id, true, encryptedPasscode);
        VerifyWindowsDeviceDetails(updateLockedDevice2, _deviceId2, testDataKey.Id, true, encryptedPasscode);

        var updateUnLockTargets = new HashSet<int> { _deviceId2, _deviceId3 };

        _windowsDeviceProvider.BulkUpdate(updateUnLockTargets, false, null, null);

        var updateUnLockedDevice1 = _windowsDeviceProvider.Get(_deviceId2);
        var updateUnLockedDevice2 = _windowsDeviceProvider.Get(_deviceId3);
        Assert.IsNotNull(updateUnLockedDevice1);
        Assert.IsNotNull(updateUnLockedDevice2);

        VerifyWindowsDeviceDetails(updateUnLockedDevice1, _deviceId2);
        VerifyWindowsDeviceDetails(updateUnLockedDevice2, _deviceId3);

        DeleteTestData(_deviceId1);
        DeleteTestData(_deviceId2);
        DeleteTestData(_deviceId3);
    }

    [Test]
    public void UpdateLastCheckInDeviceUserTime_ExceptionTest()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceProvider.UpdateLastCheckInDeviceUserTime(0, DateTime.UtcNow));
    }

    [Test]
    public void UpdateLastCheckInDeviceUserTime_SuccessTest()
    {
        _deviceId1 = GetDeviceId(_devId1);
        var lastCheckInDeviceUserTime = new DateTime(2022, 1, 28, 10, 30, 0).ToUniversalTime();

        var testData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = null
        };

        _windowsDeviceProvider.Insert(testData);
        _windowsDeviceProvider.UpdateLastCheckInDeviceUserTime(_deviceId1, lastCheckInDeviceUserTime);

        var result = _windowsDeviceProvider.Get(_deviceId1);
        Assert.IsNotNull(result);
        Assert.AreEqual(result.LastCheckInDeviceUserTime, lastCheckInDeviceUserTime);
    }

    [TestCase(0, "SampleHardwareId")]
    [TestCase(-1, "SampleHardwareId")]
    public void UpdateHardwareId_Throws_ArgumentOutOfRangeException(int deviceId, string hardwareId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceProvider.UpdateHardwareId(deviceId, hardwareId));
    }

    [TestCase(100007, null)]
    [TestCase(1000008, "")]
    [TestCase(1000009, " ")]
    public void UpdateHardwareId_Throws_ArgumentException(int deviceId, string hardwareId)
    {
        Assert.Throws<ArgumentException>(() =>
            _windowsDeviceProvider.UpdateHardwareId(deviceId, hardwareId));
    }

    [Test]
    public void UpdateHardwareId_SuccessTest()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var testData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = null,
            HardwareId = null
        };
        _windowsDeviceProvider.Insert(testData);
        _windowsDeviceProvider.UpdateHardwareId(1001, _hardwareId);

        var result = _windowsDeviceProvider.Get(_deviceId1);
        Assert.IsNotNull(result);
    }

    [TestCase(0, "SampleWifiSubnet", "SampleHardwareId")]
    [TestCase(-1, "SampleWifiSubnet", "SampleHardwareId")]
    public void UpdateWifiSubnet_Throws_ArgumentOutOfRangeException(int deviceId, string wifiSubnet, string hardwareId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceProvider.UpdateHardwareIdWifiSubnet(deviceId, wifiSubnet, hardwareId));
    }

    [Test]
    public void UpdateWifiSubnet_SuccessTest()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var testData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = null,
            HardwareId = null
        };
        _windowsDeviceProvider.Insert(testData);
        _windowsDeviceProvider.UpdateHardwareIdWifiSubnet(_deviceId1, _wifiSubnet, _hardwareId);

        var result = _windowsDeviceProvider.Get(_deviceId1);
        Assert.IsNotNull(result);
        Assert.AreEqual(_hardwareId, result.HardwareId);
        Assert.AreEqual(_wifiSubnet, result.WifiSubnet);
    }

    [Test]
    public void UpdateWindowsDeviceDetails_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceProvider.UpdateWindowsDeviceDetails(null));
    }

    [TestCaseSource(nameof(UpdateWindowsDeviceDetailsArgumentOutOfRangeExceptionTestData))]
    public void UpdateWindowsDeviceDetails_Throws_ArgumentOutOfRangeException(WindowsModernSnapshot windowsModernSnapshot)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceProvider.UpdateWindowsDeviceDetails(windowsModernSnapshot));
    }

    [Test]
    public void UpdateWindowsDeviceDetails_SuccessTest()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var testData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = null,
            HardwareId = null
        };
        _windowsDeviceProvider.Insert(testData);
        _windowsDeviceProvider.UpdateWindowsDeviceDetails(new WindowsModernSnapshot { DeviceId = _deviceId1, WifiModeId = WifiModeType.ESS, NetworkAuthenticationId = NetworkAuthenticationType.Open, WirelessLanModeId = WirelessLanModeType.Auto });

        var result = _windowsDeviceProvider.Get(_deviceId1);
        Assert.IsNotNull(result);
        Assert.AreEqual(WifiModeType.ESS, result.WifiModeId);
        Assert.AreEqual(NetworkAuthenticationType.Open, result.NetworkAuthenticationId);
        Assert.AreEqual(WirelessLanModeType.Auto, result.WirelessLanModeId);
    }

    [Test]
    public void UpdateDefenderScanInfo_SuccessTest()
    {
        _deviceId1 = GetDeviceId(_devId1);
        var lastQuickScanTime = new DateTime(2024, 09, 04, 10, 30, 0).ToUniversalTime();
        var lastFullScanTime = new DateTime(2023, 10, 03, 10, 35, 0).ToUniversalTime();
        var lastSyncTime = new DateTime(2024, 10, 04, 16, 30, 0).ToUniversalTime();

        var testData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            AntivirusLastQuickScanTime = null,
            AntivirusLastFullScanTime = null,
            AntivirusLastSyncTime = null,
        };

        _windowsDeviceProvider.Insert(testData);
        _windowsDeviceProvider.UpdateDefenderScanInfo(_deviceId1, lastQuickScanTime, lastFullScanTime, lastSyncTime);

        var result = _windowsDeviceProvider.Get(_deviceId1);
        Assert.IsNotNull(result);
        Assert.AreEqual(result.AntivirusLastQuickScanTime, lastQuickScanTime);
        Assert.AreEqual(result.AntivirusLastFullScanTime, lastFullScanTime);
        Assert.AreEqual(result.AntivirusLastSyncTime, lastSyncTime);
    }

    #region UpdateOsImageInfo

    [TestCase(-1, typeof(ArgumentOutOfRangeException))]
    [TestCase(0, typeof(ArgumentOutOfRangeException))]
    public void UpdateOsImageInfo_Throws(int deviceId, Type exception)
    {
        Assert.Throws(exception, () => _windowsDeviceProvider.UpdateOsImageInfo(deviceId, null, DateTime.Now));
    }

    [Test]
    public void UpdateOsImageInfo_Success()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var testData = new WindowsDeviceData
        {
            DeviceId = _deviceId1
        };

        _windowsDeviceProvider.Insert(testData);
        var result = _windowsDeviceProvider.Get(_deviceId1);
        Assert.IsNotNull(result);
        Assert.IsNull(result.OsImageDeploymentTime);
        _windowsDeviceProvider.UpdateOsImageInfo(_deviceId1, null, DateTime.UtcNow);
        result = _windowsDeviceProvider.Get(_deviceId1);
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.OsImageDeploymentTime);
    }

    #endregion

    [TestCase(0, (byte)1, "deviceId")] // invalid deviceId
    [TestCase(1, (byte)255, "biosPasswordStatusId")]
    public void UpdateBiosPasswordStatusId_Throws_On_Invalid_Input(int deviceId, byte statusId, string expectedParam)
    {
        _deviceId1 = GetDeviceId(_devId1);
        var testData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
        };

        _windowsDeviceProvider.Insert(testData);

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceProvider.UpdateBiosPasswordStatusId(deviceId == 1 ? _deviceId1 : deviceId, statusId));

        Assert.That(ex.ParamName, Is.EqualTo(expectedParam));
    }

    [Test]
    public void UpdateBiosPasswordStatusId_Updates_Status_Successfully()
    {
        _deviceId1 = GetDeviceId(_devId1);
        var testData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
        };

        _windowsDeviceProvider.Insert(testData);

        var newStatus = (byte)BiosPasswordStatusType.Enforced;

        _windowsDeviceProvider.UpdateBiosPasswordStatusId(_deviceId1, newStatus);

        var updated = _windowsDeviceProvider.Get(_deviceId1);
        Assert.IsNotNull(updated);
        Assert.AreEqual(BiosPasswordStatusType.Enforced, updated.BiosPasswordStatusId);
    }

    private static IEnumerable<TestCaseData> UpdateWindowsDeviceDetailsArgumentOutOfRangeExceptionTestData()
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

    private static int GetDeviceId(string devId)
    {
        return TestDeviceProvider.EnsureDeviceRecord(devId, 1004);
    }

    private DataKey GenerateDataKey()
    {
        return _dataKeyService.GetKey();
    }

    private WindowsDeviceData GetWindowsDeviceData(
        int deviceId,
        int? dataKeyId = null,
        bool isLocked = false,
        string passcode = null,
        bool isSandBoxEnabled = false,
        int? osImageId = null,
        DateTime? osImageDeploymentTime = null)
    {
        return new WindowsDeviceData
        {
            DeviceId = deviceId,
            DataKeyId = dataKeyId,
            IsLocked = isLocked,
            Passcode = !string.IsNullOrEmpty(passcode) ? Encoding.UTF8.GetBytes(passcode) : null,
            IsSandBoxEnabled = isSandBoxEnabled,
            OsImageId = osImageId,
            OsImageDeploymentTime = osImageDeploymentTime
        };
    }

    private void VerifyWindowsDeviceDetails(WindowsDeviceData expectedWindowsDeviceData, WindowsDeviceData actualWindowsDeviceData)
    {
        Assert.AreEqual(expectedWindowsDeviceData.DeviceId, actualWindowsDeviceData.DeviceId);
        Assert.AreEqual(expectedWindowsDeviceData.DataKeyId, actualWindowsDeviceData.DataKeyId);
        Assert.AreEqual(expectedWindowsDeviceData.IsLocked, actualWindowsDeviceData.IsLocked);
        Assert.AreEqual(expectedWindowsDeviceData.Passcode, actualWindowsDeviceData.Passcode);
        Assert.AreEqual(expectedWindowsDeviceData.OsImageId, actualWindowsDeviceData.OsImageId);
        Assert.AreEqual(expectedWindowsDeviceData.OsImageDeploymentTime, actualWindowsDeviceData.OsImageDeploymentTime);
    }

    private void VerifyWindowsDeviceDetails(WindowsDeviceData actualWindowsDeviceData, int deviceId, int? dataKeyId = null, bool isLocked = false, byte[] passcode = null)
    {
        Assert.AreEqual(deviceId, actualWindowsDeviceData.DeviceId);
        Assert.AreEqual(dataKeyId, actualWindowsDeviceData.DataKeyId);
        Assert.AreEqual(isLocked, actualWindowsDeviceData.IsLocked);
        Assert.AreEqual(passcode, actualWindowsDeviceData.Passcode);
    }

    private void DeleteTestData(int deviceId)
    {
        if (deviceId <= 0)
        {
            return;
        }

        Database.CreateCommand(
            $"delete from DeviceAntivirusThreatStatus where DeviceId = '{deviceId}'").ExecuteNonQuery();
        TestDeviceProvider.DeleteDevice(deviceId);
    }
}

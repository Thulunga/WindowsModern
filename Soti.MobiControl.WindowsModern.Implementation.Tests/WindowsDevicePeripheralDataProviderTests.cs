using NUnit.Framework;
using Soti.MobiControl.Test.Common.Database;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;
using System;
using Soti.MobiControl.WindowsModern.Models.Enums;
using System.ComponentModel;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
[IntegrationTest]
internal sealed class WindowsDevicePeripheralDataProviderTests : ProviderTestsBase
{
    private static readonly TestDeviceProvider TestDeviceProvider = new();
    private readonly string _devId1 = Guid.NewGuid().ToString();
    private WindowsDevicePeripheralDataProvider _windowsDevicePeripheralProvider;
    private PeripheralDataProvider _peripheralProvider;
    private int _deviceId1;

    [TearDown]
    public void TearDown()
    {
        DeleteTestData(_deviceId1);
    }

    [SetUp]
    public void Setup()
    {
        _windowsDevicePeripheralProvider = new WindowsDevicePeripheralDataProvider(Database);
        _peripheralProvider = new PeripheralDataProvider(Database);
    }

    [Test]
    public void ConstructorTests()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDevicePeripheralDataProvider(null));
    }

    [Test]
    public void GetDevicePeripheralSummaryByDeviceId_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDevicePeripheralProvider.GetDevicePeripheralSummaryByDeviceId(0));
    }

    [Test]
    public void GetDevicePeripheralSummaryByDeviceId_Success()
    {
        _deviceId1 = GetDeviceId(_devId1);
        var peripheralData = new PeripheralData
        {
            Name = "peripheral5",
            ManufacturerId = 4,
            PeripheralTypeId = 1
        };
        var peripheralId = _peripheralProvider.InsertPeripheralData(peripheralData);

        var windowsDevicePeripheralData = GetWindowsDevicePeripheralData(
            _deviceId1,
            peripheralId,
            DevicePeripheralStatus.Connected,
            "version1");

        _windowsDevicePeripheralProvider.BulkModify(_deviceId1, new[] { windowsDevicePeripheralData });
        var devicePeripheralData = _windowsDevicePeripheralProvider.GetDevicePeripheralSummaryByDeviceId(_deviceId1);

        Assert.NotNull(peripheralData);
        foreach (var peripheral in devicePeripheralData)
        {
            Assert.AreEqual(peripheral.PeripheralId, peripheralId);
            Assert.AreEqual(peripheral.Status, windowsDevicePeripheralData.Status);
            Assert.AreEqual(peripheral.Version, windowsDevicePeripheralData.Version);
        }

        // Tear down.
        _windowsDevicePeripheralProvider.DeleteDevicePeripheralData(_deviceId1);
        DeleteTestData(_deviceId1);
        _peripheralProvider.BulkDeletePeripheralData(peripheralId);
    }

    [Test]
    public void BulkModify_Exception()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDevicePeripheralProvider.BulkModify(1, null));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDevicePeripheralProvider.BulkModify(0, null));
    }

    [Test]
    public void DeleteDevicePeripheralData_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDevicePeripheralProvider.DeleteDevicePeripheralData(0));
    }

    [Test]
    public void CleanUpObsoleteWindowsPeripheralData_Success()
    {
        _deviceId1 = GetDeviceId(_devId1);
        var peripheralData = new PeripheralData
        {
            Name = "peripheral5",
            ManufacturerId = 4,
            PeripheralTypeId = 1
        };
        var peripheralId = _peripheralProvider.InsertPeripheralData(peripheralData);

        var windowsDevicePeripheralData = GetWindowsDevicePeripheralData(
            _deviceId1,
            peripheralId,
            DevicePeripheralStatus.Connected,
            "version1");

        _windowsDevicePeripheralProvider.BulkModify(_deviceId1, new[] { windowsDevicePeripheralData });
        var devicePeripheralData = _windowsDevicePeripheralProvider.GetDevicePeripheralSummaryByDeviceId(_deviceId1);

        Assert.NotNull(peripheralData);
        foreach (var peripheral in devicePeripheralData)
        {
            Assert.AreEqual(peripheral.PeripheralId, peripheralId);
            Assert.AreEqual(peripheral.Status, windowsDevicePeripheralData.Status);
            Assert.AreEqual(peripheral.Version, windowsDevicePeripheralData.Version);
        }

        windowsDevicePeripheralData.Status = DevicePeripheralStatus.Disconnected;

        _windowsDevicePeripheralProvider.Update(
            windowsDevicePeripheralData,
            DateTime.UtcNow.AddHours(-24)
        );

        _windowsDevicePeripheralProvider.CleanUpObsoleteWindowsPeripheralData(out var deletedPeripheralData);
        Assert.AreNotEqual(0, deletedPeripheralData);

        // Tear down.
        _windowsDevicePeripheralProvider.DeleteDevicePeripheralData(_deviceId1);
        DeleteTestData(_deviceId1);
        _peripheralProvider.BulkDeletePeripheralData(peripheralId);
    }

    [Test]
    public void DeleteDevicePeripherals_Test()
    {
        _deviceId1 = GetDeviceId(_devId1);
        var peripheralData = new PeripheralData
        {
            Name = "peripheral5",
            ManufacturerId = 4,
            PeripheralTypeId = 1
        };
        var peripheralId = _peripheralProvider.InsertPeripheralData(peripheralData);

        var windowsDevicePeripheralData = GetWindowsDevicePeripheralData(
            _deviceId1,
            peripheralId,
            DevicePeripheralStatus.Connected,
            "version1");

        _windowsDevicePeripheralProvider.BulkModify(_deviceId1, new[] { windowsDevicePeripheralData });
        var devicePeripheralData = _windowsDevicePeripheralProvider.GetDevicePeripheralSummaryByDeviceId(_deviceId1);
        Assert.NotNull(peripheralId);
        Assert.NotNull(devicePeripheralData);

        _windowsDevicePeripheralProvider.DeleteDevicePeripheralByDeviceIdAndGetPeripheralData(_deviceId1);
        var devicePeripheralData2 = _windowsDevicePeripheralProvider.GetDevicePeripheralSummaryByDeviceId(_deviceId1);
        Assert.AreEqual(0, devicePeripheralData2.Count);
    }

    [Test]
    public void UpdateIndividualPeripheralData_Exception()
    {
        var windowsDevicePeripheralData = new WindowsDevicePeripheralData()
        {
            DeviceId = 1001,
            PeripheralId = 0,
            Status = DevicePeripheralStatus.Connected,
        };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDevicePeripheralProvider.Update(windowsDevicePeripheralData, DateTime.UtcNow.AddHours(-24)));

        windowsDevicePeripheralData.DeviceId = 0;
        windowsDevicePeripheralData.PeripheralId = 1;
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDevicePeripheralProvider.Update(windowsDevicePeripheralData, DateTime.UtcNow.AddHours(-24)));

        windowsDevicePeripheralData.DeviceId = 1;
        windowsDevicePeripheralData.PeripheralId = 1;
        windowsDevicePeripheralData.Status = (DevicePeripheralStatus)99;
        Assert.Throws<InvalidEnumArgumentException>(() =>
            _windowsDevicePeripheralProvider.Update(windowsDevicePeripheralData, DateTime.Now));

        windowsDevicePeripheralData.DeviceId = 1;
        windowsDevicePeripheralData.PeripheralId = 1;
        windowsDevicePeripheralData.Status = DevicePeripheralStatus.Connected;
        Assert.Throws<ArgumentException>(() =>
            _windowsDevicePeripheralProvider.Update(windowsDevicePeripheralData, default(DateTime)));
    }

    private void DeleteTestData(int deviceId)
    {
        if (deviceId <= 0)
        {
            return;
        }

        TestDeviceProvider.DeleteDevice(deviceId);
    }

    private static int GetDeviceId(string devId)
    {
        return TestDeviceProvider.EnsureDeviceRecord(devId, 1004);
    }
    private static WindowsDevicePeripheralData GetWindowsDevicePeripheralData(
        int deviceId,
        int peripheralId,
        DevicePeripheralStatus status,
        string version)
    {
        var x = new WindowsDevicePeripheralData
        {
            DeviceId = deviceId,
            Status = status,
            Version = version,
            PeripheralId = peripheralId
        };

        return x;
    }
}

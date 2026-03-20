using System;
using System.Collections.Generic;
using NUnit.Framework;
using Soti.MobiControl.Test.Common.Database;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
[IntegrationTest]
internal sealed class DeviceGroupPeripheralDataProviderTests : ProviderTestsBase
{
    private static int _peripheralId;
    private static readonly TestDeviceProvider TestDeviceProvider = new();
    private readonly string _devId1 = Guid.NewGuid().ToString();
    private static int _deviceId;
    private DeviceGroupPeripheralDataProvider _deviceGroupPeripheralDataProvider;
    private PeripheralDataProvider _peripheralDataProvider;
    private WindowsDevicePeripheralDataProvider _windowsDevicePeripheralDataProvider;

    [SetUp]
    public void Setup()
    {
        _deviceGroupPeripheralDataProvider = new DeviceGroupPeripheralDataProvider(Database);
        _peripheralDataProvider = new PeripheralDataProvider(Database);
        _windowsDevicePeripheralDataProvider = new WindowsDevicePeripheralDataProvider(Database);
        _peripheralId = 0;
    }

    [TearDown]
    public void Teardown()
    {
        if (_deviceId > 0)
        {
            _windowsDevicePeripheralDataProvider.DeleteDevicePeripheralData(_deviceId);
            TestDeviceProvider.DeleteDevice(_deviceId);
        }

        if (_peripheralId > 0)
        {
            _peripheralDataProvider.BulkDeletePeripheralData(_peripheralId);
        }
    }

    [Test]
    public void GetDeviceGroupPeripherals_Exception()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _deviceGroupPeripheralDataProvider.GetDeviceGroupPeripherals(null));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _deviceGroupPeripheralDataProvider.GetDeviceGroupPeripherals(new[] { 0, -1 }));
    }

    [Test]
    public void GetDeviceGroupPeripherals_Success()
    {
        _deviceId = GetDeviceId(_devId1);
        var peripheralData = new PeripheralData
        {
            ManufacturerId = 1,
            Name = "Peripheral 1",
            PeripheralTypeId = 1
        };

        _peripheralId = _peripheralDataProvider.InsertPeripheralData(peripheralData);

        var windowsDevicePeripheralData = new List<WindowsDevicePeripheralData>
        {
            new WindowsDevicePeripheralData
            {
                DeviceId = _deviceId,
                PeripheralId = _peripheralId,
                Version = "23.5678.12"
            }
        };

        _windowsDevicePeripheralDataProvider.BulkModify(_deviceId, windowsDevicePeripheralData);
        var result = _deviceGroupPeripheralDataProvider.GetDeviceGroupPeripherals(new[] { 2 });
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count);
    }

    [Test]
    public void GetPeripheralSummaryByFamilyIdAndGroupIds_Exception()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _deviceGroupPeripheralDataProvider.GetPeripheralSummaryByFamilyIdAndGroupIds(8, null));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _deviceGroupPeripheralDataProvider.GetPeripheralSummaryByFamilyIdAndGroupIds(8, new[] { 0, -1 }));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _deviceGroupPeripheralDataProvider.GetPeripheralSummaryByFamilyIdAndGroupIds(0, new[] { 1 }));
    }

    [Test]
    public void GetPeripheralSummaryByFamilyIdAndGroupIds_Success()
    {
        _deviceId = GetDeviceId(_devId1);
        var peripheralData = new PeripheralData
        {
            ManufacturerId = 1,
            Name = "Peripheral 1",
            PeripheralTypeId = 1
        };

        _peripheralId = _peripheralDataProvider.InsertPeripheralData(peripheralData);

        var windowsDevicePeripheralData = new List<WindowsDevicePeripheralData>
        {
            new WindowsDevicePeripheralData
            {
                DeviceId = _deviceId,
                PeripheralId = _peripheralId,
                Version = "23.5678.12"
            }
        };

        _windowsDevicePeripheralDataProvider.BulkModify(_deviceId, windowsDevicePeripheralData);
        var result = _deviceGroupPeripheralDataProvider.GetPeripheralSummaryByFamilyIdAndGroupIds(8, new[] { 2 });
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count);
    }

    private static int GetDeviceId(string devId)
    {
        return TestDeviceProvider.EnsureDeviceRecord(devId, 1004);
    }
}

using NUnit.Framework;
using Soti.MobiControl.WindowsModern.Implementation.Converters;
using System;
using Soti.MobiControl.WindowsModern.Implementation.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests.Converters;

[TestFixture]
internal sealed class DeviceHardwareDataConverterTest
{
    private const string HardwareManufacturerName = "Microsoft";
    private const string HardwareName = "Physical Memory";
    private const int DeviceHardwareType = 1;
    private const int HardwareStatusId = 2;
    private const string HardwareSerialNumber = "M0001";

    [Test]
    public void ToDeviceHardwareModel_ThrowsArgumentNullException_Test()
    {
        Assert.Throws<ArgumentNullException>(() => DeviceHardwareDataConverter.ToDeviceHardwareModel(null));
    }

    [Test]
    public void ToDeviceHardwareModel_Test()
    {
        var deviceHardwareData = new DeviceHardwareData
        {
            DeviceHardwareManufacturerName = HardwareManufacturerName,
            DeviceHardwareName = HardwareName,
            DeviceHardwareTypeId = DeviceHardwareType,
            HardwareStatusId = HardwareStatusId,
            DeviceHardwareSerialNumber = HardwareSerialNumber
        };

        var deviceHardwareModel = DeviceHardwareDataConverter.ToDeviceHardwareModel(deviceHardwareData);
        Assert.IsNotNull(deviceHardwareModel);
        Assert.AreEqual(deviceHardwareModel.HardwareManufacturerName, deviceHardwareData.DeviceHardwareManufacturerName);
        Assert.AreEqual(deviceHardwareModel.HardwareName, deviceHardwareData.DeviceHardwareName);
        Assert.AreEqual((int)deviceHardwareModel.DeviceHardwareType, deviceHardwareData.DeviceHardwareTypeId);
        Assert.AreEqual((int)deviceHardwareModel.HardwareStatus, deviceHardwareData.HardwareStatusId);
        Assert.AreEqual(deviceHardwareModel.HardwareSerialNumber, deviceHardwareData.DeviceHardwareSerialNumber);
    }
}

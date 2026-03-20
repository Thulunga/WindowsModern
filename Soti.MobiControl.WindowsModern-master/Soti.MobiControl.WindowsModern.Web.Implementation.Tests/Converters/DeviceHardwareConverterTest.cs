using NUnit.Framework;
using System;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.MobiControl.WindowsModern.Web.Implementation.Converters;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests.Converters;

internal sealed class DeviceHardwareConverterTest
{
    private const string HardwareManufacturerName = "Microsoft";
    private const string HardwareName = "Physical Memory";
    private const int DeviceHardwareType = 1;
    private const int HardwareStatus = 2;
    private const string HardwareSerialNumber = "M0001";

    [Test]
    public void ToDeviceLocalUserSummary_ExceptionTest()
    {
        Assert.Throws<ArgumentNullException>(() => DeviceHardwareConverter.ToDeviceHardwareSummary(null));
    }

    [Test]
    public void ToDeviceHardwareModel_Test()
    {
        var deviceHardwareModel = new DeviceHardwareSummary
        {
            HardwareManufacturerName = HardwareManufacturerName,
            HardwareName = HardwareName,
            DeviceHardwareType = (DeviceHardwareType)DeviceHardwareType,
            HardwareStatus = (HardwareStatus)HardwareStatus,
            HardwareSerialNumber = HardwareSerialNumber
        };

        var deviceHardwareSummary = DeviceHardwareConverter.ToDeviceHardwareSummary(deviceHardwareModel);
        Assert.IsNotNull(deviceHardwareSummary);
        Assert.AreEqual(deviceHardwareSummary.HardwareManufacturerName, deviceHardwareModel.HardwareManufacturerName);
        Assert.AreEqual(deviceHardwareSummary.HardwareName, deviceHardwareModel.HardwareName);
        Assert.AreEqual((DeviceHardwareType)deviceHardwareSummary.DeviceHardwareType, deviceHardwareModel.DeviceHardwareType);
        Assert.AreEqual((HardwareStatus)deviceHardwareSummary.HardwareStatus, deviceHardwareModel.HardwareStatus);
        Assert.AreEqual(deviceHardwareSummary.HardwareSerialNumber, deviceHardwareModel.HardwareSerialNumber);
    }
}

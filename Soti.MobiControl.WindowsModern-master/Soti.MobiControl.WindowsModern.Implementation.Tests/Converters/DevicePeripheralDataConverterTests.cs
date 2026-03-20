using System;
using Soti.MobiControl.WindowsModern.Implementation.Converters;
using NUnit.Framework;
using Soti.MobiControl.WindowsModern.Implementation.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests.Converters;

[TestFixture]
internal sealed class DevicePeripheralDataConverterTests
{
    [Test]
    public void ToDevicePeripheralsModel_ExceptionTest()
    {
        Assert.Throws<ArgumentNullException>(() => DevicePeripheralDataConverter.ToDevicePeripheralsModel(null, null, null, 0));
    }

    [TestCase(" ", "manufacturerName1")]
    [TestCase(null, "manufacturerName2")]
    [TestCase("peripheralName1", " ")]
    [TestCase("peripheralName2", null)]
    public void ToDevicePeripheralsModel_ArgumentExceptionTest(string peripheralName, string manufacturerName)
    {
        Assert.Throws<ArgumentException>(() => new WindowsDevicePeripheralData().ToDevicePeripheralsModel(peripheralName, manufacturerName, 1));
    }

    [Test]
    public void ToDevicePeripheralData_ArgumentNullExceptionTest()
    {
        Assert.Throws<ArgumentNullException>(() => DevicePeripheralDataConverter.ToDevicePeripheralData(null, 1));
    }

    [Test]
    public void ToDevicePeripheralData_ArgumentOutOfRangeExceptionTest()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new WindowsDevicePeripheralSnapShot().ToDevicePeripheralData(0));
    }
}

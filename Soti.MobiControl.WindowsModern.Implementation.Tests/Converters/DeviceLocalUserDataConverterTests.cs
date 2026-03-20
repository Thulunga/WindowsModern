using System;
using NUnit.Framework;
using Soti.MobiControl.WindowsModern.Implementation.Converters;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests.Converters;

[TestFixture]
public class DeviceLocalUserDataConverterTests
{
    [Test]
    public void ToWindowsDeviceUserData_ExceptionTest()
    {
        Assert.Throws<ArgumentNullException>(() => DeviceLocalUserDataConverter.ToWindowsDeviceUserData(null, null, default));
    }

    [Test]
    public void ToDeviceLocalUsersBasicModel_ExceptionTest()
    {
        Assert.Throws<ArgumentNullException>(() => DeviceLocalUserDataConverter.ToDeviceLocalUsersBasicModel(null, null));
    }

    [Test]
    public void ToDeviceLocalUsersModel_ExceptionTest()
    {
        Assert.Throws<ArgumentNullException>(() => DeviceLocalUserDataConverter.ToDeviceLocalUsersModel(null, null, null));
    }
}

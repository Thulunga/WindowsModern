using System;
using System.Text;
using NUnit.Framework;
using Soti.MobiControl.WindowsModern.Implementation.Converters;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests.Converters;

[TestFixture]
public class WindowsDeviceLoggedInUserDataConverterTests
{
    [Test]
    public void ToWindowsDeviceUserData_ExceptionTest()
    {
        Assert.Throws<ArgumentNullException>(() => WindowsDeviceLoggedInUserDataConverter.ToWindowsDeviceUserData(null, default));
    }

    [Test]
    public void ToWindowsDeviceUserData_SuccessTest()
    {
        var windowsDeviceLoggedInUserModel = new WindowsDeviceLoggedInUserModel()
        {
            DeviceId = 10001,
            UserDomain = null,
            UserFullName = "TestFullName",
            UserSID = "S-100798-280198-220222",
            UserName = "TestUserName",
            WindowsDeviceUserType = WindowsDeviceUserType.Domain,
            IsUserLoggedIn = true,
        };
        var dataKeyId = 1;
        var result = windowsDeviceLoggedInUserModel.ToWindowsDeviceUserData(dataKeyId);
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(result.WindowsDeviceUserId, windowsDeviceLoggedInUserModel.WindowsDeviceUserId);
        Assert.AreEqual(result.DeviceId, windowsDeviceLoggedInUserModel.DeviceId);
        Assert.AreEqual(result.IsUserLoggedIn, windowsDeviceLoggedInUserModel.IsUserLoggedIn);
        Assert.AreEqual(result.UserFullName, Encoding.UTF8.GetBytes("TestFullName"));
        Assert.AreEqual(result.UserDomain, windowsDeviceLoggedInUserModel.UserDomain);
        Assert.AreEqual(result.UserName, Encoding.UTF8.GetBytes("TestUserName"));
        Assert.AreEqual(result.UserSID, windowsDeviceLoggedInUserModel.UserSID);
        Assert.AreEqual(result.WindowsDeviceUserType, windowsDeviceLoggedInUserModel.WindowsDeviceUserType);
        Assert.AreEqual(result.DataKeyId, dataKeyId);
    }

    [Test]
    public void ToWindowsDeviceUserModel_ExceptionTest()
    {
        Assert.Throws<ArgumentNullException>(() => WindowsDeviceLoggedInUserDataConverter.ToWindowsDeviceUserModel(null));
    }

    [Test]
    public void ToWindowsDeviceUserModel_SuccessTest()
    {
        var windowsDeviceUserData = new WindowsDeviceUserData()
        {
            WindowsDeviceUserId = 1,
            DeviceId = 10001,
            UserDomain = null,
            UserFullName = "TestFullName",
            UserSID = "S-100798-280198-220222",
            UserName = "TestUserName",
            WindowsDeviceUserType = WindowsDeviceUserType.Domain,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        var result = windowsDeviceUserData.ToWindowsDeviceUserModel();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(result.WindowsDeviceUserId, windowsDeviceUserData.WindowsDeviceUserId);
        Assert.AreEqual(result.DeviceId, windowsDeviceUserData.DeviceId);
        Assert.AreEqual(result.UserFullName, "TestFullName");
        Assert.AreEqual(result.UserDomain, windowsDeviceUserData.UserDomain);
        Assert.AreEqual(result.UserName, "TestUserName");
        Assert.AreEqual(result.UserSID, windowsDeviceUserData.UserSID);
        Assert.AreEqual(result.WindowsDeviceUserType, windowsDeviceUserData.WindowsDeviceUserType);
        Assert.AreEqual(result.LastCheckInDeviceUserTime, windowsDeviceUserData.LastCheckInDeviceUserTime);
    }
}

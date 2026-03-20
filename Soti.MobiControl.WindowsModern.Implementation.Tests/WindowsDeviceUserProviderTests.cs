using Moq;
using NUnit.Framework;
using Soti.Diagnostics;
using Soti.MobiControl.Test.Common.Database;
using Soti.MobiControl.Test.Common.SensitiveDataProtection;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.SensitiveDataProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
[IntegrationTest]
public class WindowsDeviceUserProviderTests : ProviderTestsBase
{
    private static readonly TestDeviceProvider TestDeviceProvider = new();
    private readonly string _devId1 = Guid.NewGuid().ToString();
    private readonly string _devId2 = Guid.NewGuid().ToString();

    private WindowsDeviceUserProvider _windowsDeviceUserProvider;
    private WindowsDeviceDataProvider _windowsDeviceProvider;
    private IDataKeyService _dataKeyService;
    private Mock<IProgramTrace> _programTraceMock;

    private int _deviceId1;
    private int _deviceId2;
    private readonly IEnumerable<string> _userGroups = ["s1", "s2"];

    [SetUp]
    public void Setup()
    {
        _programTraceMock = new Mock<IProgramTrace>();
        _windowsDeviceUserProvider = new WindowsDeviceUserProvider(Database, _programTraceMock.Object);
        _windowsDeviceProvider = new WindowsDeviceDataProvider(Database);
        _dataKeyService = new TestDataKeyService(Database);
    }

    [TearDown]
    public void TearDown()
    {
        DeleteTestData(_deviceId1);
        DeleteTestData(_deviceId2);
    }

    [Test]
    public void ConstructorTests()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new WindowsDeviceUserProvider(null, null));
    }

    [Test]
    public void GetAllLocalUsersDataByDeviceId_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceUserProvider.GetAllLocalUsersDataByDeviceId(0));
    }

    [Test]
    public void GetAllLocalUsersBasicDataByDeviceId_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceUserProvider.GetAllLocalUsersBasicDataByDeviceId(0));
    }

    [Test]
    public void GetAllLocalUsersBasicDataByDeviceId_Success()
    {
        _deviceId1 = GetDeviceId(_devId1);
        var windowsDeviceData1 = GetWindowsDeviceUserData("sid1", "userName1", _userGroups, _deviceId1, "samplePassword1", DateTime.UtcNow, GenerateDataKey(), true);
        _windowsDeviceUserProvider.Insert(windowsDeviceData1);
        var localUserData = _windowsDeviceUserProvider.GetAllLocalUsersBasicDataByDeviceId(_deviceId1);
        foreach (var localUser in localUserData)
        {
            Assert.AreEqual(localUser.UserName, windowsDeviceData1.UserName);
            Assert.AreEqual(localUser.UserSID, windowsDeviceData1.UserSID);
            Assert.AreEqual(localUser.IsMobiControlCreated, windowsDeviceData1.IsMobiControlCreated);
        }
    }

    [Test]
    public void GetAllLocalUsersData_Success()
    {
        var initialCount = _windowsDeviceUserProvider.GetAllLocalUsersData().Count();
        _deviceId1 = GetDeviceId(_devId1);
        var windowsDeviceData1 = GetWindowsDeviceUserData("sid1", "userName1", _userGroups, _deviceId1, "samplePassword1", DateTime.UtcNow, GenerateDataKey(), true);
        _windowsDeviceUserProvider.Insert(windowsDeviceData1);
        var localUserData = _windowsDeviceUserProvider.GetAllLocalUsersData();
        Assert.AreEqual(initialCount + 1, localUserData.Count());
    }

    [Test]
    public void GetLocalUsersDataByDeviceIdAndSid_ArgumentExceptions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceUserProvider.GetLocalUsersDataByDeviceIdAndSid(0, "sid"));

        Assert.Throws<ArgumentException>(() =>
            _windowsDeviceUserProvider.GetLocalUsersDataByDeviceIdAndSid(1, string.Empty));
    }

    [Test]
    public void GetLocalUserNamesByDeviceIdAndSids_ExceptionTest()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceUserProvider.GetLocalUserNamesByDeviceIdAndSids(0, ["sid"]));
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceUserProvider.GetLocalUserNamesByDeviceIdAndSids(1, null));
    }

    [Test]
    public void GetLocalUserNamesByDeviceIdAndSids_EmptyListTest()
    {
        var result = _windowsDeviceUserProvider.GetLocalUserNamesByDeviceIdAndSids(1, []);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetLocalUserNamesByDeviceIdAndSid_SuccessTest()
    {
        _deviceId1 = GetDeviceId(_devId1);
        const string sid = "S-1-2-3-4-5-6-7";
        const string userName = "userName";
        var windowsDeviceData = GetWindowsDeviceUserData(sid, userName, _userGroups, _deviceId1, null, DateTime.UtcNow, GenerateDataKey());
        _windowsDeviceUserProvider.Insert(windowsDeviceData);

        var result = _windowsDeviceUserProvider.GetLocalUserNamesByDeviceIdAndSids(_deviceId1, [sid])[0];

        Assert.AreEqual(result.WindowsDeviceLocalUserId, windowsDeviceData.WindowsDeviceUserId);
        Assert.AreEqual(result.SID, result.SID);
        Assert.AreEqual(result.UserName, result.UserName);
    }

    [Test]
    public void GetLocalUsersPasswordByDeviceIdAndSid_ArgumentExceptions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceUserProvider.GetLocalUsersPasswordByDeviceIdAndSid(0, "sid"));

        Assert.Throws<ArgumentException>(() =>
            _windowsDeviceUserProvider.GetLocalUsersPasswordByDeviceIdAndSid(1, string.Empty));
    }

    [Test]
    public void GetLocalUsersPasswordByDeviceIdAndSid_Success()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData1 = GetWindowsDeviceUserData("sid10", "userName10", _userGroups, _deviceId1, "samplePassword1", DateTime.UtcNow, GenerateDataKey(), true);
        _windowsDeviceUserProvider.Insert(windowsDeviceData1);

        var result = _windowsDeviceUserProvider.GetLocalUsersPasswordByDeviceIdAndSid(_deviceId1, "sid10");
        Assert.AreEqual(result.UserName, windowsDeviceData1.UserName);
        Assert.AreEqual(result.AutoGeneratedPassword, windowsDeviceData1.AutoGeneratedPassword);
    }

    [Test]
    public void Delete_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceUserProvider.Delete(0));
    }

    [Test]
    public void Delete_Success()
    {
        _deviceId1 = GetDeviceId(_devId1);
        var windowsDeviceData = GetWindowsDeviceUserData("1", "userName", ["Users"], _deviceId1, "samplePassword1", DateTime.UtcNow, GenerateDataKey(), false);

        _windowsDeviceUserProvider.Insert(windowsDeviceData);

        var beforeDelete = _windowsDeviceUserProvider
            .GetAllLocalUsersDataByDeviceId(_deviceId1)
            .FirstOrDefault(x => x.UserSID == windowsDeviceData.UserSID);

        Assert.IsNotNull(beforeDelete);

        _windowsDeviceUserProvider.Delete(beforeDelete.WindowsDeviceUserId);

        var afterDelete = _windowsDeviceUserProvider
            .GetAllLocalUsersDataByDeviceId(_deviceId1)
            .FirstOrDefault(x => x.UserName == "userName");

        Assert.IsNull(afterDelete);
    }

    [Test]
    public void Insert_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceUserProvider.Insert(null));
    }

    [Test]
    public void Update_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceUserProvider.Update(null));
    }

    [Test]
    public void Update_Success()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData = GetWindowsDeviceUserData("1", "userName", ["Users"], _deviceId1, "samplePassword1", DateTime.UtcNow, GenerateDataKey(), false);

        _windowsDeviceUserProvider.Insert(windowsDeviceData);

        var recordToUpdate = _windowsDeviceUserProvider
            .GetAllLocalUsersDataByDeviceId(_deviceId1)
            .FirstOrDefault(x => x.UserSID == windowsDeviceData.UserSID); // Select the record to update

        Assert.IsNotNull(recordToUpdate); // Ensure the record to update exists

        // Create updated data
        var updatedData = new WindowsDeviceUserData
        {
            WindowsDeviceUserId = recordToUpdate.WindowsDeviceUserId,
            DeviceId = recordToUpdate.DeviceId,
            UserSID = "1",
            UserName = "updatedUserName",
            UserGroups = ["Users"],
            IsMobiControlCreated = true,
            AutoGeneratedPassword = Encoding.UTF8.GetBytes("updatedPassword"),
            DataKeyId = GenerateDataKey()
        };

        // Update the record
        _windowsDeviceUserProvider.Update(updatedData);

        // Retrieve the updated record from the database
        var updatedRecord = _windowsDeviceUserProvider.GetLocalUsersDataByDeviceIdAndSid(_deviceId1, "1");

        Assert.IsNotNull(updatedRecord); // Ensure the record was updated successfully
        Assert.AreEqual(updatedData.UserName, updatedRecord.UserName); // Validate the updated data
        Assert.AreEqual(updatedData.IsMobiControlCreated, updatedRecord.IsMobiControlCreated);
    }

    [Test]
    public void DeleteByDeviceId_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceUserProvider.DeleteByDeviceId(0));
    }

    [Test]
    public void DeleteByDeviceId_Success()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData1 = GetWindowsDeviceUserData("1", "1", _userGroups, _deviceId1, "samplePassword1", DateTime.UtcNow, GenerateDataKey(), false);
        var windowsDeviceData2 = GetWindowsDeviceUserData("2", "2", _userGroups, _deviceId1, "samplePassword2", DateTime.UtcNow, GenerateDataKey(), true);
        var windowsDeviceData3 = GetWindowsDeviceUserData("3", "3", _userGroups, _deviceId1, "samplePassword3", DateTime.UtcNow, GenerateDataKey(), false);

        _windowsDeviceUserProvider.Insert(windowsDeviceData1);
        _windowsDeviceUserProvider.Insert(windowsDeviceData2);
        _windowsDeviceUserProvider.Insert(windowsDeviceData3);

        _windowsDeviceUserProvider.DeleteByDeviceId(_deviceId1, WindowsDeviceUserType.Local);

        var allUsers = _windowsDeviceUserProvider.GetAllLocalUsersDataByDeviceId(_deviceId1);
        Assert.IsNotNull(allUsers);
        Assert.AreEqual(0, allUsers.Count());
    }

    [Test]
    public void BulkModify_Exceptions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceUserProvider.BulkReplace(0, new List<WindowsDeviceUserData>()));

        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceUserProvider.BulkReplace(1, null));
    }

    [Test]
    public void BulkUpdate_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceUserProvider.BulkUpdate(null));
    }

    [Test]
    public void BulkUpdate_Throws_ArgumentException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceUserProvider.BulkUpdate(new List<WindowsDeviceUserData>()));
    }

    [Test]
    public void BulkUpdateTest()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData1 = GetWindowsDeviceUserData("1", "1", _userGroups, _deviceId1, "samplePassword1", DateTime.UtcNow, GenerateDataKey(), false);
        var windowsDeviceData2 = GetWindowsDeviceUserData("2", "2", _userGroups, _deviceId1, "samplePassword2", DateTime.UtcNow, GenerateDataKey(), true);
        var windowsDeviceData3 = GetWindowsDeviceUserData("3", "3", _userGroups, _deviceId1, "samplePassword3", DateTime.UtcNow, GenerateDataKey(), false);
        var windowsDeviceData4 = GetWindowsDeviceUserData("4", "4", _userGroups, _deviceId1, "samplePassword2", DateTime.UtcNow, GenerateDataKey(), true);
        var windowsDeviceData5 = GetWindowsDeviceUserData("5", "updated", _userGroups, _deviceId1, "samplePassword3", DateTime.UtcNow, GenerateDataKey(), false);

        _windowsDeviceUserProvider.Insert(windowsDeviceData2);
        _windowsDeviceUserProvider.Insert(windowsDeviceData3);
        _windowsDeviceUserProvider.Insert(windowsDeviceData4);

        var beforeBulkModify = _windowsDeviceUserProvider
            .GetAllLocalUsersDataByDeviceId(_deviceId1)
            .Where(x => !string.IsNullOrEmpty(x.UserSID))
            .Select(x => (x.UserSID, x.UserName))
            .ToList();

        _windowsDeviceUserProvider.BulkUpdate(new[] { windowsDeviceData1, windowsDeviceData5 });

        var afterBulkModify = _windowsDeviceUserProvider
            .GetAllLocalUsersDataByDeviceId(_deviceId1)
            .Where(x => !string.IsNullOrEmpty(x.UserSID))
            .Select(x => (x.UserSID, x.UserName))
            .ToList();

        CollectionAssert.AreEqual(beforeBulkModify, new List<(string, string)> { ("2", "2"), ("3", "3"), ("4", "4") });
        CollectionAssert.AreEqual(afterBulkModify, new List<(string, string)> { ("2", "2"), ("3", "3"), ("4", "4"), ("1", "1"), ("5", "updated") });

        _windowsDeviceUserProvider.DeleteByDeviceId(_deviceId1);
    }

    [Test]
    public void GetSingleLocalUserTest()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData1 = GetWindowsDeviceUserData("sid", "userName", _userGroups, _deviceId1, "samplePassword", DateTime.UtcNow, GenerateDataKey(), true);
        _windowsDeviceUserProvider.Insert(windowsDeviceData1);
        var res = _windowsDeviceUserProvider.GetAllLocalUsersDataByDeviceId(_deviceId1);
        var result = res.FirstOrDefault(s => s.DeviceId == _deviceId1);
        Assert.IsNotNull(result);
        Assert.IsNotNull(res);
        Assert.AreEqual(result.UserName, "userName");
    }

    [Test]
    public void GetLocalUsersTest()
    {
        _deviceId1 = GetDeviceId(_devId1);
        _deviceId2 = GetDeviceId(_devId2);

        var windowsDeviceData1 = GetWindowsDeviceUserData("sid1", "userName1", _userGroups, _deviceId1, "samplePassword1", DateTime.UtcNow, GenerateDataKey(), false);
        var windowsDeviceData2 = GetWindowsDeviceUserData("sid2", "userName2", _userGroups, _deviceId1, "samplePassword2", DateTime.UtcNow, GenerateDataKey(), true);
        var windowsDeviceData3 = GetWindowsDeviceUserData("sid3", "userName3", _userGroups, _deviceId2, "samplePassword3", DateTime.UtcNow, GenerateDataKey(), false);

        _windowsDeviceUserProvider.Insert(windowsDeviceData1);
        _windowsDeviceUserProvider.Insert(windowsDeviceData2);
        _windowsDeviceUserProvider.Insert(windowsDeviceData3);
        var res = _windowsDeviceUserProvider.GetAllLocalUsersDataByDeviceId(_deviceId1).ToList();
        Assert.IsNotNull(res);
        Assert.AreEqual(2, res.Count);
        var result = res.FirstOrDefault(s => s.DeviceId == _deviceId1 && s.UserSID == "sid1");
        Assert.IsNotNull(result);
        Assert.AreEqual("userName1", result.UserName);
        Assert.AreEqual(false, result.IsMobiControlCreated);
        result = res.FirstOrDefault(s => s.DeviceId == _deviceId1 && s.UserSID == "sid2");
        Assert.IsNotNull(result);
        Assert.AreEqual("userName2", result.UserName);
    }

    [Test]
    public void GetLocalUserPasswordTest()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData1 = GetWindowsDeviceUserData("sid1", "userName1", _userGroups, _deviceId1, "samplePassword1", DateTime.UtcNow, GenerateDataKey(), false);

        _windowsDeviceUserProvider.Insert(windowsDeviceData1);
        var result = _windowsDeviceUserProvider.GetLocalUsersDataByDeviceIdAndSid(_deviceId1, "sid1");
        Assert.IsNotNull(result);
        Assert.AreEqual("userName1", result.UserName);
        Assert.AreEqual(false, result.IsMobiControlCreated);
        Assert.AreEqual(Encoding.UTF8.GetBytes("samplePassword1"), result.AutoGeneratedPassword);
    }

    [Test]
    public void BulkModifyTest()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData1 = GetWindowsDeviceUserData("1", "1", _userGroups, _deviceId1, "samplePassword1", DateTime.UtcNow, GenerateDataKey(), false);
        var windowsDeviceData2 = GetWindowsDeviceUserData("2", "2", _userGroups, _deviceId1, "samplePassword2", DateTime.UtcNow, GenerateDataKey(), true);
        var windowsDeviceData3 = GetWindowsDeviceUserData("3", "3", _userGroups, _deviceId1, "samplePassword3", DateTime.UtcNow, GenerateDataKey(), false);
        var windowsDeviceData4 = GetWindowsDeviceUserData("4", "4", _userGroups, _deviceId1, "samplePassword2", DateTime.UtcNow, GenerateDataKey(), true);
        var windowsDeviceData5 = GetWindowsDeviceUserData("4", "updated", _userGroups, _deviceId1, "samplePassword3", DateTime.UtcNow, GenerateDataKey(), false);

        _windowsDeviceUserProvider.Insert(windowsDeviceData2);
        _windowsDeviceUserProvider.Insert(windowsDeviceData3);
        _windowsDeviceUserProvider.Insert(windowsDeviceData4);

        var beforeBulkModify = _windowsDeviceUserProvider
            .GetAllLocalUsersDataByDeviceId(_deviceId1)
            .Where(x => !string.IsNullOrEmpty(x.UserSID))
            .Select(x => (x.UserSID, x.UserName))
            .ToList();

        _windowsDeviceUserProvider.BulkReplace(_deviceId1, new[] { windowsDeviceData1, windowsDeviceData3, windowsDeviceData5 });

        var afterBulkModify = _windowsDeviceUserProvider
            .GetAllLocalUsersDataByDeviceId(_deviceId1)
            .Where(x => !string.IsNullOrEmpty(x.UserSID))
            .Select(x => (x.UserSID, x.UserName))
            .ToList();

        CollectionAssert.AreEqual(beforeBulkModify, new List<(string, string)> { ("2", "2"), ("3", "3"), ("4", "4") });
        CollectionAssert.AreEqual(afterBulkModify, new List<(string, string)> { ("3", "3"), ("4", "updated"), ("1", "1") });
    }

    [Test]
    public void DeleteByUserSid_Exception()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceUserProvider.DeleteByUserSid("success", 0));
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceUserProvider.DeleteByUserSid(null, 10));
    }

    [Test]
    public void DeleteByUserSid_Success()
    {
        _deviceId1 = GetDeviceId(_devId1);
        string sid1 = "userSid1", sid2 = "usersid2";
        var windowsDeviceData1 = GetWindowsDeviceUserData(sid1, "1", _userGroups, _deviceId1, "samplePassword1", DateTime.UtcNow, GenerateDataKey(), false);
        var windowsDeviceData2 = GetWindowsDeviceUserData(sid2, "2", _userGroups, _deviceId1, "samplePassword2", DateTime.UtcNow, GenerateDataKey(), true);

        _windowsDeviceUserProvider.Insert(windowsDeviceData1);
        _windowsDeviceUserProvider.Insert(windowsDeviceData2);

        var beforeDelete = _windowsDeviceUserProvider
            .GetAllLocalUsersDataByDeviceId(_deviceId1)
            .Where(x => !string.IsNullOrEmpty(x.UserSID) && (x.UserSID == sid1 || x.UserSID == sid2))
            .Select(x => x.UserSID).ToList();

        _windowsDeviceUserProvider.DeleteByUserSid(sid1, _deviceId1);
        _windowsDeviceUserProvider.DeleteByUserSid(sid2, _deviceId1);

        var afterDelete = _windowsDeviceUserProvider
            .GetAllLocalUsersDataByDeviceId(_deviceId1)
            .Where(x => !string.IsNullOrEmpty(x.UserSID) && (x.UserSID == sid1 || x.UserSID == sid2))
            .Select(x => x.UserSID).ToList();

        CollectionAssert.AreEquivalent(beforeDelete, new List<string> { sid1, sid2 });
        CollectionAssert.AreEqual(afterDelete, new List<string>());
    }

    [Test]
    public void UpdatePassword_Exception()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceUserProvider.ResetPassword(0));
    }

    [Test]
    public void UpdatePassword_Success()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData1 = GetWindowsDeviceUserData("1", "1", _userGroups, _deviceId1, "samplePassword1", DateTime.UtcNow, GenerateDataKey(), false);
        _windowsDeviceUserProvider.Insert(windowsDeviceData1);

        var beforeUpdate = _windowsDeviceUserProvider
            .GetAllLocalUsersDataByDeviceId(_deviceId1)
            .Where(x => !string.IsNullOrEmpty(x.UserSID))
            .Select(x => (x.WindowsDeviceUserId, x.AutoGeneratedPassword)).ToList();

        _windowsDeviceUserProvider.ResetPassword(windowsDeviceData1.WindowsDeviceUserId);

        var afterUpdate = _windowsDeviceUserProvider
            .GetAllLocalUsersDataByDeviceId(_deviceId1)
            .Where(x => !string.IsNullOrEmpty(x.UserSID) && x.WindowsDeviceUserId == windowsDeviceData1.WindowsDeviceUserId)
            .Select(x => (x.WindowsDeviceUserId, x.AutoGeneratedPassword)).ToList();

        Assert.IsNotNull(beforeUpdate.FirstOrDefault().AutoGeneratedPassword);
        Assert.IsNull(afterUpdate.FirstOrDefault().AutoGeneratedPassword);
    }

    [Test]
    public void ModifyLoggedInUser_ExceptionTest()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceUserProvider.ModifyLoggedInUser(null));
    }

    [Test]
    public void ModifyLoggedInUser_SuccessTest()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);

        const string fullName = "UserFullName";
        var dataKeyId = GenerateDataKey();
        var windowsDeviceUserData = GetWindowsDeviceUserData("1", "ABC", null, _deviceId1, null, DateTime.UtcNow, dataKeyId, false, null, fullName, 0);
        _windowsDeviceUserProvider.Insert(windowsDeviceUserData);

        const string updatedFullName = "TestUserFullName";
        var updatedWindowsDeviceUserData = GetWindowsDeviceUserData("1", "ABC", null, _deviceId1, null, DateTime.UtcNow, dataKeyId, false, null, updatedFullName, 0);
        _windowsDeviceUserProvider.ModifyLoggedInUser(updatedWindowsDeviceUserData);
        var result = _windowsDeviceUserProvider.GetLoggedInUserDataByDeviceId(_deviceId1);
        Assert.IsNotNull(result);
        Assert.AreNotEqual(result.UserFullName, fullName);
        Assert.AreEqual(result.UserFullName, updatedFullName);
        Assert.AreEqual(result.DataKeyId, dataKeyId);
    }

    [Test]
    public void LogOffUserByDeviceId_ExceptionTest()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceUserProvider.LogOffUserByDeviceId(0));
    }

    [Test]
    public void GetLoggedInUserDataByDeviceId_ExceptionTest()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceUserProvider.GetLoggedInUserDataByDeviceId(0));
    }

    [Test]
    public void LogOffUserByDeviceId_SuccessTest()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);

        var windowsDeviceUserData = GetWindowsDeviceUserData("1", "ABC", null, _deviceId1, null, DateTime.UtcNow, GenerateDataKey(), false, null, "UserFullName", 0);
        _windowsDeviceUserProvider.Insert(windowsDeviceUserData);

        const string updatedFullName = "TestUserFullName";
        var updatedWindowsDeviceUserData = GetWindowsDeviceUserData("1", "ABC", null, _deviceId1, null, DateTime.UtcNow, GenerateDataKey(), false, null, updatedFullName, 0);
        _windowsDeviceUserProvider.ModifyLoggedInUser(updatedWindowsDeviceUserData);
        var beforeData = _windowsDeviceUserProvider.GetLoggedInUserDataByDeviceId(_deviceId1);
        Assert.IsNotNull(beforeData);
        _windowsDeviceUserProvider.LogOffUserByDeviceId(_deviceId1);
        var afterData = _windowsDeviceUserProvider.GetLoggedInUserDataByDeviceId(_deviceId1);
        Assert.IsNull(afterData);
    }

    [Test]
    public void GetLoggedInUserDataByDeviceIds_ExceptionTest()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceUserProvider.GetLoggedInUserDataByDeviceIds(null));
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceUserProvider.GetLoggedInUserDataByDeviceIds(new List<int>()));
    }

    [Test]
    public void GetLoggedInUserDataByDeviceIds_SuccessTest()
    {
        _deviceId1 = GetDeviceId(_devId1);
        _deviceId2 = GetDeviceId(_devId2);
        var deviceIds = new List<int>() { _deviceId1, _deviceId2 };

        var windowsDeviceData1 = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        var windowsDeviceData2 = new WindowsDeviceData
        {
            DeviceId = _deviceId2,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        _windowsDeviceProvider.Insert(windowsDeviceData1);
        _windowsDeviceProvider.Insert(windowsDeviceData2);
        var dataKeyId1 = GenerateDataKey();
        var windowsDeviceUserData1 = GetWindowsDeviceUserData("1", "ABC", null, _deviceId1, null, DateTime.UtcNow, dataKeyId1, false, null, "UserFullName", 0);
        _windowsDeviceUserProvider.ModifyLoggedInUser(windowsDeviceUserData1);

        var dataKeyId2 = GenerateDataKey();
        var windowsDeviceUserData2 = GetWindowsDeviceUserData("2", "BCD", null, _deviceId2, null, DateTime.UtcNow, dataKeyId2, false, null, "TestUserFullName", 0);
        _windowsDeviceUserProvider.ModifyLoggedInUser(windowsDeviceUserData2);

        var result = _windowsDeviceUserProvider.GetLoggedInUserDataByDeviceIds(deviceIds).ToList();
        Assert.IsNotNull(result);
        Assert.AreEqual(result[0].DeviceId, windowsDeviceUserData1.DeviceId);
        Assert.AreEqual(result[0].UserDomain, windowsDeviceUserData1.UserDomain);
        Assert.AreEqual(result[0].UserFullName, windowsDeviceUserData1.UserFullName);
        Assert.AreEqual(result[0].UserName, windowsDeviceUserData1.UserName);
        Assert.AreEqual(result[0].UserSID, windowsDeviceUserData1.UserSID);
        Assert.AreEqual(result[0].DataKeyId, windowsDeviceUserData1.DataKeyId);
        Assert.AreEqual(result[1].DeviceId, windowsDeviceUserData2.DeviceId);
        Assert.AreEqual(result[1].UserDomain, windowsDeviceUserData2.UserDomain);
        Assert.AreEqual(result[1].UserFullName, windowsDeviceUserData2.UserFullName);
        Assert.AreEqual(result[1].UserName, windowsDeviceUserData2.UserName);
        Assert.AreEqual(result[1].UserSID, windowsDeviceUserData2.UserSID);
        Assert.AreEqual(result[1].DataKeyId, windowsDeviceUserData2.DataKeyId);
    }

    [Test]
    public void GetUserSidByUserNameAndDeviceId_ExceptionTest()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceUserProvider.GetUserSidByUserNameAndDeviceId(0, "validUserName"));
        Assert.Throws<ArgumentException>(() =>
            _windowsDeviceUserProvider.GetUserSidByUserNameAndDeviceId(1, null));
        Assert.Throws<ArgumentException>(() =>
            _windowsDeviceUserProvider.GetUserSidByUserNameAndDeviceId(1, string.Empty));
        Assert.Throws<ArgumentException>(() =>
            _windowsDeviceUserProvider.GetUserSidByUserNameAndDeviceId(1, " "));
    }

    [Test]
    public void GetUserSidByUserNameAndDeviceId_SuccessTest()
    {
        _deviceId1 = GetDeviceId(_devId1);
        var windowsDeviceData1 = GetWindowsDeviceUserData("sid1", "userName1", _userGroups, _deviceId1, "samplePassword1", DateTime.UtcNow, GenerateDataKey(), true);
        _windowsDeviceUserProvider.Insert(windowsDeviceData1);

        var sidData = _windowsDeviceUserProvider.GetUserSidByUserNameAndDeviceId(_deviceId1, "userName1");

        Assert.IsNotNull(sidData);
        Assert.AreEqual(windowsDeviceData1.UserSID, sidData.UserSID);
        Assert.AreEqual(0, sidData.DeviceId);
    }

    [Test]
    public void GetUserSidsByUserNameAndDeviceIds_ExceptionTest()
    {
        var validDeviceIds = new List<int>() { 1, 2 };

        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceUserProvider.GetUserSidsByUserNameAndDeviceIds(null, "validUserName"));
        Assert.Throws<ArgumentException>(() =>
            _windowsDeviceUserProvider.GetUserSidsByUserNameAndDeviceIds(new List<int>(), "validUserName"));
        Assert.Throws<ArgumentException>(() =>
            _windowsDeviceUserProvider.GetUserSidsByUserNameAndDeviceIds(validDeviceIds, null));
        Assert.Throws<ArgumentException>(() =>
            _windowsDeviceUserProvider.GetUserSidsByUserNameAndDeviceIds(validDeviceIds, string.Empty));
        Assert.Throws<ArgumentException>(() =>
            _windowsDeviceUserProvider.GetUserSidsByUserNameAndDeviceIds(validDeviceIds, " "));
    }

    [Test]
    public void GetUserSidsByUserNameAndDeviceIds_SuccessTest()
    {
        _deviceId1 = GetDeviceId(_devId1);
        _deviceId2 = GetDeviceId(_devId2);
        var deviceIds = new List<int>() { _deviceId1, _deviceId2 };
        var windowsDeviceData1 = GetWindowsDeviceUserData("sid1", "userName1", _userGroups, _deviceId1, "samplePassword1", DateTime.UtcNow, GenerateDataKey(), true);
        var windowsDeviceData2 = GetWindowsDeviceUserData("sid2", "userName1", _userGroups, _deviceId2, "samplePassword2", DateTime.UtcNow, GenerateDataKey(), true);
        _windowsDeviceUserProvider.Insert(windowsDeviceData1);
        _windowsDeviceUserProvider.Insert(windowsDeviceData2);

        var sidData = _windowsDeviceUserProvider.GetUserSidsByUserNameAndDeviceIds(deviceIds, "userName1");

        Assert.IsNotNull(sidData);
        Assert.AreEqual(2, sidData.Count);

        var sidData1 = sidData.ElementAt(0);
        var sidData2 = sidData.ElementAt(1);

        Assert.AreEqual(windowsDeviceData1.UserSID, sidData1.UserSID);
        Assert.AreEqual(_deviceId1, sidData1.DeviceId);
        Assert.AreEqual(windowsDeviceData2.UserSID, sidData2.UserSID);
        Assert.AreEqual(_deviceId2, sidData2.DeviceId);
    }

    [Test]
    public void UpdateUsernameByUserSidAndDeviceId_ExceptionTest()
    {
        Assert.Throws<ArgumentException>(() => _windowsDeviceUserProvider.UpdateUsernameByUserSidAndDeviceId(null, 1, "newUsername"));
        Assert.Throws<ArgumentException>(() => _windowsDeviceUserProvider.UpdateUsernameByUserSidAndDeviceId(string.Empty, 1, "newUsername"));
        Assert.Throws<ArgumentException>(() => _windowsDeviceUserProvider.UpdateUsernameByUserSidAndDeviceId(" ", 1, "newUsername"));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceUserProvider.UpdateUsernameByUserSidAndDeviceId("userSid", 0, "newUsername"));
        Assert.Throws<ArgumentException>(() => _windowsDeviceUserProvider.UpdateUsernameByUserSidAndDeviceId("userSid", 1, null));
        Assert.Throws<ArgumentException>(() => _windowsDeviceUserProvider.UpdateUsernameByUserSidAndDeviceId("userSid", 1, string.Empty));
        Assert.Throws<ArgumentException>(() => _windowsDeviceUserProvider.UpdateUsernameByUserSidAndDeviceId("userSid", 1, " "));
    }

    [Test]
    public void UpdateUsernameByUserSidAndDeviceId_SuccessTest()
    {
        _deviceId1 = GetDeviceId(_devId1);
        var windowsDeviceData = GetWindowsDeviceUserData("userSid", "oldUsername", _userGroups, _deviceId1, "localUserPassword", DateTime.UtcNow, GenerateDataKey());
        _windowsDeviceUserProvider.Insert(windowsDeviceData);

        _windowsDeviceUserProvider.UpdateUsernameByUserSidAndDeviceId("userSid", _deviceId1, "newUsername");

        var renamedUserData = _windowsDeviceUserProvider.GetLocalUsersPasswordByDeviceIdAndSid(_deviceId1, "userSid");
        Assert.IsNotNull(renamedUserData);
        Assert.AreEqual("newUsername", renamedUserData.UserName);
    }

    [Test]
    public void GetUsernameByDeviceIds_NullDeviceIds_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceUserProvider.GetUsernameByDeviceIds(null));
    }

    [Test]
    public void GetUsernameByDeviceIds_EmptyDeviceIds_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceUserProvider.GetUsernameByDeviceIds(new List<int>()));
    }

    [Test]
    public void GetUsernameByDeviceIds_WithValidDeviceIds_ReturnsExpectedUserData()
    {
        _deviceId1 = GetDeviceId(_devId1);
        var windowsDeviceData = GetWindowsDeviceUserData("1", "1", _userGroups, _deviceId1, "samplePassword1", DateTime.UtcNow, GenerateDataKey(), false);
        _windowsDeviceUserProvider.Insert(windowsDeviceData);
        var testDeviceIds = new List<int> { _deviceId1 }; // IDs known to exist in the test database

        var result = _windowsDeviceUserProvider.GetUsernameByDeviceIds(testDeviceIds).ToList();

        Assert.IsNotNull(result);
        Assert.IsNotEmpty(result);

        var user = result.FirstOrDefault(u => u.DeviceId == _deviceId1);
        Assert.IsNotNull(user);
        Assert.IsNotEmpty(user.UserName);
    }

    [Test]
    public void GetDataKeyIdsByWindowsDeviceUserIds_NullOrEmptyInput_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentNullException>(
            () => _windowsDeviceUserProvider.GetDataKeyIdsByWindowsDeviceUserIds(null)
        );

        var exception = Assert.Throws<ArgumentNullException>(
            () => _windowsDeviceUserProvider.GetDataKeyIdsByWindowsDeviceUserIds(new List<int>())
        );
        StringAssert.Contains("Collection cannot be null or empty", exception?.Message);
        StringAssert.Contains("windowsDeviceUserIds", exception?.Message);
    }

    [Test]
    public void GetDataKeyIdsByWindowsDeviceUserIds_ValidInput_ReturnsCorrectMapping()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData1 = GetWindowsDeviceUserData("sid1", "user1", _userGroups, _deviceId1,
                                                       "pass1", DateTime.UtcNow, GenerateDataKey(), true);
        var windowsDeviceData2 = GetWindowsDeviceUserData("sid2", "user2", _userGroups, _deviceId1,
                                                       "pass2", DateTime.UtcNow, GenerateDataKey(), false);

        _windowsDeviceUserProvider.Insert(windowsDeviceData1);
        _windowsDeviceUserProvider.Insert(windowsDeviceData2);

        var userIds = new List<int> { windowsDeviceData1.WindowsDeviceUserId, windowsDeviceData2.WindowsDeviceUserId };
        var result = _windowsDeviceUserProvider.GetDataKeyIdsByWindowsDeviceUserIds(userIds);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);

        var user1 = _windowsDeviceUserProvider.GetLocalUsersDataByDeviceIdAndSid(_deviceId1, "sid1");
        var user2 = _windowsDeviceUserProvider.GetLocalUsersDataByDeviceIdAndSid(_deviceId1, "sid2");

        Assert.AreEqual(user1.DataKeyId, result[windowsDeviceData1.WindowsDeviceUserId]);
        Assert.AreEqual(user2.DataKeyId, result[windowsDeviceData2.WindowsDeviceUserId]);
    }

    [Test]
    public void WindowsDeviceUserNameBulkUpdate_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceUserProvider.WindowsDeviceUserNameBulkUpdate(null));
    }

    [Test]
    public void WindowsDeviceUserNameBulkUpdate_EmptyList_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceUserProvider.WindowsDeviceUserNameBulkUpdate(new List<DecryptedLocalUserModel>()));
    }

    [Test]
    public void WindowsDeviceUserNameBulkUpdate_ValidInput_UpdatesNamesCorrectly()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData1 = GetWindowsDeviceUserData("sid1", "oldUser1", _userGroups, _deviceId1, "pass1", DateTime.UtcNow, GenerateDataKey(), true);
        var windowsDeviceData2 = GetWindowsDeviceUserData("sid2", "oldUser2", _userGroups, _deviceId1, "pass2", DateTime.UtcNow, GenerateDataKey(), false);
        _windowsDeviceUserProvider.Insert(windowsDeviceData1);
        _windowsDeviceUserProvider.Insert(windowsDeviceData2);

        var updateModels = new List<DecryptedLocalUserModel>
        {
            new()
            {
                WindowsDeviceUserId = windowsDeviceData1.WindowsDeviceUserId,
                UserName = "newUser1"
            },
            new()
            {
                WindowsDeviceUserId = windowsDeviceData2.WindowsDeviceUserId,
                UserName = "newUser2"
            }
        };

        _windowsDeviceUserProvider.WindowsDeviceUserNameBulkUpdate(updateModels);

        var updatedUser1 = _windowsDeviceUserProvider.GetLocalUsersDataByDeviceIdAndSid(_deviceId1, "sid1");
        var updatedUser2 = _windowsDeviceUserProvider.GetLocalUsersDataByDeviceIdAndSid(_deviceId1, "sid2");

        Assert.AreEqual("newUser1", updatedUser1.UserName);
        Assert.AreEqual("newUser2", updatedUser2.UserName);
    }

    private static int GetDeviceId(string devId)
    {
        return TestDeviceProvider.EnsureDeviceRecord(devId, 1004);
    }

    private static WindowsDeviceUserData GetWindowsDeviceUserData(
        string userSid,
        string userName,
        IEnumerable<string> userGroup,
        int deviceId,
        string localUserPassword,
        DateTime createdDate,
        int dataKeyId,
        bool isMobiControlCreated = false,
        string userDomain = null,
        string userFullName = null,
        WindowsDeviceUserType windowsDeviceUserType = 0)
    {
        return new WindowsDeviceUserData
        {
            UserSID = userSid,
            UserName = userName,
            UserGroups = userGroup,
            DeviceId = deviceId,
            IsMobiControlCreated = isMobiControlCreated,
            DataKeyId = dataKeyId,
            AutoGeneratedPassword =
                !string.IsNullOrEmpty(localUserPassword) ? Encoding.UTF8.GetBytes(localUserPassword) : null,
            CreatedDate = createdDate,
            UserDomain = userDomain,
            UserFullName = userFullName != null ? userFullName : null,
            WindowsDeviceUserType = windowsDeviceUserType
        };
    }

    private int GenerateDataKey()
    {
        return _dataKeyService.GetKey().Id;
    }

    private void DeleteTestData(int deviceId)
    {
        if (deviceId <= 0)
        {
            return;
        }

        _windowsDeviceUserProvider.DeleteByDeviceId(deviceId);
        TestDeviceProvider.DeleteDevice(deviceId);
    }
}

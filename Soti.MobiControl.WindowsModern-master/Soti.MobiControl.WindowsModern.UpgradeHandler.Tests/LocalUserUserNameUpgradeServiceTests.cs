using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Soti.MobiControl.InstallerServices.Config;
using Soti.MobiControl.InstallerServices.Services;
using Soti.MobiControl.WindowsModern.Models;
using Soti.SensitiveDataProtection;
using Soti.SensitiveDataProtection.Model;
using System.Text;
using System.Linq;

namespace Soti.MobiControl.WindowsModern.UpgradeHandler.Tests;

[TestFixture]
public class LocalUserUserNameUpgradeServiceTests
{
    private static readonly Version MinApplicableToVersion = new(2026, 0, 0);
    private static readonly Version MinApplicableFromVersion = new(2025, 0, 0);

    private Mock<ITraceLogger> _traceLoggerMock;
    private Mock<IWindowsDeviceLocalUsersService> _windowsDeviceLocalUsersService;
    private Mock<IDataKeyService> _dataKeyService;
    private Mock<ISensitiveDataEncryptionService> _sensitiveDataEncryptionService;

    [SetUp]
    public void Setup()
    {
        _traceLoggerMock = new Mock<ITraceLogger>(MockBehavior.Strict);
        _windowsDeviceLocalUsersService = new Mock<IWindowsDeviceLocalUsersService>(MockBehavior.Strict);
        _dataKeyService = new Mock<IDataKeyService>(MockBehavior.Strict);
        _sensitiveDataEncryptionService = new Mock<ISensitiveDataEncryptionService>(MockBehavior.Strict);
    }

    [Test]
    public void Constructor_Throw_Exception()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new LocalUserUserNameUpgradeService(null, _windowsDeviceLocalUsersService.Object, _dataKeyService.Object, _sensitiveDataEncryptionService.Object));
        Assert.Throws<ArgumentNullException>(() => _ = new LocalUserUserNameUpgradeService(_traceLoggerMock.Object, null, _dataKeyService.Object, _sensitiveDataEncryptionService.Object));
        Assert.Throws<ArgumentNullException>(() => _ = new LocalUserUserNameUpgradeService(_traceLoggerMock.Object, _windowsDeviceLocalUsersService.Object, null, _sensitiveDataEncryptionService.Object));
        Assert.Throws<ArgumentNullException>(() => _ = new LocalUserUserNameUpgradeService(_traceLoggerMock.Object, _windowsDeviceLocalUsersService.Object, _dataKeyService.Object, null));
    }

    [Test]
    public void IsUpgradeApplicable_Throw_Exception()
    {
        Assert.Throws<ArgumentNullException>(() => CreateService().IsUpgradeApplicable(null));
        Assert.AreEqual(1, CreateService().Priority);
    }
    [Test]
    public void IsUpgradeApplicable_Throw_InvalidDataException_Exception()
    {
        var v2500 = new Version(2025, 0, 0);
        var v2410 = new Version(2024, 1, 0);
        var upgradeQualifier = new UpgradeQualifier { UpgradeFrom = v2500, UpgradeTo = v2410 };

        Assert.Throws<InvalidOperationException>(() => CreateService().IsUpgradeApplicable(upgradeQualifier));
    }

    [TestCaseSource(nameof(IsUpgradeApplicableTestData))]
    public void IsUpgradeApplicable_Tests(Version fromVersion, Version toVersion, bool shouldUpgrade)
    {
        var upgradeQualifier = new UpgradeQualifier { UpgradeFrom = fromVersion, UpgradeTo = toVersion };

        Assert.That(() => CreateService().IsUpgradeApplicable(upgradeQualifier), Is.EqualTo(shouldUpgrade));
    }

    [Test]
    public void IsUpgradeApplicable_Returns()
    {
        var qualifier = new UpgradeQualifier { UpgradeFrom = MinApplicableFromVersion, UpgradeTo = MinApplicableToVersion };
        var response = CreateService().IsUpgradeApplicable(qualifier);
        Assert.IsTrue(response);
    }

    [Test]
    public void Upgrade_Returns()
    {
        _windowsDeviceLocalUsersService.Setup(x => x.GetAllTmpLocalUsers(1000));
        var qualifier = new UpgradeQualifier { UpgradeFrom = MinApplicableFromVersion, UpgradeTo = MinApplicableFromVersion };

        CreateService().Upgrade(qualifier);

        _windowsDeviceLocalUsersService.Verify(x => x.GetAllTmpLocalUsers(1000), Times.Never);
    }

    [Test]
    public void Upgrade_NoData()
    {
        _windowsDeviceLocalUsersService.Setup(s => s.GetAllTmpLocalUsers(1000)).Returns((IEnumerable<WindowsDeviceTmpLocalUserModel>)null);
        _traceLoggerMock.Setup(s => s.Trace(It.IsAny<string>(), default));

        CreateService().Upgrade(new UpgradeQualifier { UpgradeFrom = MinApplicableFromVersion, UpgradeTo = MinApplicableToVersion });

        _windowsDeviceLocalUsersService.Verify(s => s.GetAllTmpLocalUsers(1000), Times.Once);
        _traceLoggerMock.Verify(s => s.Trace(It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public void Upgrade_Processes_Users_Correctly()
    {
        // Arrange
        var encryptedUsers = new[]
        {
        new WindowsDeviceTmpLocalUserModel
        {
            WindowsDeviceUserId = 1,
            UserName = Encoding.UTF8.GetBytes("encrypted1"),  // Use byte[] directly
            UserFullName = Encoding.UTF8.GetBytes("encrypted full1")
        },
        new WindowsDeviceTmpLocalUserModel
        {
            WindowsDeviceUserId = 2,
            UserName = Encoding.UTF8.GetBytes("encrypted2"),
            UserFullName = null
        },
        new WindowsDeviceTmpLocalUserModel // Invalid user to be skipped (No data key mapping)
        {
            WindowsDeviceUserId = 3,
            UserName = "encrypted3"u8.ToArray(),
            UserFullName = null
        },
        new WindowsDeviceTmpLocalUserModel // Invalid user to be skipped (Failed to get data key for ID)
        {
            WindowsDeviceUserId = 4,
            UserName = "encrypted4"u8.ToArray(),
            UserFullName = null
        },
        new WindowsDeviceTmpLocalUserModel // Invalid user to be skipped (Username is null)
        {
            WindowsDeviceUserId = 5,
            UserName = null,
            UserFullName = null
        }
    };

        _windowsDeviceLocalUsersService.Setup(x => x.GetAllTmpLocalUsers(It.IsAny<int>())).Returns(encryptedUsers);
        _traceLoggerMock.Setup(x => x.Trace(It.IsAny<string>(), default));

        var dataKeyMapping = new Dictionary<int, int>
    {
        { 1, 100 },
        { 2, 101 },
        { 4, 102 },
        { 5, 103 }
    };
        _windowsDeviceLocalUsersService.Setup(x => x.GetDataKeyIdByUserId(It.IsAny<IEnumerable<int>>()))
            .Returns(dataKeyMapping);

        _dataKeyService.Setup(x => x.GetKey(100)).Returns(new DataKey());
        _dataKeyService.Setup(x => x.GetKey(101)).Returns(new DataKey());
        _dataKeyService.Setup(x => x.GetKey(102)).Returns((DataKey)null);
        _dataKeyService.Setup(x => x.GetKey(103)).Returns(new DataKey());

        _sensitiveDataEncryptionService.Setup(x => x.Decrypt(It.IsAny<byte[]>(), It.IsAny<DataKey>()))
            .Returns<byte[], DataKey>((data, _) => data);

        var decryptedUsers = new List<DecryptedLocalUserModel>();
        _windowsDeviceLocalUsersService.Setup(x => x.UpdateDecryptedLocalUsers(It.IsAny<IEnumerable<DecryptedLocalUserModel>>()))
            .Callback<IEnumerable<DecryptedLocalUserModel>>(users => decryptedUsers = users.ToList());
        _windowsDeviceLocalUsersService.Setup(x => x.DeleteTmpLocalUsers(It.IsAny<IEnumerable<WindowsDeviceTmpLocalUserModel>>()));

        CreateService().Upgrade(new UpgradeQualifier { UpgradeFrom = MinApplicableFromVersion, UpgradeTo = MinApplicableToVersion });

        _windowsDeviceLocalUsersService.Verify(x => x.GetAllTmpLocalUsers(It.IsAny<int>()), Times.Once);
        _windowsDeviceLocalUsersService.Verify(x => x.GetDataKeyIdByUserId(It.IsAny<IEnumerable<int>>()), Times.Once);
        _windowsDeviceLocalUsersService.Verify(x => x.UpdateDecryptedLocalUsers(It.IsAny<IEnumerable<DecryptedLocalUserModel>>()), Times.Once);
        _windowsDeviceLocalUsersService.Verify(x => x.DeleteTmpLocalUsers(It.IsAny<IEnumerable<WindowsDeviceTmpLocalUserModel>>()), Times.Once);

        Assert.AreEqual(2, decryptedUsers.Count);

        CollectionAssert.AreEqual("encrypted1", decryptedUsers[0].UserName);
        CollectionAssert.AreEqual("encrypted full1", decryptedUsers[0].UserFullName);
        CollectionAssert.AreEqual("encrypted2", decryptedUsers[1].UserName);
        Assert.IsNull(decryptedUsers[1].UserFullName);
    }

    [Test]
    public void Properties_WorkCorrectly()
    {
        // Arrange
        var model = new WindowsDeviceTmpLocalUserModel
        {
            // Act - Set all properties
            WindowsDeviceUserId = 1,
            UserName = Encoding.UTF8.GetBytes("testuser"),
            UserFullName = Encoding.UTF8.GetBytes("Test User")
        };

        // Assert - Verify all properties
        Assert.That(model.WindowsDeviceUserId, Is.EqualTo(1));
        Assert.That(Encoding.UTF8.GetString(model.UserName), Is.EqualTo("testuser"));
        Assert.That(Encoding.UTF8.GetString(model.UserFullName), Is.EqualTo("Test User"));
    }
    private LocalUserUserNameUpgradeService CreateService()
    {
        return new LocalUserUserNameUpgradeService(_traceLoggerMock.Object, _windowsDeviceLocalUsersService.Object, _dataKeyService.Object, _sensitiveDataEncryptionService.Object);
    }

    private static IEnumerable<TestCaseData> IsUpgradeApplicableTestData()
    {
        var v1510 = new Version(13, 4, 0);
        var v2400 = new Version(14, 4, 0);
        var v2410 = new Version(15, 2, 0);
        var v2500 = new Version(2025, 0, 0);
        var v2510 = new Version(2025, 1, 0);
        var v2520 = new Version(2025, 2, 0);
        var v2600 = new Version(2026, 0, 0);

        yield return new TestCaseData(v1510, v2410, false);
        yield return new TestCaseData(v1510, v2500, false);
        yield return new TestCaseData(v1510, v2510, false);
        yield return new TestCaseData(v2400, v2510, false);
        yield return new TestCaseData(v2500, v2510, false);
        yield return new TestCaseData(v2510, v2510, false);
        yield return new TestCaseData(v2510, v2600, true);
        yield return new TestCaseData(v2520, v2600, true);
        yield return new TestCaseData(v2500, v2600, true);
    }
}

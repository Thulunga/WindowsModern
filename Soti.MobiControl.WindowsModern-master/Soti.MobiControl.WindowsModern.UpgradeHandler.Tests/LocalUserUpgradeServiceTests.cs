using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Soti.MobiControl.InstallerServices.Config;
using Soti.MobiControl.InstallerServices.Services;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern.UpgradeHandler.Tests;

[TestFixture]
public class LocalUserUpgradeServiceTests
{
    private static readonly Version MinApplicableToVersion = new(2025, 1, 0);
    private static readonly Version MinApplicableFromVersion = new(2025, 0, 0);

    private Mock<ITraceLogger> _traceLoggerMock;
    private Mock<IWindowsDeviceLocalUsersService> _windowsDeviceLocalUsersService;
    private Mock<IWindowsDeviceLocalGroupsService> _windowsDeviceLocalGroupsService;

    [SetUp]
    public void Setup()
    {
        _traceLoggerMock = new Mock<ITraceLogger>(MockBehavior.Strict);
        _windowsDeviceLocalUsersService = new Mock<IWindowsDeviceLocalUsersService>(MockBehavior.Strict);
        _windowsDeviceLocalGroupsService = new Mock<IWindowsDeviceLocalGroupsService>(MockBehavior.Strict);
    }

    [Test]
    public void Constructor_Throw_Exception()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new LocalUserUpgradeService(null, _windowsDeviceLocalUsersService.Object, _windowsDeviceLocalGroupsService.Object));
        Assert.Throws<ArgumentNullException>(() => _ = new LocalUserUpgradeService(_traceLoggerMock.Object, null, _windowsDeviceLocalGroupsService.Object));
        Assert.Throws<ArgumentNullException>(() => _ = new LocalUserUpgradeService(_traceLoggerMock.Object, _windowsDeviceLocalUsersService.Object, null));
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
        _windowsDeviceLocalUsersService.Setup(x => x.GetDeviceLocalUsersSummary());
        var qualifier = new UpgradeQualifier { UpgradeFrom = MinApplicableFromVersion, UpgradeTo = MinApplicableFromVersion };

        CreateService().Upgrade(qualifier);

        _windowsDeviceLocalUsersService.Verify(x => x.GetDeviceLocalUsersSummary(), Times.Never);
    }

    [Test]
    public void Upgrade_NoData()
    {
        _windowsDeviceLocalUsersService.Setup(s => s.GetDeviceLocalUsersSummary()).Returns((IReadOnlyList<WindowsDeviceLocalUserModel>)null);
        _traceLoggerMock.Setup(s => s.Trace(It.IsAny<string>(), default));

        CreateService().Upgrade(new UpgradeQualifier { UpgradeFrom = MinApplicableFromVersion, UpgradeTo = MinApplicableToVersion });

        _windowsDeviceLocalUsersService.Verify(s => s.GetDeviceLocalUsersSummary(), Times.Once);
        _traceLoggerMock.Verify(s => s.Trace(It.IsAny<string>(), default), Times.Once);
    }

    [Test]
    public void Upgrade_Success()
    {
        _windowsDeviceLocalUsersService.Setup(s => s.GetDeviceLocalUsersSummary()).Returns(TestData());
        _traceLoggerMock.Setup(s => s.Trace(It.IsAny<string>(), default));
        _windowsDeviceLocalGroupsService.Setup(s => s.InsertLocalGroupName(It.IsAny<string>())).Returns(1001);
        _windowsDeviceLocalGroupsService.Setup(s => s.UpsertWindowsDeviceLocalGroup(It.IsAny<int>(), It.IsAny<int>())).Returns(2001);
        _windowsDeviceLocalGroupsService.Setup(s => s.InsertLocalGroupUser(It.IsAny<int>(), It.IsAny<int>()));
        CreateService().Upgrade(new UpgradeQualifier { UpgradeFrom = MinApplicableFromVersion, UpgradeTo = MinApplicableToVersion });

        _windowsDeviceLocalUsersService.Verify(v => v.GetDeviceLocalUsersSummary(), Times.Once);
        _traceLoggerMock.Verify(v => v.Trace(It.IsAny<string>(), default), Times.Exactly(1));
        _windowsDeviceLocalGroupsService.Verify(v => v.InsertLocalGroupName(It.IsAny<string>()), Times.Exactly(3));
        _windowsDeviceLocalGroupsService.Verify(v => v.UpsertWindowsDeviceLocalGroup(It.IsAny<int>(), It.IsAny<int>()));
        _windowsDeviceLocalGroupsService.Verify(v => v.InsertLocalGroupUser(It.IsAny<int>(), It.IsAny<int>()));
    }

    private static IReadOnlyList<WindowsDeviceLocalUserModel> TestData()
    {
        return new List<WindowsDeviceLocalUserModel>()
        {
            new()
            {
                WindowsDeviceLocalUserId = 0, UserName = "Cal", DeviceId = 1, CreatedDate = DateTime.Now,
                SID = "sid1", IsMobiControlCreated = true, AutoGeneratedPassword = "12345678",
                UserGroups = new List<string>() { "g1" }
            },
            new()
            {
                WindowsDeviceLocalUserId = 0, UserName = "Hyperion", DeviceId = 2, CreatedDate = DateTime.Now,
                SID = "sid1", UserGroups = new List<string>() { "g2","g3" }
            },
        };
    }
    private LocalUserUpgradeService CreateService()
    {
        return new LocalUserUpgradeService(_traceLoggerMock.Object, _windowsDeviceLocalUsersService.Object, _windowsDeviceLocalGroupsService.Object);
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
        yield return new TestCaseData(v2500, v2510, true);
        yield return new TestCaseData(v2510, v2510, false);
        yield return new TestCaseData(v2510, v2600, false);
        yield return new TestCaseData(v2520, v2600, false);
        yield return new TestCaseData(v2500, v2600, true);
    }
}

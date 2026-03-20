using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Soti.MobiControl.Test.Common.Database;
using System.Text;
using Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
[IntegrationTest]
internal sealed class TmpWindowsDeviceUserDataProviderTests : ProviderTestsBase
{
    private static int _windowsDeviceUserId1;
    private static int _windowsDeviceUserId2;

    private TmpWindowsDeviceUserDataProvider _provider;

    [SetUp]
    public void Setup()
    {
        CleanTestData();
        _provider = new TmpWindowsDeviceUserDataProvider(Database);
    }

    [TearDown]
    public void TearDown()
    {
        CleanTestData();
    }

    [TestCase(null)]
    public void Delete_Throws_ArgumentNullException(IEnumerable<int> entities)
    {
        Assert.Throws<ArgumentNullException>(() => _provider.Delete(entities));
    }

    [Test]
    public void Delete_SuccessTest()
    {
        EnsureTestData();
        var idsToDelete = new[] { 1, 2, 3 };

        _provider.Delete(idsToDelete);

        var remainingCount = Database.CreateCommand(
            $"SELECT COUNT(*) FROM TmpWindowsDeviceUser WHERE WindowsDeviceUserId IN ({_windowsDeviceUserId1}, {_windowsDeviceUserId2})")
            .ExecuteScalar<int>();

        Assert.AreEqual(0, remainingCount);
    }

    [Test]
    public void GetTmpLocalUsersBatch_SuccessTest()
    {
        EnsureTestData();

        var allRecords = _provider.GetTmpLocalUsersBatch(0, 3).ToList();

        var userNames = allRecords.Select(x => x.UserName != null ? Encoding.UTF8.GetString(x.UserName) : null).ToList();
        CollectionAssert.AreEquivalent(new[] { "user1", "user2", "user3" }, userNames);

        var firstPage = _provider.GetTmpLocalUsersBatch(1, 2).ToList();
        Assert.AreEqual(2, firstPage.Count);

        var firstPageUserNames = firstPage.Select(x => Encoding.UTF8.GetString(x.UserName)).ToList();

        Assert.AreEqual(2, firstPageUserNames.Intersect(new[] { "user1", "user2", "user3" }).Count());

        var secondPage = _provider.GetTmpLocalUsersBatch(2, 1).ToList();
        Assert.AreEqual(1, secondPage.Count);
        Assert.IsTrue(new[] { "user1", "user2", "user3" }.Contains(Encoding.UTF8.GetString(secondPage[0].UserName)));
    }

    private void EnsureTestData()
    {
        CleanTestData();

        try
        {
            Database.CreateCommand(
                "BEGIN TRANSACTION; " +
                "INSERT INTO TmpWindowsDeviceUser (WindowsDeviceUserId, UserNameEncrypted, UserFullNameEncrypted) " +
                "VALUES (1, CAST('user1' AS varbinary(100)), CAST('User One' AS varbinary(100))); " +

                "INSERT INTO TmpWindowsDeviceUser (WindowsDeviceUserId, UserNameEncrypted, UserFullNameEncrypted) " +
                "VALUES (2, CAST('user2' AS varbinary(100)), NULL); " +

                "INSERT INTO TmpWindowsDeviceUser (WindowsDeviceUserId, UserNameEncrypted, UserFullNameEncrypted) " +
                "VALUES (3, CAST('user3' AS varbinary(100)), CAST('User Three' AS varbinary(100))); " +
                "COMMIT;")
                .ExecuteNonQuery();
        }
        catch
        {
            CleanTestData();
            throw;
        }
    }

    private void CleanTestData()
    {
        try
        {
            Database.CreateCommand(
                "DELETE FROM TmpWindowsDeviceUser WHERE WindowsDeviceUserId IN (1, 2, 3)")
                .ExecuteNonQuery();

            var remaining = Database.CreateCommand(
                "SELECT COUNT(*) FROM TmpWindowsDeviceUser WHERE WindowsDeviceUserId IN (1, 2, 3)")
                .ExecuteScalar<int>();

            if (remaining > 0)
            {
                throw new Exception($"CleanTestData failed - {remaining} records remaining");
            }
        }
        finally
        {
            _windowsDeviceUserId1 = 0;
            _windowsDeviceUserId2 = 0;
        }
    }
}

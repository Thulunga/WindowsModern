using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Soti.Api.Metadata.DataRetrieval;
using Soti.MobiControl.DeviceGroupManagement;
using Soti.MobiControl.Devices;
using Soti.MobiControl.Emails;
using Soti.MobiControl.Emails.Model;
using Soti.MobiControl.Emails.Model.Profile;
using Soti.MobiControl.Events;
using Soti.MobiControl.Exceptions;
using Soti.MobiControl.Security.Authorization;
using Soti.MobiControl.Security.Authorization.Permissions;
using Soti.MobiControl.Security.Identity;
using Soti.MobiControl.Security.Identity.Model;
using Soti.MobiControl.Security.Management.Services;
using Soti.MobiControl.WebApi.Foundation.Mvc;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.MobiControl.WindowsModern.Web.Contracts;
using Soti.MobiControl.WindowsModern.Web.Implementation.Events;
using Soti.Time;
using DeviceGroupPeripheralSummary = Soti.MobiControl.WindowsModern.Models.DeviceGroupPeripheralSummary;
using ICsvConverter = Soti.Csv.ICsvConverter;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests;

[TestFixture]
internal sealed class DeviceGroupsPeripheralControllerTests
{
    private const int DeviceId = 1000;
    private const string GroupPath = "groupPath";
    private const int GroupId = 1;

    private Mock<IDeviceGroupPeripheralService> _deviceGroupPeripheralServiceMock;
    private Mock<IDeviceGroupIdentityMapper> _deviceGroupIdentityMapperMock;
    private Mock<IAccessibleDeviceGroupService> _accessibleDeviceGroupServiceMock;
    private Mock<IAccessControlManager> _accessControlManagerMock;
    private Mock<IEventDispatcher> _eventDispatcher;
    private Mock<IEmailConfigurationService> _emailConfigurationService;
    private Mock<IEmailNotificationService> _emailNotificationService;
    private Mock<ICsvConverter> _csvConverter;
    private Mock<IUserIdentityProvider> _userIdentityProvider;
    private Mock<ICurrentTimeSupplier> _timeSupplierMock;
    private DeviceGroupsPeripheralController _controller;

    [SetUp]
    public void SetUp()
    {
        _deviceGroupPeripheralServiceMock = new Mock<IDeviceGroupPeripheralService>();
        _deviceGroupIdentityMapperMock = new Mock<IDeviceGroupIdentityMapper>();
        _accessibleDeviceGroupServiceMock = new Mock<IAccessibleDeviceGroupService>();
        _accessControlManagerMock = new Mock<IAccessControlManager>();
        _eventDispatcher = new Mock<IEventDispatcher>();
        _emailConfigurationService = new Mock<IEmailConfigurationService>();
        _emailNotificationService = new Mock<IEmailNotificationService>();
        _csvConverter = new Mock<ICsvConverter>();
        _userIdentityProvider = new Mock<IUserIdentityProvider>();
        _timeSupplierMock = new Mock<ICurrentTimeSupplier>();

        _controller = new DeviceGroupsPeripheralController(
            _deviceGroupPeripheralServiceMock.Object,
            _deviceGroupIdentityMapperMock.Object,
            _accessibleDeviceGroupServiceMock.Object,
            _accessControlManagerMock.Object,
            _eventDispatcher.Object,
            _emailConfigurationService.Object,
            _emailNotificationService.Object,
            _csvConverter.Object,
            _userIdentityProvider.Object,
            _timeSupplierMock.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext,
        };
    }

    #region GetDeviceGroupsPeripherals

    [Test]
    public void GetDeviceGroupsPeripherals_Exception()
    {
        Assert.Throws<ValidationException>(() =>
            _controller.GetDeviceGroupsPeripherals(null, DeviceFamily.WindowsPhone));
        Assert.Throws<ValidationException>(() =>
            _controller.GetDeviceGroupsPeripherals("", DeviceFamily.WindowsPhone));
        Assert.Throws<ValidationException>(() =>
            _controller.GetDeviceGroupsPeripherals("9E581C83-8AB5-4889-9EB0-BBBA0C0E44A6", (DeviceFamily)567));

        var dataRetrievalOption = new DataRetrievalOptions
        {
            Skip = -1,
            Take = 1
        };

        Assert.Throws<ValidationException>(() =>
            _controller.GetDeviceGroupsPeripherals("9E581C83-8AB5-4889-9EB0-BBBA0C0E44A6", DeviceFamily.WindowsPhone, null, null, dataRetrievalOption));

        dataRetrievalOption = new DataRetrievalOptions
        {
            Skip = 0,
            Take = 0
        };

        Assert.Throws<ValidationException>(() =>
            _controller.GetDeviceGroupsPeripherals("9E581C83-8AB5-4889-9EB0-BBBA0C0E44A6",
                DeviceFamily.WindowsPhone,
                null,
                null, dataRetrievalOption));
    }

    [Test]
    public void GetDeviceGroupsPeripherals_Invalid_Statues()
    {
        Assert.Throws<ValidationException>(() =>
            _controller.GetDeviceGroupsPeripherals(
                "9E581C83-8AB5-4889-9EB0-BBBA0C0E44A6",
                DeviceFamily.WindowsPhone,
                null,
                new List<Enums.DevicePeripheralStatus> { (Enums.DevicePeripheralStatus)100 }));
    }

    [Test]
    public void GetDeviceGroupsPeripherals_Success()
    {
        var peripheralSummary = new List<DeviceGroupPeripheralSummary>
        {
            new Models.DeviceGroupPeripheralSummary
            {
                DeviceId = DeviceId,
                Manufacturer = "Lenovo",
                Name = "Peripheral1",
                PeripheralId = 1,
                PeripheralType = 1,
                Status = DevicePeripheralStatus.Connected,
                Version = "12.5678.9"
            }
        };
        _accessControlManagerMock.Setup(a =>
            a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.DeviceGroup, GroupId));
        _deviceGroupIdentityMapperMock.Setup(m =>
            m.GetId(GroupPath)).Returns(GroupId);
        _accessibleDeviceGroupServiceMock.Setup(s =>
            s.GetPermittedDeviceGroupIds(It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(new HashSet<int> { 1, 2, 3 });
        _deviceGroupPeripheralServiceMock.Setup(m =>
            m.GetPeripheralSummaryByFamilyIdAndGroupIds(It.IsAny<int>(), It.IsAny<List<int>>())).Returns(peripheralSummary);

        var result = _controller.GetDeviceGroupsPeripherals(
            GroupPath,
            DeviceFamily.WindowsPhone,
            "Peripheral1",
            new List<Enums.DevicePeripheralStatus> { Enums.DevicePeripheralStatus.Connected },
            new DataRetrievalOptions { Skip = 0, Take = 50 }).ToList();

        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count);

        _accessControlManagerMock.Verify(a =>
            a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.DeviceGroup, GroupId), Times.Once);
        _deviceGroupIdentityMapperMock.Verify(m =>
            m.GetId(GroupPath), Times.Once);
        _deviceGroupPeripheralServiceMock.Verify(m =>
            m.GetPeripheralSummaryByFamilyIdAndGroupIds(It.IsAny<int>(), It.IsAny<List<int>>()), Times.Once);
    }

    [Test]
    public void GetDeviceGroupsPeripherals_Empty_Success()
    {
        _accessControlManagerMock.Setup(a =>
            a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.DeviceGroup, GroupId));
        _deviceGroupIdentityMapperMock.Setup(m =>
            m.GetId(GroupPath)).Returns(GroupId);
        _accessibleDeviceGroupServiceMock.Setup(s => s.GetPermittedDeviceGroupIds(It.IsAny<string>(), It.IsAny<bool>())).Returns(new HashSet<int> { 1, 2, 3 });
        _deviceGroupPeripheralServiceMock.Setup(m =>
            m.GetPeripheralSummaryByFamilyIdAndGroupIds(It.IsAny<int>(), It.IsAny<List<int>>())).Returns([]);

        var result = _controller.GetDeviceGroupsPeripherals(
            GroupPath,
            DeviceFamily.WindowsPhone,
            null,
            null,
            new DataRetrievalOptions { Skip = 0, Take = 50 }).ToList();

        Assert.NotNull(result);
        Assert.AreEqual(0, result.Count);

        _accessControlManagerMock.Verify(a =>
            a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.DeviceGroup, GroupId), Times.Once);
        _deviceGroupIdentityMapperMock.Verify(m =>
            m.GetId(GroupPath), Times.Once);
        _deviceGroupPeripheralServiceMock.Verify(m =>
            m.GetPeripheralSummaryByFamilyIdAndGroupIds(It.IsAny<int>(), It.IsAny<List<int>>()), Times.Once);
    }

    #endregion

    #region EmailPeripheralsReport

    [Test]
    public void EmailPeripheralsReportInvalidParameter_ThrowsException()
    {
        Assert.Throws<ValidationException>(() => Task.Run(async () => { await _controller.EmailPeripheralsReport(" ", DeviceFamily.WindowsPhone, null); }).GetAwaiter().GetResult());
        Assert.Throws<ValidationException>(() => Task.Run(async () => { await _controller.EmailPeripheralsReport("path", (DeviceFamily)111, null); }).GetAwaiter().GetResult());
        Assert.AreEqual("Requested Parameter cannot be null.", Assert.Throws<ValidationException>(() => Task.Run(async () => { await _controller.EmailPeripheralsReport("path", DeviceFamily.WindowsPhone, null); }).GetAwaiter().GetResult())?.Message);

        var parameters = new PeripheralEmailReportParameters { EmailProfileName = "TestProfile" };
        Assert.AreEqual("Report Header Fields must be specified.", Assert.Throws<ValidationException>(() => Task.Run(async () => { await _controller.EmailPeripheralsReport("path", DeviceFamily.WindowsPhone, parameters); }).GetAwaiter().GetResult())?.Message);

        parameters.ReportHeaderFields = new[] { "InvalidColumnName" };
        Assert.AreEqual("Report Header Fields have invalid values.", Assert.Throws<ValidationException>(() => Task.Run(async () => { await _controller.EmailPeripheralsReport("path", DeviceFamily.WindowsPhone, parameters); }).GetAwaiter().GetResult())?.Message);
    }

    [Test]
    public void EmailPeripheralsReport_ThrowsSecurityException()
    {
        var report = new PeripheralEmailReportParameters
        {
            ReportHeaderFields = new[] { "Name" },
            EmailProfileName = "InvalidProfile",
            EmailSubject = "EmailSubject",
            EmailBody = "EmailBody"
        };
        _emailConfigurationService.Setup(m => m.GetByName(report.EmailProfileName)).Returns(new EmailServerConnection());
        Assert.AreEqual("SMTP connection with Name 'InvalidProfile' is not accessible or does not have a server Id.", Assert.Throws<SecurityException>(() => Task.Run(async () => { await _controller.EmailPeripheralsReport("path", DeviceFamily.WindowsPhone, report); }).GetAwaiter().GetResult())?.Message);
    }

    [Test]
    public void EmailPeripheralsReport_ThrowsValidationError()
    {
        var emailServerConnection = new EmailServerConnection
        {
            EmailServerId = 2,
        };
        var report = new PeripheralEmailReportParameters
        {
            ReportHeaderFields = new[] { "Name" },
            EmailProfileName = "ValidProfile",
            EmailSubject = "EmailSubject",
            EmailBody = "EmailBody",
            AppendAddresses = true,
        };
        _emailConfigurationService.Setup(m => m.GetByName("ValidProfile")).Returns(emailServerConnection);
        _emailConfigurationService.Setup(m => m.GetDefaultRecipients(2)).Returns(new DefaultRecipients());
        Assert.AreEqual("No destination email addresses specified.", Assert.Throws<ValidationException>(() => Task.Run(async () => { await _controller.EmailPeripheralsReport("path", DeviceFamily.WindowsPhone, report); }).GetAwaiter().GetResult())?.Message);
        report.ToAddresses = new[] { "abc" };
        Assert.AreEqual("Invalid email address specified 'abc'.", Assert.Throws<ValidationException>(() => Task.Run(async () => { await _controller.EmailPeripheralsReport("path", DeviceFamily.WindowsPhone, report); }).GetAwaiter().GetResult())?.Message);
        report.EmailProfileName = " ";
        Assert.AreEqual("Email Profile Name cannot be null.", Assert.Throws<ValidationException>(() => Task.Run(async () => { await _controller.EmailPeripheralsReport("path", DeviceFamily.WindowsPhone, report); }).GetAwaiter().GetResult())?.Message);
    }

    [Test]
    public async Task EmailPeripheralsReport_Empty_Success()
    {
        var emailServerConnection = new EmailServerConnection
        {
            EmailServerId = 2,
        };
        var report = new PeripheralEmailReportParameters
        {
            ReportHeaderFields = new[] { "Name" },
            Statuses = new[] { "Connected", "Disconnected" },
            PeripheralType = new[] { "Mouse", "Keyboard" },
            EmailProfileName = "TestProfile",
            EmailSubject = "TestSubject",
            ToAddresses = new[] { "test1@test.ng" },
            AppendAddresses = true
        };
        _userIdentityProvider.Setup(m => m.GetUserIdentity()).Returns(new UserIdentity());
        _accessibleDeviceGroupServiceMock.Setup(s => s.GetPermittedDeviceGroupIds(It.IsAny<string>(), It.IsAny<bool>())).Returns(new HashSet<int> { 1, 2, 3 });
        _deviceGroupPeripheralServiceMock.Setup(s => s.GetDeviceGroupsPeripheralSummary(It.IsAny<List<int>>())).Returns([]);
        _emailConfigurationService.Setup(m => m.GetByName("TestProfile")).Returns(emailServerConnection);
        var dummyData = Encoding.UTF8.GetBytes("Namedata");
        _csvConverter.Setup(m => m.GenerateCsvContent(It.IsAny<IEnumerable<IDictionary<string, string>>>(), It.IsAny<string[]>(), It.IsAny<Stream>(), It.IsAny<int>(), It.IsAny<CultureInfo>(), It.IsAny<Func<string, string>>(), It.IsAny<bool>())).Callback((IEnumerable<IDictionary<string, string>> csvData, string[] responseHeaderFields, Stream outputStream, int timeZoneOffset, CultureInfo cultureInfo, Func<string, string> localizeHeader, bool shouldCloseStream) =>
        {
            outputStream.Write(dummyData, 0, dummyData.Length);
        }).Returns(Task.CompletedTask);
        _emailNotificationService.Setup(m => m.SendImmediateEmailAndThrowExceptionOnFailure(It.IsAny<EmailServerConnection>(), It.IsAny<EmailMessage>())).Callback(
            (EmailServerConnection connection, EmailMessage message) =>
            {
                Assert.That(connection, Is.Not.Null);
                Assert.That(message, Is.Not.Null);
                Assert.That(message.Attachment, Is.Not.Null);
                Assert.That(message.Priority, Is.EqualTo(EmailPriority.Normal));
                Assert.That(message.Subject, Is.EqualTo("TestSubject"));
                Assert.That(message.Recipients, Is.Not.Null);
            });
        _timeSupplierMock.Setup(x => x.GetUtcNow()).Returns(DateTime.UtcNow);
        await _controller.EmailPeripheralsReport("Path", DeviceFamily.All, report);
    }

    [Test]
    public async Task EmailWindowsFirmwareUpdatePoliciesReport_OnSuccess()
    {
        var emailServerConnection = new EmailServerConnection
        {
            EmailServerId = 2,
        };
        var report = new PeripheralEmailReportParameters
        {
            ReportHeaderFields = new[] { "Name" },
            NameContains = "Summary",
            Statuses = new[] { "Connected", "Disconnected" },
            PeripheralType = new[] { "Mouse" },
            EmailProfileName = "TestProfile",
            EmailSubject = "TestSubject",
            EmailBody = "TestBody",
            ToAddresses = new[] { "test1@test.ng" },
            CcAddresses = new[] { "test2@test.ng" },
            BccAddresses = new[] { "test3@test.ng" },
            AppendAddresses = true
        };
        _userIdentityProvider.Setup(m => m.GetUserIdentity()).Returns(new UserIdentity());
        _accessibleDeviceGroupServiceMock.Setup(s => s.GetPermittedDeviceGroupIds(It.IsAny<string>(), It.IsAny<bool>())).Returns(new HashSet<int> { 1, 2, 3 });
        _deviceGroupPeripheralServiceMock.Setup(s => s.GetPeripheralSummaryByFamilyIdAndGroupIds(It.IsAny<int>(), It.IsAny<List<int>>())).Returns(GetDeviceGroupPeripheralSummaries);
        _emailConfigurationService.Setup(m => m.GetDefaultRecipients(2)).Returns(
            new DefaultRecipients
            {
                Recipients = new[]
                {
                        new DefaultEmailRecipient
                        {
                            AddresseeType = EmailAddresseeType.To,
                            Email = "DefaultMail1@test.ng",
                            Enabled = false,
                            Name = "TestUser1"
                        },
                        new DefaultEmailRecipient
                        {
                            AddresseeType = EmailAddresseeType.CarbonCopy,
                            Email = "DefaultMail2@test.ng",
                            Enabled = false,
                            Name = "TestUser2"
                        },
                        new DefaultEmailRecipient
                        {
                            AddresseeType = EmailAddresseeType.BlindCarbonCopy,
                            Email = "DefaultMail3@test.ng",
                            Enabled = false,
                            Name = "TestUser3"
                        }
                }
            });
        _emailConfigurationService.Setup(m => m.GetByName("TestProfile")).Returns(emailServerConnection);
        var dummyData = Encoding.UTF8.GetBytes("Namedata");
        _csvConverter.Setup(m => m.GenerateCsvContent(
            It.IsAny<IEnumerable<IDictionary<string, string>>>(),
            It.IsAny<string[]>(),
            It.IsAny<Stream>(),
            It.IsAny<int>(),
            It.IsAny<CultureInfo>(),
            It.IsAny<Func<string, string>>(),
            It.IsAny<bool>()))
            .Callback((
                      IEnumerable<IDictionary<string, string>>
                      csvData, string[] responseHeaderFields,
                      Stream outputStream, int timeZoneOffset,
                      CultureInfo cultureInfo, Func<string, string> localizeHeader, bool shouldCloseStream) =>
            {
                outputStream.Write(dummyData, 0, dummyData.Length);
            }).Returns(Task.CompletedTask);
        _emailNotificationService.Setup(m => m.SendImmediateEmailAndThrowExceptionOnFailure(It.IsAny<EmailServerConnection>(), It.IsAny<EmailMessage>())).Callback(
            (EmailServerConnection connection, EmailMessage message) =>
            {
                Assert.That(connection, Is.Not.Null);
                Assert.That(message, Is.Not.Null);
                Assert.That(connection, Is.EqualTo(emailServerConnection));
                Assert.That(message.Attachment, Is.Not.Null);
                Assert.That(message.Priority, Is.EqualTo(EmailPriority.Normal));
                Assert.That(message.TextBody, Is.EqualTo("TestBody"));
                Assert.That(message.Subject, Is.EqualTo("TestSubject"));
                Assert.That(message.Recipients, Is.Not.Null);
                Assert.That(message.Recipients.SingleOrDefault(r => r.AddresseeType == EmailAddresseeType.To && r.Email == "test1@test.ng"), Is.Not.Null);
                Assert.That(message.Recipients.SingleOrDefault(r => r.AddresseeType == EmailAddresseeType.To && r.Email == "DefaultMail1@test.ng"), Is.Not.Null);
                Assert.That(message.Recipients.SingleOrDefault(r => r.AddresseeType == EmailAddresseeType.CarbonCopy && r.Email == "test2@test.ng"), Is.Not.Null);
                Assert.That(message.Recipients.SingleOrDefault(r => r.AddresseeType == EmailAddresseeType.CarbonCopy && r.Email == "DefaultMail2@test.ng"), Is.Not.Null);
                Assert.That(message.Recipients.SingleOrDefault(r => r.AddresseeType == EmailAddresseeType.BlindCarbonCopy && r.Email == "test3@test.ng"), Is.Not.Null);
                Assert.That(message.Recipients.SingleOrDefault(r => r.AddresseeType == EmailAddresseeType.BlindCarbonCopy && r.Email == "DefaultMail3@test.ng"), Is.Not.Null);
            });
        _eventDispatcher.Setup(m => m.DispatchEvent(It.IsAny<IEvent>())).Callback(
            (IEvent e) =>
            {
                var ev = e as PeripheralEmailReportSuccessEvent;
                Assert.AreEqual("DefaultMail1@test.ng, DefaultMail2@test.ng, DefaultMail3@test.ng, test1@test.ng, test2@test.ng, test3@test.ng", ev?.EventAdditionalParameters[0]);
            });

        _timeSupplierMock.Setup(x => x.GetUtcNow()).Returns(DateTime.UtcNow);

        await _controller.EmailPeripheralsReport("path", DeviceFamily.WindowsPhone, report);

        _deviceGroupPeripheralServiceMock.Verify(m => m.GetPeripheralSummaryByFamilyIdAndGroupIds(It.IsAny<int>(), It.IsAny<List<int>>()), Times.Once);
        _emailConfigurationService.Verify(m => m.GetByName("TestProfile"), Times.Once);
        _emailNotificationService.Verify(m => m.SendImmediateEmailAndThrowExceptionOnFailure(It.IsAny<EmailServerConnection>(), It.IsAny<EmailMessage>()), Times.Once);
        _eventDispatcher.Verify(m => m.DispatchEvent(It.IsAny<IEvent>()), Times.Once);
    }

    #endregion

    #region DownloadPeripheralsReport

    [Test]
    public void DownloadPeripheralsReport_ThrowsContractValidationError()
    {
        string[] dummy = ["ReferenceId"];
        string[] validHeaderFields = ["Name"];
        string[] invalidStatus = ["Enabled"];
        string[] invalidPeripheralTypes = ["Mouse1"];
        Assert.Throws<ValidationException>(() => _controller.DownloadPeripheralsReport(" ", DeviceFamily.WindowsPhone));
        Assert.Throws<ValidationException>(() => _controller.DownloadPeripheralsReport("path", (DeviceFamily)111));
        Assert.AreEqual("Report Header Fields must be specified.", Assert.Throws<ValidationException>(() => _controller.DownloadPeripheralsReport("path", DeviceFamily.WindowsPhone))?.Message);
        Assert.AreEqual("Report Header Fields have invalid values.", Assert.Throws<ValidationException>(() => _controller.DownloadPeripheralsReport("path", DeviceFamily.WindowsPhone, dummy))?.Message);
        Assert.AreEqual("Peripheral status 'Enabled' is invalid.", Assert.Throws<ValidationException>(() => _controller.DownloadPeripheralsReport("path", DeviceFamily.WindowsPhone, validHeaderFields, "Test", invalidStatus))?.Message);
        Assert.AreEqual("Peripheral type 'Mouse1' is invalid.", Assert.Throws<ValidationException>(() => _controller.DownloadPeripheralsReport("path", DeviceFamily.WindowsPhone, validHeaderFields, "Test", null, invalidPeripheralTypes))?.Message);
    }

    [Test]
    public void DownloadPeripheralsReport_Empty_Success()
    {
        string[] validHeaderFields = ["Name"];
        _accessibleDeviceGroupServiceMock.Setup(s =>
            s.GetPermittedDeviceGroupIds(It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(new HashSet<int> { 1, 2, 3 });
        _deviceGroupPeripheralServiceMock.Setup(s =>
            s.GetDeviceGroupsPeripheralSummary(It.IsAny<List<int>>())).Returns([]);
        _controller.DownloadPeripheralsReport("path", DeviceFamily.All, validHeaderFields);
    }

    [Test]
    public void DownloadWindowsFirmwareUpdatePoliciesSummary_Success()
    {
        string[] dummy1 = ["Name"];
        string[] peripheralType = ["Mouse"];
        _userIdentityProvider.Setup(m => m.GetUserIdentity()).Returns(new UserIdentity());
        _accessibleDeviceGroupServiceMock.Setup(s => s.GetPermittedDeviceGroupIds(It.IsAny<string>(), It.IsAny<bool>())).Returns(new HashSet<int> { 1, 2, 3 });
        _deviceGroupPeripheralServiceMock.Setup(s => s.GetPeripheralSummaryByFamilyIdAndGroupIds(It.IsAny<int>(), It.IsAny<List<int>>())).Returns(GetDeviceGroupPeripheralSummaries);

        var dummyData = Encoding.UTF8.GetBytes("Data");
        _csvConverter.Setup(m => m.GenerateCsvContent(It.IsAny<IEnumerable<IDictionary<string, string>>>(), It.IsAny<string[]>(), It.IsAny<Stream>(), It.IsAny<int>(), It.IsAny<CultureInfo>(), It.IsAny<Func<string, string>>(), It.IsAny<bool>())).Callback((IEnumerable<IDictionary<string, string>> csvData, string[] responseHeaderFields, Stream outputStream, int timeZoneOffset, CultureInfo cultureInfo, Func<string, string> localizeHeader, bool shouldCloseStream) =>
        {
            outputStream.Write(dummyData, 0, dummyData.Length);
            outputStream.Close();
        }).Returns(Task.CompletedTask);
        _eventDispatcher.Setup(m => m.DispatchEvent(It.IsAny<PeripheralDownloadReportSuccessEvent>()));
        _timeSupplierMock.Setup(x => x.GetUtcNow()).Returns(DateTime.UtcNow);
        var statuses = new[] { "Connected" };
        var result = _controller.DownloadPeripheralsReport(
            "path",
            DeviceFamily.WindowsPhone,
            dummy1,
            null,
            statuses,
            peripheralType);

        Assert.NotNull(result);
        var pushStreamResult = result as PushStreamResult;
        Assert.NotNull(pushStreamResult);
        _eventDispatcher.Verify(m => m.DispatchEvent(It.IsAny<PeripheralDownloadReportSuccessEvent>()), Times.Once);
    }

    #endregion

    #region Private Methods

    private static IReadOnlyList<DeviceGroupPeripheralSummary> GetDeviceGroupPeripheralSummaries()
    {
        return new List<DeviceGroupPeripheralSummary>
        {
            new DeviceGroupPeripheralSummary
            {
                DeviceId = 10001,
                Name = "peripheral1",
                PeripheralType = 1,
                Manufacturer = "Lenovo",
                PeripheralId = 1,
                Status = DevicePeripheralStatus.Connected,
                Version = "12.45.67"
            },
            new DeviceGroupPeripheralSummary
            {
                DeviceId = 10002,
                Name = "peripheral2",
                PeripheralType = 2,
                Manufacturer = "Dell",
                PeripheralId = 2,
                Status = DevicePeripheralStatus.Disconnected,
                Version = "32.75.67"
            }
        };
    }

    #endregion
}

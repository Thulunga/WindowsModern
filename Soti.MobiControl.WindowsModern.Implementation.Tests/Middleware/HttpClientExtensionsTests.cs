using NUnit.Framework;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Soti.MobiControl.WindowsModern.Implementation.Middleware;
using Soti.MobiControl.WindowsModern.Models.GroupPolicy;
using RichardSzalay.MockHttp;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests.Middleware;

[TestFixture]
internal sealed class HttpClientExtensionsTests
{
    private const string TestRegCode = "TestRegCode";
    private const string TestInstallationId = "TestInstallationId";
    private const string SharedFilesUri = "https://services-test.soti.net/SharedFiles/GroupPolicyObjectsSettings_v1";

    private MockHttpMessageHandler _httpMessageHandlerMock;
    private HttpClient _httpClient;

    [SetUp]
    public void Setup()
    {
        _httpMessageHandlerMock = new MockHttpMessageHandler();

        _httpClient = new HttpClient(_httpMessageHandlerMock)
        {
            BaseAddress = new Uri(SharedFilesUri)
        };
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
        _httpMessageHandlerMock?.Dispose();
    }

    [Test]
    public async Task GetGroupPolicySettingsAsync_Success()
    {
        var testAuthSettings = new AuthenticationSettings
        {
            RegistrationCode = TestRegCode,
            InstallationId = TestInstallationId
        };

        var latestWindowsModernGroupPolicyObjectsSettings = new WindowsModernGroupPolicyObjectsSettings
        {
            Version = new Version(2, 0, 0),
            GroupPolicySettingGroups = []
        };

        _httpMessageHandlerMock
            .When(SharedFilesUri)
            .Respond("application/json", JsonConvert.SerializeObject(latestWindowsModernGroupPolicyObjectsSettings));

        var result = await _httpClient.GetSettingsAsync(testAuthSettings, SharedFilesUri);
        Assert.IsNotNull(result);
    }
}

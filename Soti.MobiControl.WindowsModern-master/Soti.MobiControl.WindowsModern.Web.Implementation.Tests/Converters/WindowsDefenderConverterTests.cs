using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Web.Enums;
using Soti.MobiControl.WindowsModern.Web.Implementation.Converters;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests.Converters;

[TestFixture]
internal sealed class WindowsDefenderConverterTests
{
    #region Tests
    [Test]
    public void ToModel_AntivirusThreatCategory_Test()
    {
        Assert.AreEqual(Enum.GetValues(typeof(Models.Enums.AntivirusThreatType)).Cast<Models.Enums.AntivirusThreatType>(), Enum.GetValues(typeof(AntivirusThreatCategory)).Cast<AntivirusThreatCategory>().ToModel());
    }

    [Test]
    public void ToModel_AntivirusThreatStatus_Test()
    {
        Assert.AreEqual(Enum.GetValues(typeof(Models.Enums.AntivirusThreatStatus)).Cast<Models.Enums.AntivirusThreatStatus>(), Enum.GetValues(typeof(AntivirusThreatStatus)).Cast<AntivirusThreatStatus>().ToModel());
    }

    [Test]
    public void ToModel_AntivirusThreatSeverity_Test()
    {
        Assert.AreEqual(Enum.GetValues(typeof(Models.Enums.AntivirusThreatSeverity)).Cast<Models.Enums.AntivirusThreatSeverity>(), Enum.GetValues(typeof(AntivirusThreatSeverity)).Cast<AntivirusThreatSeverity>().ToModel());
    }

    [Test]
    public void ToDeviceThreatContract_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => WindowsDefenderConverter.ToDeviceThreatContract(null));
    }

    [Test]
    public void ToGroupThreatContract_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => WindowsDefenderConverter.ToGroupThreatContract(null));
    }

    [Test]
    public void ToLastScanSummary_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => WindowsDefenderConverter.ToLastScanSummary(null));
    }

    [TestCaseSource(nameof(AntivirusThreatHistoryTestData))]
    public void ToContract_AntivirusThreatHistory_Test(AntivirusDeviceThreatInfo antivirusThreatInfo)
    {
        var antivirusThreatHistories = new List<AntivirusDeviceThreatInfo> { antivirusThreatInfo }.ToDeviceThreatContract();
        var antivirusThreatHistory = antivirusThreatHistories.FirstOrDefault();

        Assert.IsNotNull(antivirusThreatHistory);
        Assert.AreEqual(antivirusThreatInfo.ExternalThreatId, antivirusThreatHistory.ThreatId);
        Assert.AreEqual(antivirusThreatInfo.CurrentDetectionCount, antivirusThreatHistory.NumberOfDetectionsTillDate);
        Assert.AreEqual(antivirusThreatInfo.InitialDetectionTime, antivirusThreatHistory.InitialDetectionTime);
        Assert.AreEqual(antivirusThreatInfo.LastStatusChangeTime, antivirusThreatHistory.LastStatusChangeTime);
        Assert.AreEqual(antivirusThreatInfo.ThreatName, antivirusThreatHistory.Name);
    }

    [Test]
    public void ToDictionary_DeviceThreatDetails_Test()
    {
        var threatStatus = Models.Enums.AntivirusThreatStatus.Active;
        var deviceReferenceId = Guid.NewGuid().ToString();
        var initialDetectionTime = new DateTime(2024, 10, 22, 10, 0, 0, DateTimeKind.Utc);
        var lastStatusChangeTime = new DateTime(2024, 10, 22, 12, 0, 0, DateTimeKind.Utc);

        var threatDetails = new DeviceThreatDetails
        {
            DevId = deviceReferenceId,
            InitialDetectionTime = initialDetectionTime,
            LastStatusChangeTime = lastStatusChangeTime,
            ThreatStatus = threatStatus
        };

        var result = threatDetails.ToDictionary();

        Assert.IsNotNull(result);
        Assert.AreEqual(4, result.Count);
        Assert.AreEqual(threatStatus.ToString(), result[nameof(DeviceThreatDetails.ThreatStatus)]);
        Assert.AreEqual(deviceReferenceId, result[nameof(DeviceThreatDetails.DevId)]);
        Assert.AreEqual(initialDetectionTime.ToString(CultureInfo.InvariantCulture), result[nameof(DeviceThreatDetails.InitialDetectionTime)]);
        Assert.AreEqual(lastStatusChangeTime.ToString(CultureInfo.InvariantCulture), result[nameof(DeviceThreatDetails.LastStatusChangeTime)]);
    }

    [Test]
    public void ToDictionary_DeviceThreatDetails_Throws_ArgumentNullException()
    {
        DeviceThreatDetails threatDetails = null;
        var ex = Assert.Throws<ArgumentNullException>(() => threatDetails.ToDictionary());
        Assert.AreEqual("threatDetails", ex.ParamName);
    }

    [Test]
    public void ToDictionary_AntivirusGroupThreatHistoryDetails_Test()
    {
        var threatId = 1;
        var threatCategory = Models.Enums.AntivirusThreatType.Malware;
        var severity = Models.Enums.AntivirusThreatSeverity.Low;
        var initialDetectionTime = new DateTime(2024, 10, 22, 10, 0, 0, DateTimeKind.Utc);
        var lastStatusChangeTime = new DateTime(2024, 10, 22, 12, 0, 0, DateTimeKind.Utc);

        var deviceGroupThreatHistory = new AntivirusGroupThreatHistoryDetails
        {
            ExternalThreatId = threatId,
            ThreatCategory = threatCategory,
            Severity = severity,
            InitialDetectionTime = initialDetectionTime,
            LastStatusChangeTime = lastStatusChangeTime,
            NumberOfDevices = 1
        };

        var result = deviceGroupThreatHistory.ToDictionary();

        Assert.IsNotNull(result);
        Assert.AreEqual(6, result.Count);
        Assert.AreEqual(threatCategory.ToString(), result[nameof(AntivirusGroupThreatHistoryDetails.ThreatCategory)]);
        Assert.AreEqual(severity.ToString(), result[nameof(AntivirusGroupThreatHistoryDetails.Severity)]);
        Assert.AreEqual(initialDetectionTime.ToString(CultureInfo.InvariantCulture), result[nameof(DeviceThreatDetails.InitialDetectionTime)]);
        Assert.AreEqual(lastStatusChangeTime.ToString(CultureInfo.InvariantCulture), result[nameof(DeviceThreatDetails.LastStatusChangeTime)]);
    }

    [Test]
    public void ToDictionary_AntivirusGroupThreatHistoryDetails_Throws_ArgumentNullException()
    {
        AntivirusGroupThreatHistoryDetails antivirusGroupThreatHistoryDetails = null;
        var ex = Assert.Throws<ArgumentNullException>(() => antivirusGroupThreatHistoryDetails.ToDictionary());
        Assert.AreEqual("antivirusGroupThreatHistoryDetails", ex.ParamName);
    }

    [Test]
    public void ToDictionary_AntivirusLastScanDeviceSummaryContract_Test()
    {
        var devId = Guid.NewGuid().ToString();
        var lastScanDate = new DateTime(2024, 10, 22, 12, 0, 0, DateTimeKind.Utc);

        var antivirusLastScanDeviceSummary = new AntivirusLastScanDeviceSummary
        {
            DevId = devId,
            LastScanDate = lastScanDate
        };

        var result = antivirusLastScanDeviceSummary.ToDictionary();

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(devId, result[nameof(AntivirusLastScanDeviceSummary.DevId)]);
        Assert.AreEqual(lastScanDate.ToString(CultureInfo.InvariantCulture), result[nameof(AntivirusLastScanDeviceSummary.LastScanDate)]);
    }

    [Test]
    public void ToDictionary_AntivirusLastScanDeviceSummaryContract_Throws_ArgumentNullException()
    {
        AntivirusLastScanDeviceSummary antivirusLastScanDeviceSummary = null;
        var ex = Assert.Throws<ArgumentNullException>(() => antivirusLastScanDeviceSummary.ToDictionary());
        Assert.AreEqual("antivirusLastScanDeviceSummaryModel", ex.ParamName);
    }

    [Test]
    public void ToExportThreatStatusDictionary_DeviceThreatStatus_Test()
    {
        var threatStatus = new AntivirusThreatStatusDeviceDetails
        {
            DevId = "device-123",
            LastStatusChangeTime = new DateTime(2024, 10, 22, 12, 0, 0, DateTimeKind.Utc),
            ExternalThreatId = 78757,
            ThreatCategory = Models.Enums.AntivirusThreatType.Cookie,
            Severity = Models.Enums.AntivirusThreatSeverity.Low,
        };

        var result = threatStatus.ToExportThreatStatusDictionary();

        Assert.IsNotNull(result);
        Assert.AreEqual(5, result.Count);
        Assert.AreEqual("device-123", result[nameof(AntivirusThreatStatusDeviceDetails.DevId)]);
        Assert.AreEqual("10/22/2024 12:00:00", result[nameof(AntivirusThreatStatusDeviceDetails.LastStatusChangeTime)]);
        Assert.AreEqual("78757", result[nameof(AntivirusThreatStatusDeviceDetails.ExternalThreatId)]);
        Assert.AreEqual("Cookie", result[nameof(AntivirusThreatStatusDeviceDetails.ThreatCategory)]);
        Assert.AreEqual("Low", result[nameof(AntivirusThreatStatusDeviceDetails.Severity)]);
    }

    [Test]
    public void ToExportThreatStatusDictionary_DeviceThreatStatus_Throws_ArgumentNullException()
    {
        AntivirusThreatStatusDeviceDetails threatStatus = null;
        var ex = Assert.Throws<ArgumentNullException>(() => threatStatus.ToExportThreatStatusDictionary());
        Assert.AreEqual("threatStatus", ex.ParamName);
    }

    [Test]
    public void ToExportableDictionary_AntivirusDeviceThreatInfo_Test()
    {
        var date = new DateTime(2024, 10, 21, 12, 12, 29);

        var antivirusDeviceThreatInfo = new AntivirusDeviceThreatInfo()
        {
            ThreatName = "Test",
            Type = Models.Enums.AntivirusThreatType.Malware,
            InitialDetectionTime = date,
            LastStatusChangeTime = date,
            LastThreatStatus = Models.Enums.AntivirusThreatStatus.Active,
            CurrentDetectionCount = 2,
            Severity = Models.Enums.AntivirusThreatSeverity.High,
            ExternalThreatId = 1
        };
        var expectedData = new Dictionary<string, string>
        {
            [nameof(antivirusDeviceThreatInfo.ThreatName)] = "Test",
            [nameof(antivirusDeviceThreatInfo.Type)] = nameof(Models.Enums.AntivirusThreatType.Malware),
            [nameof(antivirusDeviceThreatInfo.InitialDetectionTime)] = date.ToString(CultureInfo.InvariantCulture),
            [nameof(antivirusDeviceThreatInfo.LastStatusChangeTime)] = date.ToString(CultureInfo.InvariantCulture),
            [nameof(antivirusDeviceThreatInfo.LastThreatStatus)] = nameof(Models.Enums.AntivirusThreatStatus.Active),
            [nameof(antivirusDeviceThreatInfo.CurrentDetectionCount)] = "2",
            [nameof(antivirusDeviceThreatInfo.Severity)] = nameof(Models.Enums.AntivirusThreatSeverity.High),
            [nameof(antivirusDeviceThreatInfo.ExternalThreatId)] = "1"
        };

        var output = antivirusDeviceThreatInfo.ToExportableDictionary(0);
        Assert.IsNotNull(output);
        Assert.AreEqual(expectedData.Count, output.Count);
        CollectionAssert.AreEquivalent(output, expectedData);
    }

    [Test]
    public void ToExportableDictionary_AntivirusDeviceThreatInfo_ThrowsArgumentNullException()
    {
        AntivirusDeviceThreatInfo antivirusDeviceThreatInfo = null;
        Assert.Throws<ArgumentNullException>(() => antivirusDeviceThreatInfo.ToExportableDictionary());
    }

    #endregion

    #region Test Source

    private static IEnumerable<TestCaseData> AntivirusThreatHistoryTestData()
    {
        var categories = Enum.GetValues(typeof(Models.Enums.AntivirusThreatType)).Cast<Models.Enums.AntivirusThreatType>();
        foreach (var category in categories)
        {
            yield return new TestCaseData(new AntivirusDeviceThreatInfo()
            {
                ExternalThreatId = 1,
                Type = category,
                CurrentDetectionCount = 1,
                InitialDetectionTime = DateTime.MinValue,
                LastStatusChangeTime = DateTime.MaxValue,
                LastThreatStatus = Models.Enums.AntivirusThreatStatus.Active,
                Severity = Models.Enums.AntivirusThreatSeverity.Unknown,
                ThreatName = "ABC"
            });
        }

        var statuses = Enum.GetValues(typeof(Models.Enums.AntivirusThreatStatus)).Cast<Models.Enums.AntivirusThreatStatus>();
        foreach (var status in statuses)
        {
            yield return new TestCaseData(new AntivirusDeviceThreatInfo()
            {
                ExternalThreatId = 1,
                Type = Models.Enums.AntivirusThreatType.Adware,
                CurrentDetectionCount = 1,
                InitialDetectionTime = DateTime.MinValue,
                LastStatusChangeTime = DateTime.MaxValue,
                LastThreatStatus = status,
                Severity = Models.Enums.AntivirusThreatSeverity.Unknown,
                ThreatName = "ABC"
            });
        }

        var severities = Enum.GetValues(typeof(Models.Enums.AntivirusThreatSeverity)).Cast<Models.Enums.AntivirusThreatSeverity>();
        foreach (var severity in severities)
        {
            yield return new TestCaseData(new AntivirusDeviceThreatInfo()
            {
                ExternalThreatId = 1,
                Type = Models.Enums.AntivirusThreatType.Adware,
                CurrentDetectionCount = 1,
                InitialDetectionTime = DateTime.MinValue,
                LastStatusChangeTime = DateTime.MaxValue,
                LastThreatStatus = Models.Enums.AntivirusThreatStatus.Active,
                Severity = severity,
                ThreatName = "ABC"
            });
        }
    }

    #endregion
}

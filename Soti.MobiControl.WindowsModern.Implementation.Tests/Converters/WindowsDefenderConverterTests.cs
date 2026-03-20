using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Soti.MobiControl.WindowsModern.Implementation.Converters;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests.Converters;

[TestFixture]
internal sealed class WindowsDefenderConverterTests
{
    [Test]
    public void ToModel_AntivirusThreatData_ThrowsArgumentNullException_Test()
    {
        List<AntivirusThreatData> threatData = null;
        Assert.Throws<ArgumentNullException>(() => threatData.ToDeviceThreatInfoModel());
    }

    [Test]
    public void ToModel_AntivirusThreatData_SuccessTest()
    {
        var date = DateTime.Now;
        var threatData = new List<AntivirusThreatData>
        {
            new()
            {
                ExternalThreatId = 1,
                CurrentDetectionCount = 1,
                InitialDetectionTime = date,
                LastStatusChangeTime = date,
                LastThreatStatusId = 1,
                SeverityId = 1,
                ThreatName = "Test",
                TypeId = 1
            }
        };
        var expected = new AntivirusDeviceThreatInfo
        {
            ExternalThreatId = 1,
            CurrentDetectionCount = 1,
            InitialDetectionTime = date,
            LastStatusChangeTime = date,
            LastThreatStatus = AntivirusThreatStatus.ActionFailed,
            Severity = AntivirusThreatSeverity.Low,
            ThreatName = "Test",
            Type = AntivirusThreatType.Malware
        };
        var result = threatData.ToDeviceThreatInfoModel().FirstOrDefault();

        Assert.NotNull(result);
        Assert.AreEqual(expected.ExternalThreatId, result.ExternalThreatId);
        Assert.AreEqual(expected.CurrentDetectionCount, result.CurrentDetectionCount);
        Assert.AreEqual(expected.InitialDetectionTime, result.InitialDetectionTime);
        Assert.AreEqual(expected.LastStatusChangeTime, result.LastStatusChangeTime);
        Assert.AreEqual(expected.LastThreatStatus, result.LastThreatStatus);
        Assert.AreEqual(expected.Severity, result.Severity);
        Assert.AreEqual(expected.ThreatName, result.ThreatName);
        Assert.AreEqual(expected.Type, result.Type);
    }
}

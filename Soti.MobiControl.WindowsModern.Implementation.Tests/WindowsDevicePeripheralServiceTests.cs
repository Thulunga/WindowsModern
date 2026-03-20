using Moq;
using NUnit.Framework;
using Soti.Diagnostics;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.MobiControl.Events;
using Soti.MobiControl.Search.Database;
using Soti.MobiControl.Search.Database.Enums;
using Soti.MobiControl.Search.Database.Identities;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
internal sealed class WindowsDevicePeripheralServiceTests
{
    private Mock<IWindowsDevicePeripheralDataProvider> _windowsDevicePeripheralProviderMock;
    private Mock<IPeripheralManufacturerProvider> _manufacturerProviderMock;
    private Mock<IPeripheralDataProvider> _peripheralProviderMock;
    private Mock<IProgramTrace> _programTraceMock;
    private Mock<IEventDispatcher> _dispatcherMock;
    private Mock<IDeviceSearchInfoService> _deviceSearchInfoService;
    private WindowsDevicePeripheralService _service;
    private static IEnumerable<TestCaseData> PeripheralDataTestCases => new[]
    {
            new TestCaseData(
            new TestData
            {
                DeviceId = 1,
                DevId = "9F89BA4166428C44974842C49C960C9D",
                PeripheralKeysData =
                    "#Name:Peripheral3,Manufacturer:Manufacture1,DeviceID:17EF,Version:10.0.22621.1,PnPClass:Keyboard#Name:Peripheral4,Manufacturer:Manufacture2,DeviceID:17EF,Version:10.0.22621.2,PnPClass:Mouse#",
                ManufacturerData =
                [
                    new PeripheralManufacturerTestData
                    {
                        ManufacturerName = "Lenovo",
                        ManufacturerCode = "17EF",
                        ManufacturerId = 1
                    }
                ],
                PeripheralData =
                [
                    new PeripheralTestData
                    {
                        Name = "HID-compliant mouse",
                        PeripheralId = 1,
                        ManufacturerId = 1
                    },
                    new PeripheralTestData
                    {
                        Name = "HID-compliant keyboard",
                        PeripheralId = 2,
                        ManufacturerId = 1
                    }
                ]
            }),
            new TestCaseData(
                new TestData
                {
                    DeviceId = 1,
                    DevId = "9F89BA4166428C44974842C49C960C9D",
                    PeripheralKeysData =
                        "#Name:HID-compliant mouse,Manufacturer:Microsoft,DeviceID:17EF,Version:10.0.22621.1,PnPClass:Mouse#Name:HID-compliant keyboard,Manufacturer:Microsoft,DeviceID:17EF,Version:10.0.22621.2,PnPClass:Keyboard#",
                    ManufacturerData =
                    [
                        new PeripheralManufacturerTestData
                        {
                            ManufacturerName = "Lenovo",
                            ManufacturerCode = "17EF",
                            ManufacturerId = 1
                        }
                    ],
                    PeripheralData =
                    [
                        new PeripheralTestData
                        {
                            Name = "HID-compliant mouse",
                            PeripheralId = 1,
                            ManufacturerId = 1
                        },
                        new PeripheralTestData
                        {
                            Name = "HID-compliant keyboard",
                            PeripheralId = 2,
                            ManufacturerId = 1
                        }
                    ]
                }),
            new TestCaseData(
            new TestData
            {
                DeviceId = 1,
                DevId = "9F89BA4166428C44974842C49C960C9D",
                PeripheralKeysData = null,
                ManufacturerData =
                [
                    new PeripheralManufacturerTestData
                    {
                        ManufacturerName = "Lenovo",
                        ManufacturerCode = "17EF",
                        ManufacturerId = 1
                    }
                ],
                PeripheralData =
                [
                    new PeripheralTestData
                    {
                        Name = "HID-compliant mouse",
                        PeripheralId = 1,
                        ManufacturerId = 1
                    }
                ]
            }
        )
        };

    [SetUp]
    public void SetUp()
    {
        _windowsDevicePeripheralProviderMock = new Mock<IWindowsDevicePeripheralDataProvider>(MockBehavior.Strict);
        _manufacturerProviderMock = new Mock<IPeripheralManufacturerProvider>(MockBehavior.Strict);
        _peripheralProviderMock = new Mock<IPeripheralDataProvider>(MockBehavior.Strict);
        _dispatcherMock = new Mock<IEventDispatcher>(MockBehavior.Strict);
        _deviceSearchInfoService = new Mock<IDeviceSearchInfoService>(MockBehavior.Strict);
        _programTraceMock = new Mock<IProgramTrace>(MockBehavior.Strict);
        _service = new WindowsDevicePeripheralService(
            _programTraceMock.Object,
            _windowsDevicePeripheralProviderMock.Object,
            _peripheralProviderMock.Object,
            _manufacturerProviderMock.Object,
            _deviceSearchInfoService.Object,
            _dispatcherMock.Object
        );
    }

    [TearDown]
    public void TearDown()
    {
        _service.Dispose();
    }

    [Test]
    public void ConstructorTests()
    {
        var argumentCount = 1;
        for (var i = 0; i < argumentCount; i++)
        {
            var j = 0;
            Assert.Throws<ArgumentNullException>(() => _ = new WindowsDevicePeripheralService(
                i == j++ ? null : _programTraceMock.Object,
                i == j++ ? null : _windowsDevicePeripheralProviderMock.Object,
                i == j++ ? null : _peripheralProviderMock.Object,
                i == j++ ? null : _manufacturerProviderMock.Object,
                i == j++ ? null : _deviceSearchInfoService.Object,
                i == j++ ? null : _dispatcherMock.Object));

            argumentCount = j;
        }
    }

    [Test]
    public void GetDevicePeripheralsSummaryInfo_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetDevicePeripheralsSummaryInfo(-1));
    }

    [Test]
    public void GetDevicePeripheralsSummaryInfo_Success()
    {
        const int deviceId = 11;
        var devicePeripheralData1 = CreateDevicePeripheralData(deviceId, DevicePeripheralStatus.Disconnected, "10.0.22621.1", 1);
        var devicePeripheralData2 = CreateDevicePeripheralData(deviceId, DevicePeripheralStatus.Connected, "10.2.24621.1", 2);
        var peripheralData = CreatePeripheralData("HID-compliant mouse", 1, PeripheralType.Mouse);
        _windowsDevicePeripheralProviderMock.Setup(x => x.GetDevicePeripheralSummaryByDeviceId(deviceId))
            .Returns(new List<WindowsDevicePeripheralData> { devicePeripheralData1, devicePeripheralData2 });
        _peripheralProviderMock.Setup(x => x.GetPeripheralData(It.IsAny<int>())).Returns(peripheralData);
        _manufacturerProviderMock.Setup(x => x.GetAllPeripheralManufacturerData()).Returns(ManufacturerTestData);

        var result = _service.GetDevicePeripheralsSummaryInfo(deviceId);
        Assert.NotNull(result);
    }

    [Test]
    public void GetDevicePeripheralsSummaryInfo_Success_NoPeripherals()
    {
        const int deviceId = 10;
        _windowsDevicePeripheralProviderMock
            .Setup(x => x.GetDevicePeripheralSummaryByDeviceId(deviceId))
            .Returns((IReadOnlyList<WindowsDevicePeripheralData>)null);
        var result = _service.GetDevicePeripheralsSummaryInfo(deviceId);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [Test]
    public void GetDevicePeripheralsSummaryInfo_ManufacturerNotFoundInCache_RefreshesCache()
    {
        // Arrange
        const int deviceId = 11;
        const short missingManufacturerId = 99;
        var devicePeripheralData = CreateDevicePeripheralData(deviceId, DevicePeripheralStatus.Connected, "10.0.22621.1", 1);
        var peripheralData = CreatePeripheralData("HID-compliant mouse", missingManufacturerId, PeripheralType.Mouse);

        var initialManufacturers = new List<PeripheralManufacturerData>
        {
            new()
            {
                ManufacturerName = "Dell",
                ManufacturerId = 1,
                ManufacturerCode = "047C"
            }
        };

        var updatedManufacturers = new List<PeripheralManufacturerData>
        {
            new()
            {
                ManufacturerName = "Dell",
                ManufacturerId = 1,
                ManufacturerCode = "047C"
            },
            new()
            {
                ManufacturerName = "NewManufacturer",
                ManufacturerId = missingManufacturerId,
                ManufacturerCode = "99XX"
            }
        };

        _windowsDevicePeripheralProviderMock.Setup(x => x.GetDevicePeripheralSummaryByDeviceId(deviceId))
            .Returns(new List<WindowsDevicePeripheralData> { devicePeripheralData });
        _peripheralProviderMock.Setup(x => x.GetPeripheralData(It.IsAny<int>())).Returns(peripheralData);

        // First call returns manufacturers without the missing one, second call includes it
        _manufacturerProviderMock.SetupSequence(x => x.GetAllPeripheralManufacturerData())
            .Returns(initialManufacturers)
            .Returns(updatedManufacturers);

        // Act
        var result = _service.GetDevicePeripheralsSummaryInfo(deviceId);

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        _manufacturerProviderMock.Verify(x => x.GetAllPeripheralManufacturerData(), Times.Exactly(2));
    }

    [Test]
    public void SynchronizePeripheralDataWithSnapshot_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.SynchronizePeripheralDataWithSnapshot(0, "devId", "PeripheralsKeysData"));
    }

    [Test]
    public void SynchronizePeripheralDataWithSnapshot_FormatException()
    {
        var peripheralKeysData = "#Name:HID-compliant mouse,DeviceID:17EF,Version:10.0.22621.1,PnPClass:Mouse";
        _programTraceMock.Setup(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()));
        _windowsDevicePeripheralProviderMock.Setup(x => x.BulkModify(It.IsAny<int>(), It.IsAny<IEnumerable<WindowsDevicePeripheralData>>()))
            .Returns(new Dictionary<int, (byte, byte)>());
        _deviceSearchInfoService.Setup(m => m.AddOrUpdateRecords(SearchTable.Peripherals, It.IsAny<IEnumerable<Identifier>>())).Verifiable();
        _deviceSearchInfoService.Setup(m => m.AddOrUpdateRecords(SearchTable.DevicePeripherals, It.IsAny<IEnumerable<LinkedIdentifier>>())).Verifiable();

        _service.SynchronizePeripheralDataWithSnapshot(1, "9F89BA4166428C44974842C49C960C9D", peripheralKeysData);
        _programTraceMock.Verify(x => x.Write(TraceLevel.Error, It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        _deviceSearchInfoService.Verify(m => m.AddOrUpdateRecords(SearchTable.Peripherals, It.IsAny<IEnumerable<Identifier>>()), Times.Exactly(1));
        _deviceSearchInfoService.Verify(m => m.AddOrUpdateRecords(SearchTable.DevicePeripherals, It.IsAny<IEnumerable<LinkedIdentifier>>()), Times.Exactly(1));
    }

    [TestCase(" ")]
    [TestCase(null)]
    public void SynchronizePeripheralDataWithSnapshot_ArgumentException(string devId)
    {
        Assert.Throws<ArgumentException>(() => _service.SynchronizePeripheralDataWithSnapshot(1, devId, "PeripheralsKeysData"));
    }

    [TestCaseSource(nameof(PeripheralDataTestCases))]
    public void SynchronizePeripheralDataWithSnapshot_Success(TestData testData)
    {
        // Arrange
        _manufacturerProviderMock.Setup(x => x.GetPeripheralManufacturerByManufacturerCode(It.IsAny<string>())).Returns(ToPeripheralManufacturerData(testData.ManufacturerData)?.FirstOrDefault());
        _manufacturerProviderMock.Setup(x => x.InsertPeripheralManufacturerData(It.IsAny<PeripheralManufacturerData>()));
        _peripheralProviderMock.Setup(x => x.InsertPeripheralData(It.IsAny<PeripheralData>())).Returns(4);
        _peripheralProviderMock.Setup(x => x.GetAllPeripheralData()).Returns(ToPeripheralData(testData.PeripheralData));
        _peripheralProviderMock.Setup(m => m.GetPeripheralData(It.IsAny<int>())).Returns(ToPeripheralData(testData.PeripheralData).FirstOrDefault());
        _manufacturerProviderMock.Setup(x => x.GetAllPeripheralManufacturerData()).Returns(ToPeripheralManufacturerData(testData.ManufacturerData));
        _deviceSearchInfoService.Setup(p => p.AddOrUpdateRecords(SearchTable.Peripherals, It.IsAny<IEnumerable<Identifier>>()));
        _deviceSearchInfoService.Setup(p => p.AddOrUpdateRecords(SearchTable.DevicePeripherals, It.IsAny<IEnumerable<Identifier>>()));
        _windowsDevicePeripheralProviderMock.Setup(x => x.BulkModify(It.IsAny<int>(), It.IsAny<IEnumerable<WindowsDevicePeripheralData>>()))
            .Returns(new Dictionary<int, (byte, byte)> { { testData.PeripheralData[0].PeripheralId, (1, 2) } }).Callback((int deviceId, IEnumerable<WindowsDevicePeripheralData> devicePeripherals) =>
            {
                var peripherals = devicePeripherals.ToArray();
                Assert.IsNotNull(deviceId);
                Assert.IsNotNull(peripherals);
            });
        _programTraceMock.Setup(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()));
        _dispatcherMock.Setup(m => m.DispatchEvent(It.IsAny<IEvent>()));
        _peripheralProviderMock.Setup(m => m.UpdatePeripheralType(It.IsAny<int>(), It.IsAny<short>()));

        // Act & Assert
        _service.SynchronizePeripheralDataWithSnapshot(testData.DeviceId, testData.DevId, testData.PeripheralKeysData);

        _windowsDevicePeripheralProviderMock.Verify(x => x.BulkModify(It.IsAny<int>(), It.IsAny<IEnumerable<WindowsDevicePeripheralData>>()), Times.Once);
        _deviceSearchInfoService.Verify(m => m.AddOrUpdateRecords(SearchTable.DevicePeripherals, It.IsAny<IEnumerable<LinkedIdentifier>>()), Times.Exactly(1));
    }

    [Test]
    public void CleanUpObsoleteWindowsPeripheralData_Success()
    {
        IReadOnlyList<WindowsDevicePeripheralData> dummyData = new List<WindowsDevicePeripheralData>();
        _windowsDevicePeripheralProviderMock.Setup(m => m.CleanUpObsoleteWindowsPeripheralData(out dummyData)).Returns(5);
        _deviceSearchInfoService
        .Setup(m => m.DeleteRecords(SearchTable.DevicePeripherals, It.IsAny<IEnumerable<LinkedIdentifier>>()));

        var result = _service.CleanUpObsoleteWindowsPeripheralData();
        Assert.AreEqual(5, result);
        _deviceSearchInfoService.Verify(m => m.DeleteRecords(SearchTable.DevicePeripherals, It.IsAny<IEnumerable<LinkedIdentifier>>()), Times.Exactly(1));
    }

    [Test]
    public void DeleteDevicePeripheralsByDeviceId_ThrowsArgumentOutOfRangeException_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.CleanUpDevicePeripherals(-1));
    }

    [Test]
    public void DeleteDevicePeripheralsByDeviceId_Test()
    {
        const int deviceId = 1000001;

        _windowsDevicePeripheralProviderMock
            .Setup(x => x.DeleteDevicePeripheralByDeviceIdAndGetPeripheralData(deviceId))
            .Returns(Array.Empty<WindowsDevicePeripheralData>());
        _deviceSearchInfoService
            .Setup(m => m.DeleteRecords(SearchTable.DevicePeripherals, It.IsAny<IEnumerable<LinkedIdentifier>>()))
            .Verifiable();

        _service.CleanUpDevicePeripherals(deviceId);

        _windowsDevicePeripheralProviderMock.Verify(
            provider => provider.DeleteDevicePeripheralByDeviceIdAndGetPeripheralData(It.IsAny<int>()),
            Times.Exactly(1)
        );
        _deviceSearchInfoService.Verify(m => m.DeleteRecords(SearchTable.DevicePeripherals, It.IsAny<IEnumerable<LinkedIdentifier>>()), Times.Exactly(1));
    }

    [Test]
    public void DisposeTest()
    {
        Assert.DoesNotThrow(_service.Dispose);
    }

    private static WindowsDevicePeripheralData CreateDevicePeripheralData(int deviceId, DevicePeripheralStatus status, string version, int peripheralId)
    {
        return new WindowsDevicePeripheralData
        {
            DeviceId = deviceId,
            Status = status,
            Version = version,
            PeripheralId = peripheralId
        };
    }

    private static PeripheralData CreatePeripheralData(string name, short manufacturerId, PeripheralType peripheralType)
    {
        return new PeripheralData
        {
            Name = name,
            ManufacturerId = manufacturerId,
            PeripheralTypeId = (short)peripheralType
        };
    }

    private static IReadOnlyList<PeripheralManufacturerData> ToPeripheralManufacturerData(IEnumerable<PeripheralManufacturerTestData> data)
    {
        return data?.Select(x => new PeripheralManufacturerData
        {
            ManufacturerName = x.ManufacturerName,
            ManufacturerId = x.ManufacturerId,
            ManufacturerCode = x.ManufacturerCode
        }).ToList();
    }

    private static IReadOnlyList<PeripheralData> ToPeripheralData(IEnumerable<PeripheralTestData> data)
    {
        return data?.Select(x => new PeripheralData
        {
            Name = x.Name,
            PeripheralId = x.PeripheralId,
            ManufacturerId = x.ManufacturerId
        }).ToList();
    }

    private static IReadOnlyList<PeripheralManufacturerData> ManufacturerTestData()
    {
        return new List<PeripheralManufacturerData>
            {
                new()
                {
                    ManufacturerName = "Dell",
                    ManufacturerId = 1,
                    ManufacturerCode = "047C"
                },
                new()
                {
                    ManufacturerName = "Lenovo",
                    ManufacturerCode = "17EF",
                    ManufacturerId = 2
                }
            };
    }
    public class TestData
    {
        public int DeviceId { get; set; }

        public string DevId { get; set; }

        public string PeripheralKeysData { get; set; }

        public IReadOnlyList<PeripheralManufacturerTestData> ManufacturerData { get; set; }

        public IReadOnlyList<PeripheralTestData> PeripheralData { get; set; }
    }

    public class PeripheralManufacturerTestData
    {
        public short ManufacturerId { get; set; }

        public string ManufacturerCode { get; set; }

        public string ManufacturerName { get; set; }
    }

    public class PeripheralTestData
    {
        public string Name { get; set; }

        public int PeripheralId { get; set; }

        public short ManufacturerId { get; set; }
    }
}

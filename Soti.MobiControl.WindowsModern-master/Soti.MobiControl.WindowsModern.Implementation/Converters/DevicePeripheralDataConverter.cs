using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using System;

namespace Soti.MobiControl.WindowsModern.Implementation.Converters;

/// <summary>
/// Device peripheral data converter class.
/// </summary>
internal static class DevicePeripheralDataConverter
{
    /// <summary>
    /// Convert Windows Device Peripheral Data to device peripheral model class.
    /// </summary>
    /// <param name="devicePeripheralData">The device peripheral data.</param>
    /// <param name="peripheralName">The peripheral name.</param>
    /// <param name="manufacturerName">The manufacture name.</param>
    /// <param name="peripheralTypeId">The peripheral type id.</param>
    /// <returns>Device Peripheral Summary</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static DevicePeripheralSummary ToDevicePeripheralsModel(this WindowsDevicePeripheralData devicePeripheralData, string peripheralName, string manufacturerName, short peripheralTypeId)
    {
        if (devicePeripheralData == null)
        {
            throw new ArgumentNullException(nameof(devicePeripheralData));
        }

        if (string.IsNullOrWhiteSpace(peripheralName))
        {
            throw new ArgumentException(nameof(peripheralName));
        }

        if (string.IsNullOrWhiteSpace(manufacturerName))
        {
            throw new ArgumentException(nameof(manufacturerName));
        }

        return new DevicePeripheralSummary
        {
            Name = peripheralName,
            Manufacturer = manufacturerName,
            Version = devicePeripheralData.Version,
            Status = devicePeripheralData.Status,
            PeripheralType = (PeripheralType)peripheralTypeId
        };
    }

    /// <summary>
    /// Convert Windows Device Peripheral snapshot to device peripheral data class.
    /// </summary>
    /// <param name="snapshot">The device peripheral data.</param>
    /// <param name="peripheralId">The peripheral id.</param>
    /// <returns>Windows Device Peripheral Data</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static WindowsDevicePeripheralData ToDevicePeripheralData(this WindowsDevicePeripheralSnapShot snapshot, int peripheralId)
    {
        if (snapshot == null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        if (peripheralId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(peripheralId));
        }

        return new WindowsDevicePeripheralData
        {
            DeviceId = snapshot.DeviceId,
            PeripheralId = peripheralId,
            Status = DevicePeripheralStatus.Connected,
            Version = snapshot.Version
        };
    }
}

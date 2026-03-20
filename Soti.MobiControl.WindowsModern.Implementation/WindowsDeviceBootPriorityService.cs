using Soti.MobiControl.WindowsModern.Implementation.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern.Implementation;

/// <summary>
/// Service for managing Windows device boot priorities.
/// </summary>
internal sealed class WindowsDeviceBootPriorityService : IWindowsDeviceBootPriorityService
{
    private readonly IWindowsDeviceBootPriorityDataProvider _windowsDeviceBootPriorityDataProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsDeviceBootPriorityService"/> class
    /// with the specified boot priority provider.
    /// </summary>
    /// <param name="windowsDeviceBootPriorityDataProvider">The provider used to access and manage Windows device boot priority data.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="windowsDeviceBootPriorityDataProvider"/> is <c>null</c>.</exception>
    public WindowsDeviceBootPriorityService(IWindowsDeviceBootPriorityDataProvider windowsDeviceBootPriorityDataProvider)
    {
        _windowsDeviceBootPriorityDataProvider = windowsDeviceBootPriorityDataProvider ?? throw new ArgumentNullException(nameof(windowsDeviceBootPriorityDataProvider));
    }

    /// <inheritdoc />
    public void DeleteByDeviceId(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        _windowsDeviceBootPriorityDataProvider.DeleteByDeviceId(deviceId);
    }

    /// <inheritdoc />
    public IReadOnlyList<WindowsDeviceBootPriority> GetByDeviceId(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        return _windowsDeviceBootPriorityDataProvider.GetByDeviceId(deviceId);
    }

    /// <inheritdoc />
    public void BulkModify(IEnumerable<WindowsDeviceBootPriority> data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data), "Boot priority data cannot be null.");
        }

        var list = data.ToList();
        if (!list.Any())
        {
            throw new ArgumentException("Boot priority data cannot be empty.", nameof(data));
        }

        _windowsDeviceBootPriorityDataProvider.BulkModify(list);
    }

    /// <inheritdoc />
    public void BulkModifyForDevice(IEnumerable<WindowsDeviceBootPriority> data, int deviceId)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data), "Boot priority data cannot be null.");
        }

        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var list = data.ToList();
        if (!list.Any())
        {
            throw new ArgumentException("Boot priority data cannot be empty.", nameof(data));
        }

        _windowsDeviceBootPriorityDataProvider.BulkModifyForDevice(list, deviceId);
    }

    /// <inheritdoc />
    public IReadOnlyList<WindowsDeviceBootPriority> BulkGetByDeviceId(IEnumerable<int> deviceIds)
    {
        if (deviceIds == null)
        {
            throw new ArgumentNullException(nameof(deviceIds));
        }

        var ids = deviceIds.ToList();
        if (!ids.Any())
        {
            throw new ArgumentException($"{nameof(deviceIds)} cannot be empty", nameof(deviceIds));
        }

        return _windowsDeviceBootPriorityDataProvider.BulkGetByDeviceId(ids);
    }

    /// <inheritdoc />
    public IReadOnlyList<WindowsBootPriority> BulkInsertAndGet(IEnumerable<string> priorityNames)
    {
        if (priorityNames == null)
        {
            throw new ArgumentNullException(nameof(priorityNames), "Priority names list cannot be null.");
        }

        var names = priorityNames.ToList();
        if (!names.Any())
        {
            throw new ArgumentException("Priority names list cannot be empty.", nameof(priorityNames));
        }

        return _windowsDeviceBootPriorityDataProvider.BulkInsertAndGet(names);
    }

    /// <inheritdoc />
    public IReadOnlyList<WindowsBootPriority> BulkGetByIds(IEnumerable<short> priorityIds)
    {
        if (priorityIds == null)
        {
            throw new ArgumentNullException(nameof(priorityIds), "Priority ID list cannot be null.");
        }

        var ids = priorityIds.ToList();
        if (!ids.Any())
        {
            throw new ArgumentException("Priority ID list cannot be empty.", nameof(priorityIds));
        }

        return _windowsDeviceBootPriorityDataProvider.BulkGetByIds(ids);
    }
}

using System;
using Microsoft.Extensions.DependencyInjection;
using Soti.HostModule;

[assembly: HostModule(typeof(Soti.MobiControl.WindowsModern.Web.Implementation.HostApplicationModule))]

namespace Soti.MobiControl.WindowsModern.Web.Implementation;

public class HostApplicationModule : IHostApplicationModule
{
    /// <inheritdoc/>
    public void ConfigureHost(IHostApplicationModuleContext module)
    {
        ArgumentNullException.ThrowIfNull(module);

        module.Services.AddTransient<IWindowsDeviceProfileUiController, WindowsDeviceProfileUiController>();
        module.Services.AddTransient<IWindowsDevInfoUiController, WindowsDevInfoUiController>();
        module.Services.AddTransient<IWindowsDeviceConfigurationsController, WindowsDeviceConfigurationsController>();
        module.Services.AddTransient<IWindowsDeviceController, WindowsDeviceController>();
        module.Services.AddTransient<IWindowsDevicesPeripheralController, WindowsDevicesPeripheralController>();
        module.Services.AddTransient<IDeviceHardwareController, DeviceHardwareController>();
        module.Services.AddTransient<IWindowsDeviceGroupsController, WindowsDeviceGroupsController>();
        module.Services.AddTransient<IWindowsDeviceGroupsUiController, WindowsDeviceGroupsUiController>();
        module.Services.AddTransient<IDeviceGroupsPeripheralController, DeviceGroupsPeripheralController>();
    }
}

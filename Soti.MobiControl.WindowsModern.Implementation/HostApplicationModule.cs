using System;
using Microsoft.Extensions.DependencyInjection;
using Soti.HostModule;
using Soti.MobiControl.Messaging;
using Soti.MobiControl.WindowsModern.Implementation.Middleware;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;
using Soti.MobiControl.WindowsModern.Middleware;

[assembly: HostModule(typeof(Soti.MobiControl.WindowsModern.Implementation.HostApplicationModule))]

namespace Soti.MobiControl.WindowsModern.Implementation;

public class HostApplicationModule : IHostApplicationModule
{
    /// <inheritdoc/>
    public void ConfigureHost(IHostApplicationModuleContext module)
    {
        ArgumentNullException.ThrowIfNull(module);

        module.Services.AddSingleton<IDeviceBitLockerKeyProvider, DeviceBitLockerKeyProvider>();
        module.Services.AddSingleton<IDeviceBitLockerKeyService, DeviceBitLockerKeyService>();
        module.Services.AddSingleton<IWindowsPhoneDeviceService, WindowsPhoneDeviceService>();
        module.Services.AddSingleton<WindowsPhoneDeviceProvider, WindowsPhoneDeviceProvider>();
        module.Services.AddSingleton<IWindowsDeviceDataProvider, WindowsDeviceDataProvider>();
        module.Services.AddSingleton<IWindowsDeviceService, WindowsDeviceService>();
        module.Services.AddSingleton<IWindowsDeviceLocalUsersService, WindowsDeviceLocalUsersService>();
        module.Services.AddSingleton<IWindowsDeviceLocalGroupsService, WindowsDeviceLocalGroupsService>();
        module.Services.AddKeyedSingleton<IMessageConsumer, LocalGroupCacheMessageConsumer>(nameof(LocalGroupCacheMessageConsumer));
        module.Services.AddKeyedSingleton<IMessageConsumer, LocalGroupClearCacheMessageConsumer>(nameof(LocalGroupClearCacheMessageConsumer));
        module.Services.AddKeyedSingleton<IMessageConsumer, InvalidateLoggedInUserCacheMessageConsumer>(nameof(InvalidateLoggedInUserCacheMessageConsumer));
        module.Services.AddKeyedSingleton<IMessageConsumer, InvalidateBitLockerCacheMessageConsumer>(nameof(InvalidateBitLockerCacheMessageConsumer));
        module.Services.AddSingleton<IWindowsDeviceLoggedInUserService, WindowsDeviceLoggedInUserService>();
        module.Services.AddSingleton<IWindowsDeviceUserProvider, WindowsDeviceUserProvider>();
        module.Services.AddSingleton<ITmpWindowsDeviceUserDataProvider, TmpWindowsDeviceUserDataProvider>();
        module.Services.AddSingleton<IWindowsDeviceLocalGroupProvider, WindowsDeviceLocalGroupProvider>();
        module.Services.AddSingleton<IWindowsDeviceConfigurationsService, WindowsDeviceConfigurationsService>();
        module.Services.AddSingleton<IHttpClientProvider, HttpClientProvider>();
        module.Services.AddSingleton<IWindowsDevicePeripheralService, WindowsDevicePeripheralService>();
        module.Services.AddSingleton<IWindowsDevicePeripheralDataProvider, WindowsDevicePeripheralDataProvider>();
        module.Services.AddSingleton<IPeripheralDataProvider, PeripheralDataProvider>();
        module.Services.AddSingleton<IPeripheralManufacturerProvider, PeripheralManufacturerProvider>();
        module.Services.AddSingleton<IWindowsDeviceBiosService, WindowsDeviceBiosService>();
        module.Services.AddSingleton<IWindowsDeviceBootPriorityService, WindowsDeviceBootPriorityService>();
        module.Services.AddSingleton<IWindowsDeviceBootPriorityDataProvider, WindowsDeviceBootPriorityDataProvider>();
        module.Services.AddSingleton<IDeviceHardwareService, DeviceHardwareService>();
        module.Services.AddSingleton<IDeviceHardwareProvider, DeviceHardwareDataProvider>();
        module.Services.AddSingleton<IWindowsDeviceGroupsService, WindowsDeviceGroupsService>();
        module.Services.AddSingleton<IDeviceGroupSyncRequestDataProvider, DeviceGroupSyncRequestDataProvider>();
        module.Services.AddSingleton<IWindowsDefenderService, WindowsDefenderService>();
        module.Services.AddSingleton<IWindowsDefenderDataProvider, WindowsDefenderDataProvider>();
        module.Services.AddSingleton<IDeviceGroupPeripheralDataProvider, DeviceGroupPeripheralDataProvider>();
        module.Services.AddSingleton<IDeviceGroupPeripheralService, DeviceGroupPeripheralService>();
    }
}

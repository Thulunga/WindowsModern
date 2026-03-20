using System;
using Microsoft.Extensions.DependencyInjection;
using Soti.HostModule;
using Soti.MobiControl.InstallerServices.Services;

[assembly: HostModule(typeof(Soti.MobiControl.WindowsModern.UpgradeHandler.HostApplicationModule))]

namespace Soti.MobiControl.WindowsModern.UpgradeHandler;

public class HostApplicationModule : IHostApplicationModule
{
    /// <inheritdoc/>
    public void ConfigureHost(IHostApplicationModuleContext module)
    {
        ArgumentNullException.ThrowIfNull(module);

        module.Services.AddKeyedSingleton<IUpgradeService, LocalUserUpgradeService>(nameof(LocalUserUpgradeService));
        module.Services.AddKeyedSingleton<IUpgradeService, LocalUserUserNameUpgradeService>(nameof(LocalUserUserNameUpgradeService));
    }
}

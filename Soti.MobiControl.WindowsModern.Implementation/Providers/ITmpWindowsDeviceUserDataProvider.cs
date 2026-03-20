using Soti.MobiControl.WindowsModern.Models;
using System.Collections.Generic;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers;

/// <summary>
/// Data provider interface for <see cref="TmpWindowsDeviceUserData"/> entity.
/// </summary>
internal interface ITmpWindowsDeviceUserDataProvider
{
    /// <summary>
    /// Bulk Delete operation.
    /// </summary>
    /// <param name="entities">Entities to process.</param>
    void Delete(IEnumerable<int> entities);

    /// <summary>
    /// Gets batch of temporary local users data.
    /// </summary>
    /// <param name="skip">Skip</param>
    /// <param name="take">Take</param>
    /// <returns> WindowsDeviceTmpLocalUserModel.</returns>
    IEnumerable<WindowsDeviceTmpLocalUserModel> GetTmpLocalUsersBatch(int skip, int take);
}

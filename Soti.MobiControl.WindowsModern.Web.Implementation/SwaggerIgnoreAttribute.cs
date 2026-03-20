using System;

namespace Soti.MobiControl.WindowsModern.Web.Implementation;

/// <summary>
/// Used to suppress controller swagger generation.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter)]
internal sealed class SwaggerIgnoreAttribute : Attribute
{
}

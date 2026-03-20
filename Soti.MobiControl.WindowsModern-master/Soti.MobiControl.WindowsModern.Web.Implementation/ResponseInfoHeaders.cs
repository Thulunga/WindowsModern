using System;
using Microsoft.AspNetCore.Mvc;
using Soti.MobiControl.WebApi.Foundation.InfoHeaders;

namespace Soti.MobiControl.WindowsModern.Web.Implementation;

internal static class ResponseInfoHeaders
{
    public static void SetInfoHeader(this ControllerBase controller, InfoHeaderType infoType, object value)
    {
        if (controller == null)
        {
            throw new ArgumentNullException(nameof(controller));
        }

        var headerKey = GetHeaderName(infoType);

        if (headerKey == null)
        {
            throw new NotSupportedException("Unknown header type");
        }

        controller.HttpContext.Items[headerKey] = value;
    }

    private static string GetHeaderName(InfoHeaderType infoType)
    {
        switch (infoType)
        {
            case InfoHeaderType.TotalItemsCount:
                return "x-total-count";
            default:
                return null;
        }
    }
}

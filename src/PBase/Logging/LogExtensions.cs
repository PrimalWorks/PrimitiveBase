using Microsoft.Extensions.Logging;
using PBase.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace PBase.Logging
{
    public static class LogExtensions
    {
        public static void BoilerPlate(this ILogger logger, string name, int width = 70)
        {
            var assembly = Assembly.GetCallingAssembly();
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            var fileVersion = $"     File Version          : {versionInfo.FileVersion}";
            var prodVersion = $"     Product Version       : {versionInfo.ProductVersion}";
            var compName = $"     Company Name          : {versionInfo.CompanyName}";
            var prodName = $"     Product Name          : {versionInfo.ProductName}";
            var copyright = $"     Copyright             : {versionInfo.LegalCopyright}";
            logger.LogInformation("*".PadRight(width, '*'));
            logger.LogInformation($"**{" ".PadCenter(width - 4)}**");
            logger.LogInformation($"**{name.PadCenter(width - 4)}**");
            logger.LogInformation($"**{" ".PadCenter(width - 4)}**");
            logger.LogInformation("*".PadRight(width, '*'));
            logger.LogInformation($"**{" ".PadCenter(width - 4)}**");
            logger.LogInformation($"**{compName.PadRight(width - 4)}**");
            logger.LogInformation($"**{prodName.PadRight(width - 4)}**");
            logger.LogInformation($"**{copyright.PadRight(width - 4)}**");
            logger.LogInformation($"**{fileVersion.PadRight(width - 4)}**");
            logger.LogInformation($"**{prodVersion.PadRight(width - 4)}**");
            logger.LogInformation($"**{" ".PadCenter(width - 4)}**");
            logger.LogInformation("*".PadRight(width, '*'));
        }
    }
}

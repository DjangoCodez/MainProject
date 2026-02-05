using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SoftOne.Soe.Business.Security
{
    public static class IDataProtectionBuilderExtensions
    {
        public static IDataProtectionBuilder PersistKeysToSysDb(this IDataProtectionBuilder builder, string connstring)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            builder.Services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(services =>
            {
                var loggerFactory = services.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
                return new ConfigureOptions<KeyManagementOptions>(options =>
                {
                    options.XmlRepository = new SysDbXmlRepository(connstring, loggerFactory.CreateLogger<SysDbXmlRepository>());
                });
            });

            return builder;
        }
    }
}
using System.IO;
using Microsoft.Extensions.Configuration;

namespace IdentityServer4SingleHost.Infrastructure.IdentityAndAccess.Configuration
{
    public static class IdentityServerConfigurationBuilder
    {
        public static IConfigurationRoot GetConfiguration()
        {
            string basePath = Path.Combine(Directory.GetCurrentDirectory(), @"../IdentityServer4SingleHost.Web");          
            
            // Build config
            return new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile($"appsettings.Development.json", optional: false)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4SingleHost.Infrastructure.IdentityAndAccess.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IdentityServer4SingleHost.Infrastructure.IdentityAndAccess.DesignFactories
{
    public class ConfigurationDbContextDesignFactory: IDesignTimeDbContextFactory<ConfigurationDbContext>
    {
        private readonly IConfiguration _configuration;
        public ConfigurationDbContextDesignFactory()
        {
            _configuration = IdentityServerConfigurationBuilder.GetConfiguration();;
        }
        public ConfigurationDbContext CreateDbContext(string[] args)
        {
            string migrationsAssembly = typeof(DatabaseContext).GetTypeInfo().Assembly.GetName().Name;

            var optionsBuilder =  new DbContextOptionsBuilder<ConfigurationDbContext>()
                .UseSqlServer(
                    _configuration["ConnectionString"],
                    x => x.MigrationsAssembly(migrationsAssembly));

            return new ConfigurationDbContext(optionsBuilder.Options, new ConfigurationStoreOptions());
        }
    }
}

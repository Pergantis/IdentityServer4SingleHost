using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4SingleHost.Infrastructure.IdentityAndAccess.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IdentityServer4SingleHost.Infrastructure.IdentityAndAccess.DesignFactories
{
    public class PersistedGrantDbContextDesignFactory : IDesignTimeDbContextFactory<PersistedGrantDbContext>
    {
        private readonly IConfiguration _configuration;

        public PersistedGrantDbContextDesignFactory()
        {
            _configuration = IdentityServerConfigurationBuilder.GetConfiguration();
        }

        public PersistedGrantDbContext CreateDbContext(string[] args)
        {
            string migrationsAssembly = typeof(DatabaseContext).GetTypeInfo().Assembly.GetName().Name;

            var optionsBuilder =  new DbContextOptionsBuilder<PersistedGrantDbContext>()
                .UseSqlServer(
                    _configuration["ConnectionString"],
                    x => x.MigrationsAssembly(migrationsAssembly));

            return new PersistedGrantDbContext(optionsBuilder.Options, new OperationalStoreOptions());
        }
    }
}

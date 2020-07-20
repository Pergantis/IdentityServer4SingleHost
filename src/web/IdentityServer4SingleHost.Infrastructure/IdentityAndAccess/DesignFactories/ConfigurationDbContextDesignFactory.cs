using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityServer4SingleHost.Infrastructure.IdentityAndAccess.DesignFactories
{
    public class ConfigurationDbContextDesignFactory: IDesignTimeDbContextFactory<ConfigurationDbContext>
    {
        public ConfigurationDbContext CreateDbContext(string[] args)
        {
            string migrationsAssembly = typeof(DatabaseContext).GetTypeInfo().Assembly.GetName().Name;

            var optionsBuilder =  new DbContextOptionsBuilder<ConfigurationDbContext>()
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=IdentityServer4SingleHostDatabase;Trusted_Connection=True;MultipleActiveResultSets=true",
                    x => x.MigrationsAssembly(migrationsAssembly));

            return new ConfigurationDbContext(optionsBuilder.Options, new ConfigurationStoreOptions());
        }
    }
}

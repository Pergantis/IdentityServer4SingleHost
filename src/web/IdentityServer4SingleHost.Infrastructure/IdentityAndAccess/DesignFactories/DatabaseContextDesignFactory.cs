using System.Reflection;
using IdentityServer4SingleHost.Infrastructure.IdentityAndAccess.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IdentityServer4SingleHost.Infrastructure.IdentityAndAccess.DesignFactories
{
    public class DatabaseContextDesignFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        // add-migration AddIdentityUsersFirstMigration -Project IdentityServer4SingleHost.Infrastructure -c DatabaseContext -o IdentityAndAccess/Migrations

        private readonly IConfiguration _configuration;

        public DatabaseContextDesignFactory()
        {
            _configuration = IdentityServerConfigurationBuilder.GetConfiguration();
        }

        public DatabaseContext CreateDbContext(string[] args)
        {
            var migrationsAssembly = typeof(DatabaseContext).GetTypeInfo().Assembly.GetName().Name;

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlServer(
                    _configuration["ConnectionString"],
                    x => x.MigrationsAssembly(migrationsAssembly));

            return new DatabaseContext(optionsBuilder.Options);
        }
    }

}

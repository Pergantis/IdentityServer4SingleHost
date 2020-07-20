using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityServer4SingleHost.Infrastructure.IdentityAndAccess.DesignFactories
{
    public class DatabaseContextDesignFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        // add-migration AddIdentityUsersFirstMigration -Project IdentityServer4SingleHost.Infrastructure -c DatabaseContext -o IdentityAndAccess/Migrations

        public DatabaseContext CreateDbContext(string[] args)
        {
            var migrationsAssembly = typeof(DatabaseContext).GetTypeInfo().Assembly.GetName().Name;

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlServer(
                    "Server=(localdb)\\MSSQLLocalDB;Database=IdentityServer4SingleHostDatabase;Trusted_Connection=True;MultipleActiveResultSets=true",
                    x => x.MigrationsAssembly(migrationsAssembly));

            return new DatabaseContext(optionsBuilder.Options);
        }
    }

}

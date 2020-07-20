using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityServer4SingleHost.Infrastructure.IdentityAndAccess.DesignFactories
{
    public class PersistedGrantDbContextDesignFactory : IDesignTimeDbContextFactory<PersistedGrantDbContext>
    {
        public PersistedGrantDbContext CreateDbContext(string[] args)
        {
            string migrationsAssembly = typeof(DatabaseContext).GetTypeInfo().Assembly.GetName().Name;

            var optionsBuilder =  new DbContextOptionsBuilder<PersistedGrantDbContext>()
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=IdentityServer4SingleHostDatabase;Trusted_Connection=True;MultipleActiveResultSets=true",
                    x => x.MigrationsAssembly(migrationsAssembly));

            return new PersistedGrantDbContext(optionsBuilder.Options, new OperationalStoreOptions());
        }
    }
}

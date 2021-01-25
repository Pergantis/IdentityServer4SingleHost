using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4SingleHost.Infrastructure.IdentityAndAccess.Configuration;

namespace IdentityServer4SingleHost.Infrastructure.IdentityAndAccess.DataSeed
{
    public static class ConfigurationDbContextSeed
    {

        public static async Task SeedAsync(ConfigurationDbContext context)
        {
            if (!context.ApiScopes.Any())
            {
                foreach(var scope in Config.GetScopes)
                {
                    await context.ApiScopes.AddAsync(scope.ToEntity());
                }

                await context.SaveChangesAsync();
            }
            
            if (!context.Clients.Any())
            {
                foreach (var client in Config.GetClients)
                {
                    await context.Clients.AddAsync(client.ToEntity());
                }
                
                await context.SaveChangesAsync();
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var resource in Config.GetResources)
                {
                    await context.IdentityResources.AddAsync(resource.ToEntity());
                }
                
                await context.SaveChangesAsync();
            }
            
            if (!context.ApiResources.Any())
            {
                foreach (var api in Config.GetApis)
                {
                    await context.ApiResources.AddAsync(api.ToEntity());
                }

                await context.SaveChangesAsync();
            }
        }
    }
}

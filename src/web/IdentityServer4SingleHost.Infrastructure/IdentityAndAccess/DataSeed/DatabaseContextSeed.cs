using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4SingleHost.Domain.IdentityAndAccess.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer4SingleHost.Infrastructure.IdentityAndAccess.DataSeed
{
    public static class DatabaseContextSeed
    {
        private static readonly IPasswordHasher<ApplicationUser> _passwordHasher = new PasswordHasher<ApplicationUser>();

        public static async Task SeedAsync(DatabaseContext context)
        {
            if (!context.Users.Any())
            {
                await context.Users.AddRangeAsync(GetDefaultUser());

                await context.SaveChangesAsync();
            }

        }

        private static IEnumerable<ApplicationUser> GetDefaultUser()
        {
            var user =
            new ApplicationUser()
            {
                Email = "admin@user.com",
                Id = Guid.NewGuid().ToString(),
                PhoneNumber = "1234567890",
                UserName = "Admin",
                NormalizedEmail = "ADMIN@USER.COM",
                NormalizedUserName = "ADMIN@USER.COM",
                SecurityStamp = Guid.NewGuid().ToString("D"),
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, "123456");

            //TODO ADD THE ADMIN CLAIM HERE WITH C# OR IN DB

            return new List<ApplicationUser>()
            {
                user
            };
        }

    }
}

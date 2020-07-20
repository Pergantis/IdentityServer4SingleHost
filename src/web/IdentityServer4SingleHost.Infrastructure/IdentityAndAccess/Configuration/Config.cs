using System.Collections.Generic;
using IdentityServer4.Models;

namespace IdentityServer4SingleHost.Infrastructure.IdentityAndAccess.Configuration
{
    public static class Config
    {
        public static IEnumerable<ApiResource> GetApis =>

            new List<ApiResource>
            {
                new ApiResource("api")
                {
                    ApiSecrets =
                    {
                        new Secret("@p!Sup3RS3cr3T".Sha256())
                    },
                    Scopes =
                    {

                        new Scope
                        {
                            Name = "api.mobile.user",
                            DisplayName = "Public data scope .",
                        },
                        new Scope()
                        {
                            Name = "api.admin.user",
                            DisplayName = "Admin data scope",
                            UserClaims = {"can.access.statistics", "can.access.revenues" }
                        }
                    },
                }
            };

        public static IEnumerable<IdentityResource> GetResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };


        public static IEnumerable<Client> GetClients =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets = {new Secret("secret".Sha256())},
                    AllowedScopes = {"openid", "api.mobile.user", "api.admin.user"}
                },
                new Client
                {
                    ClientId = "xamarin.native.hybrid.android",
                    ClientName = "Xamarin Native Client (Hybrid with PKCE) For Android",
                    ClientSecrets = {new Secret("aV3ry$tr0nGP@$$w0rD^@F0rAndR0id".Sha256())},
                    RedirectUris = { "app.native.android://callback" },
                    PostLogoutRedirectUris = { "app.native.android://callback" },
                    RequireConsent = false,
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    RequirePkce = true,
                    AllowedScopes = {"openid", "api.mobile.user", "api.admin.user"},
                    Properties =
                    {
                        {"open_email_app","app.native.android://open_email_app"}
                    },
                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.ReUse
                }

            };
    }
}

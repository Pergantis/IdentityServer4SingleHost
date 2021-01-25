using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.AccessTokenValidation;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4SingleHost.Domain.IdentityAndAccess.Models;
using IdentityServer4SingleHost.Infrastructure;
using IdentityServer4SingleHost.Infrastructure.Emails;
using IdentityServer4SingleHost.Infrastructure.IdentityAndAccess.DataSeed;
using IdentityServer4SingleHost.Web.Extensions;
using IdentityServer4SingleHost.Web.IdentityAndAccess.Certificate;
using IdentityServer4SingleHost.Web.IdentityAndAccess.Extensions;
using IdentityServer4SingleHost.Web.IdentityAndAccess.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IdentityServer4SingleHost.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }


        public void ConfigureServices(IServiceCollection services)
       {
           services
               .AddCustomDbContext(Configuration)
               .AddRouting(options =>
               {
                   options.LowercaseUrls = true;
                   options.LowercaseQueryStrings = false;
               })
               .AddCustomAuthenticationAndAuthorization(Configuration, Environment)
               .AddDependencyInjectionServices()
               .AddConfigurationOptions(Configuration)
               .AddCustomMvc();
       }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            InitializeDatabase(app);

            app.UseDeveloperExceptionPage();
            app.UseCookiePolicy();
            if (env.IsProduction())
            {
                app.UseHttpsRedirection();
            }
            app.UseStaticFiles();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }

        private void InitializeDatabase( IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();

            var persistedGrantDbContext = serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
            var configurationContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            var databaseContext = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();

            // Migrate
            persistedGrantDbContext.Database.Migrate();
            configurationContext.Database.Migrate();
            databaseContext.Database.Migrate();

            DatabaseContextSeed.SeedAsync(databaseContext).Wait();
            ConfigurationDbContextSeed.SeedAsync(configurationContext).Wait();
        }
    }

    public static class CustomServiceExtensionMethods
    {

        public static IServiceCollection AddCustomMvc(this IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddHttpContextAccessor();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddScoped<IUrlHelper>(x => {
                var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
                var factory = x.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });

            return services;
        }


        public static IServiceCollection AddCustomDbContext(this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration["ConnectionString"];
            var migrationsAssembly = typeof(DatabaseContext).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(migrationsAssembly);
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
                options.EnableSensitiveDataLogging(true); // Logging sensitive data ��� console ids ���.
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(config =>
            {
                config.User.RequireUniqueEmail = true;
                //config.SignIn.RequireConfirmedEmail = true;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireLowercase = false;
                config.Password.RequireUppercase = false;
                config.Password.RequireDigit = false;
            }).AddEntityFrameworkStores<DatabaseContext>().AddDefaultTokenProviders();

            return services;
        }

        public static IServiceCollection AddCustomAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            var connectionString = configuration["ConnectionString"];
            var certificateThumbprint = configuration["CertificateThumbprint"];

            var migrationsAssembly = typeof(DatabaseContext).GetTypeInfo().Assembly.GetName().Name;

            services.AddSameSiteCookiePolicy();

            services.AddIdentityServer(setup =>
            {
                setup.IssuerUri = null;
                setup.Authentication.CookieLifetime = TimeSpan.FromHours(16);
            })
            //.AddDeveloperSigningCredential()
            .AddSigningCredential(Certificate.Get())
            .AddAspNetIdentity<ApplicationUser>()
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = builder =>
                {
                    builder.UseSqlServer(connectionString,
                        sqlServerOptionsAction: sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(migrationsAssembly);
                            sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorNumbersToAdd: null);
                        });

                    //builder.EnableSensitiveDataLogging(); // Logging sensitive data in console ids etc
                };

            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = builder =>
                {
                    builder.UseSqlServer(connectionString,
                        sqlServerOptionsAction: sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(migrationsAssembly);
                            sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorNumbersToAdd: null);
                        });

                    //builder.EnableSensitiveDataLogging(); // Logging sensitive data in console ids etc
                };
            })
            .AddProfileService<ProfileService>();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddGoogle("Google", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = configuration["GoogleAuthentication:GoogleClientId"];
                    options.ClientSecret = configuration["GoogleAuthentication:GoogleClientSecret"];
                    options.ClaimActions.MapJsonKey("urn:google:image", "picture");
                })
                .AddFacebook(facebookOptions =>
                {
                    facebookOptions.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    facebookOptions.AppId = configuration["FacebookAuthentication:FacebookClientId"];
                    facebookOptions.AppSecret = configuration["FacebookAuthentication:FacebookClientSecret"];
                    facebookOptions.Events.OnCreatingTicket = (context) =>
                    {
                        var picture = $"https://graph.facebook.com/{context.Principal.FindFirstValue(ClaimTypes.NameIdentifier)}/picture?type=large";
                        context.Identity.AddClaim(new Claim("picture", picture));
                        return Task.CompletedTask;
                    };
                })
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = configuration["IdentityServerConfiguration:BearerAuthority"];
                    options.ApiName = "api";
                    options.ApiSecret = "@p!Sup3RS3cr3T";
                    options.SupportedTokens = SupportedTokens.Both;
                    options.RequireHttpsMetadata = true;
                    options.EnableCaching = true;
                    options.CacheDuration = TimeSpan.FromMinutes(10); // that's the default
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsAuthenticatedAdministrator", policy =>
                {
                    policy.RequireClaim("is.administrator", new[] {"true"});
                });

                options.AddPolicy("CanAccessSensitiveData", policyAdmin =>
                {
                    policyAdmin.RequireClaim("can.access.statistics", new [] {"true"});
                    policyAdmin.RequireClaim("can.access.revenues", new[] {"true"});
                    policyAdmin.RequireScope(new[] {"api.admin.user"});
                });

                options.AddPolicy("CanAccessPublicApiData", policyAdmin =>
                {
                    policyAdmin.RequireScope(new[] {"api.mobile.user", "api.admin.user"});
                });
            });

            return services;
        }

        public static IServiceCollection AddDependencyInjectionServices(this IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();

            services.AddTransient<ILoginService<ApplicationUser>, LoginService>();
            services.AddTransient<IRedirectService, RedirectService>();



            return services;
        }

        public static IServiceCollection AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            return services;
        }
    }
}

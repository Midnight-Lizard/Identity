using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MidnightLizard.Web.Identity.Data;
using MidnightLizard.Web.Identity.Models;
using MidnightLizard.Web.Identity.Services;
using MidnightLizard.Web.Identity.Configuration;
using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;
using MidnightLizard.Web.Identity.Security.Claims;
using Microsoft.Extensions.Logging;

namespace MidnightLizard.Web.Identity
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            var connectionString = Configuration.GetValue<string>("IDDB_CONNECTION") ?? "local";

            services.AddDbContext<ApplicationDbContext>(
                options => options.UseNpgsql(connectionString,
                    sql => sql.MigrationsAssembly(migrationsAssembly)));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddMvc();

            var cert = Certificate.Get(Configuration);

            // Adds IdentityServer
            var idSrv = services.AddIdentityServer()
                .AddInMemoryIdentityResources(Resources.GetIdentityResources())
                .AddInMemoryApiResources(Resources.GetApiResources())
                .AddInMemoryClients(Clients.Get(Configuration))
                .AddAspNetIdentity<ApplicationUser>()
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseNpgsql(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    // options.TokenCleanupInterval = 15; // interval in seconds. 15 seconds useful for debugging
                });
            if (cert == null)
            {
                idSrv.AddDeveloperSigningCredential();
            }
            else
            {
                idSrv.AddSigningCredential(cert);
            }

            services.AddTransient<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = Configuration.GetValue<string>("IDENTITY_GOOGLE_CLIENT_ID");
                    options.ClientSecret = Configuration.GetValue<string>("IDENTITY_GOOGLE_CLIENT_SECRET");
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                try
                {
                    app.ApplicationServices.GetRequiredService<ApplicationDbContext>().Database.Migrate();
                    app.ApplicationServices.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();
                    logger.LogError(ex, "An error occurred during Database.Migrate.");
                }
            }
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            //app.UseAuthentication();
            app.UseIdentityServer();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

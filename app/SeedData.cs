using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MidnightLizard.Web.Identity.Data;
using MidnightLizard.Web.Identity.Models;
using MidnightLizard.Web.Identity.Security.Claims;
using Newtonsoft.Json;

namespace MidnightLizard.Web.Identity
{
    public static class SeedData
    {
        public static async Task EnsureSeedData(IServiceProvider services, IConfiguration configuration)
        {
            services.GetRequiredService<ApplicationDbContext>().Database.Migrate();
            services.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
            await CreateUserRoles(services, configuration);
        }

        private static async Task CreateUserRoles(IServiceProvider services, IConfiguration configuration)
        {
            var norm = services.GetRequiredService<ILookupNormalizer>();
            var newOwnerEmails = JsonConvert.DeserializeObject<string[]>(
                        configuration.GetValue<string>("IDENTITY_OWNER_EMAILS_JSON_ARRAY"))
                        .Select(e => norm.Normalize(e));
            var RoleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            if (!await RoleManager.RoleExistsAsync(nameof(AppRole.Owner)))
            {
                await RoleManager.CreateAsync(new IdentityRole(nameof(AppRole.Owner)));
            }

            // removing users from the role if they are not in the list
            var currentOwners = await UserManager.GetUsersInRoleAsync(nameof(AppRole.Owner));
            foreach (var oldOwner in
                from oldOwner in currentOwners
                join newOwnerEmail in newOwnerEmails
                   on oldOwner.NormalizedEmail equals newOwnerEmail into right
                from newOwnerEmail in right.DefaultIfEmpty()
                where newOwnerEmail is null
                select oldOwner)
            {
                await UserManager.RemoveFromRoleAsync(oldOwner, nameof(AppRole.Owner));
            }

            // adding users from the list to the role if they are not in it yet
            foreach (var newOwnerEmail in
                from newOwnerEmail in newOwnerEmails
                join currentOwner in currentOwners
                    on newOwnerEmail equals currentOwner.NormalizedEmail into right
                from currentOwner in right.DefaultIfEmpty()
                where currentOwner is null
                select newOwnerEmail)
            {
                var newOwner = await UserManager.FindByEmailAsync(newOwnerEmail);
                if (newOwner != null && newOwner.EmailConfirmed)
                {
                    await UserManager.AddToRoleAsync(newOwner, nameof(AppRole.Owner));
                }
            }
        }
    }
}
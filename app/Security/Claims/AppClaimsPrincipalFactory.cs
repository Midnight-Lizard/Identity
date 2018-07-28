using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MidnightLizard.Web.Identity.Models;

namespace MidnightLizard.Web.Identity.Security.Claims
{

    public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        private readonly IConfiguration configuration;

        public AppClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            IConfiguration configuration) : base(userManager, roleManager, optionsAccessor)
        {
            this.configuration = configuration;
        }

        public async override Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);

            var identity = principal.Identities.First();

            var identityUri = new Uri(this.configuration.GetValue<string>("IDENTITY_URL") ?? "http://localhost:7002");
            identity.AddClaim(new Claim(JwtClaimTypes.Profile, new Uri(identityUri, "/Manage/Index").AbsoluteUri));

            var usernameClaim = identity.FindFirst(claim => claim.Type == Options.ClaimsIdentity.UserNameClaimType && claim.Value == user.UserName);
            if (usernameClaim != null)
            {
                identity.RemoveClaim(usernameClaim);
                identity.AddClaim(new Claim(JwtClaimTypes.PreferredUserName, user.DisplayName));
            }

            if (!identity.HasClaim(x => x.Type == JwtClaimTypes.Name))
            {
                identity.AddClaim(new Claim(JwtClaimTypes.Name, user.DisplayName));
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                identity.AddClaims(new[]
                {
                    new Claim(JwtClaimTypes.Email, user.Email),
                    new Claim(JwtClaimTypes.EmailVerified,
                        user.EmailConfirmed ? "true" : "false", ClaimValueTypes.Boolean)
                });
            }

            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                identity.AddClaims(new[]
                {
                    new Claim(JwtClaimTypes.PhoneNumber, user.PhoneNumber),
                    new Claim(JwtClaimTypes.PhoneNumberVerified,
                        user.PhoneNumberConfirmed ? "true" : "false", ClaimValueTypes.Boolean)
                });
            }

            return principal;
        }
    }
}
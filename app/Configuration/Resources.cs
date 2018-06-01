using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MidnightLizard.Web.Identity.Configuration
{
    public static class Resources
    {
        // scopes define the resources in your system
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };
        }

        // scopes define the API resources in your system
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource(Api.SchemesCommander, "MidnightLizard Color Schemes Commander Api"),
                new ApiResource(Api.SchemesQuerier, "MidnightLizard Color Schemes Querier Api")
            };
        }

        public static class Api
        {
            public static readonly string SchemesCommander = "schemes-commander";
            public static readonly string SchemesQuerier = "schemes-querier";
        }
    }
}

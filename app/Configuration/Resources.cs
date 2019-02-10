using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
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
        public static IEnumerable<ApiResource> GetApiResources(IConfiguration configuration)
        {
            var impressionsCommanderApiSecret = new Secret(configuration.GetValue<string>("IDENTITY_IMPRESSIONS_COMMANDER_API_SECRET").Sha256());
            var schemesCommanderApiSecret = new Secret(configuration.GetValue<string>("IDENTITY_SCHEMES_COMMANDER_API_SECRET").Sha256());
            var schemesQuerierApiSecret = new Secret(configuration.GetValue<string>("IDENTITY_SCHEMES_QUERIER_API_SECRET").Sha256());
            return new List<ApiResource>
            {
                new ApiResource(Api.ImpressionsCommander, "MidnightLizard Impressions Commander Api")
                {
                    ApiSecrets = { impressionsCommanderApiSecret }
                },
                new ApiResource(Api.SchemesCommander, "MidnightLizard Color Schemes Commander Api")
                {
                    ApiSecrets = { schemesCommanderApiSecret }
                },
                new ApiResource(Api.SchemesQuerier, "MidnightLizard Color Schemes Querier Api")
                {
                    ApiSecrets = { schemesQuerierApiSecret }
                }
            };
        }

        public static class Api
        {
            public static readonly string ImpressionsCommander = "impressions-commander";
            public static readonly string SchemesCommander = "schemes-commander";
            public static readonly string SchemesQuerier = "schemes-querier";
        }
    }
}

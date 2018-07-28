﻿using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MidnightLizard.Web.Identity.Configuration
{
    public class Clients
    {
        public static IEnumerable<Client> Get(IConfiguration configuration)
        {
            var portalUrl = configuration.GetValue<string>("PORTAL_URL");
            var portalClientSecret = new Secret(configuration.GetValue<string>("IDENTITY_PORTAL_CLIENT_SECRET").Sha256());
            var portalUri = new Uri(portalUrl ?? "http://localhost:7000");

            return new List<Client>
            {
                new Client
                {
                    ClientId = "client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "SchemesApi" }
                },

                // resource owner password grant client
                new Client
                {
                    ClientId = "ro.client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "SchemesApi" }
                },

                new Client
                {
                    ClientId = "portal-client",
                    ClientName = "Midnight Lizard Web Portal",
                    ClientUri = portalUrl,
                    LogoUri = "https://pbs.twimg.com/profile_images/806483891771076610/vX_54HlQ_400x400.jpg",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RequireClientSecret = false,
                    RequireConsent = false,
                    AccessTokenType = AccessTokenType.Reference,

                    RedirectUris = {
                         new Uri(portalUri, "silentsignedin").AbsoluteUri,
                         new Uri(portalUri, "signedin").AbsoluteUri,
                         new Uri(portalUri, "signin-oidc").AbsoluteUri
                    },
                    FrontChannelLogoutUri = new Uri(portalUri, "signout-oidc").AbsoluteUri,
                    PostLogoutRedirectUris = {
                         new Uri(portalUri, "signedout").AbsoluteUri,
                         new Uri(portalUri, "signout-callback-oidc").AbsoluteUri
                    },
                    AllowedCorsOrigins = { portalUrl },

                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        Resources.Api.SchemesCommander,
                        Resources.Api.SchemesQuerier
                    }
                },

                new Client
                {
                    ClientId = "portal-server",
                    ClientName = "Midnight Lizard Web Portal",
                    ClientUri = portalUrl,
                    LogoUri = "https://pbs.twimg.com/profile_images/806483891771076610/vX_54HlQ_400x400.jpg",

                    AllowedGrantTypes = GrantTypes.Hybrid,
                    AllowAccessTokensViaBrowser = false,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RequireClientSecret = true,
                    ClientSecrets = { portalClientSecret },
                    RequireConsent = false,
                    AccessTokenType = AccessTokenType.Reference,

                    AllowOfflineAccess = true,
                    AccessTokenLifetime = 3600,
                    AbsoluteRefreshTokenLifetime = 2592000,
                    SlidingRefreshTokenLifetime = 1296000,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    UpdateAccessTokenClaimsOnRefresh = true,

                    RedirectUris = {
                         new Uri(portalUri, "silentsignedin").AbsoluteUri,
                         new Uri(portalUri, "signedin").AbsoluteUri,
                         new Uri(portalUri, "signin-oidc").AbsoluteUri
                    },
                    FrontChannelLogoutUri = new Uri(portalUri, "signout-oidc").AbsoluteUri,
                    PostLogoutRedirectUris = {
                         new Uri(portalUri, "signedout").AbsoluteUri,
                         new Uri(portalUri, "signout-callback-oidc").AbsoluteUri
                    },
                    AllowedCorsOrigins = { portalUrl },

                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        Resources.Api.SchemesCommander,
                        Resources.Api.SchemesQuerier
                    }
                }
            };
        }
    }
}

using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace MidnightLizard.Web.Identity.Configuration
{
    public class Clients
    {
        public static IEnumerable<Client> Get(IConfiguration configuration)
        {
            var portalSystemAccessTokenLifetime = configuration.GetValue<double>("IDENTITY_PORTAL_SYSTEM_ACCESS_TOKEN_LIFETIME");
            var portalServerAccessTokenLifetime = configuration.GetValue<double>("IDENTITY_PORTAL_SERVER_ACCESS_TOKEN_LIFETIME");
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
                    AccessTokenLifetime = (int)portalServerAccessTokenLifetime,
                    AbsoluteRefreshTokenLifetime = (int)TimeSpan.FromDays(15).TotalSeconds,
                    SlidingRefreshTokenLifetime = (int)TimeSpan.FromDays(7).TotalSeconds,
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
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

                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        Resources.Api.SchemesCommander
                    }
                },

                new Client
                {
                    ClientId = "portal-system",
                    ClientName = "Midnight Lizard Web Portal system client",

                    // no interactive user, use the `Basic clientid:secret` for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    AccessTokenType = AccessTokenType.Reference,
                    AccessTokenLifetime = (int)portalSystemAccessTokenLifetime,

                    // secret for authentication
                    ClientSecrets = { portalClientSecret },
                    RequireClientSecret = true,

                    // scopes that client has access to
                    AllowedScopes = {
                        Resources.Api.SchemesQuerier
                    }
                },
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Helpers;
using Ajf.Nuget.Logging;
using AutoMapper;
using HansJuergenWeb.WebHJ;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using Serilog;

[assembly: OwinStartup(typeof(Startup))]
namespace HansJuergenWeb.WebHJ
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Log.Logger = StandardLoggerConfigurator
                .GetLoggerConfig().MinimumLevel
                .Debug()
                .CreateLogger();

            Log.Logger.Information("Starting...");
            Log.Logger.Information("Version is... " + ConfigurationManager.AppSettings["ReleaseNumber"]);

            //Mapper.Initialize(cfg =>
            //{
            //    cfg.CreateMap<Event, EventViewModel>()
            //        .ForMember(x => x.ViewModelMode, x => x.Ignore())
            //        .ForMember(x => x.CarEmail, x => x.Ignore())
            //        .ForMember(x => x.CarName, x => x.Ignore())
            //        .ForMember(x => x.CarPhone, x => x.Ignore());
            //});

            //Mapper.Configuration.AssertConfigurationIsValid();

            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            AntiForgeryConfig.UniqueClaimTypeIdentifier =
                JwtClaimTypes.Name;

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies",
                ExpireTimeSpan = new TimeSpan(0, 30, 0),
                SlidingExpiration = true
            });

            app.Use((context, next) =>
            {
                Debug.WriteLine("Hello world");
                return next.Invoke();
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = "hjhwebhybrid",
                Authority = ConfigurationManager.AppSettings["IdentityServerApplicationUrl"],
                RedirectUri = ConfigurationManager.AppSettings["UrlHansJuergenWeb"],
                SignInAsAuthenticationType = "Cookies",
                ResponseType = "code id_token token",
                Scope = "openid profile address gallerymanagement roles offline_access email",
                UseTokenLifetime = false,
                PostLogoutRedirectUri = ConfigurationManager.AppSettings["UrlHansJuergenWeb"],

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = async n =>
                    {
                        TokenHelper.DecodeAndWrite(n.ProtocolMessage.IdToken);
                        TokenHelper.DecodeAndWrite(n.ProtocolMessage.AccessToken);

                        var givenNameClaim = n.AuthenticationTicket
                            .Identity.FindFirst(JwtClaimTypes.GivenName);

                        var familyNameClaim = n.AuthenticationTicket
                            .Identity.FindFirst(JwtClaimTypes.FamilyName);

                        var subClaim = n.AuthenticationTicket
                            .Identity.FindFirst(JwtClaimTypes.Subject);

                        var roleClaim = n.AuthenticationTicket
                            .Identity.FindFirst(JwtClaimTypes.Role);

                        var emailClaim = n.AuthenticationTicket
                            .Identity.FindFirst(JwtClaimTypes.Email);

                        // create a new claims, issuer + sub as unique identifier
                        var nameClaim = new Claim(JwtClaimTypes.Name,
                            Constants.TripGalleryIssuerUri + subClaim.Value);

                        var newClaimsIdentity = new ClaimsIdentity(
                            n.AuthenticationTicket.Identity.AuthenticationType,
                            JwtClaimTypes.Name,
                            JwtClaimTypes.Role);

                        newClaimsIdentity.AddClaim(nameClaim);

                        if (givenNameClaim != null)
                            newClaimsIdentity.AddClaim(givenNameClaim);

                        if (familyNameClaim != null)
                            newClaimsIdentity.AddClaim(familyNameClaim);

                        if (roleClaim != null)
                            newClaimsIdentity.AddClaim(roleClaim);

                        if (emailClaim != null)
                            newClaimsIdentity.AddClaim(emailClaim);

                        // request a refresh token
                        var tokenClientForRefreshToken = new TokenClient(
                            ConfigurationManager.AppSettings["IdentityServerApplicationUrl"] + "/connect/token",
                            "hjhwebhybrid",
                            Constants.TripGalleryClientSecret);

                        var refreshResponse = await
                            tokenClientForRefreshToken.RequestAuthorizationCodeAsync(
                                n.ProtocolMessage.Code,
                                ConfigurationManager.AppSettings["UrlHansJuergenWeb"]);

                        var expirationDateAsRoundtripString
                            = DateTime.SpecifyKind(DateTime.UtcNow.AddSeconds(refreshResponse.ExpiresIn)
                                , DateTimeKind.Utc).ToString("o");


                        newClaimsIdentity.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
                        newClaimsIdentity.AddClaim(new Claim("refresh_token", refreshResponse.RefreshToken));
                        newClaimsIdentity.AddClaim(new Claim("access_token", refreshResponse.AccessToken));
                        newClaimsIdentity.AddClaim(new Claim("expires_at", expirationDateAsRoundtripString));
                        newClaimsIdentity.AddClaim(new Claim("sub", subClaim.Value));

                        // create a new authentication ticket, overwriting the old one.
                        n.AuthenticationTicket = new AuthenticationTicket(
                            newClaimsIdentity,
                            n.AuthenticationTicket.Properties);

                        //await UserService.CreateUserIfNotExisting(newClaimsIdentity);
                    },
                    RedirectToIdentityProvider = async n =>
                    {
                        await Task.FromResult(0);

                        // get id token to add as id token hint
                        if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
                        {
                            var identityTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");

                            if (identityTokenHint != null)
                                n.ProtocolMessage.IdTokenHint = identityTokenHint.Value;
                        }
                        //else if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.AuthenticationRequest)
                        //{
                        //    string existingAcrValues = null;
                        //    if (n.ProtocolMessage.Parameters.TryGetValue("acr_values", out existingAcrValues))
                        //    {     
                        //        // add "2fa" - acr_values are separated by a space
                        //        n.ProtocolMessage.Parameters["acr_values"] = existingAcrValues + " 2fa";
                        //    }

                        //    n.ProtocolMessage.Parameters["acr_values"] = "2fa";
                        //}
                    }
                }
            });
        }
    }
}
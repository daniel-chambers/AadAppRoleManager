using System;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AadAppRoleManager.Web.Modules;
using AadAppRoleManager.Web.Utilities;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.Owin;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

[assembly: OwinStartup(typeof(AadAppRoleManager.Web.Startup))]

namespace AadAppRoleManager.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var container = new IoC().BuildContainer();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            app.UseAutofacMiddleware(container);

            ConfigureAuth(app);
        }

        private void ConfigureAuth(IAppBuilder app)
        {
            var clientId = ConfigurationManager.AppSettings["ida:ClientId"];
            var appKey = ConfigurationManager.AppSettings["ida:AppKey"];
            var graphResourceId = "https://graph.windows.net";
            var authority = "https://login.windows.net/common/";

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = clientId,
                Authority = authority,
                TokenValidationParameters = new TokenValidationParameters
                {
                    // instead of using the default validation (validating against a single issuer value, as we do in line of business apps),
                    // we inject our own multitenant validation logic
                    ValidateIssuer = false,
                },
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthorizationCodeReceived = async context =>
                    {
                        var code = context.Code;

                        var credential = new ClientCredential(clientId, appKey);
                        var tenantId = context.AuthenticationTicket.Identity.FindFirst(AadClaimTypes.TenantId).Value;
                        var userObjectId = context.AuthenticationTicket.Identity.FindFirst(AadClaimTypes.ObjectId).Value;

                        var authContext = context.OwinContext.GetAutofacLifetimeScope().Resolve<AuthenticationContextFactory>()(tenantId, userObjectId);
                        //This causes the token to be acquired and stored in table storage via the token cache
                        await authContext.AcquireTokenByAuthorizationCodeAsync(code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, graphResourceId);
                    },
                    RedirectToIdentityProvider = context =>
                    {
                        // This ensures that the address used for sign in and sign out is picked up dynamically from the request
                        // this allows you to deploy your app (to Azure Web Sites, for example)without having to change settings
                        // Remember that the base URL of the address used here must be provisioned in Azure AD beforehand.
                        string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
                        context.ProtocolMessage.RedirectUri = appBaseUrl + "/";
                        context.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl;
                        return Task.FromResult(0);
                    },
                    AuthenticationFailed = context =>
                    {
                        context.OwinContext.Response.Redirect("/Home/Error");
                        context.HandleResponse();
                        return Task.FromResult(0);
                    }
                }
            });
        }
    }
}

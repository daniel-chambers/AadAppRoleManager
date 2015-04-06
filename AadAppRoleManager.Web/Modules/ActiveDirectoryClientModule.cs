using System;
using AadAppRoleManager.Web.Services;
using Autofac;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AadAppRoleManager.Web.Modules
{
    public delegate IActiveDirectoryClient ActiveDirectoryClientFactory(string tenantId, string userObjectId);
    public delegate TokenCache TokenCacheFactory(string tenantId, string userObjectId);
    public delegate AuthenticationContext AuthenticationContextFactory(string tenantId, string userObjectId);
    public delegate IAadAccessTokenRepository AadAccessTokenRepositoryFactory(string tenantId, string userObjectId);

    public class ActiveDirectoryClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<TableStorageTokenCache>().As<TokenCache>();

            builder.Register((c, p) =>
            {
                var tokenCacheFactory = c.Resolve<TokenCacheFactory>();
                var tenantId = p.Named<string>("tenantId");
                var userObjectId = p.Named<string>("userObjectId");

                return new AuthenticationContext(string.Format("https://login.microsoftonline.com/{0}", tenantId), tokenCacheFactory(tenantId, userObjectId));
            })
                .As<AuthenticationContext>();
           
            builder.Register((c, p) =>
            {
                var tenantId = p.Named<string>("tenantId");
                var userObjectId = p.Named<string>("userObjectId");

                return new ActiveDirectoryClient(
                    new Uri(AadAccessTokenRepository.GraphResourceId + "/" + tenantId),
                    c.Resolve<AadAccessTokenRepositoryFactory>()(tenantId, userObjectId).GetAccessTokenAsync);
            })
                .As<IActiveDirectoryClient>();

            builder.RegisterType<AadAccessTokenRepository>()
                .As<IAadAccessTokenRepository>();
        }
    }
}
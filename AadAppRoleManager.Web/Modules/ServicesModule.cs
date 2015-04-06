using AadAppRoleManager.Web.Services;
using Autofac;
using Microsoft.WindowsAzure.Storage;

namespace AadAppRoleManager.Web.Modules
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConfigurationSettings>()
                .As<IConfigurationSettings>()
                .SingleInstance();

            builder.Register(c => CloudStorageAccount.Parse(c.Resolve<IConfigurationSettings>().StorageAccountConnectionString))
                .SingleInstance();
        }
    }
}
using AadAppRoleManager.Web.Services;
using Autofac;
using Autofac.Integration.Mvc;

namespace AadAppRoleManager.Web.Modules
{
    public class IoC
    {
        public IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            ConfigureBuilder(builder);

            return builder.Build();
        }

        protected virtual void ConfigureBuilder(ContainerBuilder builder)
        {
            builder.RegisterControllers(typeof(IoC).Assembly);
            builder.RegisterAssemblyModules(typeof(IoC).Assembly);
        }

        protected virtual void RegisterStartables(ContainerBuilder builder)
        {
            builder.RegisterType<InitializationService>().As<IStartable>();
        }
    }
}
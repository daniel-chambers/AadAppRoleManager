using System.Configuration;

namespace AadAppRoleManager.Web.Services
{
    public class ConfigurationSettings : IConfigurationSettings
    {
        public string StorageAccountConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["StorageAccount"].ConnectionString; }
        }

        public string AadClientId
        {
            get { return ConfigurationManager.AppSettings["ida:ClientId"]; }
        }

        public string AadAppKey
        {
            get { return ConfigurationManager.AppSettings["ida:AppKey"]; }
        }
    }
}
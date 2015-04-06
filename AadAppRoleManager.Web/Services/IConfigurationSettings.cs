using System;

namespace AadAppRoleManager.Web.Services
{
    public interface IConfigurationSettings
    {
        string StorageAccountConnectionString { get; }
        string AadClientId { get; }
        string AadAppKey { get; }
        TimeZoneInfo LocalTimeZoneInfo { get; }
    }
}
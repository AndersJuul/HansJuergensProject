using System.Configuration;
using Ajf.Nuget.Logging;

namespace HansJuergenWeb.MessageHandlers
{
    public class AppSettings : ServiceSettingsFromConfigFile, IAppSettings
    {
    }
}
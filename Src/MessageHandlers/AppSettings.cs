using System.Configuration;
using Ajf.Nuget.Logging;

namespace HansJuergenWeb.MessageHandlers
{
    public class AppSettings : ServiceSettingsFromConfigFile, IAppSettings
    {
        public AppSettings()
        {
            CcAddress = ConfigurationManager.AppSettings["CcAddress"];
            SenderAddress = ConfigurationManager.AppSettings["SenderAddress"];
            Subject = ConfigurationManager.AppSettings["Subject"];
        }

        public string CcAddress { get; set; }

        public string SenderAddress { get; set; }

        public string Subject { get; set; }
    }
}
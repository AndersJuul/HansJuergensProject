using System.Configuration;
using Ajf.Nuget.Logging;

namespace HansJuergenWeb.WebHJ
{
    public class AppSettings:WebSettingsFromConfigFile,IAppSettings
    {
        public AppSettings()
        {
            ExchangeName = $"{Environment}.{SuiteName}.HjFileUploaded";
            UploadDir = ConfigurationManager.AppSettings["UploadDir"];
        }
        public string ExchangeName { get; set; }

        public string UploadDir { get; set; }
    }
}
using System.Configuration;
using Ajf.Nuget.Logging;

namespace HansJuergenWeb.MessageHandlers.Settings
{
    public class AppSettings : ServiceSettingsFromConfigFile, IAppSettings
    {
        public AppSettings()
        {
            CcAddress = ConfigurationManager.AppSettings["CcAddress"];
            SenderAddress = ConfigurationManager.AppSettings["SenderAddress"];
            SubjectConfirmation = ConfigurationManager.AppSettings["SubjectConfirmation"];
            SubjectResults = ConfigurationManager.AppSettings["SubjectResults"];
            UploadDir = ConfigurationManager.AppSettings["UploadDir"];
            FlowCytoConnection = ConfigurationManager.AppSettings["FlowCytoConnection"];
            PathToR = ConfigurationManager.AppSettings["PathToR"];
        }

        public string UploadDir { get; set; }

        public string FlowCytoConnection { get; set; }

        public string PathToR { get; set; }

        public string SubjectResults { get; set; }

        public string CcAddress { get; set; }

        public string SenderAddress { get; set; }

        public string SubjectConfirmation { get; set; }
    }
}
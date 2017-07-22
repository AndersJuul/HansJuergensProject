using Ajf.Nuget.Logging;

namespace WebHJ
{
    public class AppSettings:WebSettingsFromConfigFile
    {
        public AppSettings()
        {
            ExchangeName = $"{Environment}.{SuiteName}.HjFileUploaded";
        }
        public string ExchangeName { get; set; }
    }
}
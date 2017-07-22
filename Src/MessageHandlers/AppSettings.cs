using Ajf.Nuget.Logging;
using MessageHandlers;

namespace HansJuergenWeb.MessageHandlers
{
    public class AppSettings : ServiceSettingsFromConfigFile, IAppSettings
    {
        public AppSettings()
        {
            //QueueName = $"{Environment}.{SuiteName}.MailsToSend";
            //ExchangeName = $"{Environment}.{SuiteName}.MailsToSend";
        }

        //public string ExchangeName { get; set; }

        //public string QueueName { get; set; }
    }
}
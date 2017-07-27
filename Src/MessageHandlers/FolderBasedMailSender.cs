using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ajf.Nuget.Logging;
using Serilog;

namespace HansJuergenWeb.MessageHandlers
{
    public class FolderBasedMailSender : IFolderBasedMailSender
    {
        private readonly IAppSettings _appSettings;
        private readonly IMailSender _mailSender;

        public FolderBasedMailSender(IAppSettings appSettings, IMailSender mailSender)
        {
            _appSettings = appSettings;
            _mailSender = mailSender;
        }

        public async Task DoMailSending(string templateName, string messageEmail, string messageDataFolder, string subject, string body)
        {
            try
            {
                var attachments = new string[] { };// GetFolderContents(messageDataFolder).Where(x => x.ToLower().Contains("out"));


                //Log.Logger.Information($"Result of sending mail: {httpStatusCode}");
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Error sending email.");
                throw;
            }
        }

    }
}
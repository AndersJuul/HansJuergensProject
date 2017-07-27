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

        public async Task DoMailSending(string templateName, string messageEmail, string messageDataFolder,
            string subject)
        {
            try
            {
                var template = File.ReadAllLines(templateName);

                var attachments = GetFolderContents(messageDataFolder).Where(x => x.ToLower().Contains("out"));
                var folderContentsOverview = GetFolderContentsOverview(messageDataFolder)
                    .Select(current => "<li>" + current + "</li>")
                    .Aggregate((current, next) => current + next);

                var folderContents = "$$folder-contents$$";
                var body = template
                    .Select(x => x.Replace(folderContents, folderContentsOverview))
                    .Select(x => x.Replace("$$mail-sender$$", _appSettings.SenderAddress))
                    .Aggregate((current, next) => current + next);

                var httpStatusCode = _mailSender.SendMailAsync(
                        messageEmail,
                        _appSettings.CcAddress,
                        _appSettings.SenderAddress,
                        subject,
                        body,
                        attachments)
                    .Result;

                Log.Logger.Information($"Result of sending mail: {httpStatusCode}");
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Error sending email.");
                throw;
            }
        }

        private IEnumerable<string> GetFolderContentsOverview(string messageDataFolder)
        {
            var folderContents = GetFolderContents(messageDataFolder)
                .Select(x => x.Substring(messageDataFolder.Length));
            return folderContents;
        }

        private IEnumerable<string> GetFolderContents(string messageDataFolder)
        {
            return Directory.GetFiles(messageDataFolder);
        }
    }
}
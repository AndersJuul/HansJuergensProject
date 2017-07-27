using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HansJuergenWeb.MessageHandlers.Settings;
using Serilog;

namespace HansJuergenWeb.MessageHandlers.Services
{
    public class MailMessageService : IMailMessageService
    {
        const string FolderContents = "$$folder-contents$$";
        const string MailSender = "$$mail-sender$$";
        private const string AllergeneSubscriptions = "$$allergene-subscriptions$$";

        private readonly IAppSettings _appSettings;

        public MailMessageService(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public string GetTemplateBasedMailBody(string templatePath, string messageDataFolder, string searchPattern)
        {
            try
            {
                var dict = new Dictionary<string, string>();

                var template = File.ReadAllLines(templatePath);

                var folderContentsOverview = GetFolderContentsOverview(messageDataFolder, searchPattern)
                    .Select(current => "<li>" + current + "</li>")
                    .Aggregate((current, next) => current + next);
                dict.Add(FolderContents, folderContentsOverview);

                dict.Add(MailSender, _appSettings.SenderAddress);

                var body = template.ToList();
                foreach (var dictKey in dict.Keys)
                {
                    for (int i = 0; i < body.Count; i++)
                    {
                        body[i] = body[i].Replace(dictKey, dict[dictKey]);
                    }
                }
                
                return body.Aggregate((current, next) => current + next);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Error forming mail body.");
                throw;
            }
        }
        private IEnumerable<string> GetFolderContentsOverview(string messageDataFolder, string searchPattern)
        {
            var folderContents = Directory.GetFiles(messageDataFolder, searchPattern)
                .Select(x => x.Substring(messageDataFolder.Length));
            return folderContents;
        }

    }
}
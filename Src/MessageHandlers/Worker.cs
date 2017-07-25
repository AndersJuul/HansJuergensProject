using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ajf.Nuget.Logging;
using AutoMapper;
using EasyNetQ;
using HansJuergenWeb.Contracts;
using Serilog;

namespace HansJuergenWeb.MessageHandlers
{
    public class Worker
    {
        private readonly IAppSettings _appSettings;
        private IBus _bus;
        private IMailSender _mailSender;
        private IRadapter _radapter;

        public Worker(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public bool WorkDone { get; set; }

        public void Start()
        {
            try
            {
                _bus = RabbitHutch.CreateBus(_appSettings.EasyNetQConfig);
                _radapter = new Radapter();
                _mailSender = new MailSender();

                _bus.SubscribeAsync<FileUploadedEvent>("SendEmailConfirmingUpload", SendEmailConfirmingUpload);
                _bus.SubscribeAsync<FileReadyForProcessingEvent>("ProcessUploadedFileThroughR",ProcessUploadedFileThroughR);
                _bus.SubscribeAsync<FileProcessedEvent>("SendEmailWithResults", SendEmailWithResults);
                _bus.SubscribeAsync<FileProcessedEvent>("UpdateSubscriptionDatabase", UpdateSubscriptionDatabase);
                _bus.SubscribeAsync<FileProcessedEvent>("RemoveOldDataFolders", RemoveOldDataFolders);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "During Start", new object[0]);
                throw;
            }
        }

        private async Task RemoveOldDataFolders(FileProcessedEvent message)
        {
            Log.Logger.Information("Message received in RemoveOldDataFolders : {@message}", message);

            await Task.FromResult(0);
        }

        private async Task UpdateSubscriptionDatabase(FileProcessedEvent message)
        {
            Log.Logger.Information("Message received in UpdateSubscriptionDatabase FAKING : {@message}", message);

            await Task.FromResult(0);
        }

        private async Task SendEmailWithResults(FileProcessedEvent message)
        {
            try
            {
                Log.Logger.Information("Message received in SendEmailConfirmingUpload: {@message}", message);

                await DoMailSending("ResultsMailTemplate.html", message.Email, message.DataFolder, _appSettings.SubjectResults + " " + message.Id)
                    .ConfigureAwait(false);

                await _bus.PublishAsync(Mapper.Map<FileReadyForCleanupEvent>(message))
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e,"Error during results sending");
                throw;
            }
        }

        private async Task SendEmailConfirmingUpload(FileUploadedEvent message)
        {
            try
            {
                Log.Logger.Information("Message received in SendEmailConfirmingUpload: {@message}", message);

                await DoMailSending("ConfirmationMailTemplate.html", message.Email, message.DataFolder, _appSettings.SubjectConfirmation + " " +message.Id)
                    .ConfigureAwait(false);

                await _bus.PublishAsync(Mapper.Map<FileReadyForProcessingEvent>(message))
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e,"Error during email confirmation sending");
                throw;
            }
        }

        private async Task DoMailSending(string templateName, string messageEmail, string messageDataFolder, string subject)
        {
            try
            {
                var template = File.ReadAllLines(templateName);

                var attachments = GetFolderContents(messageDataFolder).Where(x => x.ToLower().Contains("out"));
                var folderContentsOverview = GetFolderContentsOverview(messageDataFolder)
                    .Select(current => "<li>" + current + "</li>")
                    .Aggregate((current, next) => current + next);

                var folderContents = "$$folder-contents$$";
                var body = template.Select(x => x.Replace(folderContents, folderContentsOverview))
                    .Aggregate((current, next) => current + next);

                await _mailSender.SendMailAsync(
                        messageEmail,
                        _appSettings.CcAddress,
                        _appSettings.SenderAddress,
                        subject,
                        body,
                        attachments)
                    .ConfigureAwait(false);
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

        private async Task ProcessUploadedFileThroughR(FileReadyForProcessingEvent message)
        {
            Log.Logger.Information("Message received ProcessUploadedFileThroughR : {@message}", message);

            _radapter.BatchProcess(@".\TheScript.R", message.Id, _appSettings.UploadDir);

            await _bus.PublishAsync(Mapper.Map<FileProcessedEvent>(message))
                .ConfigureAwait(false);
        }

        public void Stop()
        {
        }
    }
}
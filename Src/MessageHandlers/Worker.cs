using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ajf.Nuget.Logging;
using AutoMapper;
using EasyNetQ;
using HansJuergenWeb.Contracts;
using Serilog;
using Serilog.Context;

namespace HansJuergenWeb.MessageHandlers
{
    public class Worker
    {
        private readonly IAppSettings _appSettings;
        private IBus _bus;
        private IMailSender _mailSender;
        private IRadapter _radapter;
        private readonly ISubscriptionManager _subscriptionManager;

        public Worker(IAppSettings appSettings, ISubscriptionManager subscriptionManager)
        {
            _appSettings = appSettings;
            _subscriptionManager = subscriptionManager;
        }

        public bool WorkDone { get; set; }

        public void Start()
        {
            try
            {
                _bus = RabbitHutch.CreateBus(_appSettings.EasyNetQConfig);
                _radapter = new Radapter();
                _mailSender = new MailSender();

                SubscriptionDone = false;

                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += BackgroundWorker_DoWork;
                backgroundWorker.RunWorkerAsync();

            }
            catch (Exception ex)
            {
                Log.Error(ex, "During Start", new object[0]);
                throw;
            }
        }

        public bool SubscriptionDone { get; set; }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _bus.SubscribeAsync<FileUploadedEvent>("SendEmailConfirmingUpload", SendEmailConfirmingUpload);
            _bus.SubscribeAsync<FileReadyForProcessingEvent>("ProcessUploadedFileThroughR",
                ProcessUploadedFileThroughR);
            _bus.SubscribeAsync<FileProcessedEvent>("SendEmailWithResults", SendEmailWithResults);
            _bus.SubscribeAsync<FileProcessedEvent>("UpdateSubscriptionDatabase", UpdateSubscriptionDatabase);
            _bus.SubscribeAsync<FileProcessedEvent>("RemoveOldDataFolders", RemoveOldDataFolders);

            SubscriptionDone = true;
        }

        private async Task RemoveOldDataFolders(FileProcessedEvent message)
        {
            Log.Logger.Information("Message received in RemoveOldDataFolders : {@message}", message);

            while (!SubscriptionDone) ;

            var directories = Directory.GetDirectories(_appSettings.UploadDir);
            foreach (var directory in directories)
            {
                var creationTime = Directory.GetCreationTime(directory);
                var timeSpan = DateTime.Now.Subtract(creationTime);
                var daysToKeepDataFiles = double.Parse(ConfigurationManager.AppSettings["DaysToKeepDataFiles"],
                    CultureInfo.InvariantCulture);
                if (timeSpan > TimeSpan.FromDays(daysToKeepDataFiles))
                {
                    Log.Logger.Information($"Time to remove old folder: {directory} from {creationTime}");

                    var files = Directory.GetFiles(directory);
                    foreach (var file in files)
                        File.Delete(file);
                    Directory.Delete(directory);
                }
            }

            await Task.FromResult(0);
        }

        private async Task UpdateSubscriptionDatabase(FileProcessedEvent message)
        {
            try
            {
                Log.Logger.Information("Message received in UpdateSubscriptionDatabase FAKING : {@message}", message);

                while (!SubscriptionDone) ;

                if (string.IsNullOrEmpty(message.Email))
                {
                    Log.Logger.Information("Skipping subscription update as no email is supplied");
                    return;
                }
                if (string.IsNullOrEmpty(message.Allergene))
                {
                    Log.Logger.Information("Skipping subscription update as no allergene is supplied");
                    return;
                }

                await _subscriptionManager
                    .AddUploaderToAllergeneSubscriptionAsync(message.Email, message.Allergene)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task SendEmailWithResults(FileProcessedEvent message)
        {
            try
            {
                Log.Logger.Information("Message received in SendEmailWithResults: {@message}", message);

                while (!SubscriptionDone) ;

                await DoMailSending("ResultsMailTemplate.html", message.Email, message.DataFolder,
                        _appSettings.SubjectResults + " " + message.Id)
                    .ConfigureAwait(false);

                await _bus.PublishAsync(Mapper.Map<FileReadyForCleanupEvent>(message))
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Error during results sending");
                throw;
            }
        }

        private async Task SendEmailConfirmingUpload(FileUploadedEvent message)
        {
            try
            {
                Log.Logger.Information("Message received in SendEmailConfirmingUpload: {@message}", message);

                while (!SubscriptionDone) ;

                await DoMailSending("ConfirmationMailTemplate.html", message.Email, message.DataFolder,
                        _appSettings.SubjectConfirmation + " " + message.Id)
                    .ConfigureAwait(false);

                await _bus.PublishAsync(Mapper.Map<FileReadyForProcessingEvent>(message))
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Error during email confirmation sending");
                throw;
            }
        }

        private async Task DoMailSending(string templateName, string messageEmail, string messageDataFolder,
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

            while (!SubscriptionDone) ;

            _radapter.BatchProcess(@".\TheScript.R", message.Id, _appSettings.UploadDir);

            await _bus.PublishAsync(Mapper.Map<FileProcessedEvent>(message))
                .ConfigureAwait(false);
        }

        public void Stop()
        {
        }
    }
}
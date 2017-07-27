using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Threading;
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
        private readonly IRadapter _radapter;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IFolderBasedMailSender _folderBasedMailSender;
        private BackgroundWorker _backgroundWorkerCleaning;

        public Worker(IAppSettings appSettings, ISubscriptionManager subscriptionManager, IFolderBasedMailSender folderBasedMailSender, IRadapter radapter)
        {
            _appSettings = appSettings;
            _subscriptionManager = subscriptionManager;
            _folderBasedMailSender = folderBasedMailSender;
            _radapter = radapter;
        }

        public bool WorkDone { get; set; }

        public void Start()
        {
            try
            {
                _bus = RabbitHutch.CreateBus(_appSettings.EasyNetQConfig);

                SubscriptionDone = false;

                var backgroundWorkerSetup = new BackgroundWorker();
                backgroundWorkerSetup.DoWork += BackgroundWorker_DoWork;
                backgroundWorkerSetup.RunWorkerAsync();

                _backgroundWorkerCleaning = new BackgroundWorker
                {
                    WorkerSupportsCancellation = true
                };
                _backgroundWorkerCleaning.DoWork += BackgroundWorkerCleaning_DoWork;
                _backgroundWorkerCleaning.RunWorkerAsync();


            }
            catch (Exception ex)
            {
                Log.Error(ex, "During Start", new object[0]);
                throw;
            }
        }

        private void BackgroundWorkerCleaning_DoWork(object sender, DoWorkEventArgs e)
        {
            var backgroundWorker = (sender as BackgroundWorker);
            if (backgroundWorker == null) return;

            DateTime lastClean = DateTime.MinValue;

            while (!backgroundWorker.CancellationPending)
            {
                Thread.Sleep(100);
                if (!backgroundWorker.CancellationPending)
                {
                    if (DateTime.Now.Subtract(lastClean) > TimeSpan.FromMinutes(1))
                    {
                        var currentTime = DateTime.Now;
                        var daysToKeepDataFiles = double.Parse(ConfigurationManager.AppSettings["DaysToKeepDataFiles"],
                            CultureInfo.InvariantCulture);
                        var earliestToKeep = currentTime.Subtract(TimeSpan.FromDays(daysToKeepDataFiles));

                        var directories = Directory.GetDirectories(_appSettings.UploadDir);
                        foreach (var directory in directories)
                        {
                            var creationTime = Directory.GetCreationTime(directory);
                            
                            if (earliestToKeep>creationTime)
                            {
                                Log.Logger.Information($"Time to remove old folder: {directory} from {creationTime}");

                                var files = Directory.GetFiles(directory);
                                foreach (var file in files)
                                    File.Delete(file);
                                Directory.Delete(directory);
                                break;
                            }
                        }
                    }
                }
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

                lock (_subscriptionManager)
                {
                    _subscriptionManager
                        .AddUploaderToAllergeneSubscriptionAsync(message.Email, message.Allergene)
                        .Wait();
                }
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

                await _folderBasedMailSender.DoMailSending("ResultsMailTemplate.html", message.Email, message.DataFolder,
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

                await _folderBasedMailSender.DoMailSending("ConfirmationMailTemplate.html", message.Email, message.DataFolder,
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
            _backgroundWorkerCleaning?.CancelAsync();
        }
    }
}
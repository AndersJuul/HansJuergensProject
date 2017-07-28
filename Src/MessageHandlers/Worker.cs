using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Threading;
using HansJuergenWeb.Contracts;
using HansJuergenWeb.MessageHandlers.Adapters;
using HansJuergenWeb.MessageHandlers.MessageHandlers;
using HansJuergenWeb.MessageHandlers.Settings;
using Serilog;

namespace HansJuergenWeb.MessageHandlers
{
    public class Worker
    {
        private readonly IAppSettings _appSettings;
        private readonly IBusAdapter _bus;
        private readonly IHandleProcessUploadedFileThroughR _handleProcessUploadedFileThroughR;
        private readonly IHandleSendEmailConfirmingUpload _handleSendEmailConfirmingUpload;
        private readonly IHandleSendEmailWithResults _handleSendEmailWithResults;
        private readonly IHandleUpdateSubscriptionDatabase _handleUpdateSubscriptionDatabase;
        private BackgroundWorker _backgroundWorkerCleaning;

        public Worker(IBusAdapter bus, IAppSettings appSettings,
            IHandleSendEmailConfirmingUpload handleSendEmailConfirmingUpload,
            IHandleProcessUploadedFileThroughR handleProcessUploadedFileThroughR,
            IHandleSendEmailWithResults handleSendEmailWithResults,
            IHandleUpdateSubscriptionDatabase handleUpdateSubscriptionDatabase)
        {
            _bus = bus;
            _appSettings = appSettings;
            _handleSendEmailConfirmingUpload = handleSendEmailConfirmingUpload;
            _handleProcessUploadedFileThroughR = handleProcessUploadedFileThroughR;
            _handleSendEmailWithResults = handleSendEmailWithResults;
            _handleUpdateSubscriptionDatabase = handleUpdateSubscriptionDatabase;
        }

        public bool WorkDone { get; set; }

        public void Start()
        {
            try
            {
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
            var backgroundWorker = sender as BackgroundWorker;
            if (backgroundWorker == null) return;

            var lastClean = DateTime.MinValue;

            while (!backgroundWorker.CancellationPending)
            {
                Thread.Sleep(100);
                if (!backgroundWorker.CancellationPending)
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

                            if (earliestToKeep > creationTime)
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

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _bus.Bus.SubscribeAsync<FileUploadedEvent>("SendEmailConfirmingUpload",
                _handleSendEmailConfirmingUpload.Handle);
            _bus.Bus.SubscribeAsync<FileReadyForProcessingEvent>("ProcessUploadedFileThroughR",
                _handleProcessUploadedFileThroughR.Handle);
            _bus.Bus.SubscribeAsync<FileProcessedEvent>("SendEmailWithResults", _handleSendEmailWithResults.Handle);
            _bus.Bus.SubscribeAsync<FileProcessedEvent>("UpdateSubscriptionDatabase",
                _handleUpdateSubscriptionDatabase.Handle);

            Log.Logger.Information("Done starting consumers");
        }

        public void Stop()
        {
            _backgroundWorkerCleaning?.CancelAsync();
        }
    }
}
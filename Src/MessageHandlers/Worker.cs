﻿using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Threading;
using EasyNetQ;
using HansJuergenWeb.Contracts;
using HansJuergenWeb.MessageHandlers.MessageHandlers;
using HansJuergenWeb.MessageHandlers.Settings;
using Serilog;

namespace HansJuergenWeb.MessageHandlers
{
    public class Worker
    {
        private readonly IAppSettings _appSettings;
        private readonly IBus _bus;
        private readonly IHandleProcessUploadedFileThroughR _handleProcessUploadedFileThroughR;
        private readonly IHandleSendEmailConfirmingUpload _handleSendEmailConfirmingUpload;
        private readonly IHandleSendEmailWithResults _handleSendEmailWithResults;
        private readonly IHandleUpdateSubscriptionDatabase _handleUpdateSubscriptionDatabase;
        private BackgroundWorker _backgroundWorkerCleaning;

        public Worker(IBus bus, IAppSettings appSettings,
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
                Log.Logger.Information("A");

                var backgroundWorkerSetup = new BackgroundWorker();
                backgroundWorkerSetup.DoWork += BackgroundWorker_DoWork;
                //backgroundWorkerSetup.RunWorkerAsync();
                Log.Logger.Information("B");

                _backgroundWorkerCleaning = new BackgroundWorker
                {
                    WorkerSupportsCancellation = true
                };
                _backgroundWorkerCleaning.DoWork += BackgroundWorkerCleaning_DoWork;
                //_backgroundWorkerCleaning.RunWorkerAsync();
                Log.Logger.Information("C");

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
            Log.Logger.Information("D");
            _bus.SubscribeAsync<FileUploadedEvent>("SendEmailConfirmingUpload",
                _handleSendEmailConfirmingUpload.Handle);
            Log.Logger.Information("E");
            _bus.SubscribeAsync<FileReadyForProcessingEvent>("ProcessUploadedFileThroughR",
                _handleProcessUploadedFileThroughR.Handle);
            Log.Logger.Information("F");
            _bus.SubscribeAsync<FileProcessedEvent>("SendEmailWithResults", _handleSendEmailWithResults.Handle);
            Log.Logger.Information("G");
            _bus.SubscribeAsync<FileProcessedEvent>("UpdateSubscriptionDatabase",
                _handleUpdateSubscriptionDatabase.Handle);
            Log.Logger.Information("H");
        }

        public void Stop()
        {
            _backgroundWorkerCleaning?.CancelAsync();
        }
    }
}
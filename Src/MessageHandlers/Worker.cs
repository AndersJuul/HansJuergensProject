using System;
using System.Threading.Tasks;
using EasyNetQ;
using HansJuergenWeb.Contracts;
using Serilog;

namespace HansJuergenWeb.MessageHandlers
{
    public class Worker
    {
        private readonly IAppSettings _appSettings;
        private readonly IRadapter _radapter;
        private IBus _bus;

        public Worker(IAppSettings appSettings, IRadapter radapter)
        {
            _appSettings = appSettings;
            _radapter = radapter;
        }

        public bool WorkDone { get; set; }

        public void Start()
        {
            try
            {
                SetupSubscriptions();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "During Start", new object[0]);
                throw;
            }
        }

        private void SetupSubscriptions()
        {
            _bus = RabbitHutch.CreateBus(_appSettings.EasyNetQConfig);

            _bus.SubscribeAsync<FileUploadedEvent>("SendEmailConfirmingUpload",
                message => Task.Factory.StartNew(SendEmailConfirmingUpload)
                    .ContinueWith(DefaultErrorHandling()));

            _bus.SubscribeAsync<FileUploadedEvent>("ProcessUploadedFileThroughR",
                message => Task.Factory.StartNew(() => ProcessUploadedFileThroughR(message))
                    .ContinueWith(DefaultErrorHandling()));
        }

        private void ProcessUploadedFileThroughR(FileUploadedEvent message)
        {
            Log.Logger.Information("Message received <Faking process through R>");

            _radapter.BatchProcess(@".\TheScript.R",message.Id);

            _bus.PublishAsync(new FileProcessedEvent
            {
                Description = message.Description,
                Email = message.Email,
                FileName = message.FileName
            });
        }

        private static Action<Task> DefaultErrorHandling()
        {
            return task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    //Log.Logger.Information("Everything worked out ok");
                }
                else
                {
                    // Dont catch this, it is caught further up the heirarchy and results in being sent to the default error queue
                    // on the broker
                    Log.Logger.Information("Message exception");

                    throw new EasyNetQException(
                        "Message processing exception - look in the default error queue (broker)");
                }
            };
        }

        private void SendEmailConfirmingUpload()
        {
            Log.Logger.Information("Message received <Faking send confirmation>");
        }

        public void Stop()
        {
        }
    }
}
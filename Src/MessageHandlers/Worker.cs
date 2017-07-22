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
        private IBus _bus;

        public Worker(IAppSettings appSettings)
        {
            _appSettings = appSettings;
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

            _bus.SubscribeAsync<FileUploadedEvent>("Queue_Identifier",
                message => Task.Factory.StartNew(() => { Log.Logger.Information("Message received"); })
                .ContinueWith(
                    task =>
                    {
                        if (task.IsCompleted && !task.IsFaulted)
                        {
                            // Everything worked out ok
                            Log.Logger.Information("Everything worked out ok");
                        }
                        else
                        {
                            // Dont catch this, it is caught further up the heirarchy and results in being sent to the default error queue
                            // on the broker
                            Log.Logger.Information("Message exception");

                            throw new EasyNetQException(
                                "Message processing exception - look in the default error queue (broker)");
                        }
                    }));
        }

        public void Stop()
        {
        }
    }
}
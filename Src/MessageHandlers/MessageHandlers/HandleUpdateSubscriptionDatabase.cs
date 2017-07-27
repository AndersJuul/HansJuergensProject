using System;
using System.Threading.Tasks;
using EasyNetQ;
using HansJuergenWeb.Contracts;
using HansJuergenWeb.MessageHandlers.Services;
using Serilog;

namespace HansJuergenWeb.MessageHandlers.MessageHandlers
{
    public class HandleUpdateSubscriptionDatabase : IHandleUpdateSubscriptionDatabase
    {
        private readonly ISubscriptionService _subscriptionService;

        public HandleUpdateSubscriptionDatabase(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }
        public async Task Handle(FileProcessedEvent message)
        {
            try
            {
                Log.Logger.Information("Message received in UpdateSubscriptionDatabase FAKING : {@message}", message);

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

                lock (_subscriptionService)
                {
                    _subscriptionService
                        .AddUploaderToAllergeneSubscriptionAsync(message.Email, message.Allergene)
                        .Wait();
                }
                await Task.FromResult(0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
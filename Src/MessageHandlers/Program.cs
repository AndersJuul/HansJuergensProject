using Ajf.Nuget.Logging;
using AutoMapper;
using HansJuergenWeb.Contracts;
using HansJuergenWeb.MessageHandlers.Adapters;
using HansJuergenWeb.MessageHandlers.MessageHandlers;
using HansJuergenWeb.MessageHandlers.Repositories;
using HansJuergenWeb.MessageHandlers.Services;
using HansJuergenWeb.MessageHandlers.Settings;
using Serilog;

namespace HansJuergenWeb.MessageHandlers
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Log.Logger = StandardLoggerConfigurator.GetEnrichedLogger();

            using (var wrapper = new TopshelfWrapper<Worker>(
                () =>
                {
                    Mapper.Initialize(cfg =>
                    {
                        cfg.CreateMap<FileProcessedEvent, FileReadyForCleanupEvent>();
                        cfg.CreateMap<FileUploadedEvent, FileReadyForProcessingEvent>();
                        cfg.CreateMap<FileReadyForProcessingEvent, FileProcessedEvent>();
                    });
                },
                s =>
                {
                    var appSettings = new AppSettings();
                    var repository = new Repository(appSettings);
                    var subscriptionService = new SubscriptionService(repository);
                    var mailSender = new MailSender();
                    var radapter = new Radapter(appSettings);
                    IBusAdapter bus = new BusAdapter(appSettings);
                    var mailMessageService = new MailMessageService(appSettings, subscriptionService);

                    s.ConstructUsing(name => new Worker(bus, appSettings,
                        new HandleSendEmailConfirmingUpload(bus, mailMessageService, mailSender, appSettings),
                        new HandleProcessUploadedFileThroughR(bus, appSettings, radapter),
                        new HandleSendEmailWithResults(bus, mailMessageService, mailSender, appSettings),
                        new HandleUpdateSubscriptionDatabase(subscriptionService))); 

                }))
            {
                wrapper.Run();
            }
        }
    }
}
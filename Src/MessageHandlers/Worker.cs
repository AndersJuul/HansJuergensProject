using System;
using System.Reflection;
using System.Threading.Tasks;
using Ajf.Nuget.Logging;
using EasyNetQ;
using EasyNetQ.AutoSubscribe;
using HansJuergenWeb.Contracts;
using Serilog;
using StructureMap;

namespace HansJuergenWeb.MessageHandlers
{
    public class Worker
    {
        private readonly IAppSettings _appSettings;
        private IBus _bus;
        private IRadapter _radapter;
        private  IMailSender _mailSender;

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
                _mailSender=new MailSender();

                _bus.SubscribeAsync<FileUploadedEvent>("ProcessUploadedFileThroughR", ProcessUploadedFileThroughR);
                _bus.SubscribeAsync<FileUploadedEvent>("SendEmailConfirmingUpload", SendEmailConfirmingUpload);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "During Start", new object[0]);
                throw;
            }
        }

        private Task SendEmailConfirmingUpload(FileUploadedEvent message)
        {
            Log.Logger.Information("Message received in SendEmailConfirmingUpload: {@fileUploadedEvent}", message);

            var ajf = "andersjuulsfirma@gmail.com";
            return _mailSender.SendMailAsync(ajf, ajf, ajf, "Notification: Files uploaded", "");
        }

        private Task ProcessUploadedFileThroughR(FileUploadedEvent message)
        {
            Log.Logger.Information("Message received ProcessUploadedFileThroughR");

            _radapter.BatchProcess(@".\TheScript.R", message.Id);

            _bus.PublishAsync(new FileProcessedEvent
            {
                Description = message.Description,
                Email = message.Email,
                FileNames = message.FileNames,
                DataFolder = message.DataFolder
            });
            return Task.FromResult("");
        }

        public void Stop()
        {
        }
    }
}
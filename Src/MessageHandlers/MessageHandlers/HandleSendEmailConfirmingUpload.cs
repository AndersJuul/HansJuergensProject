using System;
using System.Threading.Tasks;
using Ajf.Nuget.Logging;
using AutoMapper;
using EasyNetQ;
using HansJuergenWeb.Contracts;
using HansJuergenWeb.MessageHandlers.Services;
using HansJuergenWeb.MessageHandlers.Settings;
using Serilog;

namespace HansJuergenWeb.MessageHandlers.MessageHandlers
{
    public class HandleSendEmailConfirmingUpload : IHandleSendEmailConfirmingUpload
    {
        private readonly IBus _bus;
        private readonly IMailMessageService _mailMessageService;
        private readonly IMailSender _mailSender;
        private readonly IAppSettings _appSettings;

        public HandleSendEmailConfirmingUpload(IBus bus, IMailMessageService mailMessageService, IMailSender mailSender, IAppSettings appSettings)
        {
            _bus = bus;
            _mailMessageService = mailMessageService;
            _mailSender = mailSender;
            _appSettings = appSettings;
        }

        public async Task Handle(FileUploadedEvent message)
        {
            try
            {
                Log.Logger.Information("Message received in SendEmailConfirmingUpload: {@message}", message);

                var body = _mailMessageService.GetTemplateBasedMailBody("ConfirmationMailTemplate.html",
                    message.DataFolder, "*.*", message.Email);
                var httpStatusCode = _mailSender.SendMailAsync(
                        message.Email,
                        _appSettings.CcAddress,
                        _appSettings.SenderAddress,
                        _appSettings.SubjectConfirmation + " " + message.Id,
                        body,
                        new string[] { })
                    .Result;

                await _bus.PublishAsync(Mapper.Map<FileReadyForProcessingEvent>(message))
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Error during email confirmation sending");
                throw;
            }
        }
    }
}
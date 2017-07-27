﻿using System;
using System.IO;
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
    public class HandleSendEmailWithResults : IHandleSendEmailWithResults
    {
        private readonly IBus _bus;
        private readonly IMailMessageService _mailMessageService;
        private readonly IMailSender _mailSender;
        private readonly IAppSettings _appSettings;

        public HandleSendEmailWithResults(IBus bus, IMailMessageService mailMessageService, IMailSender mailSender,IAppSettings appSettings)
        {
            _bus = bus;
            _mailMessageService = mailMessageService;
            _mailSender = mailSender;
            _appSettings = appSettings;
        }
        public async Task Handle(FileProcessedEvent message)
        {
            try
            {
                Log.Logger.Information("Message received in SendEmailWithResults: {@message}", message);

                var body = _mailMessageService.GetTemplateBasedMailBody("ResultsMailTemplate.html", message.DataFolder,
                    "*.*", message.Email);
                var attachmentPaths = Directory.GetFiles(message.DataFolder, "*.Rout");
                var httpStatusCode = _mailSender.SendMailAsync(
                        message.Email,
                        _appSettings.CcAddress,
                        _appSettings.SenderAddress,
                        _appSettings.SubjectResults + " " + message.Id,
                        body,
                        attachmentPaths)
                    .Result;

                await _bus.PublishAsync(Mapper.Map<FileReadyForCleanupEvent>(message))
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Error during results sending");
                throw;
            }
        }
    }
}
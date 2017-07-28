using System;
using System.Threading.Tasks;
using AutoMapper;
using HansJuergenWeb.Contracts;
using HansJuergenWeb.MessageHandlers.Adapters;
using HansJuergenWeb.MessageHandlers.Settings;
using Serilog;

namespace HansJuergenWeb.MessageHandlers.MessageHandlers
{
    public class HandleProcessUploadedFileThroughR : IHandleProcessUploadedFileThroughR
    {
        private readonly IAppSettings _appSettings;
        private readonly IBusAdapter _bus;
        private readonly IRadapter _radapter;

        public HandleProcessUploadedFileThroughR(IBusAdapter bus, IAppSettings appSettings, IRadapter radapter)
        {
            _radapter = radapter;
            _appSettings = appSettings;
            _bus = bus;
        }

        public async Task Handle(FileReadyForProcessingEvent message)
        {
            try
            {
                Log.Logger.Information("Message received ProcessUploadedFileThroughR : {@message}", message);

                _radapter.BatchProcess(@".\TheScript.R", message.Id, _appSettings.UploadDir);

                await _bus.Bus.PublishAsync(Mapper.Map<FileProcessedEvent>(message))
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Exception during R processing");
                throw;
            }
        }
    }
}
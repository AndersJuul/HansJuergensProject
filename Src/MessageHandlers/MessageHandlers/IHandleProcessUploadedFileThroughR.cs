using System.Threading.Tasks;
using HansJuergenWeb.Contracts;

namespace HansJuergenWeb.MessageHandlers.MessageHandlers
{
    public interface IHandleProcessUploadedFileThroughR
    {
        Task Handle(FileReadyForProcessingEvent message);
    }
}
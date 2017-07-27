using System.Threading.Tasks;
using HansJuergenWeb.Contracts;

namespace HansJuergenWeb.MessageHandlers.MessageHandlers
{
    public interface IHandleSendEmailWithResults
    {
        Task Handle(FileProcessedEvent message);
    }
}
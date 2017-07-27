using System.Threading.Tasks;
using HansJuergenWeb.Contracts;

namespace HansJuergenWeb.MessageHandlers.MessageHandlers
{
    public interface IHandleUpdateSubscriptionDatabase
    {
        Task Handle(FileProcessedEvent message);
    }
}
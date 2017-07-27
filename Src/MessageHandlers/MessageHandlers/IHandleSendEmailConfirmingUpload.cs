using System.Threading.Tasks;
using HansJuergenWeb.Contracts;

namespace HansJuergenWeb.MessageHandlers.MessageHandlers
{
    public interface IHandleSendEmailConfirmingUpload
    {
        Task Handle(FileUploadedEvent message);
    }
}
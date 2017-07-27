using System.Threading.Tasks;

namespace HansJuergenWeb.MessageHandlers
{
    public interface IFolderBasedMailSender
    {
        Task DoMailSending(string templateName, string messageEmail, string messageDataFolder, string subject, string body);
    }
}
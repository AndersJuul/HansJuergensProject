using System.Threading.Tasks;

namespace HansJuergenWeb.MessageHandlers
{
    public interface ISubscriptionManager
    {
        Task AddUploaderToAllergeneSubscriptionAsync(string email, string allergene);
    }
}
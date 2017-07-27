using System.Threading.Tasks;

namespace HansJuergenWeb.MessageHandlers.Services
{
    public interface ISubscriptionService
    {
        Task AddUploaderToAllergeneSubscriptionAsync(string email, string allergene);
    }
}
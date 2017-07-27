using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using HansJuergenWeb.MessageHandlers.Models;

namespace HansJuergenWeb.MessageHandlers.Services
{
    public interface ISubscriptionService
    {
        Task AddUploaderToAllergeneSubscriptionAsync(string email, string allergene);
        IEnumerable<Subscription> GetSubscriptions(string messageEmail);
    }
}
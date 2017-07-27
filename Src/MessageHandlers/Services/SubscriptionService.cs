using System.Linq;
using System.Threading.Tasks;
using HansJuergenWeb.MessageHandlers.Repositories;
using Serilog;

namespace HansJuergenWeb.MessageHandlers.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IRepository _repository;

        public SubscriptionService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task AddUploaderToAllergeneSubscriptionAsync(string email, string allergene)
        {
            var allergeneIds = await _repository
                .GetAllergeneIdsByNameAsync(allergene)
                .ConfigureAwait(false);

            // Lookup allergene
            var allergeneId = allergeneIds.SingleOrDefault();
            if (allergeneId == 0)
            {
                Log.Logger.Information($"Didn't find Allergene Id for {allergene}. Aborting.");
                return;
            }
            Log.Logger.Information("Found Allergene Id to be {allergeneid}", allergeneId);

            // Lookup uploader
            var uploaderIds = await _repository
                .GetUploaderIdsByEmailAsync(email)
                .ConfigureAwait(false);
            var uploaderId = uploaderIds.SingleOrDefault();

            if (uploaderId == 0)
                uploaderId = await _repository
                    .CreateUploaderAsync(email)
                    .ConfigureAwait(false);

            var subscriptionIds = await _repository
                .GetAllergeneSubscriptionIdsAsync(allergeneId, uploaderId)
                .ConfigureAwait(false);
            var subscriptionId = subscriptionIds.SingleOrDefault();

            if (subscriptionId > 0)
            {
                Log.Logger.Information(
                    $"Subscription already exists for email={email}, allergene={allergene}. Aborting.");
                return;
            }

            subscriptionId = await _repository
                .CreateAllergeneSubscriptionAsync(allergeneId, uploaderId)
                .ConfigureAwait(false);
            Log.Logger.Information($"Subscription created for email={email}, allergene={allergene}: {subscriptionId}");
        }
    }
}
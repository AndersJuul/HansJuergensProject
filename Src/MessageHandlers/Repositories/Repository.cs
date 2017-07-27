using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using HansJuergenWeb.MessageHandlers.Models;
using HansJuergenWeb.MessageHandlers.Settings;

namespace HansJuergenWeb.MessageHandlers.Repositories
{
    public class Repository : IRepository
    {
        private readonly SqlConnection _sqlConnection;

        public Repository(IAppSettings appSettings)
        {
            _sqlConnection = new SqlConnection(appSettings.FlowCytoConnection);
        }

        public async Task<IEnumerable<int>> GetAllergeneIdsByNameAsync(string allergene)
        {
            var allergeneid = await _sqlConnection
                .QueryAsync<int>($"select Id from Allergenes where Name='{allergene}'")
                .ConfigureAwait(false);

            return allergeneid;
        }

        public async Task<IEnumerable<int>> GetUploaderIdsByEmailAsync(string email)
        {
            var allergeneids = await _sqlConnection
                .QueryAsync<int>($"select Id from Uploaders where Email='{email}'")
                .ConfigureAwait(false);

            return allergeneids;
        }

        public async Task<int> CreateUploaderAsync(string email)
        {
            var uploaderIds = await _sqlConnection
                .QueryAsync<int>($"Insert into Uploaders output Inserted.Id values('{email}')")
                .ConfigureAwait(false);

            return uploaderIds.Single();
        }

        public async Task<IEnumerable<int>> GetAllergeneSubscriptionIdsAsync(int allergeneId, int uploaderId)
        {
            var ids = await _sqlConnection
                .QueryAsync<int>(
                    $"select Id from AllergeneSubscriptions where Allergene_id={allergeneId} and Uploader_id={uploaderId}")
                .ConfigureAwait(false);

            return ids;
        }

        public async Task<int> CreateAllergeneSubscriptionAsync(int allergeneId, int uploaderId)
        {
            var allergeneSubscriptionIds = await _sqlConnection
                .QueryAsync<int>($"Insert into AllergeneSubscriptions output Inserted.Id values({allergeneId},{uploaderId})")
                .ConfigureAwait(false);

            return allergeneSubscriptionIds.Single();
        }

        public IEnumerable<Subscription> GetAllergeneSubscriptions(string messageEmail)
        {
            return _sqlConnection
                .Query<Subscription>("SELECT alsu.[Id] as Id,alsu.[Allergene_Id] as AllergeneId, alsu.[Uploader_Id] as UploaderId, up.Email, al.Name FROM [AllergeneSubscriptions] alsu inner join [Uploaders] up on alsu.Uploader_Id = up.Id inner join[Allergenes] al on alsu.Allergene_Id = al.Id ");
        }
    }
}
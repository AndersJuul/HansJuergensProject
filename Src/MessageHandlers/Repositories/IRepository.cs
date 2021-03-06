﻿using System.Collections.Generic;
using System.Threading.Tasks;
using HansJuergenWeb.MessageHandlers.Models;

namespace HansJuergenWeb.MessageHandlers.Repositories
{
    public interface IRepository
    {
        Task<IEnumerable<int>> GetAllergeneIdsByNameAsync(string allergene);
        Task<IEnumerable<int>> GetUploaderIdsByEmailAsync(string email);
        Task<int> CreateUploaderAsync(string email);
        Task<IEnumerable<int>> GetAllergeneSubscriptionIdsAsync(int allergeneId, int uploaderId);
        Task<int> CreateAllergeneSubscriptionAsync(int allergeneId, int uploaderId);
        IEnumerable<Subscription> GetAllergeneSubscriptions(string messageEmail);
    }
}
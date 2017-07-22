using System;
using EasyNetQ;

namespace HansJuergenWeb.MessageHandlers
{
    public interface IAppSettings
    {
        string EasyNetQConfig { get; set; }
    }
}
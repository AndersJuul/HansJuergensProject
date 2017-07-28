using EasyNetQ;
using HansJuergenWeb.MessageHandlers.Settings;

namespace HansJuergenWeb.MessageHandlers.Adapters
{
    public class BusAdapter : IBusAdapter
    {
        private readonly IAppSettings _appSettings;

        public BusAdapter(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }
        private IBus _bus;

        // 
        public IBus Bus
        {
            get
            {
                if(_bus==null)
                    _bus= RabbitHutch.CreateBus(_appSettings.EasyNetQConfig);
                return _bus;
            }
        }
    }
}
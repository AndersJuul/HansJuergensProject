using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyNetQ;

namespace HansJuergenWeb.MessageHandlers.Adapters
{
    public interface IBusAdapter
    {
        IBus Bus { get; }
    }
}

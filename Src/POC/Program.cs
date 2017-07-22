using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyNetQ;
using HansJuergenWeb.Contracts;

namespace POC
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var bus = RabbitHutch.CreateBus("host=ajf-elastic-01;username=anders;password=21Bananer;timeout=30"))
            {
                var message = new FileUploadedEvent
                {
                    FileName = "dummy.txt",
                    Email = "foo@bar.org",
                    Description = "Lorem ipsum"

                };
                bus.Publish(message);
            }

        }
    }
}

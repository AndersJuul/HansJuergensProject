using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using EasyNetQ;
using HansJuergenWeb.Contracts;
using Newtonsoft.Json;
using System.IO;
using Ajf.Nuget.Logging;

namespace POC
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var mailSender = new MailSender();
            do
            {
                Console.WriteLine("Press enter");
                Console.ReadLine();

                var httpStatusCode = mailSender
                    .SendMailAsync("andersjuulsfirma@gmail.com", "andersjuulsfirma@gmail.com", "andersjuulsfirma@gmail.com", 
                    "Sub","<html>Hello</html>", 
                    new string[]{})
                    .Result;
            } while (true);
        }
    }
}
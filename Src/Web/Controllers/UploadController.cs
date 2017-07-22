using System;
using System.IO;
using System.Text;
using System.Web.Mvc;
using EasyNetQ;
using RabbitMQ.Client;
using WebHJ.Models;

namespace WebHJ.Controllers
{
    public class UploadController : Controller
    {
        // GET: Upload
        public ActionResult Post(ExpenseModel expenseModel)
        {
            foreach (string upload in Request.Files)
            {
                if (Request.Files[upload].ContentLength == 0) continue;
                var pathToSave = Path.GetTempPath();
                var filename = Path.GetFileName(Request.Files[upload].FileName);
                Request.Files[upload].SaveAs(Path.Combine(pathToSave, filename));

                var appSettings = new AppSettings();

                using (var bus =RabbitHutch.CreateBus("host=ajf-elastic-01;username=Anders;password=21Bananer;timeout=30"))
                {
                    var message = new FileUploadedEvent
                    {
                        FileName = filename,
                        Email = expenseModel.Email,
                        Description = expenseModel.Description

                    };
                    bus.Publish(message);
                }
                //var connectioFa = new ConnectionFactory
                //{
                //    HostName = "ajf-elastic-01",
                //    UserName = "anders",
                //    Password = "21Bananer"
                //};

                //var connection = connectioFa.CreateConnection();
                //var model = connection.CreateModel();

                //var properties = model.CreateBasicProperties();
                //properties.Persistent = true;

                //var messageBuffer = Encoding.Default.GetBytes("Hello");

                //model.BasicPublish(appSettings.ExchangeName, "", properties, messageBuffer);

            }

            return RedirectToAction("Index", "Home");
        }
    }
}
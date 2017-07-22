using System.IO;
using System.Web.Mvc;
using EasyNetQ;
using HansJuergenWeb.Contracts;
using WebHJ;
using WebHJ.Models;

namespace HansJuergenWeb.WebHJ.Controllers
{
    public class UploadController : Controller
    {
        private readonly IBus _bus;

        public UploadController(IBus bus)
        {
            _bus = bus;
        }
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

                //var message = new FileUploadedEvent
                //{
                //    FileName = filename,
                //    Email = expenseModel.Email,
                //    Description = expenseModel.Description

                //};
                //_bus.Publish(message);

                //using (var bus = RabbitHutch.CreateBus("host=ajf-elastic-01;username=anders;password=21Bananer;timeout=10"))
                //{
                    var message = new FileUploadedEvent
                    {
                        FileName = "dummy.txt",
                        Email = "foo@bar.org",
                        Description = "Lorem ipsum"

                    };
                    _bus.Publish(message);
                //}


            }

            return RedirectToAction("Index", "Home");
        }
    }
}
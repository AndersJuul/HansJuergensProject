using System.IO;
using System.Web.Mvc;
using EasyNetQ;
using HansJuergenWeb.Contracts;
using Serilog;
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

                Log.Logger.Information("Received file: {rfile}",filename);

                    var message = new FileUploadedEvent
                    {
                        FileName = filename,
                        Email =expenseModel.Email,
                        Description = expenseModel.Description
                    };
                    _bus.Publish(message);

                Log.Logger.Information("Message broadcasted that file was uploaded: {@message}", message);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
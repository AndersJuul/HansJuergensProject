using System;
using System.IO;
using System.Web.Mvc;
using EasyNetQ;
using HansJuergenWeb.Contracts;
using HansJuergenWeb.WebHJ.Models;
using Serilog;

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
        public ActionResult Post(UploadModel uploadModel)
        {
            var uploadDir = @"c:\temp\hjuploads\";
            var guid = Guid.NewGuid();
            var pathToSave = Path.Combine(uploadDir, guid.ToString());

            Directory.CreateDirectory(pathToSave);

            foreach (string upload in Request.Files)
            {
                if (Request.Files[upload].ContentLength == 0) continue;
                
                var filename = Path.GetFileName(Request.Files[upload].FileName);
                Request.Files[upload].SaveAs(Path.Combine(pathToSave, filename));

                Log.Logger.Information("Received file: {rfile}",filename);

                    var message = new FileUploadedEvent
                    {
                        FileName = filename,
                        Email =uploadModel.Email,
                        Description = uploadModel.Description,
                        Id = guid
                    };
                    _bus.Publish(message);

                Log.Logger.Information("Message broadcasted that file was uploaded: {@message}", message);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
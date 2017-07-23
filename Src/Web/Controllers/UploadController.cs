using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
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
            var guid = Guid.NewGuid();
            Log.Logger.Information("Received request to upload: {uploadId}", guid);

            var uploadDir = @"c:\temp\hjuploads\";
            var pathToSave = Path.Combine(uploadDir, guid.ToString());

            Directory.CreateDirectory(pathToSave);

            var fileNames = new List<string>();
            for (int i = 0; i < Request.Files.Count; i++)
            {
                if (Request.Files[i].ContentLength == 0) continue;
                
                var filename = Path.GetFileName(Request.Files[i].FileName);
                Request.Files[i].SaveAs(Path.Combine(pathToSave, filename));
                fileNames.Add(filename);
            }
            

            Log.Logger.Information("Received file: {rfile}", fileNames.ToArray());

            var message = new FileUploadedEvent
            {
                FileNames = fileNames.ToArray(),
                Email = uploadModel.Email,
                Description = uploadModel.Description,
                Id = guid
            };
            _bus.Publish(message);

            Log.Logger.Information("Message broadcasted that file was uploaded: {@message}", message);

            return RedirectToAction("Index", "Home");
        }
    }
}
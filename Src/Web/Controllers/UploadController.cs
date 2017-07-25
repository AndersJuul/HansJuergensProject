using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using EasyNetQ;
using HansJuergenWeb.Contracts;
using HansJuergenWeb.WebHJ.Validators;
using Serilog;

namespace HansJuergenWeb.WebHJ.Controllers
{
    public class UploadController : Controller
    {
        private readonly IBus _bus;
        private readonly IEnumerable<IValidateUpload> _uploadValidators;
        private readonly IAppSettings _appSettings;

        public UploadController(IBus bus, IEnumerable<IValidateUpload> uploadValidators,IAppSettings appSettings)
        {
            _bus = bus;
            _uploadValidators = uploadValidators;
            _appSettings = appSettings;
        }

        public ActionResult Error(UploadErrorModel uploadErrorModel)
        {
            ViewBag.Message = "Error uploading.";

            return View("Error", uploadErrorModel);
        }

        public ActionResult Post(UploadModel uploadModel)
        {
            if(uploadModel==null)
                return Error(new UploadErrorModel { Errors = new string[]{"uploadmodel is null"} });

            //if (!ModelState.IsValid)
            //{
            //    var enumerable = ModelState.Select(x => x.Value.Errors.First().ErrorMessage);
            //    return Error(new UploadErrorModel{Errors =  enumerable});
            //}

            if (Request.Files.Count == 0)
            {
                return Error(new UploadErrorModel {Errors = new[] {"At least one file must be uploaded!"}});
            }

            var guid = Guid.NewGuid();
            Log.Logger.Information("Received request to upload: {uploadId}", guid);

            var uploadDir = _appSettings.UploadDir;
            var dataFolder = Path.Combine(uploadDir, guid.ToString());

            Directory.CreateDirectory(dataFolder);

            var fileNames = new List<string>();
            for (var i = 0; i < Request.Files.Count; i++)
            {
                if (Request.Files[i].ContentLength == 0) continue;

                var filename = Path.GetFileName(Request.Files[i].FileName);
                Request.Files[i].SaveAs(Path.Combine(dataFolder, filename));
                fileNames.Add(filename);
            }

            foreach (var uploadValidator in _uploadValidators)
            {
                var validationResult = uploadValidator.Validate(uploadModel, dataFolder, fileNames);
                if (validationResult != null)
                {
                    return Error(new UploadErrorModel {Errors = new[] {validationResult.ErrorMessage}});
                }
            }

            Log.Logger.Information("Done writing and validating the following uploaded files: {uploadedFiles}", fileNames.ToArray());

            var fileUploadedEvent = new FileUploadedEvent
            {
                FileNames = fileNames.ToArray(),
                Email = uploadModel.Email,
                Description = uploadModel.Description,
                Id = guid,
                DataFolder = dataFolder,
                Allergene = uploadModel.Allergene
            };
            _bus.Publish(fileUploadedEvent);
            Log.Logger.Information("Message broadcasted that files were uploaded: {@message}", fileUploadedEvent);

            return RedirectToAction("Thanks", "Home");
        }
    }
}
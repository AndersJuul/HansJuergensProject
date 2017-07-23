using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HansJuergenWeb.WebHJ.Models;

namespace HansJuergenWeb.WebHJ.Controllers
{
    public interface IValidateUpload
    {
        ValidationResult Validate(UploadModel uploadModel, string pathToSave, List<string> fileNames);
    }
}
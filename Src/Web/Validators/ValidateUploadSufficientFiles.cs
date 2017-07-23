using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HansJuergenWeb.WebHJ.Controllers;
using HansJuergenWeb.WebHJ.Models;

namespace HansJuergenWeb.WebHJ.Validators
{
    public class ValidateUploadSufficientFiles : IValidateUpload
    {
        public ValidationResult Validate(UploadModel uploadModel, string pathToSave, List<string> fileNames)
        {
            if (fileNames.Count == 0)
                return new ValidationResult(
                    "At least one data file, one positive file and one negative file must be uploaded.");

            return null;
        }
    }
}
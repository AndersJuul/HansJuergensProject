using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HansJuergenWeb.Contracts;

namespace HansJuergenWeb.WebHJ.Validators
{
    public interface IValidateUpload
    {
        ValidationResult Validate(UploadModel uploadModel, string pathToSave, List<string> fileNames);
    }
}
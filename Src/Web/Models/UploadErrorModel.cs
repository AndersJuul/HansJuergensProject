using System.Collections.Generic;

namespace HansJuergenWeb.WebHJ.Models
{
    public class UploadErrorModel
    {
        public IEnumerable<string> Errors { get; set; }

        public UploadErrorModel()
        {
        }
    }
}
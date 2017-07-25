using System.Collections.Generic;

namespace HansJuergenWeb.Contracts
{
    public class UploadErrorModel
    {
        public IEnumerable<string> Errors { get; set; }
    }
}
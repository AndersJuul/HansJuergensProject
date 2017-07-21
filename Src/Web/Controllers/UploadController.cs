using System.IO;
using System.Web.Mvc;
using WebHJ.Models;

namespace WebHJ.Controllers
{
    public class UploadController : Controller
    {
        // GET: Upload
        public ActionResult Post(ExpenseModel expenseModel)
        {
            foreach (string upload in Request.Files)
            {
                if (Request.Files[upload].ContentLength == 0) continue;
                var pathToSave = Path.GetTempPath();
                var filename = Path.GetFileName(Request.Files[upload].FileName);
                Request.Files[upload].SaveAs(Path.Combine(pathToSave, filename));
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
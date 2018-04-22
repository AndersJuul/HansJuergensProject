using System.Web.Mvc;
using HansJuergenWeb.Contracts;
using Serilog;

namespace HansJuergenWeb.WebHJ.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Log.Logger.Debug("Displaying " + Request.RawUrl);
            return View();
        }

        public ActionResult About()
        {
            Log.Logger.Debug("Displaying " + Request.RawUrl);

            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            Log.Logger.Debug("Displaying " + Request.RawUrl);

            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Thanks(UploadModel uploadModel)
        {
            ViewBag.Message = "Your data has been submitted. If you supplied an email address, you should receive an email confirmation as well as the results of processing the data through R.";

            return View("Thanks");
        }

    }
}
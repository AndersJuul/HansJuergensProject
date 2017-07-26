using System.Web.Mvc;
using HansJuergenWeb.Contracts;

namespace HansJuergenWeb.WebHJ.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
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
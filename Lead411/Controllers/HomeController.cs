using System.Web.Mvc;

namespace Lead411.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return RedirectToAction("Login", "Admin");
        }

        public ActionResult About()
        {
            return View();
        }

    }
}
using System.Web.Mvc;

namespace LoginProvider.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            return this.View();
        }
    }
}

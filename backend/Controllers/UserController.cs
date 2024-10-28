using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    public class UserController : Controller
    {
        // GET: UserController
        public ActionResult Index()
        {
            return View();
        }

    }
}

using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    public class GameController : Controller
    {
        // GET: GameController
        public ActionResult Index()
        {
            return View();
        }

    }
}

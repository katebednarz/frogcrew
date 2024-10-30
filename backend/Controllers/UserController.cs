using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : Controller
    {
        [HttpGet("{id}")]
        // GET: UserController
        public ActionResult Index(int id)
        {
            return Ok("ID number given: " + id);
        }

    }
}

using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers {
    public class UsersController : Controller {
        [HttpGet]
        public IActionResult SignIn() {
            return View();
        }
        [HttpPost]
        public IActionResult SignIn(string username, string password) {
            return View();
        }
        [HttpGet]
        public IActionResult SignUp() {
            return View();
        }
        [HttpPost]
        public IActionResult SignUp(string username, string password) {
            return View();
        }
        [HttpGet]
        public IActionResult SignOut() {
            return View();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BrewBattle.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            var model = new UserViewModel(User.Identity.Name);
            return View(model);
        }

        public class UserViewModel
        {
            public string UserName { get; set; }

            public UserViewModel(string userName)
            {
                this.UserName = userName;
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LocationTracker.Models;
using LocationTracker.Repositories.Core;
using LocationTracker.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System;

namespace LocationTracker.Controllers {
	[AllowAnonymous]
	public class LoginController : Controller {
		private readonly IUserRepository userRepository;
		private readonly IUserSessionRepository userSessionRepository;

		public LoginController(
            IUserRepository userRepository,
			IUserSessionRepository userSessionRepository
		) {
			this.userRepository = userRepository;
			this.userSessionRepository = userSessionRepository;
		}

		public IActionResult Index() {
            return View(new User());
        }

		[HttpPost]
		public async Task<IActionResult> Index(User _user) {
			var user = await userRepository.GetUser(_user.Email, _user.Password);
			if (!(user is User)) {
				ModelState.AddModelError("invalid-login", "Ongeldige inlog");

				return View(_user);
			}

			var session = await userSessionRepository.CreateSession(user.Id);
			if (!(session is UserSession)) {
				return View(_user);
			}

			HttpContext.Response.Cookies.Append("User", string.Format("IdUser={0}&Code={1}", user.Id, session.Token), new Microsoft.AspNetCore.Http.CookieOptions
			{
				Expires = DateTimeOffset.Now.AddMonths(1)
			});

			return RedirectToAction("Index", "Home");
		}
    }
}
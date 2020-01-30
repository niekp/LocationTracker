using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Locatie.Models;
using Locatie.Repositories.Core;
using Microsoft.Extensions.Hosting;
using Locatie.Utils;

namespace Locatie.Utils.Authentication {
    
    public class CookieAuthHandler : AuthenticationHandler<CookieAuthOptions> {
        IWebHostEnvironment hostingEnvironment;
        private readonly IUserRepository userRepository;
        private readonly IUserSessionRepository userSessionRepository;

        public CookieAuthHandler(
            IOptionsMonitor<CookieAuthOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IWebHostEnvironment hostingEnvironment,
            IUserRepository userRepository,
            IUserSessionRepository userSessionRepository
        ) : base(options, logger, encoder, clock) {
            this.hostingEnvironment = hostingEnvironment;
            this.userRepository = userRepository;
            this.userSessionRepository = userSessionRepository;
        }

        private async Task<AuthenticateResult> GetSuccesTicket(User user) {
            var claims = new List<Claim>();
            claims.Add(new Claim(Constants.USER_ID_CLAIM, user.Id.ToString()));
            claims.Add(new Claim(Constants.USER_NAME_CLAIM, user.Email));
            claims.Add(new Claim(Constants.USER_EMAIL_CLAIM, user.Email));
            var identities = new List<ClaimsIdentity> { new ClaimsIdentity(claims, "Cookie") };
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identities), "Cookie");

            return await Task.FromResult(AuthenticateResult.Success(ticket));
        }
        
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            int? IdUser = Context.Session.GetInt32("IdUser");
			User user = null;
            if (IdUser != null && IdUser > 0) {
                user = await userRepository.GetByIdAsync((int)IdUser);
            }

            if (user == null && Context.Request.Cookies["User"] != null) {
                var userCookie = HttpUtility.ParseQueryString(Context.Request.Cookies["User"]);

                if (Int32.TryParse(userCookie["IdUser"], out var id)
                    && id > 0
                    && userCookie["Code"] != null
                    && userCookie["Code"] != string.Empty
                ) {
                    var session = await userSessionRepository.GetSession(id, userCookie["Code"]);
                    if (session is UserSession)
                    {
                        user = session.User;
                    }

                    if (user != null) {
                        Context.Session.SetInt32("IdUser", user.Id);
                    }
                }
            }

			if (user != null) {
				return await GetSuccesTicket(user);
			}

            return await Task.FromResult(AuthenticateResult.Fail("Geen toegang."));
        }

    }
}

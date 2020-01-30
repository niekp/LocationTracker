using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace Locatie.Utils.Authentication {
    public static class AuthExtensions {

        public static AuthenticationBuilder AddCookieAuth(this AuthenticationBuilder builder, Action<CookieAuthOptions> configureOptions) {
            return builder.AddScheme<CookieAuthOptions, CookieAuthHandler>("Cookie", "Cookie", configureOptions);
        }
    }
}

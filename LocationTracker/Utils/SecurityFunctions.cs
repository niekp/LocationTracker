using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LocationTracker.Utils {
    public static class SecurityFunctions {

        public static string GetHash(string rawData) {
            using (var sha256Hash = SHA256.Create()) {

                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                var builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

		private static Random random = new Random();
		public static string RandomString(int length) {
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(Enumerable.Repeat(chars, length)
			  .Select(s => s[random.Next(s.Length)]).ToArray());
		}

		public static string GetClaim(ClaimsPrincipal User, string ClaimType) {
			var claim = User.Claims.Where(c => c.Type == ClaimType).FirstOrDefault();
			if (claim == null) {
				return null;
			}

			return claim.Value;
		}

	}
}

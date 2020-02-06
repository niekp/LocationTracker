using System;
using System.Linq;
using System.Threading.Tasks;
using LocationTracker.Data;
using LocationTracker.Models;
using LocationTracker.Repositories.Core;
using LocationTracker.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LocationTracker.Repositories.Persistence
{
    public class UserSessionRepository : Repository<UserSession>, IUserSessionRepository
    {
		private readonly ICache cache;
        public UserSessionRepository(
            LocationContext locatieContext,
            ICache cache
        ) : base(locatieContext)
        {
			this.cache = cache;
        }

        public async Task<UserSession> CreateSession(int IdUser)
        {
            var session = new UserSession()
            {
                UserId = IdUser,
                Token = SecurityFunctions.RandomString(100),
                ValidTill = DateTime.Now.AddDays(30)
            };
            Insert(session);
            await SaveAsync();
            return session;
        }

        public Task<UserSession> GetSession(int IdUser, string Token)
        {
            return dbSet.Where(t =>
                    t.UserId == IdUser
                    && t.Token == Token
                    && t.ValidTill >= DateTime.Now
            ).Include(s => s.User)
            .FirstOrDefaultAsync();
        }
    }
}

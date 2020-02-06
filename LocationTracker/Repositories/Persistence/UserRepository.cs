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
    public class UserRepository : Repository<User>, IUserRepository
    {
		private readonly ICache cache;
		private readonly IUserSessionRepository userSessionRepository;
        private readonly Microsoft.Extensions.Options.IOptions<AppSettings> options;

		public UserRepository(
            LocationContext locatieContext,
            IUserSessionRepository userSessionRepository,
			ICache cache,
			Microsoft.Extensions.Options.IOptions<AppSettings> options
		) : base(locatieContext)
        {
			this.cache = cache;
			this.userSessionRepository = userSessionRepository;
			this.options = options;
		}

		// Overschrijven om te cachen. Deze functie wordt veel aangeroepen door de cookie auth handler
		public override async Task<User> GetByIdAsync(object id)
		{
			return await cache.GetOrCreate(String.Format("GebruikerRepository_GetByIdAsync_{0}", id), cache_entry => {
				cache_entry.SetOptions(cache.GetCacheOption());

				return base.GetByIdAsync(id);
			});
		}

		public async Task<User> GetUserByEmail(string Email)
		{
			return await cache.GetOrCreate(String.Format("GebruikerRepository_GetUserByEmail_{0}", Email), cache_entry => {
				cache_entry.SetOptions(cache.GetCacheOption());

				return dbSet.Where(g => g.Email == Email).FirstOrDefaultAsync();
			});
		}

		public async Task<User> GetUser(string Email, string Password)
		{
			var user = await GetUserByEmail(Email);
			if (!(user is User))
			{
				return null;
			}

			var hash = SecurityFunctions.GetHash(
				String.Concat(
                    options.Value.Pepper ?? "",
					Password
				)
			);

			return await dbSet.Where(g =>
				g.Email == Email
				&& g.Password == hash
			).FirstOrDefaultAsync();
		}
	}
}

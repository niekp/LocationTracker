using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocationTracker.Models;

namespace LocationTracker.Repositories.Core
{
    public interface IUserRepository : IRepository<User>
    {
		Task<User> GetUser(string Email, string Password);

		Task<User> GetUserByEmail(string Email);

	}
}

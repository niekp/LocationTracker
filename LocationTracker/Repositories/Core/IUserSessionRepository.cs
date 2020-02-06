using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocationTracker.Models;

namespace LocationTracker.Repositories.Core
{
    public interface IUserSessionRepository : IRepository<UserSession>
    {
        Task<UserSession> GetSession(int IdUser, string Token);
        Task<UserSession> CreateSession(int IdUser);
    }
}

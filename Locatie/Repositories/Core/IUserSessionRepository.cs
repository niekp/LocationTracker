using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Locatie.Models;

namespace Locatie.Repositories.Core
{
    public interface IUserSessionRepository : IRepository<UserSession>
    {
        Task<UserSession> GetSession(int IdUser, string Token);
        Task<UserSession> CreateSession(int IdUser);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Locatie.Models;

namespace Locatie.Repositories.Core
{
    public interface IPingRepository : IRepository<Ping>
    {
        Task<List<Ping>> GetPings(Day day);
    }
}

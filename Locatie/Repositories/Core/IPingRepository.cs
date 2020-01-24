using System;
using System.Collections.Generic;
using Locatie.Models;

namespace Locatie.Repositories.Core
{
    public interface IPingRepository : IRepository<Ping>
    {
        Ping GetLastPing();
    }
}

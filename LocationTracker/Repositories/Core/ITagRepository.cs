using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocationTracker.Models;

namespace LocationTracker.Repositories.Core
{
    public interface ITagRepository : IRepository<Tag>
    {
        Task<Tag> GetOrCreate(string tag);
    }
}

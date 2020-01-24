using System;
using System.Linq;
using Locatie.Data;
using Locatie.Models;
using Locatie.Repositories.Core;

namespace Locatie.Repositories.Persistence
{
    public class TagRepository : Repository<Tag>, ITagRepository
    {
        public TagRepository(LocatieContext locatieContext) : base(locatieContext)
        {
        }
    }
}

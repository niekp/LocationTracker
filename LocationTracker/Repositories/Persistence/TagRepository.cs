using System;
using System.Linq;
using System.Threading.Tasks;
using LocationTracker.Data;
using LocationTracker.Models;
using LocationTracker.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace LocationTracker.Repositories.Persistence
{
    public class TagRepository : Repository<Tag>, ITagRepository
    {
        public TagRepository(LocationContext locatieContext) : base(locatieContext)
        {
        }

        public async Task<Tag> GetOrCreate(string tag)
        {
            var _tag = await dbSet.Where(t =>
                t.Label.ToLower().Trim() == tag.ToLower().Trim()
            ).FirstOrDefaultAsync();

            if (!(_tag is Tag))
            {
                _tag = new Tag()
                {
                    Label = tag
                };

                Insert(_tag);
                await SaveAsync();
            }

            return _tag;
        }
    }
}

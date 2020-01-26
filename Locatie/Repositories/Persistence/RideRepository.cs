using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Locatie.Data;
using Locatie.Models;
using Locatie.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace Locatie.Repositories.Persistence
{
    public class RideRepository : Repository<Ride>, IRideRepository
    {
        private readonly ITagRepository tagRepository;

        public RideRepository(
            LocatieContext locatieContext,
            ITagRepository tagRepository
        ) : base(locatieContext)
        {
            this.tagRepository = tagRepository;
        }

        public Task<Ride> GetByIdFull(int id)
        {
            return dbSet.Where(r => r.Id == id)
                .Include(r => r.Pings)
                .Include(r => r.Tags)
                .Include("Tags.Tag")
                .FirstOrDefaultAsync();
        }

        public async Task SetTags(int rideId, string tags)
        {
            var ride = await GetByIdFull(rideId);

            var oldTagIds = ride.Tags.Select(t => t.TagId).ToList();
            var newTagIds = new List<int>();

            foreach (var tagLabel in tags.Split(','))
            {
                var tag = await tagRepository.GetOrCreate(tagLabel);
                newTagIds.Add(tag.Id);
            }

            var removeTags = oldTagIds.Except(newTagIds);
            var addTags = newTagIds.Except(oldTagIds);

            foreach (RideTag rideTag in ride.Tags.Where(t => removeTags.Contains(t.TagId)))
            {
                db.Remove(rideTag);
            }

            foreach (var tagId in addTags)
            {
                var rt = new RideTag()
                {
                    RideId = rideId,
                    TagId = tagId
                };

                await db.AddAsync(rt);
            }

            await SaveAsync();
        }
    }
}

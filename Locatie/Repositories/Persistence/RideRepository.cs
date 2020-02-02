using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Locatie.Data;
using Locatie.Models;
using Locatie.Repositories.Core;
using Locatie.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Locatie.Repositories.Persistence
{
    public class RideRepository : Repository<Ride>, IRideRepository
    {
        private readonly ICache cache;
        private readonly ITagRepository tagRepository;

        public RideRepository(
            LocatieContext locatieContext,
            ICache cache,
            ITagRepository tagRepository
        ) : base(locatieContext)
        {
            this.cache = cache;
            this.tagRepository = tagRepository;
        }

        public Task<Ride> GetByIdFull(int id)
        {
            return dbSet.Where(r => r.Id == id)
                .Include(r => r.Pings)
                .Include(r => r.Tags)
                .Include(r => r.Day)
                .Include("Tags.Tag")
                .FirstOrDefaultAsync();
        }

        public async Task<List<Ride>> GetByTag(Tag tag)
        {
            return await cache.GetOrCreate(string.Format("RideRepository_GetByTag_{0}", tag.Id), cache_item =>
            {
                cache_item.SetOptions(cache.GetCacheOption());

                return (from r in db.Ride
                        join rt in db.RideTag on r.Id equals rt.RideId
                        where rt.TagId == tag.Id
                        select r)
                        .Include(r => r.Pings)
                        .ToListAsync();
            });
        }

        public async Task SetTags(int rideId, string tags)
        {
            var ride = await GetByIdFull(rideId);
            if (!(ride is Ride))
            {
                throw new ArgumentException("Ride not found");
            }

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

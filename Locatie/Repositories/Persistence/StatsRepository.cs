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
    public class StatsRepository : IStatsRepository
    {
        private readonly ITagRepository tagRepository;
        private readonly LocatieContext db;
        private readonly ICache cache;

        public StatsRepository(
            LocatieContext db,
            ITagRepository tagRepository,
            ICache cache
        )
        {
            this.tagRepository = tagRepository;
            this.db = db;
            this.cache = cache;
        }

        private async Task<List<int>> GetRidesIdsForTagIds(List<int> TagIds)
        {
            return await cache.GetOrCreate(string.Format("StatsRepository_GetRidesIdsForTagIds_{0}", string.Join(',', TagIds)), async cache_entry =>
            {
                cache_entry.SetOptions(cache.GetCacheOption());

                return await db.RideTag.Where(rt => TagIds.Contains(rt.TagId)).Select(rt => rt.RideId).ToListAsync();
            });
        }

        private async Task<(int minutes, int meters)> GetMinutesMovingAsync(DateTime From, DateTime To, double MaxKmh, double MaxKm, List<Tag> Tags = null)
        {
            var tagIds = new List<int>();
            var rideIds = new List<int>();
            if (Tags != null)
            {
                tagIds = Tags.Select(t => t.Id).ToList();
                rideIds = await GetRidesIdsForTagIds(tagIds);
            }

            var rides = (from d in db.Day
                     join r in db.Ride on d.RideId equals r.Id
                     where (r.TimeFrom >= From && r.TimeTo <= To)
                     && ((
                        MaxKmh > 0 && MaxKm > 0
                        && r.DistanceInMeters <= (MaxKm * 1000.0)
                        && ((r.DistanceInMeters / 1000.0) / (r.TimeTo - r.TimeFrom).TotalHours) <= MaxKmh
                     )
                     || rideIds.Contains(r.Id)
                     )
                     select new
                     {
                         r.TimeFrom,
                         r.TimeTo,
                         r.DistanceInMeters
                     });
            
            double minutes = 0;
            int meters = 0;
            foreach (var ride in rides)
            {
                minutes += (ride.TimeTo - ride.TimeFrom).TotalMinutes;
                meters += ride.DistanceInMeters ?? 0;
            }

            return (Convert.ToInt32(minutes), meters);
        }

        public async Task<Dictionary<string, int>> GetTopLocations(DateTime From, DateTime To, int Amount = 5)
        {
            return await (from l in db.Location
                             join d in db.Day on l.Id equals d.LocationId
                             where (d.TimeFrom >= From && d.TimeTo <= To)
                             group d by new { d.LocationId, l.Label } into dg
                             select new
                             {
                                 dg.Key.Label,
                                 Time = dg.Sum(x => (x.TimeTo - x.TimeFrom).TotalMinutes)
                             }
                            ).OrderByDescending(x => x.Time)
                            .Take(Amount)
                            .ToDictionaryAsync(x => x.Label, x => Convert.ToInt32(x.Time));
        }

        public async Task<Stats> GetStats(DateTime from, DateTime to)
        {
            return await cache.GetOrCreate(string.Format("StatsRepository_GetStats_{0}_{1}", from, to), async cache_entry =>
            {
                if (to >= DateTime.Now)
                {
                    cache_entry.SetOptions(cache.GetCacheOption(Minuten: 30));
                }
                else
                {
                    cache_entry.SetOptions(cache.GetCacheOption(Minuten: 365*24*60));
                }

                if (to > DateTime.Now)
                {
                    to = DateTime.Now;
                }

                var stats = new Stats
                {
                    TotalHours = (to - from).TotalHours
                };
                // Total movement
                var (minutes, meters) = await GetMinutesMovingAsync(from, to, 999.0, 9999.0);
                stats.All.Minutes = minutes;
                stats.All.Meters = meters;

                // Walking & running
                var runningTag = await tagRepository.GetOrCreate("Hardlopen");
                (minutes, meters) = await GetMinutesMovingAsync(from, to, 6.0, 25.0, new List<Tag>() { runningTag });
                stats.Walking.Minutes = minutes;
                stats.Walking.Meters = meters;

                // Running only
                (minutes, meters) = await GetMinutesMovingAsync(from, to, 0, 0, new List<Tag>() { runningTag });
                stats.Running.Minutes = minutes;
                stats.Running.Meters = meters;

                // Subtract running from walking (so if a 'run' under 6km/h it counts as running)
                stats.Walking.Minutes -= stats.Running.Minutes;
                stats.Walking.Meters -= stats.Running.Meters;

                stats.Locations = await GetTopLocations(from, to);

                /*
                var bikingTag = await tagRepository.GetOrCreate("Fietsen");
                (minutes, meters) = GetMinutesMoving(from, to, 0, 0, new List<Tag>() { bikingTag });
                stats.Biking.Minutes = minutes;
                stats.Biking.Meters = meters;
                */

            return stats;
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Locatie.Models;
using Locatie.Repositories.Core;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Locatie.Controllers
{
    public class StatsController : Controller
    {
        private readonly IStatsRepository statsRepository;
        public StatsController(
            IStatsRepository statsRepository
        )
        {
            this.statsRepository = statsRepository;
        }
        public async Task<IActionResult> Year()
        {
            var stats = new List<Stats>();
            DateTime dateFrom, dateTo;
            for (var year = 2019; year <= DateTime.Now.Year; year++)
            {
                var monthTo = 12;
                if(year == DateTime.Now.Year)
                {
                    monthTo = DateTime.Now.Month;
                }

                for (var month = 1; month <= monthTo; month++)
                {
                    dateFrom = new DateTime(year, month, 1);
                    dateTo = dateFrom.AddMonths(1).AddSeconds(-1);
                    var monthData = await statsRepository.GetStats(dateFrom, dateTo);
                    monthData.Label = string.Format("{0:00}-{1:0000}", month, year);
                    stats.Add(monthData);
                }

                dateFrom = new DateTime(year, 1, 1);
                dateTo = dateFrom.AddMonths(12).AddSeconds(-1);
                var yearData = await statsRepository.GetStats(dateFrom, dateTo);
                yearData.Label = year.ToString();
                yearData.SummaryRow = true;
                stats.Add(yearData);
            }

            stats.Reverse();

            return View(stats);
        }
    }
}

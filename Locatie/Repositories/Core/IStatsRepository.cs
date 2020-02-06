﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Locatie.Models;

namespace Locatie.Repositories.Core
{
    public interface IStatsRepository
    {
        Task<Stats> GetStats(DateTime from, DateTime to);
    }
}
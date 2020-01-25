﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Locatie.Models;

namespace Locatie.Repositories.Core
{
    public interface IDayRepository : IRepository<Day>
    {
        Task<List<Day>> GetDays(DateTime From, DateTime To);
    }
}
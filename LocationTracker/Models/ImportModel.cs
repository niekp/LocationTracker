using System;
using Microsoft.AspNetCore.Http;

namespace LocationTracker.Models
{
    public class ImportModel
    {
        public IFormFile WayPoints { get; set; }
        public IFormFile Track { get; set; }
    }
}

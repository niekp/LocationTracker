using System;
using Microsoft.AspNetCore.Http;

namespace Locatie.Models
{
    public class ImportModel
    {
        public IFormFile WayPoints { get; set; }
        public IFormFile Track { get; set; }
    }
}

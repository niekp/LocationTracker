using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocationTracker.Repositories.Core;
using LocationTracker.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace LocationTracker.Controllers
{
    [Authorize]
    public class NoteController : Controller
    {
        private INoteRepository noteRepository;

        public NoteController(
            INoteRepository noteRepository
        )
        {
            this.noteRepository = noteRepository;
        }

        public async Task<IActionResult> Index(string from = "", string to = "")
        {
            var _from = DateFunctions.GetDate(from, DateTime.Now.Date.AddDays(-7));
            var _to = DateFunctions.GetDate(to, DateTime.Now.Date);
            var notes = await noteRepository.GetBetween(_from, _to);

            ViewBag.From = _from;
            ViewBag.To = _to;

            return View(notes);
        }

        public async Task<IActionResult> Search(string search)
        {
            ViewBag.From = DateTime.Now.Date.AddDays(-7);
            ViewBag.To = DateTime.Now.Date;
            ViewBag.Search = search;

            return View("Index",
                await noteRepository.SearchNotes(search)
            );
        }
    }
}

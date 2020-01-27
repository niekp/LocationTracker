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
    public class TagController : Controller
    {
        private readonly ITagRepository tagRepository;

        public TagController(
            ITagRepository tagRepository
        )
        {
            this.tagRepository = tagRepository;
        }

        public async Task<IActionResult> Index()
        {
            var tags = await tagRepository.GetAllASync();
            return View(tags.OrderBy(t => t.Label).ToList());
        }

        public async Task<IActionResult> Edit(int id)
        {
            var tag = await tagRepository.GetByIdAsync(id);
            return View(tag);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Tag _tag)
        {
            var tag = await tagRepository.GetByIdAsync(_tag.Id);
            tag.Label = _tag.Label;
            tagRepository.Update(tag);
            await tagRepository.SaveAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int Id)
        {
            var tag = await tagRepository.GetByIdAsync(Id);
            if (tag is Tag)
            {
                tagRepository.Delete(Id);
                tagRepository.Save();
            }

            return RedirectToAction("Index");
        }
    }
}

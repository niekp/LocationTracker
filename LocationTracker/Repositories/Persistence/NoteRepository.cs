using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocationTracker.Data;
using LocationTracker.Models;
using LocationTracker.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace LocationTracker.Repositories.Persistence
{
    public class NoteRepository : Repository<Note>, INoteRepository
    {
        public NoteRepository(LocationContext locatieContext) : base(locatieContext)
        {
        }

        public async Task<Note> GetNote(DateTime date)
        {
            var note = await dbSet.Where(n => n.Date.Date == date.Date).FirstOrDefaultAsync();
            return note ?? new Note(date, "");
        }

        public async Task SaveNote(DateTime date, string text)
        {
            var note = await GetNote(date);
            if (!(note is Note) || note.Id == 0)
            {
                note = new Note(date, text);
                Insert(note);
            }
            else
            {
                note.Text = text;
                Update(note);
            }

            await SaveAsync();
        }

        public Task<List<Note>> SearchNotes(string search)
        {
            return dbSet
                .Where(n => n.Text.Contains(search))
                .OrderByDescending(n => n.Date)
                .ToListAsync();
        }
        
    }
}

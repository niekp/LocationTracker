using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocationTracker.Models;

namespace LocationTracker.Repositories.Core
{
    public interface INoteRepository : IRepository<Note>
    {
        Task<Note> GetNote(DateTime date);
        Task SaveNote(DateTime date, string note);
        Task<List<Note>> SearchNotes(string search);
        Task<List<Note>> GetBetween(DateTime from, DateTime to);
    }
}

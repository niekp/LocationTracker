using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocationTracker.Data;
using LocationTracker.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace LocationTracker.Repositories.Persistence
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly LocationContext db = null;
        protected readonly DbSet<T> dbSet = null;

        public Repository(LocationContext locatieContext)
        {
            db = locatieContext;
            dbSet = db.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllASync()
        {
            return await dbSet.ToListAsync();
        }

        public virtual Task<T> GetByIdAsync(object id)
        {
            return dbSet.FindAsync(id);
        }

        public virtual void Insert(T obj)
        {
            dbSet.Add(obj);
        }

        public void Update(T obj)
        {
            dbSet.Attach(obj);
            db.Entry(obj).State = EntityState.Modified;
        }

        public void Delete(object id)
        {
            T existing = dbSet.Find(id);
            dbSet.Remove(existing);
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public Task SaveAsync()
        {
            return db.SaveChangesAsync();
        }

    }
}

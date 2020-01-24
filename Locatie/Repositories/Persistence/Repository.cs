using System;
using System.Collections.Generic;
using System.Linq;
using Locatie.Data;
using Locatie.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace Locatie.Repositories.Persistence
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly LocatieContext db = null;
        protected readonly DbSet<T> dbSet = null;

        public Repository(LocatieContext locatieContext)
        {
            db = locatieContext;
            dbSet = db.Set<T>();
        }

        public IEnumerable<T> GetAll()
        {
            return dbSet.ToList();
        }

        public T GetById(object id)
        {
            return dbSet.Find(id);
        }

        public void Insert(T obj)
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

    }
}

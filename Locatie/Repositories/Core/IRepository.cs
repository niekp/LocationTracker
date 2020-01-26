using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Locatie.Repositories.Core
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllASync();
        Task<T> GetByIdAsync(object id);
        void Insert(T obj);
        void Update(T obj);
        void Delete(object id);
        void Save();
        Task SaveAsync();
    }
}

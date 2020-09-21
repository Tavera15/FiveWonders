using FiveWonders.core.Models;
using System.Linq;

namespace FiveWonders.DataAccess.InMemory
{
    public interface IRepository<T> where T : BaseEntity
    {
        void Commit();
        void Delete(T item);
        T Find(string ID, bool bThrowException = false);
        IQueryable<T> GetCollection();
        void Insert(T newItem);
        void Update(T item);
    }
}
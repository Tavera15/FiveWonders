using FiveWonders.core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.DataAccess.InMemory
{
    public class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
    {
        ObjectCache cache = MemoryCache.Default;
        List<T> items;
        string className;

        public InMemoryRepository()
        {
            className = typeof(T).Name;
            items = cache[className] as List<T>;

            if (items == null)
            {
                items = new List<T>();
            }
        }

        public void Commit()
        {
            cache[className] = items;
        }

        public void Insert(T newItem)
        {
            items.Add(newItem);
        }

        public T Find(string ID, bool bThrowException = false)
        {
            T itemToFind = items.FirstOrDefault<T>(x => x.mID == ID);

            if (bThrowException && itemToFind == null)
                throw new Exception("Item " + ID + " does not exist");

            return itemToFind;
        }

        public void Update(T item)
        {
            T itemToUpdate = items.FirstOrDefault<T>(x => x.mID == item.mID);

            if (itemToUpdate == null)
            {
                throw new Exception("Category " + item.mID + " does not exist");
            }

            itemToUpdate = item;
        }

        public void Delete(T item)
        {
            T itemToDelete = items.FirstOrDefault<T>(x => x.mID == item.mID);

            if (itemToDelete == null)
            {
                throw new Exception("Category does not exist");
            }

            items.Remove(itemToDelete);
        }

        public IQueryable<T> GetCollection()
        {
            return items.AsQueryable();
        }
    }
}

using FiveWonders.core.Models;
using FiveWonders.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.DataAccess.SQL
{
    public class SQLRepository<T> : IRepository<T> where T : BaseEntity
    {
        internal DataContext context;
        internal DbSet<T> dbSet;

        public SQLRepository(DataContext c)
        {
            context = c;
            dbSet = context.Set<T>();
        }

        public void Commit()
        {
            context.SaveChanges();
        }

        public void Delete(T item)
        {
            var t = Find(item.mID);

            if(context.Entry(t).State == EntityState.Deleted)
            {
                dbSet.Attach(t);
            }

            dbSet.Remove(t);
        }

        public T Find(string ID)
        {
            return dbSet.Find(ID);
        }

        public IQueryable<T> GetCollection()
        {
            return dbSet;
        }

        public void Insert(T newItem)
        {
            dbSet.Add(newItem);
        }

        public void Update(T item)
        {
            dbSet.Attach(item);
            context.Entry(item).State = EntityState.Modified;
        }
    }
}

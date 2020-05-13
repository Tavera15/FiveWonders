using FiveWonders.core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace FiveWonders.DataAccess.InMemory
{
    public class CategoryRepository
    {
        ObjectCache cache = MemoryCache.Default;
        List<Category> categories;

        public CategoryRepository()
        {
            categories = cache["category"] as List<Category>;

            if (categories == null)
            {
                categories = new List<Category>();
            }
        }

        public void Commit()
        {
            cache["category"] = categories;
        }

        public void Insert(Category cat)
        {
            categories.Add(cat);
        }

        public Category Find(string ID)
        {
            Category c = categories.FirstOrDefault<Category>(x => x.mID == ID);

            if (c == null)
                throw new Exception("Category " + ID + " does not exist");

            return c;
        }

        public void Update(Category c)
        {
            Category categoryToUpdate = categories.FirstOrDefault<Category>(x => x.mID == c.mID);

            if (categoryToUpdate == null)
            {
                throw new Exception("Category " + c.mID + " does not exist");
            }

            categoryToUpdate = c;
        }

        public void Delete(Category c)
        {
            Category categoryToDelete = categories.FirstOrDefault<Category>(x => x.mID == c.mID);

            if (categoryToDelete == null)
            {
                throw new Exception("Category does not exist");
            }

            categories.Remove(categoryToDelete);
        }

        public IQueryable<Category> GetCollection()
        {
            return categories.AsQueryable();
        }
    }
}

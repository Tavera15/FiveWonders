using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using FiveWonders.core.Models;

namespace FiveWonders.DataAccess.InMemory
{
    public class ProductRepository
    {
        ObjectCache cache = MemoryCache.Default;
        List<Product> products;

        public ProductRepository()
        {
            products = cache["products"] as List<Product>;

            if (products == null)
            {
                products = new List<Product>();
            }
        }

        public void Commit()
        {
            cache["products"] = products;
        }

        public void Insert(Product p)
        {
            products.Add(p);
        }

        public Product Find(string ID)
        {
            Product p = products.FirstOrDefault<Product>(x => x.mID == ID);

            if (p == null)
                throw new Exception("Product " + ID + " does not exist");

            return p;
        }

        public void Update(Product p)
        {
            Product productToUpdate = products.FirstOrDefault<Product>(x => x.mID == p.mID);

            if (productToUpdate == null)
            {
                throw new Exception("Product " + p.mID + " does not exist");
            }

            productToUpdate = p;
        }

        public void Delete(Product p)
        {
            Product productToDelete = products.FirstOrDefault<Product>(x => x.mID == p.mID);

            if (productToDelete == null)
            {
                throw new Exception("Product does not exist");
            }

            products.Remove(productToDelete);
        }

        public IQueryable<Product> GetCollection()
        {
            return products.AsQueryable();
        }
    }
}

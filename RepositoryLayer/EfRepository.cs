using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace RepositoryLayer
{
    public class EfRepository<T> : IEfRepository<T> where T : class
    {
        protected IDbSet<T> ObjectSet;

        public EfRepository(DbContext dbContext)
        {
            ObjectSet = dbContext.Set<T>();
        }
        public IQueryable<T> AsQuerable()
        {
            return ObjectSet.AsQueryable();
        }
        public IEnumerable<T> GetAll(Func<T, bool> predicate = null)
        {
            if (predicate != null)
            {
                return ObjectSet.Where(predicate);
            }
            return ObjectSet.AsEnumerable();
        }
        public T Get(Func<T, bool> predicate)
        {
            return ObjectSet.FirstOrDefault(predicate);
        }
        public void Add(T entity)
        {
            ObjectSet.Add(entity);
        }
        public void Attach(T entity)
        {
            ObjectSet.Attach(entity);
        }
        public void Delete(T entity)
        {
            ObjectSet.Remove(entity);
        }
    }
}

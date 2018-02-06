using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;

namespace RepositoryLayer
{
    public class UnityOfWork : IDisposable
    {
        private readonly DbContext _context;
        public UnityOfWork(DbContext context)
        {
            _context = context;
        }

        public Dictionary<Type, object> Repositories = new Dictionary<Type, object>();

        public IEfRepository<T> Repository<T>() where T : class
        {
            if (Repositories.Keys.Contains(typeof(T)))
            {
                return Repositories[typeof(T)] as IEfRepository<T>;
            }
            IEfRepository<T> repo = new EfRepository<T>(_context);
            Repositories.Add(typeof(T), repo);
            return repo;
        }
  
        public void SaveChanges()
        {

            try
            {
                _context.SaveChanges();
            }
            catch (DbEntityValidationException)
            {
            }

        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}


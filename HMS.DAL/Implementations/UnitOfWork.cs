using HMS.DAL.Contracts;
using HMS.DAL.Models;
using HMS.DAL.Data.DbContexts;
using System.Collections.Concurrent;

namespace HMS.DAL.Implementations
{

    public class UnitOfWork : IUnitOfWork
    {
        private readonly HospitalDbContext _dbContext;
        private readonly ConcurrentDictionary<string, object> _repositories;
        public UnitOfWork(HospitalDbContext dbContext)
        {
            _dbContext = dbContext;
            _repositories = new();
        }
        public IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntity<TKey>
            => (IGenericRepository<TEntity, TKey>)_repositories.GetOrAdd(typeof(TEntity).Name,
                (_) => new GenericRepository<TEntity, TKey>(_dbContext));

        public async Task<int> SaveChangesAsync() => await _dbContext.SaveChangesAsync();

        public async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync()
            => await _dbContext.Database.BeginTransactionAsync();

    }
}

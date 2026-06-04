using HMS.DAL.Contracts;
using HMS.DAL.Models;
using Microsoft.EntityFrameworkCore;
using HMS.DAL.Data.DbContexts;

namespace HMS.DAL.Implementations
{
    public class GenericRepository<TEntity, TKey>(HospitalDbContext _dbContext) 
        : IGenericRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        //private readonly HospitalDbContext _dbContext;

        //public GenericRepository(HospitalDbContext dbContext)
        //{
        //    _dbContext = dbContext;
        //}

        public async Task AddAsync(TEntity entity)
            => await _dbContext.Set<TEntity>().AddAsync(entity);

        public void Delete(TEntity entity)
            => _dbContext.Set<TEntity>().Remove(entity);

        public async Task<IEnumerable<TEntity>> GetAllAsync(bool asNoTracking = false)
            => asNoTracking
                ? await _dbContext.Set<TEntity>().AsNoTracking().ToListAsync()
                : await _dbContext.Set<TEntity>().ToListAsync();

        public async Task<TEntity?> GetByIdAsync(TKey id)
            => await _dbContext.Set<TEntity>().FindAsync(id);

        public void Update(TEntity entity)
            => _dbContext.Set<TEntity>().Update(entity);

        // Specification overloads
        public async Task<IEnumerable<TEntity>> GetAllAsync(ISpecifications<TEntity, TKey> specifications)
            => await SpecificationEvaluator
                .CreateQuery(_dbContext.Set<TEntity>(), specifications)
                .ToListAsync();

        public async Task<TEntity?> GetByIdAsync(ISpecifications<TEntity, TKey> specifications)
            => await SpecificationEvaluator
                .CreateQuery(_dbContext.Set<TEntity>(), specifications)
                .FirstOrDefaultAsync();

        public async Task<int> CountAsync(ISpecifications<TEntity, TKey> specifications)
            => await SpecificationEvaluator
                .CreateQuery(_dbContext.Set<TEntity>(), specifications)
                .CountAsync();
    }
}

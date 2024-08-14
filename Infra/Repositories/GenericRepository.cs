using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly DbContext _dbContext;
        private readonly DbSet<TEntity> _dbSet;

        public GenericRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<TEntity>();
        }

        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task CreateAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(TEntity entity)
        {
            if (entity is ISoftDeletable softDeletableEntity)
            {
                softDeletableEntity.IsActive = false;
                softDeletableEntity.DeletedAt = DateTime.UtcNow;
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            else
            {
                _dbSet.Remove(entity);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}

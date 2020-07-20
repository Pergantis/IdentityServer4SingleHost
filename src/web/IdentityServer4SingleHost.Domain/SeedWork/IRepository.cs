using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4SingleHost.Domain.SeedWork
{
    public interface IRepository<T> where T: IAggregateRoot
    {
        IUnitOfWork UnitOfWork { get; }

        Task<int> FindByIdAsync(int id);

        Task<List<T>> GetAsync(ISpecification<T> spec, bool enableTracking = true);
        
        Task<T> GetByIdAsync(ISpecification<T> spec, bool enableTracking = true);

        Task<int> CountAsync();
        
        Task AddAsync(T entity);
        
        void Update(T entity);
        
        void Delete(T entity);
        
    }
}

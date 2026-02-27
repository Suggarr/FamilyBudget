using System.Linq.Expressions;

namespace FamilyBudget.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task AddAsync(T entity);
        void Delete(T entity);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> ListAsync(Expression<Func<T, bool>> predicate);
        void Update(T entity);
    }
}
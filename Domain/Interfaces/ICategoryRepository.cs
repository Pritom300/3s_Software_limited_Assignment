using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetCategoryWithProductsAsync(int id);
        Task<IEnumerable<Category>> GetAllWithProductCountAsync();
        Task<Category?> GetByNameAsync(string name);
        Task<bool> HasProductsAsync(int categoryId);
    }
}

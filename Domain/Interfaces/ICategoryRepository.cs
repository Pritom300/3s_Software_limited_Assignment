using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetCategoryWithProductsAsync(int id);
        Task<IEnumerable<Category>> GetAllWithProductCountAsync();
    }
}

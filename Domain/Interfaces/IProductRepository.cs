using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);

        // Search with pagination (keyword search across name/description)
        Task<(IEnumerable<Product> Items, int TotalCount)> SearchAsync(string keyword, int page, int limit);

        // Get one product including its Category
        Task<Product?> GetByIdWithCategoryAsync(int id);

        // Filtered + paginated list with total count
        Task<(IEnumerable<Product> Items, int TotalCount)> GetFilteredAsync(
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            int page,
            int limit);
    }
}

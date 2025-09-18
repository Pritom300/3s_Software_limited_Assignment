using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }

        // Get a single category including its products
        public async Task<Category?> GetCategoryWithProductsAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // Get all categories with product count
        public async Task<IEnumerable<Category>> GetAllWithProductCountAsync()
        {
            return await _dbSet
                .Include(c => c.Products)
                .Select(c => new Category
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Products = c.Products // navigation for count
                })
                .ToListAsync();
        }
    }
}

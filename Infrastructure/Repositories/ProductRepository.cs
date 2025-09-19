using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdWithCategoryAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetFilteredAsync(
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            int page,
            int limit)
        {
            if (page <= 0) page = 1;
            if (limit <= 0) limit = 10;

            var query = _dbSet
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (items, total);
        }

        public async Task<(IEnumerable<Product> Items, int TotalCount)> SearchAsync(string keyword, int page, int limit)
        {
            if (page <= 0) page = 1;
            if (limit <= 0) limit = 10;

            var query = _dbSet
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p =>
                    EF.Functions.Like(p.Name, $"%{keyword}%") ||
                    EF.Functions.Like(p.Description, $"%{keyword}%"));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (items, total);
        }
    }
}

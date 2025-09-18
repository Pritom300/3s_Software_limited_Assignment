using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;

namespace Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IProductRepository Products { get; }
        public ICategoryRepository Categories { get; }
        public IUserRepository Users { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Products = new ProductRepository(_context);
            Categories = new CategoryRepository(_context);
            Users = new UserRepository(_context);
        }

        // Commit all changes in a single transaction
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Dispose DbContext 
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

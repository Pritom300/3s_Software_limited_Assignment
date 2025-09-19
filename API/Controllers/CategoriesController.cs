using API.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _uow;

        public CategoriesController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // POST /api/categories
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
        {
            var existing = await _uow.Categories.GetByNameAsync(dto.Name);
            if (existing != null)
                return Conflict(new { message = "Category name already exists!" });

            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };

            await _uow.Categories.AddAsync(category);
            await _uow.CompleteAsync();

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }

        // GET /api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryReadDto>>> GetAll()
        {
            var categories = await _uow.Categories.GetAllAsync();
            var result = categories.Select(c => new CategoryReadDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ProductCount = _uow.Products.GetAllAsync().Result.Count(p => p.CategoryId == c.Id)
            });

            return Ok(result);
        }

        // GET /api/categories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _uow.Categories.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            var response = new CategoryReadDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ProductCount = _uow.Products.GetAllAsync().Result.Count(p => p.CategoryId == id)
            };

            return Ok(response);
        }

        // PUT /api/categories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto)
        {
            var category = await _uow.Categories.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            category.Name = dto.Name;
            category.Description = dto.Description;

            _uow.Categories.Update(category);
            await _uow.CompleteAsync();

            return NoContent();
        }

        // DELETE /api/categories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _uow.Categories.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            if (await _uow.Categories.HasProductsAsync(id))
                return Conflict(new { message = "Cannot delete category with linked products" }); //409

            _uow.Categories.Remove(category);
            await _uow.CompleteAsync();

            return NoContent();
        }
    }
}

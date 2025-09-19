using API.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        public ProductsController(IUnitOfWork uow, IMapper mapper, IWebHostEnvironment env)
        {
            _uow = uow;
            _mapper = mapper;
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductCreateDto dto)
        {
            string? imagePath = null;

            if (dto.Image != null)
            {
                var uploadsFolder = Path.Combine("wwwroot", "images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(dto.Image.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                imagePath = $"/images/{fileName}"; // URL to access the image
            }

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                CategoryId = dto.CategoryId,
                ImageUrl = imagePath
            };
          
            await _uow.Products.AddAsync(product);

            await _uow.CompleteAsync();

            return Ok(product);
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? categoryId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            var (items, total) = await _uow.Products.GetFilteredAsync(categoryId, minPrice, maxPrice, page, limit);
            var dtos = items.Select(p => _mapper.Map<ProductReadDto>(p));

            var result = new
            {
                Data = dtos,
                Page = page,
                Limit = limit,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)limit)
            };

            return Ok(result);
        }



        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _uow.Products.GetByIdWithCategoryAsync(id);
            if (product == null) return NotFound();

            return Ok(_mapper.Map<ProductReadDto>(product));
        }

        // PUT: api/products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto)
        {
            var product = await _uow.Products.GetByIdWithCategoryAsync(id);
            if (product == null) return NotFound();

            // Validate category change
            var category = await _uow.Categories.GetByIdAsync(dto.CategoryId);
            if (category == null)
                return BadRequest(new { message = "Category not found." });

            //remove also image from path during update

            if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                var p = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", product.ImageUrl);
                if (System.IO.File.Exists(p))
                {
                    System.IO.File.Delete(p);
                }
            }

            //remove image end


            //Image upload mechanism 

            string? imagePath = null;

            if (dto.ImageUrl != null)
            {
                var uploadsFolder = Path.Combine("wwwroot", "images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(dto.ImageUrl.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageUrl.CopyToAsync(stream);
                }

                imagePath = $"/images/{fileName}"; // URL to access the image
            }

            //Image upload mechanism end


            // Update fields
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.CategoryId = dto.CategoryId;
            product.ImageUrl = imagePath;

            _uow.Products.Update(product);
            await _uow.CompleteAsync();

            return NoContent();
        }


        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _uow.Products.GetByIdWithCategoryAsync(id);
            if (product == null) return NotFound(new { message = "Did not find the product" }); //404

            //remove also image from path
            
            if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", product.ImageUrl);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            //remove also image end

            _uow.Products.Remove(product);
            await _uow.CompleteAsync();

            return NoContent();
        }


        // GET: api/products/search?q=...
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var (items, total) = await _uow.Products.SearchAsync(q ?? string.Empty, page, limit);
            var dtos = items.Select(p => _mapper.Map<ProductReadDto>(p));

            var result = new
            {
                Data = dtos,
                Page = page,
                Limit = limit,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)limit)
            };

            return Ok(result);
        }



    }
}

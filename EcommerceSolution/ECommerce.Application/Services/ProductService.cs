using ECommerce.Models.DTOs.Product;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed; // Para IDistributedCache (Redis)
using System.Text.Json; // Para serialização/deserialização JSON
using ECommerce.Infrastructure.Backup; // Para IMongoDbBackupService

namespace ECommerce.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache; // Injetar Redis
        private readonly IMongoDbBackupService _mongoDbBackupService; // Injetar MongoDB Backup Service

        public ProductService(ApplicationDbContext context, IDistributedCache cache, IMongoDbBackupService mongoDbBackupService)
        {
            _context = context;
            _cache = cache;
            _mongoDbBackupService = mongoDbBackupService;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(ProductQueryParams queryParams)
        {
            string cacheKey = $"products_{queryParams.CategoryId}_{queryParams.OrderBy}_{queryParams.SearchTerm}";
            string? cachedProductsJson = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedProductsJson))
            {
                return JsonSerializer.Deserialize<List<ProductDto>>(cachedProductsJson)!;
            }

            IQueryable<Product> query = _context.Products.Include(p => p.Category);

            if (queryParams.CategoryId.HasValue && queryParams.CategoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == queryParams.CategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(queryParams.SearchTerm) || p.Description.Contains(queryParams.SearchTerm));
            }

            switch (queryParams.OrderBy)
            {
                case "priceAsc": query = query.OrderBy(p => p.Price); break;
                case "priceDesc": query = query.OrderByDescending(p => p.Price); break;
                case "newest": query = query.OrderByDescending(p => p.CreatedAt); break;
                case "oldest": query = query.OrderBy(p => p.CreatedAt); break;
                default: query = query.OrderByDescending(p => p.CreatedAt); break;
            }

            var products = await query
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();

            // Armazenar no cache
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)); // Cache por 5 minutos
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(products), cacheOptions);

            return products;
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            string cacheKey = $"product_{id}";
            string? cachedProductJson = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedProductJson))
            {
                return JsonSerializer.Deserialize<ProductDto>(cachedProductJson)!;
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return null;

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name
            };

            // Armazenar no cache
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10)); // Cache por 10 minutos
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(productDto), cacheOptions);

            return productDto;
        }

        public async Task<ProductDto> AddProductAsync(ProductDto productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Stock = productDto.Stock,
                ImageUrl = productDto.ImageUrl,
                CategoryId = productDto.CategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Invalida o cache de todos os produtos
            await _cache.RemoveAsync("products_*"); // Use um padrão mais específico se tiver muitos filtros
            await _cache.RemoveAsync($"product_{product.Id}");

            // Backup para MongoDB (exemplo: a cada adição de produto)
            await _mongoDbBackupService.BackupProductsAsync(new[] { product });

            product.Category = await _context.Categories.FindAsync(product.CategoryId);
            productDto.Id = product.Id;
            productDto.CategoryName = product.Category.Name;
            return productDto;
        }

        public async Task UpdateProductAsync(ProductDto productDto)
        {
            var product = await _context.Products.FindAsync(productDto.Id);
            if (product == null) return;

            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Price = productDto.Price;
            product.Stock = productDto.Stock;
            product.ImageUrl = productDto.ImageUrl;
            product.CategoryId = productDto.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Invalida o cache
            await _cache.RemoveAsync("products_*");
            await _cache.RemoveAsync($"product_{product.Id}");

            // Atualizar backup no MongoDB (se você tiver um mecanismo de upsert/replace)
            // Por simplicidade, o BackupProductsAsync acima faria um insertMany.
            // Para updates, você precisaria de um método específico no MongoDbBackupService.
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            // Invalida o cache
            await _cache.RemoveAsync("products_*");
            await _cache.RemoveAsync($"product_{id}");
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
             string cacheKey = "all_categories";
            string? cachedCategoriesJson = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedCategoriesJson))
            {
                return JsonSerializer.Deserialize<List<CategoryDto>>(cachedCategoriesJson)!;
            }

            var categories = await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToListAsync();

            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(1)); // Categorias raramente mudam, cache mais longo
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(categories), cacheOptions);

            return categories;
        }
    }
}
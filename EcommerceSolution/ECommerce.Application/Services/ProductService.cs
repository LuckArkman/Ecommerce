// ECommerce.Application/Services/ProductService.cs
using ECommerce.Models.DTOs.Product;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
// ***** REMOVER ESTES USINGS *****
// using Microsoft.Extensions.Caching.Distributed;
// using System.Text.Json;
// ********************************
using ECommerce.Infrastructure.Backup; // Para IMongoDbBackupService
using System; // Para DateTime (se usado apenas para CreatedAt/UpdatedAt)

namespace ECommerce.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        // ***** REMOVER ESTA LINHA *****
        // private readonly IDistributedCache _cache;
        private readonly IMongoDbBackupService _mongoDbBackupService;
        // Adicionar IFileUploadService aqui
        private readonly IFileUploadService _fileUploadService;


        // ***** CORREÇÃO AQUI: REMOVER IDistributedCache do construtor *****
        public ProductService(ApplicationDbContext context, IMongoDbBackupService mongoDbBackupService, IFileUploadService fileUploadService)
        {
            _context = context;
            // ***** REMOVER ESTA LINHA *****
            // _cache = cache;
            _mongoDbBackupService = mongoDbBackupService;
            _fileUploadService = fileUploadService;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(ProductQueryParams queryParams)
        {
            // ***** REMOVER TODA A LÓGICA DE CACHE AQUI *****
            // string cacheKey = $"products_{queryParams.CategoryId}_{queryParams.OrderBy}_{queryParams.SearchTerm}";
            // string? cachedProductsJson = await _cache.GetStringAsync(cacheKey);
            // if (!string.IsNullOrEmpty(cachedProductsJson)) { return JsonSerializer.Deserialize<List<ProductDto>>(cachedProductsJson)!; }

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

            return await query
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    IsFreeShipping = p.IsFreeShipping,
                    FreeShippingRegionsJson = p.FreeShippingRegionsJson
                })
                .ToListAsync();

            // ***** REMOVER LÓGICA DE CACHE AQUI *****
            // var cacheOptions = new DistributedCacheEntryOptions()
            //     .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            // await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(products), cacheOptions);

            // return products; // Não remova este return, ele deve estar fora da lógica de cache
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            // ***** REMOVER LÓGICA DE CACHE AQUI *****
            // string cacheKey = $"product_{id}";
            // string? cachedProductJson = await _cache.GetStringAsync(cacheKey);
            // if (!string.IsNullOrEmpty(cachedProductJson)) { return JsonSerializer.Deserialize<ProductDto>(cachedProductJson)!; }

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

            // ***** REMOVER LÓGICA DE CACHE AQUI *****
            // var cacheOptions = new DistributedCacheEntryOptions()
            //     .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
            // await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(productDto), cacheOptions);

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
                UpdatedAt = DateTime.UtcNow,
                IsFreeShipping = productDto.IsFreeShipping,
                FreeShippingRegionsJson = productDto.FreeShippingRegionsJson
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            // A linha "return productDto;" estava duplicada ou mal posicionada.
            // Ela precisa ser após a lógica de backup e o preenchimento de Category.

            // Invalida o cache de todos os produtos (REMOVA)
            // await _cache.RemoveAsync("products_*");
            // await _cache.RemoveAsync($"product_{product.Id}");

            // Backup para MongoDB (exemplo: a cada adição de produto)
            await _mongoDbBackupService.BackupProductsAsync(new[] { product });

            // Recuperar a categoria para preencher o DTO de retorno
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
            product.IsFreeShipping = productDto.IsFreeShipping;
            product.FreeShippingRegionsJson = productDto.FreeShippingRegionsJson;

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Invalida o cache (REMOVA)
            // await _cache.RemoveAsync("products_*");
            // await _cache.RemoveAsync($"product_{product.Id}");
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            // Invalida o cache (REMOVA)
            // await _cache.RemoveAsync("products_*");
            // await _cache.RemoveAsync($"product_{id}");
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            // ***** REMOVER LÓGICA DE CACHE AQUI *****
            // string cacheKey = "all_categories";
            // string? cachedCategoriesJson = await _cache.GetStringAsync(cacheKey);
            // if (!string.IsNullOrEmpty(cachedCategoriesJson)) { return JsonSerializer.Deserialize<List<CategoryDto>>(cachedCategoriesJson)!; }

            var categories = await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToListAsync();

            // ***** REMOVER LÓGICA DE CACHE AQUI *****
            // var cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(1));
            // await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(categories), cacheOptions);

            return categories;
        }
    }
}
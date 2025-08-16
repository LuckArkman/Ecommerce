using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;
using ECommerce.Models.DTOs.Product;
using Category =  ECommerce.Domain.Entities.Category;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Description = c.Description })
            .ToListAsync();
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return null;
        return new CategoryDto { Id = category.Id, Name = category.Name, Description = category.Description };
    }

    public async Task<CategoryDto> AddCategoryAsync(CategoryDto categoryDto)
    {
        var _category = new Category { Name = categoryDto.Name, Description = categoryDto.Description };
        _context.Categories.AddAsync(_category);
        await _context.SaveChangesAsync();
        categoryDto.Id = _category.Id;
        return categoryDto;
    }

    public async Task<bool> UpdateCategoryAsync(CategoryDto categoryDto)
    {
        var category = await _context.Categories.FindAsync(categoryDto.Id);
        if (category == null) return false;

        category.Name = categoryDto.Name;
        category.Description = categoryDto.Description;
        _context.Entry(category).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}
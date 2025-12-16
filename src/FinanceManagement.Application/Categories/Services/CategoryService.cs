using FinanceManagement.Application.Categories.DTOs;
using FinanceManagement.Application.Categories.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Models;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Application.Categories.Services;

public class CategoryService : ICategoryService
{
    private readonly IApplicationDbContext _context;

    public CategoryService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CategoryDto>> CreateCategoryAsync(CreateCategoryDto dto, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Aynı isimde kategori var mı kontrol et
            var exists = await _context.Categories
                .AnyAsync(c => c.Name == dto.Name && c.UserId == userId && c.Type == dto.Type, cancellationToken);

            if (exists)
                return Result<CategoryDto>.Failure("Bu isimde bir kategori zaten mevcut");

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Icon = dto.Icon,
                Color = dto.Color,
                Type = dto.Type,
                Description = dto.Description,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync(cancellationToken);

            var categoryDto = MapToDto(category);
            return Result<CategoryDto>.Success(categoryDto);
        }
        catch (Exception ex)
        {
            return Result<CategoryDto>.Failure($"Kategori oluşturulurken hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result<CategoryDto>> GetCategoryByIdAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);

        if (category == null)
            return Result<CategoryDto>.Failure("Kategori bulunamadı");

        var categoryDto = MapToDto(category);
        return Result<CategoryDto>.Success(categoryDto);
    }

    public async Task<Result<List<CategoryDto>>> GetUserCategoriesAsync(string userId, TransactionType? type = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Categories.Where(c => c.UserId == userId);

        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);

        var categories = await query
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);

        var categoryDtos = categories.Select(MapToDto).ToList();
        return Result<List<CategoryDto>>.Success(categoryDtos);
    }

    public async Task<Result<CategoryDto>> UpdateCategoryAsync(UpdateCategoryDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == dto.Id && c.UserId == userId, cancellationToken);

        if (category == null)
            return Result<CategoryDto>.Failure("Kategori bulunamadı");

        // Aynı isimde başka kategori var mı kontrol et
        var exists = await _context.Categories
            .AnyAsync(c => c.Name == dto.Name && c.UserId == userId && c.Type == category.Type && c.Id != dto.Id, cancellationToken);

        if (exists)
            return Result<CategoryDto>.Failure("Bu isimde bir kategori zaten mevcut");

        category.Name = dto.Name;
        category.Icon = dto.Icon;
        category.Color = dto.Color;
        category.Description = dto.Description;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var categoryDto = MapToDto(category);
        return Result<CategoryDto>.Success(categoryDto);
    }

    public async Task<Result> DeleteCategoryAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);

        if (category == null)
            return Result.Failure("Kategori bulunamadı");

        // Kategoriye bağlı işlem var mı kontrol et
        var hasTransactions = await _context.Transactions
            .AnyAsync(t => t.CategoryId == id, cancellationToken);

        if (hasTransactions)
            return Result.Failure("Bu kategoriye ait işlemler bulunduğu için silinemez");

        // Soft delete
        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Icon = category.Icon,
            Color = category.Color,
            Type = category.Type,
            Description = category.Description,
            CreatedAt = category.CreatedAt
        };
    }
}

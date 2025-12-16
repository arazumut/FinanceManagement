using FinanceManagement.Application.Categories.DTOs;
using FinanceManagement.Application.Common.Models;
using FinanceManagement.Domain.Enums;

namespace FinanceManagement.Application.Categories.Interfaces;

public interface ICategoryService
{
    Task<Result<CategoryDto>> CreateCategoryAsync(CreateCategoryDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> GetCategoryByIdAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    Task<Result<List<CategoryDto>>> GetUserCategoriesAsync(string userId, TransactionType? type = null, CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> UpdateCategoryAsync(UpdateCategoryDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result> DeleteCategoryAsync(Guid id, string userId, CancellationToken cancellationToken = default);
}

using FinanceManager.Core.Entities;
using FinanceManager.Infrastructure.Contracts;
using FinanceManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Infrastructure.Services;

public class CategoryService(AppDbContext db)
{
    public async Task<List<CategoryListItem>> GetCategoriesAsync(bool includeInactive = true, CancellationToken ct = default)
    {
        var query = db.Categories.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryListItem
            {
                Id = c.Id,
                Name = c.Name,
                Kind = c.Kind,
                IsActive = c.IsActive,
                SortOrder = c.SortOrder,
                TransactionCount = db.Transactions.Count(t => t.CategoryId == c.Id),
            })
            .ToListAsync(ct);
    }

    public Task<Category?> GetAsync(int id, CancellationToken ct = default)
        => db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<List<Category>> GetActiveAsync(CancellationToken ct = default)
        => db.Categories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

    /// <summary>دستهبندیهای سازگار با نوع تراکنش</summary>
    public Task<List<Category>> GetForTypeAsync(TransactionType type, CancellationToken ct = default)
    {
        var compatible = type == TransactionType.Deposit ? CategoryKind.Deposit : CategoryKind.Withdrawal;
        return db.Categories.AsNoTracking()
            .Where(c => c.IsActive && (c.Kind == CategoryKind.Both || c.Kind == compatible))
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<ServiceResult<int>> CreateAsync(CategoryInput input, CancellationToken ct = default)
    {
        var error = ValidateInput(input);
        if (error is not null)
        {
            return ServiceResult<int>.Fail(ServiceErrorCode.Validation, error);
        }

        var name = input.Name.Trim();
        if (await db.Categories.AnyAsync(c => c.Name == name, ct))
        {
            return ServiceResult<int>.Fail(ServiceErrorCode.Conflict, "دستهبندی با همین نام از قبل وجود دارد.");
        }

        var category = new Category
        {
            Name = name,
            Kind = input.Kind,
            SortOrder = input.SortOrder,
            IsActive = input.IsActive,
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);
        return ServiceResult<int>.Success(category.Id);
    }

    public async Task<ServiceResult> UpdateAsync(int id, CategoryInput input, CancellationToken ct = default)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            return ServiceResult.Fail(ServiceErrorCode.NotFound, "دستهبندی یافت نشد.");
        }

        var error = ValidateInput(input);
        if (error is not null)
        {
            return ServiceResult.Fail(ServiceErrorCode.Validation, error);
        }

        var name = input.Name.Trim();
        if (await db.Categories.AnyAsync(c => c.Id != id && c.Name == name, ct))
        {
            return ServiceResult.Fail(ServiceErrorCode.Conflict, "دستهبندی با همین نام از قبل وجود دارد.");
        }

        if (input.Kind != category.Kind)
        {
            if (input.Kind == CategoryKind.Withdrawal
                && await db.Transactions.AnyAsync(t => t.CategoryId == id && t.Type == TransactionType.Deposit, ct))
            {
                return ServiceResult.Fail(
                    ServiceErrorCode.Validation,
                    "این دستهبندی در تراکنشهای واریز استفاده شده؛ تغییر کاربرد آن به «فقط برداشت» مجاز نیست.");
            }

            if (input.Kind == CategoryKind.Deposit
                && await db.Transactions.AnyAsync(t => t.CategoryId == id && t.Type == TransactionType.Withdrawal, ct))
            {
                return ServiceResult.Fail(
                    ServiceErrorCode.Validation,
                    "این دستهبندی در تراکنشهای برداشت استفاده شده؛ تغییر کاربرد آن به «فقط واریز» مجاز نیست.");
            }
        }

        category.Name = name;
        category.Kind = input.Kind;
        category.SortOrder = input.SortOrder;
        category.IsActive = input.IsActive;

        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> SetActiveAsync(int id, bool active, CancellationToken ct = default)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            return ServiceResult.Fail(ServiceErrorCode.NotFound, "دستهبندی یافت نشد.");
        }

        category.IsActive = active;
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            return ServiceResult.Fail(ServiceErrorCode.NotFound, "دستهبندی یافت نشد.");
        }

        // تراکنشهای مرتبط بدون دستهبندی میشوند (SET NULL) تا سابقه حفظ شود.
        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    private static string? ValidateInput(CategoryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return "نام دستهبندی الزامی است.";
        }

        if (input.Name.Trim().Length > 120)
        {
            return "نام دستهبندی حداکثر ۱۲۰ نویسه است.";
        }

        if (input.SortOrder is < 0 or > 100000)
        {
            return "ترتیب نمایش نامعتبر است.";
        }

        return null;
    }
}

using System.ComponentModel.DataAnnotations;
using FinanceManager.Core.Entities;
using FinanceManager.Infrastructure.Contracts;

namespace FinanceManager.Web.ViewModels;

public class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "نام دستهبندی را وارد کنید.")]
    [StringLength(120, ErrorMessage = "نام دستهبندی حداکثر ۱۲۰ نویسه است.")]
    public string Name { get; set; } = "";

    public CategoryKind Kind { get; set; } = CategoryKind.Both;

    [Range(0, 100000, ErrorMessage = "ترتیب نمایش نامعتبر است.")]
    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}

public class CategoryIndexViewModel
{
    public required List<CategoryListItem> Categories { get; init; }
}

using System.ComponentModel.DataAnnotations;

namespace FinanceManager.Web.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "نام کاربری را وارد کنید.")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "گذرواژه را وارد کنید.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class SetupViewModel
{
    [Required(ErrorMessage = "نام کاربری را وارد کنید.")]
    [StringLength(64, MinimumLength = 3, ErrorMessage = "نام کاربری باید بین ۳ تا ۶۴ نویسه باشد.")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "گذرواژه را وارد کنید.")]
    [StringLength(200, MinimumLength = 8, ErrorMessage = "گذرواژه باید حداقل ۸ نویسه باشد.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "تکرار گذرواژه را وارد کنید.")]
    [Compare(nameof(Password), ErrorMessage = "تکرار گذرواژه با گذرواژه یکسان نیست.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "گذرواژه فعلی را وارد کنید.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = "";

    [Required(ErrorMessage = "گذرواژه جدید را وارد کنید.")]
    [StringLength(200, MinimumLength = 8, ErrorMessage = "گذرواژه جدید باید حداقل ۸ نویسه باشد.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "تکرار گذرواژه جدید را وارد کنید.")]
    [Compare(nameof(NewPassword), ErrorMessage = "تکرار گذرواژه با گذرواژه جدید یکسان نیست.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}

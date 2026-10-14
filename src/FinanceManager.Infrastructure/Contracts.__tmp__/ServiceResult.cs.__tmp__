namespace FinanceManager.Infrastructure.Contracts;

public enum ServiceErrorCode
{
    None = 0,
    NotFound = 1,
    Validation = 2,
    AmountInvalid = 3,
    CardAccountMismatch = 4,
    CurrencyMismatch = 5,
    SameAccountTransfer = 6,
    TransferEntryEdit = 7,
    AccountInactive = 8,
    InUse = 9,
    Conflict = 10,
    IoError = 11,
    AuthFailed = 12,
}

/// <summary>نتیجه عملیات سرویس همراه با کد خطا (برای تستپذیری) و پیام فارسی (برای نمایش)</summary>
public class ServiceResult
{
    public bool Ok { get; protected init; }

    public ServiceErrorCode Code { get; protected init; } = ServiceErrorCode.None;

    public string? Message { get; protected init; }

    public static ServiceResult Success() => new() { Ok = true };

    public static ServiceResult Fail(ServiceErrorCode code, string message) => new() { Ok = false, Code = code, Message = message };
}

public sealed class ServiceResult<T> : ServiceResult
{
    public T? Value { get; private init; }

    public static ServiceResult<T> Success(T value) => new() { Ok = true, Value = value };

    public static new ServiceResult<T> Fail(ServiceErrorCode code, string message) => new() { Ok = false, Code = code, Message = message };
}

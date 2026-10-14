namespace FinanceManager.Core.Entities;

/// <summary>نوع حساب بانکی</summary>
public enum AccountType
{
    Checking = 1, // جاری
    Savings = 2,  // پسانداز
    Deposit = 3,  // سپرده
    Other = 4,    // سایر
}

/// <summary>نوع تراکنش</summary>
public enum TransactionType
{
    Deposit = 1,    // واریز
    Withdrawal = 2, // برداشت
}

/// <summary>کاربرد دستهبندی تراکنش</summary>
public enum CategoryKind
{
    Both = 0,       // هم واریز، هم برداشت
    Withdrawal = 1, // فقط برداشت
    Deposit = 2,    // فقط واریز
}

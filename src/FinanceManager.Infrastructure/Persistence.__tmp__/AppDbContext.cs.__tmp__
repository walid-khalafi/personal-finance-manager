using FinanceManager.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Card> Cards => Set<Card>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<AppSetting> Settings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(e =>
        {
            e.Property(x => x.BankName).HasMaxLength(120).IsRequired();
            e.Property(x => x.Title).HasMaxLength(120).IsRequired();
            e.Property(x => x.AccountNumber).HasMaxLength(64);
            e.Property(x => x.Iban).HasMaxLength(64);
            e.Property(x => x.CurrencyCode).HasMaxLength(8).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.HasIndex(x => x.CurrencyCode);
            e.HasIndex(x => x.IsActive);
        });

        modelBuilder.Entity<Card>(e =>
        {
            e.Property(x => x.Nickname).HasMaxLength(120).IsRequired();
            e.Property(x => x.Last4).HasMaxLength(4).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.HasOne(x => x.Account)
                .WithMany(a => a.Cards)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.AccountId);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Transaction>(e =>
        {
            e.Property(x => x.Description).HasMaxLength(400).IsRequired();
            e.Property(x => x.ReferenceNumber).HasMaxLength(120);
            e.HasOne(x => x.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Card)
                .WithMany()
                .HasForeignKey(x => x.CardId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => x.AccountId);
            e.HasIndex(x => x.TransactionDate);
            e.HasIndex(x => x.CategoryId);
            e.HasIndex(x => x.CardId);
            e.HasIndex(x => x.TransferGroupId);
            e.HasIndex(x => new { x.AccountId, x.TransactionDate });
            e.HasIndex(x => new { x.Type, x.TransactionDate });
            e.ToTable(tb => tb.HasCheckConstraint("CK_Transactions_AmountMinor_Positive", "AmountMinor > 0"));
        });

        modelBuilder.Entity<AppUser>(e =>
        {
            e.Property(x => x.Username).HasMaxLength(64).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
            e.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<AppSetting>(e =>
        {
            e.HasKey(x => x.Key);
            e.Property(x => x.Key).HasMaxLength(100);
            e.Property(x => x.Value).HasMaxLength(400).IsRequired();
        });

        // دستهبندیهای پیشفرض (قابل ویرایش/غیرفعالسازی توسط کاربر)
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "حقوق", Kind = CategoryKind.Deposit, SortOrder = 10, IsActive = true },
            new Category { Id = 2, Name = "غذا", Kind = CategoryKind.Withdrawal, SortOrder = 20, IsActive = true },
            new Category { Id = 3, Name = "حملونقل", Kind = CategoryKind.Withdrawal, SortOrder = 30, IsActive = true },
            new Category { Id = 4, Name = "قبوض", Kind = CategoryKind.Withdrawal, SortOrder = 40, IsActive = true },
            new Category { Id = 5, Name = "خرید", Kind = CategoryKind.Withdrawal, SortOrder = 50, IsActive = true },
            new Category { Id = 6, Name = "سلامت", Kind = CategoryKind.Withdrawal, SortOrder = 60, IsActive = true },
            new Category { Id = 7, Name = "سرگرمی", Kind = CategoryKind.Withdrawal, SortOrder = 70, IsActive = true },
            new Category { Id = 8, Name = "سایر", Kind = CategoryKind.Both, SortOrder = 99, IsActive = true });
    }
}

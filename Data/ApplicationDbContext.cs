using ASM_1.Models;
using ASM_1.Models.Account;
using ASM_1.Models.Food;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASM_1.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<FoodOption> FoodOptions { get; set; }
        public DbSet<OptionType> OptionTypes { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<ComboDetail> ComboDetails { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public DbSet<InvoiceDetailFoodOption> InvoiceDetailFoodOptions { get; set; }
        public DbSet<TableInvoice> TableInvoices { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderItemOption> OrderItemOptions { get; set; }
        public DbSet<OptionGroup> OptionGroups { get; set; }
        public DbSet<OptionValue> OptionValues { get; set; }
        public DbSet<MenuItemOptionGroup> MenuItemOptionGroups { get; set; }
        public DbSet<MenuItemOptionValue> MenuItemOptionValues { get; set; }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ===============================
            // 1️⃣ Category ↔ FoodItem (1-n)
            // ===============================
            builder.Entity<FoodItem>()
                .HasOne(f => f.Category)
                .WithMany(c => c.FoodItems)
                .HasForeignKey(f => f.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===============================
            // 2️⃣ FoodItem ↔ FoodOption (1-n)
            // ===============================
            builder.Entity<FoodOption>()
                .HasOne(o => o.FoodItem)
                .WithMany(f => f.FoodOptions)
                .HasForeignKey(o => o.FoodItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===============================
            // 3️⃣ FoodItem ↔ ComboDetail (1-n)
            // ===============================
            builder.Entity<ComboDetail>()
                .HasOne(cd => cd.FoodItem)
                .WithMany(f => f.ComboDetails)
                .HasForeignKey(cd => cd.FoodItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===============================
            // 4️⃣ Table ↔ TableInvoice ↔ Invoice (n-n)
            // ===============================
            builder.Entity<TableInvoice>()
                .HasOne(ti => ti.Table)
                .WithMany(t => t.TableInvoices)
                .HasForeignKey(ti => ti.TableId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TableInvoice>()
                .HasOne(ti => ti.Invoice)
                .WithMany(i => i.TableInvoices)
                .HasForeignKey(ti => ti.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===============================
            // 5️⃣ (Optional) Default values / constraints
            // ===============================
            builder.Entity<FoodItem>()
                .Property(f => f.IsAvailable)
                .HasDefaultValue(true);

            builder.Entity<FoodOption>()
                .Property(o => o.IsAvailable)
                .HasDefaultValue(true);

            // ===============================
            // 6️⃣ InvoiceDetail ↔ InvoiceDetailFoodOption ↔ FoodOption (n-n)
            // ===============================

            builder.Entity<InvoiceDetailFoodOption>()
                .HasOne(idfo => idfo.InvoiceDetail)
                .WithMany(id => id.InvoiceDetailFoodOptions)
                .HasForeignKey(idfo => idfo.InvoiceDetailId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InvoiceDetailFoodOption>()
                .HasOne(idfo => idfo.FoodOption)
                .WithMany(fo => fo.InvoiceDetailFoodOptions)
                .HasForeignKey(idfo => idfo.FoodOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===============================
            // 7️⃣ FoodOption ↔ OptionType (1-n)
            // ===============================
            builder.Entity<FoodOption>()
                .HasOne(o => o.OptionType)
                .WithMany(ot => ot.FoodOptions)
                .HasForeignKey(o => o.OptionTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ======================================================================
            // 8) OptionGroup ↔ OptionValue (1-n) + enum to string + defaults
            // ======================================================================
            builder.Entity<OptionGroup>()
                .Property(g => g.GroupType)
                .HasConversion<string>()        // lưu enum thành string
                .HasMaxLength(20);

            builder.Entity<OptionGroup>()
                .Property(g => g.IsActive).HasDefaultValue(true);
            builder.Entity<OptionGroup>()
                .Property(g => g.IsArchived).HasDefaultValue(false);
            builder.Entity<OptionGroup>()
                .Property(g => g.Version).HasDefaultValue(1);

            builder.Entity<OptionGroup>()
                .HasMany(g => g.Values)
                .WithOne(v => v.OptionGroup)
                .HasForeignKey(v => v.OptionGroupId)
                .OnDelete(DeleteBehavior.Restrict); // tránh xóa dây chuyền gây mất dữ liệu

            builder.Entity<OptionValue>()
                .HasIndex(v => new { v.OptionGroupId, v.Name }).IsUnique(); // tên không trùng trong nhóm
            builder.Entity<OptionValue>()
                .HasIndex(v => new { v.OptionGroupId, v.SortOrder });

            // ======================================================================
            // 9) MenuItemOptionGroup (item ↔ group) + unique
            // ======================================================================
            builder.Entity<MenuItemOptionGroup>()
                .HasIndex(x => new { x.FoodItemId, x.OptionGroupId })
                .IsUnique();

            builder.Entity<MenuItemOptionGroup>()
                .HasOne(x => x.FoodItem)
                .WithMany() // không bắt buộc nav ngược trong FoodItem
                .HasForeignKey(x => x.FoodItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MenuItemOptionGroup>()
                .HasOne(x => x.OptionGroup)
                .WithMany(g => g.MenuItemLinks)
                .HasForeignKey(x => x.OptionGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // ======================================================================
            // 10) MenuItemOptionValue (override value theo món) + unique
            // ======================================================================
            builder.Entity<MenuItemOptionValue>()
                .HasIndex(x => new { x.FoodItemId, x.OptionValueId })
                .IsUnique();

            builder.Entity<MenuItemOptionValue>()
                .HasOne(x => x.FoodItem)
                .WithMany()
                .HasForeignKey(x => x.FoodItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MenuItemOptionValue>()
                .HasOne(x => x.OptionValue)
                .WithMany()
                .HasForeignKey(x => x.OptionValueId)
                .OnDelete(DeleteBehavior.Restrict);

            // ======================================================================
            // 11) OrderItem
            // ======================================================================
            builder.Entity<OrderItem>()
                .HasOne(x => x.Invoice)
                .WithMany()                                // không cần nav ngược trên Invoice
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .HasOne(x => x.FoodItem)
                .WithMany()
                .HasForeignKey(x => x.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // ======================================================================
            // 12) OrderItemOption
            // ======================================================================
            builder.Entity<OrderItemOption>()
                .HasIndex(x => x.OrderItemId);

            builder.Entity<OrderItemOption>()
                .HasOne(x => x.OrderItem)
                .WithMany(i => i.Options)
                .HasForeignKey(x => x.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // == Nếu bạn chọn phương án A (có navigation) ==
            builder.Entity<OrderItemOption>()
                .HasOne(x => x.OptionValue)
                .WithMany()
                .HasForeignKey(x => x.OptionValueId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<OrderItemOption>()
                .HasOne(x => x.OptionGroup)
                .WithMany()
                .HasForeignKey(x => x.OptionGroupId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

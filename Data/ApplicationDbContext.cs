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

        // ==============================
        // DbSet khai báo bảng
        // ==============================
        public DbSet<Category> Categories { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<FoodOption> FoodOptions { get; set; }
        public DbSet<OptionType> OptionTypes { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<ComboDetail> ComboDetails { get; set; }
        public DbSet<Discount> Discounts { get; set; }

        public DbSet<Table> Tables { get; set; }
        public DbSet<TableInvoice> TableInvoices { get; set; }

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public DbSet<InvoiceDetailFoodOption> InvoiceDetailFoodOptions { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderItemOption> OrderItemOptions { get; set; }

        public DbSet<OptionGroup> OptionGroups { get; set; }
        public DbSet<OptionValue> OptionValues { get; set; }
        public DbSet<MenuItemOptionGroup> MenuItemOptionGroups { get; set; }
        public DbSet<MenuItemOptionValue> MenuItemOptionValues { get; set; }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<CartItemOption> CartItemOptions { get; set; }

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
                .OnDelete(DeleteBehavior.Restrict);

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
            // 5️⃣ Default values
            // ===============================
            builder.Entity<FoodItem>()
                .Property(f => f.IsAvailable)
                .HasDefaultValue(true);

            builder.Entity<FoodOption>()
                .Property(o => o.IsAvailable)
                .HasDefaultValue(true);

            // ===============================
            // 6️⃣ Invoice ↔ Order (1-n)
            // ===============================
            builder.Entity<Order>()
                .HasOne(o => o.Invoice)
                .WithMany(i => i.Orders)
                .HasForeignKey(o => o.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===============================
            // 7️⃣ Order ↔ OrderItem (1-n)
            // ===============================
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===============================
            // 8️⃣ OrderItem ↔ FoodItem (1-n)
            // ===============================
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.FoodItem)
                .WithMany()
                .HasForeignKey(oi => oi.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===============================
            // 9️⃣ OrderItem ↔ OrderItemOption (1-n)
            // ===============================
            builder.Entity<OrderItemOption>()
                .HasOne(oio => oio.OrderItem)
                .WithMany(oi => oi.Options)
                .HasForeignKey(oio => oio.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItemOption>()
                .HasOne(oio => oio.OptionGroup)
                .WithMany()
                .HasForeignKey(oio => oio.OptionGroupId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<OrderItemOption>()
                .HasOne(oio => oio.OptionValue)
                .WithMany()
                .HasForeignKey(oio => oio.OptionValueId)
                .OnDelete(DeleteBehavior.SetNull);

            // ===============================
            // 🔟 InvoiceDetail ↔ FoodOption (n-n)
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
            // 11️⃣ OptionGroup ↔ OptionValue (1-n)
            // ===============================
            builder.Entity<OptionGroup>()
                .Property(g => g.GroupType)
                .HasConversion<string>()
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
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OptionValue>()
                .HasIndex(v => new { v.OptionGroupId, v.Name }).IsUnique();
            builder.Entity<OptionValue>()
                .HasIndex(v => new { v.OptionGroupId, v.SortOrder });

            // ===============================
            // 12️⃣ MenuItemOptionGroup / Value
            // ===============================
            builder.Entity<MenuItemOptionGroup>()
                .HasIndex(x => new { x.FoodItemId, x.OptionGroupId })
                .IsUnique();

            builder.Entity<MenuItemOptionGroup>()
                .HasOne(x => x.FoodItem)
                .WithMany()
                .HasForeignKey(x => x.FoodItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MenuItemOptionGroup>()
                .HasOne(x => x.OptionGroup)
                .WithMany(g => g.MenuItemLinks)
                .HasForeignKey(x => x.OptionGroupId)
                .OnDelete(DeleteBehavior.Restrict);

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

            // ===============================
            // 13️⃣ Cart ↔ CartItem (1-n)
            // ===============================
            builder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartID)
                .OnDelete(DeleteBehavior.Cascade);

            // ===============================
            // 14️⃣ CartItem ↔ CartItemOption (1-n)
            // ===============================
            builder.Entity<CartItemOption>()
                .HasKey(cio => cio.CartItemOptionID);

            builder.Entity<CartItemOption>()
                .HasOne<CartItem>()
                .WithMany(ci => ci.Options)
                .HasForeignKey(cio => cio.CartItemID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

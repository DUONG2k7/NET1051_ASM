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

        }
    }
}

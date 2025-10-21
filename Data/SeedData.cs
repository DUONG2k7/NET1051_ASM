using ASM_1.Models.Account;
using ASM_1.Models.Food;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASM_1.Data
{
    public class SeedData
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // --- 1. Tạo role ---
            string[] roles = { "Admin", "AccountAdmin", "FoodAdmin" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new AppRole { Name = roleName });
                }
            }

            // --- 2. Tạo user trực tiếp ---
            var adminEmail = "admin@demo.com";
            var adminPassword = "Admin@123";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(adminUser, adminPassword);
            }

            // --- 3. Gán role ---
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // --- Tạo thêm user bình thường ---
            var AccAdminEmail = "AccAdmin@demo.com";
            var AccAdminPassword = "AccAdmin@123";

            var AccAdminUser = await userManager.FindByEmailAsync(AccAdminEmail);
            if (AccAdminUser == null)
            {
                AccAdminUser = new AppUser
                {
                    UserName = AccAdminEmail,
                    Email = AccAdminEmail,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(AccAdminUser, AccAdminPassword);
            }

            if (!await userManager.IsInRoleAsync(AccAdminUser, "AccountAdmin"))
            {
                await userManager.AddToRoleAsync(AccAdminUser, "AccountAdmin");
            }

            // =======================
            // 2. Seed Category
            // =======================
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Nước" },
                    new Category { Name = "Cơm" },
                    new Category { Name = "Tráng miệng" }
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // ==========================
            // 3️⃣ Seed OptionType
            // ==========================
            if (!context.OptionTypes.Any())
            {
                var types = new List<OptionType>
                {
                    new OptionType { TypeName = "Size", Description = "Kích thước món ăn" },
                    new OptionType { TypeName = "SugarLevel", Description = "Mức đường cho nước" },
                    new OptionType { TypeName = "Topping", Description = "Nguyên liệu thêm" }
                };
                context.OptionTypes.AddRange(types);
                await context.SaveChangesAsync();
            }

            var optionTypes = await context.OptionTypes.ToListAsync();
            var sizeType = optionTypes.First(t => t.TypeName == "Size");
            var sugarType = optionTypes.First(t => t.TypeName == "SugarLevel");

            // ==========================
            // 4️⃣ Seed FoodItem + FoodOption
            // ==========================
            if (!context.FoodItems.Any())
            {
                var categories = await context.Categories.ToListAsync();

                var foodItems = new List<FoodItem>
                {
                    new FoodItem { Name = "Coca Cola", BasePrice = 10000, CategoryId = categories.First(c => c.Name == "Nước").CategoryId, Slug = "coca-cola", StockQuantity = 50, ImageUrl = "~/Images/nuoc-cam.jpg" },
                    new FoodItem { Name = "Pepsi", BasePrice = 10000, CategoryId = categories.First(c => c.Name == "Nước").CategoryId, Slug = "pepsi", StockQuantity = 50, ImageUrl = "~/Images/nuoc-cam.jpg"  },
                    new FoodItem { Name = "Cơm Gà", BasePrice = 50000, CategoryId = categories.First(c => c.Name == "Cơm").CategoryId, Slug = "com-ga", StockQuantity = 30, ImageUrl = "~/Images/nuoc-cam.jpg"  },
                    new FoodItem { Name = "Cơm Tấm", BasePrice = 45000, CategoryId = categories.First(c => c.Name == "Cơm").CategoryId, Slug = "com-tam", StockQuantity = 30, ImageUrl = "~/Images/nuoc-cam.jpg"  },
                    new FoodItem { Name = "Bánh Flan", BasePrice = 20000, CategoryId = categories.First(c => c.Name == "Tráng miệng").CategoryId, Slug = "banh-flan", StockQuantity = 20, ImageUrl = "~/Images/nuoc-cam.jpg"  },
                    new FoodItem { Name = "Chè Thái", BasePrice = 25000, CategoryId = categories.First(c => c.Name == "Tráng miệng").CategoryId, Slug = "che-thai", StockQuantity = 20, ImageUrl = "~/Images/nuoc-cam.jpg"  }
                };
                context.FoodItems.AddRange(foodItems);
                await context.SaveChangesAsync();

                // Tạo FoodOption mặc định dựa vào OptionType
                foreach (var item in foodItems)
                {
                    // Size option chỉ cho đồ uống và cơm
                    if (item.Category!.Name == "Nước" || item.Category.Name == "Cơm")
                    {
                        int halfStock = item.StockQuantity / 2;
                        context.FoodOptions.Add(new FoodOption
                        {
                            FoodItemId = item.FoodItemId,
                            OptionTypeId = sizeType.OptionTypeId,
                            OptionName = "Size M",
                            ExtraPrice = 0,
                            StockQuantity = halfStock
                        });
                        context.FoodOptions.Add(new FoodOption
                        {
                            FoodItemId = item.FoodItemId,
                            OptionTypeId = sizeType.OptionTypeId,
                            OptionName = "Size L",
                            ExtraPrice = 5000,
                            StockQuantity = item.StockQuantity - halfStock
                        });
                    }

                    // SugarLevel chỉ cho đồ uống
                    if (item.Category.Name == "Nước")
                    {
                        context.FoodOptions.AddRange(new List<FoodOption>
                        {
                            new FoodOption { FoodItemId = item.FoodItemId, OptionTypeId = sugarType.OptionTypeId, OptionName = "0%", ExtraPrice = 0, StockQuantity = item.StockQuantity },
                            new FoodOption { FoodItemId = item.FoodItemId, OptionTypeId = sugarType.OptionTypeId, OptionName = "50%", ExtraPrice = 0, StockQuantity = item.StockQuantity },
                            new FoodOption { FoodItemId = item.FoodItemId, OptionTypeId = sugarType.OptionTypeId, OptionName = "100%", ExtraPrice = 0, StockQuantity = item.StockQuantity }
                        });
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }
}

using ASM_1.Data;
using ASM_1.Models.Account;
using ASM_1.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, AppRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ASM_1.Services.ITableTrackerService, ASM_1.Services.TableTrackerService>();
builder.Services.AddSingleton<ASM_1.Services.TableCodeService>();
builder.Services.AddScoped<UserSessionService>();

builder.Services.AddDistributedMemoryCache(); // dùng bộ nhớ trong server để lưu session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(45); // thời gian hết hạn session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/admin/account/login";
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<SlugGenerator>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.SeedAsync(services);
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "foodHome",
    pattern: "{tableCode:regex(^((?!admin$).)*$)}/home",
    defaults: new { controller = "Food", action = "Index" });

app.MapControllerRoute(
    name: "foodDetails",
    pattern: "{tableCode:regex(^((?!admin$).)*$)}/food/{slug}",
    defaults: new { controller = "Food", action = "Detail" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}/{id?}",
    defaults: new { area = "Admin", controller = "Admin", action = "Index" });


app.Run();

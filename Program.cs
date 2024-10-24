using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using tennis.Areas.Identity.Data;
using tennis.Data;
using tennis.Helpers;
using tennis.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("tennisContextConnection") ?? throw new InvalidOperationException("Connection string 'tennisContextConnection' not found.");

builder.Services.AddDbContext<tennisContext>(options => options.UseSqlite(connectionString));

builder.Services.AddDefaultIdentity<tennisUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<tennisContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Register email services
builder.Services.AddSingleton<EmailHelper>();
builder.Services.AddTransient<IEmailService, EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Initialize roles and admin account
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<tennisUser>>();

    var roles = new[] { "Admin", "Member", "Coach" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminEmail = "admin@dev.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new tennisUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(adminUser, "adminTest@Dev123");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

app.Run();

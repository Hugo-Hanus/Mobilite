using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

builder.Services.AddDbContext<InstallationDbContext>(options=>
{
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")));
});
builder.Services.AddMvc(options=>options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<InstallationDbContext>().AddRoles<IdentityRole>();
builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

    options.LoginPath = "/Home/Connexion";
    options.AccessDeniedPath = "/Home/ErrorPage";
    options.SlidingExpiration = true;

    // Set the cookie to be a session cookie
    options.Cookie.IsEssential = true;
});

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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        if (!roleManager.RoleExistsAsync("Admin").Result)
        {
            roleManager.CreateAsync(new IdentityRole("Admin")).Wait();
            roleManager.CreateAsync(new IdentityRole("Client")).Wait();
            roleManager.CreateAsync(new IdentityRole("Dispatcher")).Wait();
            roleManager.CreateAsync(new IdentityRole("Chauffeur")).Wait();


        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating roles");
    }
}

app.Run();



using Microsoft.AspNetCore.Identity;
// original part //////////////////////////////////////////////////////////

//var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
//{
//    // Password settings
//    options.Password.RequireDigit = true;
//    options.Password.RequiredLength = 6;
//    options.Password.RequireNonAlphanumeric = false;
//    options.Password.RequireUppercase = true;
//    options.Password.RequireLowercase = true;

//    // Lockout settings
//    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
//    options.Lockout.MaxFailedAccessAttempts = 5;
//    options.Lockout.AllowedForNewUsers = true;

//    // User settings
//    options.User.RequireUniqueEmail = true;

//    // Sign-in settings
//    options.SignIn.RequireConfirmedEmail = false;
//    options.SignIn.RequireConfirmedPhoneNumber = false;
//})
//.AddEntityFrameworkStores<ApplicationDbContext>() // This adds the stores
//.AddDefaultTokenProviders();

//// Configure cookie settings
//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.Cookie.HttpOnly = true;
//    options.ExpireTimeSpan = TimeSpan.FromDays(30);
//    options.LoginPath = "/Account/Login";
//    options.LogoutPath = "/Account/Logout";
//    options.AccessDeniedPath = "/Account/AccessDenied";
//    options.SlidingExpiration = true;
//});


////var app = builder.Build();

//// Add services to the container.
//builder.Services.AddControllersWithViews();
//builder.Services.AddDbContext<UserContext>(Options => Options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));


//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}



////// Configure cookie settings
////builder.Services.ConfigureApplicationCookie(options =>
////{
////    options.Cookie.HttpOnly = true;
////    options.ExpireTimeSpan = TimeSpan.FromDays(30);
////    options.LoginPath = "/Account/Login";
////    options.LogoutPath = "/Account/Logout";
////    options.AccessDeniedPath = "/Account/AccessDenied";
////    options.SlidingExpiration = true;
////});


//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//app.UseAuthorization();

//app.MapControllerRoute(

//    name: "default",
//    pattern: "{controller=Home}/{action=Index}");

//app.Run();

// original part //////////////////////////////////////////////////////////

//## ##
//

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Pomelo.EntityFrameworkCore.MySql;
using VehicleCompany.Contexts;
using VehicleCompany.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure MySQL with UserContext
builder.Services.AddDbContext<UserContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// Configure Identity with MySQL stores
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<UserContext>() // Using UserContext instead of ApplicationDbContext
.AddDefaultTokenProviders();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ViewUsers", policy =>
        policy.RequireClaim("Permission", "users.view"));
    options.AddPolicy("EditUsers", policy =>
        policy.RequireClaim("Permission", "users.edit"));
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});


builder.Services.AddScoped<IAuthService, AuthService>();

// Add email sender
//builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Ensure database is created
        var context = services.GetRequiredService<UserContext>();
        await context.Database.EnsureCreatedAsync();

        // Seed roles and admin user
        //await DbInitializer.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
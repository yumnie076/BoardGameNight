using BoardGameNight.Application.Interfaces;
using BoardGameNight.Application.Services;
using BoardGameNight.Domain.Interfaces;
using BoardGameNight.Domain.Services;
using BoardGameNight.Infrastructure.Data;
using BoardGameNight.Infrastructure.Identity;
using BoardGameNight.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Application DbContext (main database)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("BoardGameNight.Infrastructure")));

// Configure Identity DbContext (separate security database)
builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("IdentityConnection"),
        b => b.MigrationsAssembly("BoardGameNight.Infrastructure")));

// Configure Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationIdentityDbContext>();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Register Repositories (Dependency Injection)
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IGameNightRepository, GameNightRepository>();
builder.Services.AddScoped<IBoardGameRepository, BoardGameRepository>();
builder.Services.AddScoped<IFoodItemRepository, FoodItemRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

// Register Domain Services
builder.Services.AddScoped<GameNightDomainService>();

// Register Application Services
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IGameNightService, GameNightService>();
builder.Services.AddScoped<IBoardGameService, BoardGameService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.MapRazorPages(); // For Identity UI

// Seed the databases
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    // Migrate and seed Application database
    var appContext = services.GetRequiredService<ApplicationDbContext>();
    await appContext.Database.MigrateAsync();
    await DataSeeder.SeedAsync(appContext);
    
    // Migrate Identity database
    var identityContext = services.GetRequiredService<ApplicationIdentityDbContext>();
    await identityContext.Database.MigrateAsync();
    
    // Seed Identity users (links to Person entities)
    await BoardGameNight.Infrastructure.Identity.IdentitySeeder.SeedAsync(services, appContext);
}

app.Run();

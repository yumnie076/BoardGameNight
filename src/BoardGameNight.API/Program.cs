using BoardGameNight.Application.Interfaces;
using BoardGameNight.Application.Services;
using BoardGameNight.Domain.Interfaces;
using BoardGameNight.Domain.Services;
using BoardGameNight.Infrastructure.Data;
using BoardGameNight.Infrastructure.Identity;
using BoardGameNight.Infrastructure.Repositories;
using BoardGameNight.API.GraphQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "BoardGameNight API", 
        Version = "v1",
        Description = "REST API (RMM Level 2) en GraphQL endpoint voor de BoardGameNight applicatie"
    });
    
    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header. Format: Bearer {token}"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Database (separate for security)
builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationIdentityDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "DefaultSecretKeyForDevelopment12345!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "BoardGameNight";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "BoardGameNight";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Repositories
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IGameNightRepository, GameNightRepository>();
builder.Services.AddScoped<IBoardGameRepository, BoardGameRepository>();
builder.Services.AddScoped<IFoodItemRepository, FoodItemRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

// Domain Services
builder.Services.AddScoped<GameNightDomainService>();

// Application Services
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IGameNightService, GameNightService>();
builder.Services.AddScoped<IBoardGameService, BoardGameService>();

// GraphQL
builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();
    

var app = builder.Build();

// Swagger always enabled for API documentation
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BoardGameNight API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGraphQL();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var appContext = services.GetRequiredService<ApplicationDbContext>();
    await appContext.Database.MigrateAsync();
    await DataSeeder.SeedAsync(appContext);
    
    var identityContext = services.GetRequiredService<ApplicationIdentityDbContext>();
    await identityContext.Database.MigrateAsync();
    await IdentitySeeder.SeedAsync(services, appContext);
}

app.Run();

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Services;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// Configure URLs
builder.WebHost.UseUrls("http://0.0.0.0:5002");

// Configure EPPlus license and disable GDI+ completely on macOS
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// Disable System.Drawing.Common completely on macOS to avoid GDI+ issues
if (OperatingSystem.IsMacOS())
{
    Environment.SetEnvironmentVariable("DOTNET_SYSTEM_DRAWING_COMMON_SKIP_GDIP_INIT", "1");
    Environment.SetEnvironmentVariable("System.Drawing.EnableUnixSupport", "false");
    AppContext.SetSwitch("System.Drawing.EnableUnixSupport", false);
    
    // Additional EPPlus macOS configuration
    Environment.SetEnvironmentVariable("DOTNET_SYSTEM_DRAWING_COMMON_SKIP_GDIP_INIT", "1");
    Environment.SetEnvironmentVariable("DOTNET_SYSTEM_DRAWING_COMMON_SKIP_GDIP_INIT_LEGACY", "1");
    Environment.SetEnvironmentVariable("DOTNET_SYSTEM_DRAWING_COMMON_SKIP_GDIP", "1");
}

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Add authorization
builder.Services.AddAuthorization();

// Register services
builder.Services.AddScoped<IVisitorService, VisitorService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IResidentService, ResidentService>();
builder.Services.AddScoped<IMailSettingsService, MailSettingsService>();
builder.Services.AddScoped<ISmsVerificationService, SmsVerificationService>();

var app = builder.Build();

// Kestrel server options (removed duplicate URL config - using WebHost.UseUrls above)

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

// app.UseHttpsRedirection(); // Disabled for development
app.UseCors("AllowAll");
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Default route
app.MapFallbackToFile("index.html");

// Ensure database is created and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    
    // Create default admin user if not exists
    if (!context.Users.Any(u => u.Username == "admin"))
    {
        var adminUser = new VisitorManagementSystem.Models.User
        {
            Username = "admin",
            FullName = "System Administrator",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(adminUser);
        context.SaveChanges();
    }
}

app.Run();

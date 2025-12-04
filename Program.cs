using DripCube.Data;
using Microsoft.EntityFrameworkCore;
using DripCube.Helpers;
using DripCube.Services;
using Stripe;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Configure port from environment variable for Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// PostgreSQL connection string (hardcoded)
var connectionString = "Host=dpg-d4g6db8dl3ps73da19pg-a.oregon-postgres.render.com;Database=abcn;Username=user;Password=NBWohmR0QCiyPLqFd2uGR2HWMNvm9GnA;SslMode=Require;Trust Server Certificate=true;";

Console.WriteLine($"Starting application on port {port}...");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHostedService<DripCube.Services.ChatCleanupService>();
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<PhotoService>();

StripeConfiguration.ApiKey = "sk_test_51SXRvaRq3TM6Cq5tdg8kEErnMzCdzZ69B0YyCTX3FAU9UDRFWzd4HE1GnKGFSpqhbkO79iy89LKABUX1dixt1Cm700ksXoe0YG";

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        Console.WriteLine("Applying database migrations...");
        context.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration warning: {ex.Message}");
    }

    if (!context.Employees.Any())
    {
        var admin = new DripCube.Entities.Employee
        {
            Role = DripCube.Entities.EmployeeRole.Admin,
            Login = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            PersonalId = "ADMIN001",
            ChatId = "ADMINCHAT",
            IsActive = true,
            FirstName = "Big",
            LastName = "Boss"
        };
        context.Employees.Add(admin);
        context.SaveChanges();
        Console.WriteLine("--- ADMIN CREATED: Login: admin / Pass: admin123 ---");
    }
}

// Enable Swagger in all environments for Render
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine($"Application started successfully on port {port}!");
app.Run();
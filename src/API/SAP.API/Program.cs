using Marten;
using SAP.API.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Marten with PostgreSQL
builder.Services.AddMarten(options =>
{
    // Connection string - will use environment variable or appsettings
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Host=localhost;Database=sapclone;Username=sapuser;Password=sappassword";
    
    options.Connection(connectionString);
    
    // Configure document mappings
    options.Schema.For<Employee>();
});

// Add CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://sap-clone-frontend-20250625.s3-website.ap-south-1.amazonaws.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

// Keep the original hello world endpoint
app.MapGet("/", () => "SAP Clone API is running!");

// Health check endpoint
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

app.Run();

using Marten;
using SAP.API.Models;
using SAP.Infrastructure.Data;
using Wolverine;
using SAP.Core.Application.Financial.Commands;
using SAP.Core.Application.Financial.Queries;
using SAP.Core.Application.Financial.Services;
using SAP.Core.Application.Financial.EventHandlers;
using SAP.Core.Domain.Financial.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() 
    { 
        Title = "SAP Clone Financial Management API", 
        Version = "v1",
        Description = "REST API for SAP Clone Financial Management operations including Chart of Accounts and Journal Entries"
    });
});

// Add Infrastructure Data Services (includes Marten configuration)
builder.Services.AddInfrastructureData(builder.Configuration);

// Add Wolverine for CQRS messaging
builder.Host.UseWolverine(opts =>
{
    // Auto-discover handlers in Application layer
    opts.Discovery.IncludeAssembly(typeof(CreateAccountHandler).Assembly);
    
    // Configure logging
    opts.Services.AddLogging();
});

// Register Application Services
builder.Services.AddScoped<IChartOfAccountsService, ChartOfAccountsService>();

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

// Make Program class accessible for testing
public partial class Program { }

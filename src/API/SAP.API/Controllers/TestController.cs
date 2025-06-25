using Microsoft.AspNetCore.Mvc;

namespace SAP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var testData = new
        {
            Message = "Hello from SAP Clone API! 🚀",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
            Status = "Connected Successfully",
            Data = new[]
            {
                new { Id = 1, Module = "Financial Management", Status = "Active", Users = 150 },
                new { Id = 2, Module = "Human Resources", Status = "Active", Users = 89 },
                new { Id = 3, Module = "Supply Chain", Status = "Development", Users = 25 },
                new { Id = 4, Module = "Sales & Distribution", Status = "Planning", Users = 0 }
            }
        };

        return Ok(testData);
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { 
            Status = "Healthy", 
            Timestamp = DateTime.UtcNow,
            Uptime = Environment.TickCount64 
        });
    }

    [HttpGet("modules")]
    public IActionResult GetModules()
    {
        var modules = new[]
        {
            new { 
                Id = "FI", 
                Name = "Financial Management", 
                Description = "Manage accounting, budgeting, and financial reporting",
                Icon = "💰",
                Progress = 75
            },
            new { 
                Id = "HR", 
                Name = "Human Resources", 
                Description = "Employee management, payroll, and HR analytics",
                Icon = "👥",
                Progress = 60
            },
            new { 
                Id = "SCM", 
                Name = "Supply Chain Management", 
                Description = "Inventory, procurement, and logistics management",
                Icon = "📦",
                Progress = 30
            },
            new { 
                Id = "SD", 
                Name = "Sales & Distribution", 
                Description = "Customer management and order processing",
                Icon = "📊",
                Progress = 15
            }
        };

        return Ok(modules);
    }
} 
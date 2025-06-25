using Marten;
using Microsoft.AspNetCore.Mvc;
using SAP.API.Models;

namespace SAP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IDocumentSession _session;

    public EmployeesController(IDocumentSession session)
    {
        _session = session;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
    {
        var employees = await _session.Query<Employee>().ToListAsync();
        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> GetEmployee(int id)
    {
        var employee = await _session.LoadAsync<Employee>(id);
        if (employee == null)
        {
            return NotFound();
        }
        return Ok(employee);
    }

    [HttpPost]
    public async Task<ActionResult<Employee>> CreateEmployee(Employee employee)
    {
        _session.Store(employee);
        await _session.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
    }

    [HttpPost("seed")]
    public async Task<ActionResult> SeedData()
    {
        // Check if we already have data
        var existingCount = await _session.Query<Employee>().CountAsync();
        if (existingCount > 0)
        {
            return Ok(new { message = $"Database already has {existingCount} employees", seeded = false });
        }

        // Create sample employees
        var employees = new List<Employee>
        {
            new Employee
            {
                FirstName = "John",
                LastName = "Doe",
                Department = "Finance",
                Email = "john.doe@sapclone.com",
                Salary = 75000,
                HireDate = new DateTime(2023, 1, 15)
            },
            new Employee
            {
                FirstName = "Jane",
                LastName = "Smith",
                Department = "Human Resources",
                Email = "jane.smith@sapclone.com",
                Salary = 68000,
                HireDate = new DateTime(2023, 3, 22)
            },
            new Employee
            {
                FirstName = "Mike",
                LastName = "Johnson",
                Department = "IT",
                Email = "mike.johnson@sapclone.com",
                Salary = 85000,
                HireDate = new DateTime(2022, 11, 8)
            },
            new Employee
            {
                FirstName = "Sarah",
                LastName = "Wilson",
                Department = "Sales",
                Email = "sarah.wilson@sapclone.com",
                Salary = 72000,
                HireDate = new DateTime(2023, 2, 14)
            },
            new Employee
            {
                FirstName = "David",
                LastName = "Brown",
                Department = "Operations",
                Email = "david.brown@sapclone.com",
                Salary = 65000,
                HireDate = new DateTime(2023, 4, 3)
            }
        };

        _session.StoreObjects(employees);
        await _session.SaveChangesAsync();

        return Ok(new { message = $"Successfully seeded {employees.Count} employees", seeded = true });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, Employee employee)
    {
        if (id != employee.Id)
        {
            return BadRequest();
        }

        var existingEmployee = await _session.LoadAsync<Employee>(id);
        if (existingEmployee == null)
        {
            return NotFound();
        }

        _session.Store(employee);
        await _session.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _session.LoadAsync<Employee>(id);
        if (employee == null)
        {
            return NotFound();
        }

        _session.Delete<Employee>(id);
        await _session.SaveChangesAsync();

        return NoContent();
    }
} 
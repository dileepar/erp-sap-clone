import { useState, useEffect } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'

interface Employee {
  id: number;
  firstName: string;
  lastName: string;
  department: string;
  email: string;
  salary: number;
  hireDate: string;
}

function App() {
  const [count, setCount] = useState(0)
  const [employees, setEmployees] = useState<Employee[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchEmployees()
  }, [])

  const fetchEmployees = async () => {
    try {
      setLoading(true)
      const response = await fetch('http://localhost:5083/api/employees')
      if (!response.ok) {
        throw new Error('Failed to fetch employees')
      }
      const data = await response.json()
      setEmployees(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred')
    } finally {
      setLoading(false)
    }
  }

  const seedData = async () => {
    try {
      const response = await fetch('http://localhost:5083/api/employees/seed', {
        method: 'POST'
      })
      const result = await response.json()
      alert(result.message)
      if (result.seeded) {
        fetchEmployees() // Refresh the list
      }
    } catch (err) {
      alert('Failed to seed data')
    }
  }

  const formatSalary = (salary: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(salary)
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString()
  }

  return (
    <>
      <div>
        <a href="https://vitejs.dev" target="_blank">
          <img src={viteLogo} className="logo" alt="Vite logo" />
        </a>
        <a href="https://react.dev" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo" />
        </a>
      </div>
      <h1>SAP Clone - Employee Management</h1>
      
      <div className="card">
        <button onClick={() => setCount((count) => count + 1)}>
          count is {count}
        </button>
        <button onClick={seedData} style={{ marginLeft: '10px' }}>
          Seed Employee Data
        </button>
        <button onClick={fetchEmployees} style={{ marginLeft: '10px' }}>
          Refresh Data
        </button>
      </div>

      <div className="employee-section">
        <h2>Employees from PostgreSQL Database</h2>
        
        {loading && <p>Loading employees...</p>}
        {error && <p style={{ color: 'red' }}>Error: {error}</p>}
        
        {!loading && !error && (
          <div className="employees-grid">
            {employees.length === 0 ? (
              <p>No employees found. Click "Seed Employee Data" to add some test data.</p>
            ) : (
              <table className="employees-table">
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Name</th>
                    <th>Department</th>
                    <th>Email</th>
                    <th>Salary</th>
                    <th>Hire Date</th>
                  </tr>
                </thead>
                <tbody>
                  {employees.map((employee) => (
                    <tr key={employee.id}>
                      <td>{employee.id}</td>
                      <td>{employee.firstName} {employee.lastName}</td>
                      <td>{employee.department}</td>
                      <td>{employee.email}</td>
                      <td>{formatSalary(employee.salary)}</td>
                      <td>{formatDate(employee.hireDate)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}
      </div>

      <p className="read-the-docs">
        Full stack SAP Clone with React, .NET 9, Marten, and PostgreSQL
      </p>
    </>
  )
}

export default App

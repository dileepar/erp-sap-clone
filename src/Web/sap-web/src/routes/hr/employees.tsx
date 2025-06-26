import { createFileRoute } from '@tanstack/react-router'
import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { 
  Table, 
  Button, 
  Modal, 
  Form, 
  Input, 
  Select, 
  DatePicker,
  Space,
  Tag,
  Typography,
  Row,
  Col,
  Avatar
} from 'antd'
import { 
  PlusOutlined, 
  EditOutlined, 
  TeamOutlined,
  UserOutlined,
  MailOutlined,
  PhoneOutlined
} from '@ant-design/icons'
import dayjs from 'dayjs'

const { Title, Text } = Typography

export const Route = createFileRoute('/hr/employees')({
  component: EmployeesComponent,
})

interface Employee {
  id: string
  employeeNumber: string
  firstName: string
  lastName: string
  email: string
  phoneNumber?: string
  position: string
  department: string
  hireDate: string
  salary: number
  status: 'Active' | 'Inactive'
}

interface CreateEmployeeRequest {
  employeeNumber: string
  firstName: string
  lastName: string
  email: string
  phoneNumber?: string
  position: string
  department: string
  hireDate: string
  salary: number
}

function EmployeesComponent() {
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [form] = Form.useForm()

  // Mock data for employees
  const mockEmployees: Employee[] = [
    {
      id: '1',
      employeeNumber: 'EMP001',
      firstName: 'John',
      lastName: 'Doe',
      email: 'john.doe@company.com',
      phoneNumber: '+1-555-0123',
      position: 'Senior Software Engineer',
      department: 'Information Technology',
      hireDate: '2022-01-15',
      salary: 95000,
      status: 'Active'
    },
    {
      id: '2',
      employeeNumber: 'EMP002',
      firstName: 'Jane',
      lastName: 'Smith',
      email: 'jane.smith@company.com',
      phoneNumber: '+1-555-0124',
      position: 'Financial Analyst',
      department: 'Finance',
      hireDate: '2021-11-20',
      salary: 75000,
      status: 'Active'
    },
    {
      id: '3',
      employeeNumber: 'EMP003',
      firstName: 'Mike',
      lastName: 'Johnson',
      email: 'mike.johnson@company.com',
      phoneNumber: '+1-555-0125',
      position: 'HR Manager',
      department: 'Human Resources',
      hireDate: '2020-03-10',
      salary: 85000,
      status: 'Active'
    },
  ]

  // Using react-query for consistent API pattern
  const { data: employees = mockEmployees, isLoading } = useQuery({
    queryKey: ['employees'],
    queryFn: async (): Promise<Employee[]> => {
      // Return mock data for now
      return new Promise((resolve) => {
        setTimeout(() => resolve(mockEmployees), 500)
      })
    }
  })

  const handleSubmit = async (_values: CreateEmployeeRequest) => {
    // TODO: Implement employee creation
    setIsModalOpen(false)
    form.resetFields()
  }

  const getInitials = (firstName: string, lastName: string) => {
    return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase()
  }

  const columns = [
    {
      title: 'Employee',
      key: 'employee',
      render: (_: unknown, record: Employee) => (
        <div className="flex items-center gap-3">
          <Avatar size="large" className="bg-sap-primary">
            {getInitials(record.firstName, record.lastName)}
          </Avatar>
          <div>
            <div className="font-medium text-sap-neutral-800">
              {record.firstName} {record.lastName}
            </div>
            <div className="text-sm text-sap-neutral-600">
              {record.employeeNumber}
            </div>
          </div>
        </div>
      ),
      sorter: (a: Employee, b: Employee) => 
        `${a.firstName} ${a.lastName}`.localeCompare(`${b.firstName} ${b.lastName}`),
    },
    {
      title: 'Contact',
      key: 'contact',
      render: (_: unknown, record: Employee) => (
        <div className="space-y-1">
          <div className="flex items-center gap-2 text-sm">
            <MailOutlined className="text-sap-neutral-500" />
            <span>{record.email}</span>
          </div>
          {record.phoneNumber && (
            <div className="flex items-center gap-2 text-sm">
              <PhoneOutlined className="text-sap-neutral-500" />
              <span>{record.phoneNumber}</span>
            </div>
          )}
        </div>
      ),
    },
    {
      title: 'Position',
      dataIndex: 'position',
      key: 'position',
      sorter: (a: Employee, b: Employee) => a.position.localeCompare(b.position),
    },
    {
      title: 'Department',
      dataIndex: 'department',
      key: 'department',
      filters: [
        { text: 'Information Technology', value: 'Information Technology' },
        { text: 'Finance', value: 'Finance' },
        { text: 'Human Resources', value: 'Human Resources' },
        { text: 'Sales', value: 'Sales' },
        { text: 'Marketing', value: 'Marketing' },
      ],
      onFilter: (value: boolean | React.Key, record: Employee) => record.department === value,
      render: (department: string) => (
        <Tag color="blue">{department}</Tag>
      ),
    },
    {
      title: 'Hire Date',
      dataIndex: 'hireDate',
      key: 'hireDate',
      render: (date: string) => dayjs(date).format('DD/MM/YYYY'),
      sorter: (a: Employee, b: Employee) => 
        new Date(a.hireDate).getTime() - new Date(b.hireDate).getTime(),
    },
    {
      title: 'Salary',
      dataIndex: 'salary',
      key: 'salary',
      render: (salary: number) => (
        <span className="font-medium">
          ${salary.toLocaleString('en-US')}
        </span>
      ),
      sorter: (a: Employee, b: Employee) => a.salary - b.salary,
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      filters: [
        { text: 'Active', value: 'Active' },
        { text: 'Inactive', value: 'Inactive' },
      ],
      onFilter: (value: boolean | React.Key, record: Employee) => record.status === value,
      render: (status: string) => (
        <span className={status === 'Active' ? 'sap-status-active' : 'sap-status-inactive'}>
          {status}
        </span>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown) => (
        <Space size="small">
          <Button
            type="text"
            icon={<EditOutlined />}
            onClick={() => {/* TODO: Implement edit employee */}}
            className="text-sap-primary hover:bg-sap-accent-2"
          />
        </Space>
      ),
    },
  ]

  // Calculate statistics
  const totalEmployees = employees.length
  const activeEmployees = employees.filter(emp => emp.status === 'Active').length
  const averageSalary = employees.reduce((sum, emp) => sum + emp.salary, 0) / employees.length

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="sap-page-header">
        <h1 className="sap-page-title">Employee Management</h1>
        <p className="sap-page-subtitle">
          Manage employee information, positions, and organizational structure
        </p>
      </div>

      {/* Statistics Cards */}
      <Row gutter={16}>
        <Col span={8}>
          <div className="bg-white p-6 rounded-lg shadow-sap">
            <div className="flex items-center justify-between">
              <div>
                <Text className="text-sap-neutral-600 text-sm">Total Employees</Text>
                <div className="text-2xl font-bold text-sap-neutral-800 mt-1">
                  {totalEmployees}
                </div>
              </div>
              <TeamOutlined className="text-3xl text-sap-primary" />
            </div>
          </div>
        </Col>
        <Col span={8}>
          <div className="bg-white p-6 rounded-lg shadow-sap">
            <div className="flex items-center justify-between">
              <div>
                <Text className="text-sap-neutral-600 text-sm">Active Employees</Text>
                <div className="text-2xl font-bold text-sap-success mt-1">
                  {activeEmployees}
                </div>
              </div>
              <UserOutlined className="text-3xl text-sap-success" />
            </div>
          </div>
        </Col>
        <Col span={8}>
          <div className="bg-white p-6 rounded-lg shadow-sap">
            <div className="flex items-center justify-between">
              <div>
                <Text className="text-sap-neutral-600 text-sm">Average Salary</Text>
                <div className="text-2xl font-bold text-sap-neutral-800 mt-1">
                  ${Math.round(averageSalary).toLocaleString('en-US')}
                </div>
              </div>
              <div className="text-3xl">💰</div>
            </div>
          </div>
        </Col>
      </Row>

      {/* Actions Bar */}
      <div className="bg-white p-6 rounded-lg shadow-sap">
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-4">
            <TeamOutlined className="text-2xl text-sap-primary" />
            <div>
              <Title level={4} className="!mb-0 !text-sap-neutral-800">
                Employee Directory
              </Title>
              <span className="text-sap-neutral-600">
                {employees.length} employees in the system
              </span>
            </div>
          </div>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => setIsModalOpen(true)}
            size="large"
          >
            Add Employee
          </Button>
        </div>
      </div>

      {/* Employees Table */}
      <div className="bg-white rounded-lg shadow-sap">
        <div className="p-6 border-b border-gray-200">
          <h3 className="text-lg font-semibold text-sap-neutral-800">Employees</h3>
        </div>
        <div className="p-6">
          <Table
            columns={columns}
            dataSource={employees}
            rowKey="id"
            loading={isLoading}
            pagination={{
              pageSize: 10,
              showSizeChanger: true,
              showQuickJumper: true,
              showTotal: (total: number, range: [number, number]) =>
                `${range[0]}-${range[1]} of ${total} employees`,
            }}
            scroll={{ x: 1200 }}
            className="ant-table-sap"
          />
        </div>
      </div>

      {/* Add Employee Modal */}
      <Modal
        title="Add New Employee"
        open={isModalOpen}
        onCancel={() => setIsModalOpen(false)}
        footer={null}
        width={700}
        className="sap-enhanced-modal"
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          className="mt-6"
        >
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                label="Employee Number"
                name="employeeNumber"
                rules={[{ required: true, message: 'Employee number is required' }]}
              >
                <Input placeholder="e.g., EMP001" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                label="Email"
                name="email"
                rules={[
                  { required: true, message: 'Email is required' },
                  { type: 'email', message: 'Please enter a valid email' }
                ]}
              >
                <Input placeholder="john.doe@company.com" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                label="First Name"
                name="firstName"
                rules={[{ required: true, message: 'First name is required' }]}
              >
                <Input placeholder="John" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                label="Last Name"
                name="lastName"
                rules={[{ required: true, message: 'Last name is required' }]}
              >
                <Input placeholder="Doe" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                label="Position"
                name="position"
                rules={[{ required: true, message: 'Position is required' }]}
              >
                <Input placeholder="Senior Software Engineer" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                label="Department"
                name="department"
                rules={[{ required: true, message: 'Department is required' }]}
              >
                <Select
                  placeholder="Select department"
                  options={[
                    { value: 'Information Technology', label: 'Information Technology' },
                    { value: 'Finance', label: 'Finance' },
                    { value: 'Human Resources', label: 'Human Resources' },
                    { value: 'Sales', label: 'Sales' },
                    { value: 'Marketing', label: 'Marketing' },
                  ]}
                />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                label="Phone Number"
                name="phoneNumber"
              >
                <Input placeholder="+1-555-0123" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                label="Hire Date"
                name="hireDate"
                rules={[{ required: true, message: 'Hire date is required' }]}
              >
                <DatePicker className="w-full" format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            label="Salary"
            name="salary"
            rules={[
              { required: true, message: 'Salary is required' },
              { type: 'number', min: 0, message: 'Salary must be a positive number' }
            ]}
          >
            <Input
              type="number"
              placeholder="75000"
              prefix="$"
            />
          </Form.Item>

          <Form.Item className="mb-0 pt-4">
            <div className="flex justify-end gap-3">
              <Button onClick={() => setIsModalOpen(false)}>
                Cancel
              </Button>
              <Button type="primary" htmlType="submit">
                Add Employee
              </Button>
            </div>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
} 
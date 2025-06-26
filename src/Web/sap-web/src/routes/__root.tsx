import { createRootRoute, Outlet, Link } from '@tanstack/react-router'
import { TanStackRouterDevtools } from '@tanstack/router-devtools'
import { Layout, Menu, Avatar, Space, Tooltip } from 'antd'
import { 
  HomeOutlined, 
  DollarOutlined, 
  TeamOutlined, 
  UserOutlined,
  BankOutlined,
  BookOutlined,
  LogoutOutlined
} from '@ant-design/icons'
import { useState } from 'react'

const { Header, Sider, Content } = Layout

function RootComponent() {
  const [collapsed, setCollapsed] = useState(false)

  const menuItems = [
    {
      key: '/',
      icon: <Tooltip title={collapsed ? 'Dashboard' : ''} placement="right">
        <HomeOutlined />
      </Tooltip>,
      label: <Link to="/">Dashboard</Link>,
    },
    {
      key: 'financial',
      icon: <Tooltip title={collapsed ? 'Financial Management' : ''} placement="right">
        <DollarOutlined />
      </Tooltip>,
      label: 'Financial Management',
      children: [
        {
          key: '/financial/accounts',
          icon: <Tooltip title={collapsed ? 'Chart of Accounts' : ''} placement="right">
            <BankOutlined />
          </Tooltip>,
          label: <Link to="/financial/accounts">Chart of Accounts</Link>,
        },
        {
          key: '/financial/journal-entries',
          icon: <Tooltip title={collapsed ? 'Journal Entries' : ''} placement="right">
            <BookOutlined />
          </Tooltip>,
          label: <Link to="/financial/journal-entries">Journal Entries</Link>,
        },
      ],
    },
    {
      key: 'hr',
      icon: <Tooltip title={collapsed ? 'Human Resources' : ''} placement="right">
        <TeamOutlined />
      </Tooltip>,
      label: 'Human Resources',
      children: [
        {
          key: '/hr/employees',
          icon: <Tooltip title={collapsed ? 'Employees' : ''} placement="right">
            <UserOutlined />
          </Tooltip>,
          label: <Link to="/hr/employees">Employees</Link>,
        },
      ],
    },
  ]

  return (
    <Layout className="min-h-screen">
      {/* Header */}
      <Header className="flex justify-between items-center px-6 bg-sap-shell-bg">
        <div className="flex items-center">
          <div className="text-white text-xl font-bold mr-8">
            SAP Clone ERP
          </div>
        </div>
        <div className="flex items-center">
          <Space>
            <Avatar icon={<UserOutlined />} className="bg-sap-primary" />
            <span className="text-white">Admin User</span>
            <Tooltip title="Logout">
              <LogoutOutlined className="text-white cursor-pointer hover:text-sap-accent-2" />
            </Tooltip>
          </Space>
        </div>
      </Header>

      <Layout>
        {/* Sidebar */}
        <Sider
          collapsible
          collapsed={collapsed}
          onCollapse={setCollapsed}
          width={280}
          theme="light"
          className="bg-white border-r border-sap-neutral-300"
        >
          <Menu
            mode="inline"
            defaultSelectedKeys={['/']}
            defaultOpenKeys={['financial', 'hr']}
            items={menuItems}
            className="h-full border-none"
            inlineCollapsed={collapsed}
          />
        </Sider>

        {/* Main Content */}
        <Layout className="bg-sap-page-bg">
          <Content className="p-6">
            <Outlet />
          </Content>
        </Layout>
      </Layout>

      {import.meta.env.DEV && <TanStackRouterDevtools />}
    </Layout>
  )
}

export const Route = createRootRoute({
  component: RootComponent
}) 
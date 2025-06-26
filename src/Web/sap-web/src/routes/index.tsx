import { createFileRoute } from '@tanstack/react-router'
import { Row, Col, Typography, Progress } from 'antd'
import { 
  DollarOutlined, 
  TeamOutlined, 
  BankOutlined, 
  BookOutlined,
  ArrowUpOutlined,
  ArrowDownOutlined,
  FileTextOutlined
} from '@ant-design/icons'

const { Title, Text } = Typography

export const Route = createFileRoute('/')({
  component: DashboardComponent,
})

interface KPICardProps {
  title: string
  value: string
  icon: React.ReactNode
  trend?: {
    value: number
    isPositive: boolean
  }
  color?: string
}

interface RecentTransactionProps {
  description: string
  amount: number
  date: string
  type: 'debit' | 'credit'
}

interface AccountBalanceProps {
  accountName: string
  balance: number
  percentage: number
  type: 'asset' | 'liability' | 'equity'
}

function DashboardComponent() {
  // Mock data for demonstration
  const kpiData = [
    {
      title: 'Total Revenue',
      value: '$2,547,890',
      icon: <DollarOutlined />,
      trend: { value: 12.5, isPositive: true },
      color: 'text-sap-success'
    },
    {
      title: 'Total Expenses',
      value: '$1,823,450',
      icon: <FileTextOutlined />,
      trend: { value: 8.2, isPositive: false },
      color: 'text-sap-warning'
    },
    {
      title: 'Active Employees',
      value: '247',
      icon: <TeamOutlined />,
      trend: { value: 3.1, isPositive: true },
      color: 'text-sap-primary'
    },
    {
      title: 'Total Accounts',
      value: '156',
      icon: <BankOutlined />,
      trend: { value: 2.0, isPositive: true },
      color: 'text-sap-info'
    }
  ]

  const recentTransactions: RecentTransactionProps[] = [
    { description: 'Office Supplies Purchase', amount: 1250.00, date: '2024-01-15', type: 'debit' },
    { description: 'Client Payment Received', amount: 5500.00, date: '2024-01-14', type: 'credit' },
    { description: 'Software License Renewal', amount: 899.99, date: '2024-01-13', type: 'debit' },
    { description: 'Consulting Services Revenue', amount: 3200.00, date: '2024-01-12', type: 'credit' },
    { description: 'Utility Bill Payment', amount: 450.75, date: '2024-01-11', type: 'debit' },
  ]

  const accountBalances: AccountBalanceProps[] = [
    { accountName: 'Cash and Cash Equivalents', balance: 125000, percentage: 85, type: 'asset' },
    { accountName: 'Accounts Receivable', balance: 75000, percentage: 60, type: 'asset' },
    { accountName: 'Accounts Payable', balance: 45000, percentage: 40, type: 'liability' },
    { accountName: 'Retained Earnings', balance: 200000, percentage: 90, type: 'equity' },
  ]

  const KPICard = ({ title, value, icon, trend, color }: KPICardProps) => (
    <div className="bg-white p-6 rounded-lg shadow-sap">
      <div className="flex items-center justify-between mb-4">
        <div className={`p-3 rounded-lg bg-gray-50 ${color}`}>
          {icon}
        </div>
                 {trend && (
           <div className={`flex items-center gap-1 ${trend.isPositive ? 'text-sap-success' : 'text-sap-error'}`}>
             {trend.isPositive ? <ArrowUpOutlined /> : <ArrowDownOutlined />}
             <span className="text-sm font-medium">{trend.value}%</span>
           </div>
         )}
      </div>
      <div>
        <div className="text-2xl font-bold text-sap-neutral-800 mb-1">
          {value}
        </div>
        <div className="text-sm text-sap-neutral-600">
          {title}
        </div>
      </div>
    </div>
  )

  const getTransactionTypeColor = (type: 'debit' | 'credit') => {
    return type === 'credit' ? 'text-sap-success' : 'text-sap-error'
  }

  const getAccountTypeColor = (type: 'asset' | 'liability' | 'equity') => {
    const colors = {
      asset: '#1890ff',
      liability: '#fa8c16', 
      equity: '#52c41a'
    }
    return colors[type]
  }

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="sap-page-header">
        <h1 className="sap-page-title">Financial Dashboard</h1>
        <p className="sap-page-subtitle">
          Real-time overview of your organization's financial health and key metrics
        </p>
      </div>

      {/* KPI Cards */}
      <Row gutter={[16, 16]}>
        {kpiData.map((kpi, index) => (
          <Col xs={24} sm={12} lg={6} key={index}>
            <KPICard {...kpi} />
          </Col>
        ))}
      </Row>

      {/* Charts and Tables Row */}
      <Row gutter={[16, 16]}>
        {/* Recent Transactions */}
        <Col xs={24} lg={12}>
          <div className="bg-white p-6 rounded-lg shadow-sap">
            <div className="flex items-center gap-3 mb-6">
              <BookOutlined className="text-xl text-sap-primary" />
              <Title level={4} className="!mb-0 !text-sap-neutral-800">
                Recent Transactions
              </Title>
            </div>
            <div className="space-y-4">
              {recentTransactions.map((transaction, index) => (
                <div key={index} className="flex items-center justify-between p-3 bg-gray-50 rounded">
                  <div className="flex-1">
                    <div className="font-medium text-sap-neutral-800">
                      {transaction.description}
                    </div>
                    <div className="text-sm text-sap-neutral-600">
                      {new Date(transaction.date).toLocaleDateString()}
                    </div>
                  </div>
                  <div className={`font-bold ${getTransactionTypeColor(transaction.type)}`}>
                    {transaction.type === 'credit' ? '+' : '-'}
                    ${transaction.amount.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </Col>

        {/* Account Balances */}
        <Col xs={24} lg={12}>
          <div className="bg-white p-6 rounded-lg shadow-sap">
            <div className="flex items-center gap-3 mb-6">
              <BankOutlined className="text-xl text-sap-primary" />
              <Title level={4} className="!mb-0 !text-sap-neutral-800">
                Account Balances
              </Title>
            </div>
            <div className="space-y-4">
              {accountBalances.map((account, index) => (
                <div key={index} className="p-4 border border-gray-200 rounded">
                  <div className="flex justify-between items-start mb-2">
                    <div>
                      <div className="font-medium text-sap-neutral-800">
                        {account.accountName}
                      </div>
                      <div className="text-sm text-sap-neutral-600 capitalize">
                        {account.type}
                      </div>
                    </div>
                    <div className="text-right">
                      <div className="font-bold text-sap-neutral-800">
                        ${account.balance.toLocaleString('en-US')}
                      </div>
                    </div>
                  </div>
                  <Progress
                    percent={account.percentage}
                    strokeColor={getAccountTypeColor(account.type)}
                    size="small"
                    showInfo={false}
                  />
                </div>
              ))}
            </div>
          </div>
        </Col>
      </Row>

      {/* Financial Summary */}
      <div className="bg-white p-6 rounded-lg shadow-sap">
        <Title level={4} className="!mb-4 !text-sap-neutral-800">
          Financial Summary
        </Title>
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={8}>
            <div className="text-center p-4 bg-green-50 rounded">
              <div className="text-2xl font-bold text-sap-success mb-1">
                $724,440
              </div>
              <Text className="text-sap-neutral-600">Net Income</Text>
            </div>
          </Col>
          <Col xs={24} sm={8}>
            <div className="text-center p-4 bg-blue-50 rounded">
              <div className="text-2xl font-bold text-sap-primary mb-1">
                $2,125,000
              </div>
              <Text className="text-sap-neutral-600">Total Assets</Text>
            </div>
          </Col>
          <Col xs={24} sm={8}>
            <div className="text-center p-4 bg-orange-50 rounded">
              <div className="text-2xl font-bold text-sap-warning mb-1">
                $875,000
              </div>
              <Text className="text-sap-neutral-600">Total Liabilities</Text>
            </div>
          </Col>
        </Row>
      </div>
    </div>
  )
} 
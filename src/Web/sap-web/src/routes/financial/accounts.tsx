import { createFileRoute } from '@tanstack/react-router'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { 
  Table, 
  Button, 
  Modal, 
  Form, 
  Input, 
  Select, 
  message, 
  Space, 
  Tag, 
  Popconfirm,
  Typography
} from 'antd'
import { 
  PlusOutlined, 
  EditOutlined, 
  DeleteOutlined, 
  BankOutlined 
} from '@ant-design/icons'

const { Title } = Typography

export const Route = createFileRoute('/financial/accounts')({
  component: ChartOfAccounts
})

interface Account {
  id: string
  accountNumber: string
  accountName: string
  accountType: string
  description: string
  isActive: boolean
  balance: number
  currency: string
}

interface CreateAccountRequest {
  accountNumber: string
  accountName: string
  accountType: string
  description: string
  currency: string
}

function ChartOfAccounts() {
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingAccount, setEditingAccount] = useState<Account | null>(null)
  const [form] = Form.useForm()
  const queryClient = useQueryClient()

  // Fetch accounts
  const { data: accounts = [], isLoading } = useQuery({
    queryKey: ['accounts'],
    queryFn: async (): Promise<Account[]> => {
      const response = await fetch('http://localhost:5083/api/accounts')
      if (!response.ok) {
        throw new Error('Failed to fetch accounts')
      }
      return response.json()
    }
  })

  // Create account mutation
  const createAccountMutation = useMutation({
    mutationFn: async (account: CreateAccountRequest): Promise<Account> => {
      const response = await fetch('http://localhost:5083/api/accounts', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(account),
      })
      if (!response.ok) {
        const error = await response.text()
        throw new Error(error || 'Failed to create account')
      }
      return response.json()
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] })
      message.success('Account created successfully!')
      handleCloseModal()
    },
    onError: (error) => {
      message.error(`Error creating account: ${error.message}`)
    }
  })

  const handleOpenModal = (account?: Account) => {
    if (account) {
      setEditingAccount(account)
      form.setFieldsValue({
        accountNumber: account.accountNumber,
        accountName: account.accountName,
        accountType: account.accountType,
        description: account.description,
        currency: account.currency,
      })
    } else {
      setEditingAccount(null)
      form.resetFields()
    }
    setIsModalOpen(true)
  }

  const handleCloseModal = () => {
    setIsModalOpen(false)
    setEditingAccount(null)
    form.resetFields()
  }

  const handleSubmit = async (values: CreateAccountRequest) => {
    if (editingAccount) {
      // TODO: Implement update functionality
      message.info('Update functionality not yet implemented')
    } else {
      createAccountMutation.mutate(values)
    }
  }

  const columns = [
    {
      title: 'Account Number',
      dataIndex: 'accountNumber',
      key: 'accountNumber',
      sorter: (a: Account, b: Account) => a.accountNumber.localeCompare(b.accountNumber),
      render: (text: string) => (
        <span className="font-mono font-medium text-sap-neutral-700">{text}</span>
      ),
    },
    {
      title: 'Account Name',
      dataIndex: 'accountName',
      key: 'accountName',
      sorter: (a: Account, b: Account) => a.accountName.localeCompare(b.accountName),
      render: (text: string) => (
        <span className="font-medium text-sap-neutral-800">{text}</span>
      ),
    },
    {
      title: 'Type',
      dataIndex: 'accountType',
      key: 'accountType',
      filters: [
        { text: 'Asset', value: 'Asset' },
        { text: 'Liability', value: 'Liability' },
        { text: 'Equity', value: 'Equity' },
        { text: 'Revenue', value: 'Revenue' },
        { text: 'Expense', value: 'Expense' },
      ],
      onFilter: (value: boolean | React.Key, record: Account) => record.accountType === value,
      render: (type: string) => {
        const colors = {
          Asset: 'blue',
          Liability: 'orange',
          Equity: 'green',
          Revenue: 'purple',
          Expense: 'red'
        }
        return <Tag color={colors[type as keyof typeof colors] || 'default'}>{type}</Tag>
      },
    },
    {
      title: 'Balance',
      dataIndex: 'balance',
      key: 'balance',
      sorter: (a: Account, b: Account) => a.balance - b.balance,
      render: (balance: number, record: Account) => (
        <span className={`font-medium ${balance >= 0 ? 'text-sap-success' : 'text-sap-error'}`}>
          {record.currency} {balance.toLocaleString('en-US', { minimumFractionDigits: 2 })}
        </span>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      filters: [
        { text: 'Active', value: true },
        { text: 'Inactive', value: false },
      ],
      onFilter: (value: boolean | React.Key, record: Account) => record.isActive === value,
      render: (isActive: boolean) => (
        <span className={isActive ? 'sap-status-active' : 'sap-status-inactive'}>
          {isActive ? 'Active' : 'Inactive'}
        </span>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, record: Account) => (
        <Space size="small">
          <Button
            type="text"
            icon={<EditOutlined />}
            onClick={() => handleOpenModal(record)}
            className="text-sap-primary hover:bg-sap-accent-2"
          />
          <Popconfirm
            title="Delete account"
            description="Are you sure you want to delete this account?"
            onConfirm={() => message.info('Delete functionality not yet implemented')}
            okText="Yes"
            cancelText="No"
          >
            <Button
              type="text"
              icon={<DeleteOutlined />}
              danger
              className="hover:bg-sap-accent-5"
            />
          </Popconfirm>
        </Space>
      ),
    },
  ]

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="sap-page-header">
        <h1 className="sap-page-title">Chart of Accounts</h1>
        <p className="sap-page-subtitle">
          Manage your organization's account structure and financial hierarchy
        </p>
      </div>

      {/* Actions Bar */}
      <div className="bg-white p-6 rounded-lg shadow-sap">
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-4">
            <BankOutlined className="text-2xl text-sap-primary" />
            <div>
              <Title level={4} className="!mb-0 !text-sap-neutral-800">
                Account Management
              </Title>
              <span className="text-sap-neutral-600">
                {accounts.length} accounts configured
              </span>
            </div>
          </div>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => handleOpenModal()}
            size="large"
          >
            Create Account
          </Button>
        </div>
      </div>

      {/* Accounts Table */}
      <div className="bg-white rounded-lg shadow-sap">
        <div className="p-6 border-b border-gray-200">
          <h3 className="text-lg font-semibold text-sap-neutral-800">Accounts</h3>
        </div>
        <div className="p-6">
          <Table
            columns={columns}
            dataSource={accounts}
            rowKey="id"
            loading={isLoading}
            pagination={{
              pageSize: 10,
              showSizeChanger: true,
              showQuickJumper: true,
              showTotal: (total: number, range: [number, number]) =>
                `${range[0]}-${range[1]} of ${total} accounts`,
            }}
            scroll={{ x: 1000 }}
            className="ant-table-sap"
          />
        </div>
      </div>

      {/* Create/Edit Account Modal */}
      <Modal
        title={editingAccount ? 'Edit Account' : 'Create New Account'}
        open={isModalOpen}
        onCancel={handleCloseModal}
        footer={null}
        width={600}
        className="sap-enhanced-modal"
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          className="mt-6"
        >
          <Form.Item
            label="Account Number"
            name="accountNumber"
            rules={[
              { required: true, message: 'Account number is required' },
              { pattern: /^\d{4,10}$/, message: 'Account number must be 4-10 digits' }
            ]}
          >
            <Input
              placeholder="e.g., 1000"
              disabled={!!editingAccount}
            />
          </Form.Item>

          <Form.Item
            label="Account Name"
            name="accountName"
            rules={[
              { required: true, message: 'Account name is required' },
              { min: 3, message: 'Account name must be at least 3 characters' }
            ]}
          >
            <Input
              placeholder="e.g., Cash and Cash Equivalents"
            />
          </Form.Item>

          <Form.Item
            label="Account Type"
            name="accountType"
            rules={[{ required: true, message: 'Account type is required' }]}
          >
            <Select
              placeholder="Select account type"
              options={[
                { value: 'Asset', label: 'Asset' },
                { value: 'Liability', label: 'Liability' },
                { value: 'Equity', label: 'Equity' },
                { value: 'Revenue', label: 'Revenue' },
                { value: 'Expense', label: 'Expense' },
              ]}
            />
          </Form.Item>

          <Form.Item
            label="Currency"
            name="currency"
            rules={[{ required: true, message: 'Currency is required' }]}
            initialValue="USD"
          >
            <Select
              placeholder="Select currency"
              options={[
                { value: 'USD', label: 'USD - US Dollar' },
                { value: 'EUR', label: 'EUR - Euro' },
                { value: 'GBP', label: 'GBP - British Pound' },
                { value: 'JPY', label: 'JPY - Japanese Yen' },
              ]}
            />
          </Form.Item>

          <Form.Item
            label="Description"
            name="description"
            rules={[{ required: true, message: 'Description is required' }]}
          >
            <Input.TextArea
              placeholder="Enter account description"
              rows={3}
              maxLength={500}
              showCount
            />
          </Form.Item>

          <Form.Item className="mb-0 pt-4">
            <div className="flex justify-end gap-3">
              <Button onClick={handleCloseModal}>
                Cancel
              </Button>
              <Button
                type="primary"
                htmlType="submit"
                loading={createAccountMutation.isPending}
              >
                {editingAccount ? 'Update Account' : 'Create Account'}
              </Button>
            </div>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
} 
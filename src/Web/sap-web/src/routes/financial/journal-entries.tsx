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
  DatePicker, 
  message, 
  Typography,
  Row,
  Col,
  Divider
} from 'antd'
import { 
  PlusOutlined, 
  BookOutlined,
  MinusCircleOutlined
} from '@ant-design/icons'
import dayjs from 'dayjs'

const { Title, Text } = Typography

export const Route = createFileRoute('/financial/journal-entries')({
  component: JournalEntriesComponent,
})

interface JournalEntry {
  id: string
  journalEntryNumber: string
  postingDate: string
  documentDate: string
  reference: string
  description: string
  status: 'Draft' | 'Posted'
  totalDebitAmount: number
  totalCreditAmount: number
  currency: string
  lineItems: JournalEntryLineItem[]
}

interface JournalEntryLineItem {
  accountId: string
  accountNumber: string
  accountName: string
  description: string
  debitAmount: number
  creditAmount: number
}

interface CreateJournalEntryRequest {
  postingDate: string
  documentDate: string
  reference: string
  description: string
  lineItems: {
    accountId: string
    description: string
    debitAmount: number
    creditAmount: number
  }[]
}

interface Account {
  id: string
  accountNumber: string
  accountName: string
  accountType: string
}

interface FormValues {
  postingDate: dayjs.Dayjs
  documentDate: dayjs.Dayjs
  reference: string
  description: string
  lineItems: {
    accountId: string
    description: string
    debitAmount: number
    creditAmount: number
  }[]
}

interface FormListOperation {
  add: () => void
  remove: (index: number) => void
}

interface FormListField {
  key: number
  name: number
}

function JournalEntriesComponent() {
  const queryClient = useQueryClient()
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [form] = Form.useForm()

  // Fetch journal entries
  const { data: journalEntries = [], isLoading } = useQuery({
    queryKey: ['journal-entries'],
    queryFn: async (): Promise<JournalEntry[]> => {
      const response = await fetch('http://localhost:5083/api/journal-entries')
      if (!response.ok) {
        throw new Error('Failed to fetch journal entries')
      }
      return response.json()
    }
  })

  // Fetch accounts for dropdown
  const { data: accounts = [] } = useQuery({
    queryKey: ['accounts'],
    queryFn: async (): Promise<Account[]> => {
      const response = await fetch('http://localhost:5083/api/accounts')
      if (!response.ok) {
        throw new Error('Failed to fetch accounts')
      }
      return response.json()
    }
  })

  // Create journal entry mutation
  const createJournalEntryMutation = useMutation({
    mutationFn: async (journalEntry: CreateJournalEntryRequest): Promise<JournalEntry> => {
      const response = await fetch('http://localhost:5083/api/journal-entries', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(journalEntry),
      })
      if (!response.ok) {
        const error = await response.text()
        throw new Error(error || 'Failed to create journal entry')
      }
      return response.json()
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['journal-entries'] })
      message.success('Journal entry created successfully!')
      handleCloseModal()
    },
    onError: (error) => {
      message.error(`Error creating journal entry: ${error.message}`)
    }
  })

  const handleCloseModal = () => {
    setIsModalOpen(false)
    form.resetFields()
  }

  const handleSubmit = async (values: FormValues) => {
    const submitData: CreateJournalEntryRequest = {
      postingDate: values.postingDate.format('YYYY-MM-DD'),
      documentDate: values.documentDate.format('YYYY-MM-DD'),
      reference: values.reference,
      description: values.description,
      lineItems: values.lineItems || []
    }

    // Validate that the journal entry is balanced
    const totalDebits = submitData.lineItems.reduce((sum, item) => sum + (item.debitAmount || 0), 0)
    const totalCredits = submitData.lineItems.reduce((sum, item) => sum + (item.creditAmount || 0), 0)

    if (Math.abs(totalDebits - totalCredits) > 0.01) {
      message.error('Journal entry must be balanced. Total debits must equal total credits.')
      return
    }

    createJournalEntryMutation.mutate(submitData)
  }

  const columns = [
    {
      title: 'Journal Entry #',
      dataIndex: 'journalEntryNumber',
      key: 'journalEntryNumber',
      render: (text: string) => (
        <span className="font-mono font-medium text-sap-neutral-700">{text}</span>
      ),
    },
    {
      title: 'Posting Date',
      dataIndex: 'postingDate',
      key: 'postingDate',
      render: (date: string) => dayjs(date).format('DD/MM/YYYY'),
      sorter: (a: JournalEntry, b: JournalEntry) => 
        new Date(a.postingDate).getTime() - new Date(b.postingDate).getTime(),
    },
    {
      title: 'Reference',
      dataIndex: 'reference',
      key: 'reference',
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
    },
    {
      title: 'Amount',
      dataIndex: 'totalDebitAmount',
      key: 'totalDebitAmount',
      render: (amount: number, record: JournalEntry) => (
        <span className="font-medium">
          {record.currency} {amount.toLocaleString('en-US', { minimumFractionDigits: 2 })}
        </span>
      ),
      sorter: (a: JournalEntry, b: JournalEntry) => a.totalDebitAmount - b.totalDebitAmount,
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => (
        <span className={status === 'Posted' ? 'sap-status-posted' : 'sap-status-draft'}>
          {status}
        </span>
      ),
      filters: [
        { text: 'Draft', value: 'Draft' },
        { text: 'Posted', value: 'Posted' },
      ],
      onFilter: (value: boolean | React.Key, record: JournalEntry) => record.status === value,
    },
  ]

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="sap-page-header">
        <h1 className="sap-page-title">Journal Entries</h1>
        <p className="sap-page-subtitle">
          Manage double-entry bookkeeping transactions and financial postings
        </p>
      </div>

      {/* Actions Bar */}
      <div className="bg-white p-6 rounded-lg shadow-sap">
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-4">
            <BookOutlined className="text-2xl text-sap-primary" />
            <div>
              <Title level={4} className="!mb-0 !text-sap-neutral-800">
                Journal Entry Management
              </Title>
              <span className="text-sap-neutral-600">
                {journalEntries.length} entries recorded
              </span>
            </div>
          </div>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => setIsModalOpen(true)}
            size="large"
          >
            Create Journal Entry
          </Button>
        </div>
      </div>

      {/* Journal Entries Table */}
      <div className="bg-white rounded-lg shadow-sap">
        <div className="p-6 border-b border-gray-200">
          <h3 className="text-lg font-semibold text-sap-neutral-800">Journal Entries</h3>
        </div>
        <div className="p-6">
          <Table
            columns={columns}
            dataSource={journalEntries}
            rowKey="id"
            loading={isLoading}
            pagination={{
              pageSize: 10,
              showSizeChanger: true,
              showQuickJumper: true,
              showTotal: (total: number, range: [number, number]) =>
                `${range[0]}-${range[1]} of ${total} entries`,
            }}
            scroll={{ x: 1000 }}
            className="ant-table-sap"
          />
        </div>
      </div>

      {/* Create Journal Entry Modal */}
      <Modal
        title="Create New Journal Entry"
        open={isModalOpen}
        onCancel={handleCloseModal}
        footer={null}
        width={900}
        className="sap-enhanced-modal"
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          className="mt-6"
          initialValues={{
            postingDate: dayjs(),
            documentDate: dayjs(),
            lineItems: [{ accountId: '', description: '', debitAmount: 0, creditAmount: 0 }]
          }}
        >
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                label="Posting Date"
                name="postingDate"
                rules={[{ required: true, message: 'Posting date is required' }]}
              >
                <DatePicker className="w-full" format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                label="Document Date"
                name="documentDate"
                rules={[{ required: true, message: 'Document date is required' }]}
              >
                <DatePicker className="w-full" format="DD/MM/YYYY" />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            label="Reference"
            name="reference"
            rules={[{ required: true, message: 'Reference is required' }]}
          >
            <Input placeholder="Enter reference number" />
          </Form.Item>

          <Form.Item
            label="Description"
            name="description"
            rules={[{ required: true, message: 'Description is required' }]}
          >
            <Input.TextArea
              placeholder="Enter journal entry description"
              rows={2}
              maxLength={500}
              showCount
            />
          </Form.Item>

          <Divider>Line Items</Divider>

          <Form.List name="lineItems">
            {(fields: FormListField[], { add, remove }: FormListOperation) => (
              <>
                {fields.map((field: FormListField, index: number) => (
                  <div key={field.key} className="mb-4 p-4 bg-gray-50 rounded">
                    <Row gutter={16} align="middle">
                      <Col span={6}>
                        <Form.Item
                          {...field}
                          label={index === 0 ? 'Account' : ''}
                          name={[field.name, 'accountId']}
                          rules={[{ required: true, message: 'Account is required' }]}
                          className="mb-0"
                        >
                          <Select
                            placeholder="Select account"
                            className="w-full"
                            showSearch
                            filterOption={(input: string, option?: { label: string; value: string }) =>
                              (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
                            }
                            options={accounts.map(account => ({
                              value: account.id,
                              label: `${account.accountNumber} - ${account.accountName}`
                            }))}
                          />
                        </Form.Item>
                      </Col>
                      <Col span={6}>
                        <Form.Item
                          {...field}
                          label={index === 0 ? 'Description' : ''}
                          name={[field.name, 'description']}
                          rules={[{ required: true, message: 'Description is required' }]}
                          className="mb-0"
                        >
                          <Input placeholder="Line item description" />
                        </Form.Item>
                      </Col>
                      <Col span={4}>
                        <Form.Item
                          {...field}
                          label={index === 0 ? 'Debit' : ''}
                          name={[field.name, 'debitAmount']}
                          className="mb-0"
                        >
                          <Input
                            type="number"
                            placeholder="0.00"
                            step="0.01"
                            min="0"
                          />
                        </Form.Item>
                      </Col>
                      <Col span={4}>
                        <Form.Item
                          {...field}
                          label={index === 0 ? 'Credit' : ''}
                          name={[field.name, 'creditAmount']}
                          className="mb-0"
                        >
                          <Input
                            type="number"
                            placeholder="0.00"
                            step="0.01"
                            min="0"
                          />
                        </Form.Item>
                      </Col>
                      <Col span={4}>
                        {index === 0 && <Text className="text-xs">Actions</Text>}
                        <div className="flex gap-2">
                          {fields.length > 1 && (
                            <Button
                              type="text"
                              icon={<MinusCircleOutlined />}
                              onClick={() => remove(field.name)}
                              danger
                              size="small"
                            />
                          )}
                        </div>
                      </Col>
                    </Row>
                  </div>
                ))}
                <Button
                  type="dashed"
                  onClick={() => add()}
                  icon={<PlusOutlined />}
                  className="w-full mb-4"
                >
                  Add Line Item
                </Button>
              </>
            )}
          </Form.List>

          <Form.Item className="mb-0 pt-4">
            <div className="flex justify-end gap-3">
              <Button onClick={handleCloseModal}>
                Cancel
              </Button>
              <Button
                type="primary"
                htmlType="submit"
                loading={createJournalEntryMutation.isPending}
              >
                Create Journal Entry
              </Button>
            </div>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
} 
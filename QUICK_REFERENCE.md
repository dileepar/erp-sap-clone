# SAP Clone ERP - Quick Reference Guide

![Quick Reference](https://img.shields.io/badge/Quick-Reference-0070e0?style=flat-square)

## 🚀 Getting Started

### Access System
```
URL: http://localhost:3000
Browser: Chrome, Firefox, Safari, Edge (latest)
```

### Navigation
- **Dashboard**: 🏠 Main overview
- **Financial**: 💰 Accounting functions
- **HR**: 👥 Employee management

---

## 💰 Financial Management

### Create Account
1. Financial → Chart of Accounts
2. Click "Create Account"
3. Fill required fields:
   - Account Number (4-10 digits)
   - Account Name
   - Account Type (Asset/Liability/Equity/Revenue/Expense)
   - Currency (default: USD)
   - Description
4. Click "Create Account"

### Create Journal Entry
1. Financial → Journal Entries
2. Click "Create Journal Entry"
3. Fill header:
   - Posting Date
   - Document Date
   - Reference
   - Description
4. Add line items (minimum 2):
   - Select Account
   - Enter Description
   - Enter Debit OR Credit amount
5. Ensure **Debits = Credits**
6. Click "Create Journal Entry"

### Account Types Quick Reference
| Type | Debit | Credit |
|------|-------|--------|
| **Asset** | Increase | Decrease |
| **Liability** | Decrease | Increase |
| **Equity** | Decrease | Increase |
| **Revenue** | Decrease | Increase |
| **Expense** | Increase | Decrease |

---

## 👥 Human Resources

### Add Employee
1. HR → Employees
2. Click "Add Employee"
3. Fill required fields:
   - Employee Number (e.g., EMP001)
   - First Name, Last Name
   - Email (required)
   - Phone (optional)
   - Position, Department
   - Hire Date, Salary
4. Click "Add Employee"

### Employee Statistics
- **Total Employees**: Current count
- **Active Employees**: Currently employed
- **Average Salary**: Calculated average

---

## 🎯 Common Journal Entry Examples

### Cash Sale
```
Debit: Cash (Asset)           $1,000
Credit: Sales Revenue         $1,000
```

### Purchase Supplies
```
Debit: Office Supplies (Expense)  $500
Credit: Cash (Asset)               $500
```

### Pay Salary
```
Debit: Salary Expense             $5,000
Credit: Cash (Asset)              $5,000
```

### Receive Payment
```
Debit: Cash (Asset)               $2,000
Credit: Accounts Receivable       $2,000
```

---

## 🔧 Quick Fixes

### Form Won't Submit
- ✅ Check required fields (red asterisk)
- ✅ Verify email format
- ✅ Ensure dates are valid
- ✅ Check number formats

### Page Won't Load
- ✅ Refresh browser (Ctrl+F5)
- ✅ Clear cache
- ✅ Check internet connection
- ✅ Try different browser

### Journal Entry Won't Balance
- ✅ Verify Total Debits = Total Credits
- ✅ Check decimal places
- ✅ Ensure all amounts are positive
- ✅ Add missing line items

---

## ⌨️ Keyboard Shortcuts

| Action | Shortcut |
|--------|----------|
| Refresh Page | `Ctrl+F5` / `Cmd+Shift+R` |
| Close Modal | `Escape` |
| Submit Form | `Ctrl+Enter` |
| Focus Search | `Ctrl+F` / `Cmd+F` |

---

## 📊 Data Formats

### Dates
- **Format**: DD/MM/YYYY
- **Example**: 15/01/2024

### Currency
- **Format**: $X,XXX.XX
- **Example**: $1,250.75

### Account Numbers
- **Format**: 4-10 digits
- **Example**: 1000, 10001

### Employee Numbers
- **Format**: EMPXXX
- **Example**: EMP001, EMP002

---

## 🎨 UI Elements

### Button Colors
- **Blue**: Primary actions (Create, Submit)
- **Gray**: Secondary actions (Cancel, Close)
- **Red**: Danger actions (Delete)

### Status Indicators
- **Green**: Active, Posted, Success
- **Orange**: Warning, Draft
- **Red**: Error, Inactive
- **Blue**: Information

### Form Controls
- **Height**: 40px (consistent)
- **Required**: Red asterisk (*)
- **Validation**: Real-time error messages

---

## 📞 Support

### Before Contacting Support
1. ✅ Check this quick reference
2. ✅ Try refreshing the page
3. ✅ Clear browser cache
4. ✅ Review error messages

### Information to Provide
- Current page/module
- Error message (if any)
- Steps to reproduce issue
- Browser and version

---

*Keep this guide handy for quick reference while using SAP Clone ERP!*

**Last Updated**: January 2024 
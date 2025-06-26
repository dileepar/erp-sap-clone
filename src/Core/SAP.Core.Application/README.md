# SAP.Core.Application

## 📋 **Purpose**
This project contains the application services, use cases, and business logic orchestration for the SAP Clone ERP system.

## 🏗️ **Architecture**
Implements the Application layer in Clean Architecture with CQRS and Event Sourcing patterns.

## 📦 **What Goes Here**
- **Commands**: Actions that change system state
- **Queries**: Read-only operations for data retrieval
- **Command Handlers**: Process commands with business logic
- **Query Handlers**: Execute queries and return DTOs
- **Application Services**: Coordinate between domain and infrastructure
- **DTOs**: Data Transfer Objects for API contracts

## 🔄 **CQRS + Event Sourcing**
Using [Marten + Wolverine][[memory:1707800442445114549]] for:
- Command handling with event sourcing
- Query optimization with projections
- Message routing and processing

## 🎯 **ERP Use Cases**
When implemented, this will contain:
- Financial transactions and reporting
- Employee management workflows
- Order processing and fulfillment
- Customer relationship management

## 🔗 **Dependencies**
- SAP.Core.Domain (domain models)
- SAP.Core.Contracts (interfaces and DTOs)
- Marten (event sourcing)
- WolverineFx (messaging)

---
*Part of the [SAP Clone ERP system][[memory:1707800442445114549]] - Application layer implementation* 
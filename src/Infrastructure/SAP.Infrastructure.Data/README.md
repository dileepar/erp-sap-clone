# SAP.Infrastructure.Data

## 📋 **Purpose**
This project contains the data access implementation using Marten for event sourcing and PostgreSQL integration.

## 🏗️ **Architecture**
Implements the Infrastructure layer for data persistence with event sourcing patterns.

## 📦 **What Goes Here**
- **Repository Implementations**: Concrete data access classes
- **Marten Configuration**: Event store and document database setup
- **Event Store**: Event sourcing implementation
- **Projections**: Read model generation from events
- **Database Migrations**: Schema management
- **Connection Management**: PostgreSQL connection handling

## 🔄 **Event Sourcing with Marten**
Using [Marten 8.3.0][[memory:1707800442445114549]] for:
- Event store with PostgreSQL
- Aggregate persistence and retrieval
- Optimized projection rebuilds
- Identity map for aggregates

## 🗄️ **Database Features**
- **PostgreSQL 15**: Modern relational database
- **Event Streams**: Audit trail for all changes
- **Projections**: Optimized read models
- **Multi-tenancy**: Support for multiple companies

## 🔗 **Dependencies**
- SAP.Core.Domain (domain models)
- SAP.Core.Contracts (repository interfaces)
- Marten (event sourcing framework)
- Npgsql (PostgreSQL driver)

---
*Part of the [SAP Clone ERP system][[memory:1707800442445114549]] - Data persistence implementation* 
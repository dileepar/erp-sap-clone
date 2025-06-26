# SAP.Infrastructure.Messaging

## 📋 **Purpose**
This project contains the messaging and event handling implementation using WolverineFx for event-driven architecture.

## 🏗️ **Architecture**
Implements the Infrastructure layer for messaging, event handling, and cross-cutting concerns.

## 📦 **What Goes Here**
- **Message Handlers**: Process domain events and commands
- **Event Publishers**: Publish events to message queues
- **Integration Events**: Cross-bounded context communication
- **Sagas/Process Managers**: Long-running business processes
- **Message Routing**: Configure message flow and processing
- **External Integrations**: Third-party service integration

## 🔄 **Event-Driven Architecture**
Using [WolverineFx 4.4.0][[memory:1707800442445114549]] for:
- Message routing and handling
- Transactional outbox pattern
- Saga/process manager support
- Aggregate handler workflow

## 📨 **Messaging Patterns**
- **Command Handling**: Process business commands
- **Event Sourcing Integration**: Handle domain events
- **Saga Management**: Coordinate multi-step processes
- **Integration Events**: External system communication

## 🔗 **Dependencies**
- SAP.Core.Domain (domain events)
- SAP.Core.Contracts (messaging contracts)
- WolverineFx (messaging framework)
- Marten integration for event sourcing

---
*Part of the [SAP Clone ERP system][[memory:1707800442445114549]] - Messaging and event handling* 
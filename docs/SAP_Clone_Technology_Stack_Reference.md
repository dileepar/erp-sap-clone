# SAP Clone Technology Stack Reference
*Last Updated: January 2025*

## 🚀 **Backend Stack: .NET 9**

### .NET 9 Key Features (Released November 2024)
- **Version**: Latest LTS release
- **Performance Improvements**:
  - Native AOT compilation: 30-40% less memory usage
  - Dynamic PGO: 15% faster startup times
  - Enhanced GC: 8-12% less memory overhead
  - ASP.NET Core: 20% faster requests, 25% reduced latency
  - Better SIMD vectorization and AVX512 support

### Configuration Best Practices
```xml
<PropertyGroup>
  <TieredPGO>true</TieredPGO>
  <PublishAot>true</PublishAot>
</PropertyGroup>
```

### Package Installation
```bash
dotnet new web -n SAP.API --framework net9.0
dotnet add package Marten --version 8.0.0
dotnet add package Wolverine.Marten --version 3.0.0
```

## 🎯 **Frontend Stack: React 19 + TanStack**

### TanStack Ecosystem (Latest Versions)
- **React**: 19 (latest stable)
- **TanStack Router**: v1 (type-safe file-based routing)
- **TanStack Query**: v5 (advanced data fetching and caching)
- **TanStack Table**: v8.21+ (high-performance data tables)
- **TanStack Start**: Beta (full-stack React framework)
- **TanStack Form**: Latest (type-safe form management)

### Key Benefits
- Framework-agnostic
- Excellent TypeScript support
- Zero-configuration caching
- Declarative auto-managed queries
- Background updates

### Installation Commands
```bash
npx create-react-app@latest sap-web --template typescript
npm install @tanstack/react-router@latest
npm install @tanstack/react-query@latest
npm install @tanstack/react-table@latest
npm install @tanstack/react-form@latest
```

## 🎨 **UI Components: UI5 Web Components for React**

### Package Information
- **Package**: @ui5/webcomponents-react
- **Version**: v2.11+ (latest)
- **Weekly Downloads**: 26,979+
- **Maintainer**: SAP (official)

### Features
- Enterprise-grade components
- SAP Fiori design language
- Built-in accessibility
- Theming support
- Internationalization (i18n)
- Framework-agnostic web standards

### Installation
```bash
npm install @ui5/webcomponents-react @ui5/webcomponents @ui5/webcomponents-fiori
```

### Basic Usage
```typescript
import { Button, Table, Card } from '@ui5/webcomponents-react'

function Dashboard() {
  return (
    <Card heading="Financial Overview">
      <Table data={data} columns={columns} />
      <Button design="Emphasized">Create Entry</Button>
    </Card>
  )
}
```

## 🔄 **Event Sourcing: Marten + Wolverine**

### Marten v8+ Features
- .NET transactional document database
- PostgreSQL-based event store
- Native .NET 9 integration
- Optimized projection rebuilds
- Identity map for aggregates
- Async daemon processing
- Side effects in projections

### Wolverine Integration
- Message routing and handling
- Transactional outbox pattern
- Saga/process manager support
- Aggregate handler workflow
- Event-driven architecture

### Configuration
```csharp
builder.Services.AddMarten(opts =>
{
    opts.Connection(connectionString);
    opts.Events.UseOptimizedProjectionRebuilds = true;
    opts.Events.UseIdentityMapForAggregates = true;
}).IntegrateWithWolverine();
```

### Event Sourcing Example
```csharp
// Event Definition
public record JournalEntryCreated(
    Guid JournalEntryId,
    Guid CompanyId,
    DateOnly PostingDate,
    string Reference,
    List<JournalEntryLine> Lines);

// Aggregate Root
public class JournalEntry
{
    public Guid Id { get; private set; }
    public JournalEntryStatus Status { get; private set; }
    
    public void Apply(JournalEntryCreated @event)
    {
        Id = @event.JournalEntryId;
        // Apply event logic
    }
}

// Command Handler
[AggregateHandler]
public static IEnumerable<object> Handle(CreateJournalEntry command, JournalEntry entry)
{
    yield return new JournalEntryCreated(
        command.Id,
        command.CompanyId,
        command.PostingDate,
        command.Reference,
        command.Lines);
}
```

## 🏗️ **Architecture Patterns**

### Project Structure
```
SAP-Clone/
├── src/
│   ├── Core/                           # Domain & Business Logic
│   │   ├── SAP.Core.Domain/           # Aggregates, Entities, Events
│   │   ├── SAP.Core.Application/      # Use Cases, Commands, Queries
│   │   └── SAP.Core.Contracts/        # Interfaces & DTOs
│   ├── Infrastructure/                # Data & External Services
│   │   ├── SAP.Infrastructure.Data/   # Marten, Event Store
│   │   ├── SAP.Infrastructure.Messaging/ # Wolverine Integration
│   │   └── SAP.Infrastructure.External/   # APIs, File System
│   ├── API/                          # Web API Layer
│   │   └── SAP.API/                  # ASP.NET Core Web API
│   └── Web/                          # Frontend
│       └── SAP.Web/                  # React 19 + TanStack
└── tests/                            # All test projects
```

### Design Patterns
- **Clean Architecture**: Separation of concerns
- **CQRS**: Command Query Responsibility Segregation
- **Event Sourcing**: Audit trail and scalability
- **Domain-Driven Design**: Business logic focus
- **Aggregate Pattern**: Consistency boundaries

## 📊 **ERP Modules to Implement**

### 1. Financial Management (FI)
- Chart of Accounts
- Journal Entries (event-sourced)
- General Ledger
- Accounts Payable/Receivable
- Financial Reporting
- Multi-currency support

### 2. Human Resources (HR)
- Employee lifecycle events
- Payroll processing
- Time tracking
- Performance management
- Benefits administration

### 3. Supply Chain Management (SCM)
- Inventory management
- Purchase orders
- Vendor management
- Warehouse operations
- Demand planning

### 4. Sales & Distribution (SD)
- Customer management
- Sales orders
- Pricing and discounts
- Shipping and fulfillment
- Customer service

## 🚀 **Deployment Strategy**

### Containerization
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["SAP.API/SAP.API.csproj", "SAP.API/"]
RUN dotnet restore "SAP.API/SAP.API.csproj"
COPY . .
WORKDIR "/src/SAP.API"
RUN dotnet build "SAP.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SAP.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SAP.API.dll"]
```

### Cloud Options
- **Azure**: App Service + Static Web Apps + PostgreSQL
- **AWS**: ECS + S3/CloudFront + RDS
- **Google Cloud**: Cloud Run + Cloud Storage + Cloud SQL

## 🔧 **Development Commands**

### Backend Setup
```bash
# Create solution
dotnet new sln -n SAP-Clone

# Create projects
dotnet new classlib -n SAP.Core.Domain
dotnet new classlib -n SAP.Core.Application
dotnet new classlib -n SAP.Infrastructure.Data
dotnet new web -n SAP.API --framework net9.0

# Add project references
dotnet sln add **/*.csproj
```

### Frontend Setup
```bash
# Create React app
npx create-react-app@latest sap-web --template typescript
cd sap-web

# Install dependencies
npm install @tanstack/react-router@latest
npm install @tanstack/react-query@latest
npm install @tanstack/react-table@latest
npm install @ui5/webcomponents-react@latest
```

### Database Setup (PostgreSQL with Docker)
```yaml
# docker-compose.yml
version: '3.8'
services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: sapclone
      POSTGRES_USER: admin
      POSTGRES_PASSWORD: password
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

## 📚 **Key Resources**

### Official Documentation
- [.NET 9 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/)
- [TanStack Documentation](https://tanstack.com/)
- [UI5 Web Components](https://sap.github.io/ui5-webcomponents/)
- [Marten Documentation](https://martendb.io/)
- [Wolverine Documentation](https://wolverine.netlify.app/)

### Performance References
- .NET 9: 15% startup improvement, 30-40% memory reduction
- TanStack Query: Intelligent caching and background updates
- Marten: Optimized for high-throughput event sourcing
- UI5: Enterprise-grade performance and accessibility

---

*This document serves as the definitive technology reference for the SAP clone project. Update as needed when new versions or information become available.* 
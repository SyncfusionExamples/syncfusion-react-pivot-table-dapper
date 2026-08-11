# SQL Server Sales Analytics: Interactive Pivot Table Dashboard

**Bind and perform CRUD operations with interactive sales analytics using SQL Server data and Dapper**

**SQL Server Pivot Table Dashboard** is a full-stack business intelligence sample that combines a **React + TypeScript frontend** with an **ASP.NET Core backend** connected to a **Microsoft SQL Server database** through the lightweight **Dapper** micro-ORM.

## 🔄 How It Works

The application uses **Syncfusion DataManager** to handle real-time CRUD operations through an interactive pivot table interface:

- **React Client**: Renders the Syncfusion pivot table UI and lets users visualize and analyze sales data.
- **DataManager**: Sends HTTP requests for read, insert, update, and delete operations.
- **ASP.NET Core Backend**: Exposes REST endpoints under the Sales controller for data operations.
- **Dapper + Microsoft.Data.SqlClient**: Executes parameterized SQL against the SQL Server `salesdata` table.
- **Microsoft SQL Server**: Stores sales records and serves them to the UI through the backend.

The complete workflow is: user interaction in the pivot table → DataManager request → ASP.NET Core controller → Dapper query → SQL Server → UI refresh.

---

## 📋 Table of Contents

- [Quick Overview](#quick-overview)
- [Key Features](#key-features)
- [Technology Stack](#technology-stack)
- [Installation & Setup](#installation--setup)
- [Configuration](#configuration)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Troubleshooting](#troubleshooting)

---

## 🎯 Quick Overview

This sample provides an intuitive experience for exploring sales data without writing SQL. Users can:

- Visualize sales data by country, region, category, product, and salesperson
- Create, update, and delete records directly from the pivot table experience
- Drill into details and edit records inline
- Reorganize the pivot table layout with drag-and-drop fields

### Why This Stack?

- **Type-safe frontend** with React and TypeScript
- **Reliable backend** with ASP.NET Core and .NET 10
- **Lightweight data access** with Dapper and Microsoft.Data.SqlClient
- **Enterprise-grade storage** with Microsoft SQL Server
- **Professional UI** with Syncfusion pivot components

---

## ✨ Key Features

- Interactive pivot table with drag-and-drop field configuration
- End-to-end CRUD support for sales records
- Dapper-based data access with parameterized SQL for safe, fast queries
- Seamless frontend-to-backend data synchronization
- Responsive layout for desktop and tablet use
- Date and numeric formatting for sales analysis

---

## 🛠️ Technology Stack

### Frontend
| Technology | Version | Purpose |
|-----------|---------|---------|
| React | 19.2.8 | UI framework |
| TypeScript | ~6.0.2 | Type-safe JavaScript |
| Vite | 8.2.0 | Build and development tool |
| Syncfusion EJ2 | 34.2.2 | Pivot table and DataManager support |

### Backend
| Technology | Version | Purpose |
|-----------|---------|---------|
| .NET | 10.0 | Web API framework |
| ASP.NET Core | 10.0 | Backend application host |
| Dapper | 2.1.79 | Lightweight micro-ORM for data access |
| Microsoft.Data.SqlClient | 7.0.2 | SQL Server connectivity |
| Syncfusion EJ2 | 34.2.2 | DataManager integration |

### Database
| Technology | Purpose |
|-----------|---------|
| Microsoft SQL Server | Primary relational database |
| Microsoft.Data.SqlClient | .NET provider for SQL Server |

---

## 📦 Installation & Setup

### Prerequisites

- .NET 10 SDK
- Node.js 18+ and npm
- Microsoft SQL Server 2019 or later (Express edition is fine)
- Ports 1433, 7086, and 5173 available

### Database Setup

1. Open SQL Server Management Studio (or `sqlcmd`) and connect to your local instance.

2. Create the sales database:

```sql
CREATE DATABASE salesdb;
```

3. Switch to the new database and create the `salesdata` table:

```sql
USE salesdb;
GO

CREATE TABLE dbo.salesdata (
  orderid          INT IDENTITY(1,1) PRIMARY KEY,
  customername     NVARCHAR(255),
  region           NVARCHAR(255),
  country          NVARCHAR(255),
  productcategory  NVARCHAR(255),
  productname      NVARCHAR(255),
  orderdate        DATE,
  quantity         INT,
  unitprice        DECIMAL(10,2),
  totalamount      DECIMAL(10,2),
  salesperson      NVARCHAR(255)
);
```

4. Optionally seed the table with sample records.

### Backend Installation

1. Navigate to the backend folder:

```bash
cd PivotTable_Dapper.Server
```

2. Restore dependencies:

```bash
dotnet restore
```

3. Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SalesDb": "Server=localhost;Database=salesdb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> If you use Windows Authentication, replace the `User Id` and `Password` with `Integrated Security=True;`.

4. Build and run the backend:

```bash
dotnet build
dotnet run
```

The backend will run at https://localhost:7086.

### Frontend Installation

1. Navigate to the frontend folder:

```bash
cd pivottable_dapper.client
```

2. Install dependencies:

```bash
npm install
```

3. Start the development server:

```bash
npm run dev
```

The frontend will be available at http://localhost:5173.

---

## ⚙️ Configuration

### Backend Configuration

The backend reads the SQL Server connection string from `appsettings.json` using the `SalesDb` key. Dapper uses `Microsoft.Data.SqlClient` under the hood to open the connection and run parameterized queries against the `dbo.salesdata` table.

```json
{
  "ConnectionStrings": {
    "SalesDb": "Server=localhost;Database=salesdb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

### Frontend Configuration

The Vite app is already configured for local development. If needed, update the API URL in the frontend code (`pivottable_dapper.client/src/App.tsx`) to match your backend host.

---

## 🚀 Getting Started

1. Start the Microsoft SQL Server service.
2. Create the database and `dbo.salesdata` table shown above.
3. Update the backend connection string in `appsettings.json`.
4. Run the backend with `dotnet run` (defaults to https://localhost:7086).
5. Run the frontend with `npm run dev` (defaults to http://localhost:5173).
6. Open http://localhost:5173 in your browser.

You should see the pivot table load with sales data and allow CRUD operations.

---

## 📁 Project Structure

```text
syncfusion-react-pivot-table-dapper-multi-database-binding-sample/
├── README.md
├── pivottable_dapper.client/
│   ├── package.json
│   ├── src/
│   │   ├── App.tsx
│   │   ├── main.tsx
│   │   └── App.css
│   └── vite.config.ts
└── PivotTable_Dapper.Server/
    ├── Program.cs
    ├── appsettings.json
    ├── Controllers/
    │   └── SalesController.cs
    └── PivotTable_Dapper.Server.csproj
```

### Key Files

- [pivottable_dapper.client/src/App.tsx](pivottable_dapper.client/src/App.tsx): Main React component, DataManager configuration, and pivot layout
- [PivotTable_Dapper.Server/Controllers/SalesController.cs](PivotTable_Dapper.Server/Controllers/SalesController.cs): API endpoints for CRUD operations using Dapper
- [PivotTable_Dapper.Server/Program.cs](PivotTable_Dapper.Server/Program.cs): Service registration and CORS setup
- [PivotTable_Dapper.Server/appsettings.json](PivotTable_Dapper.Server/appsettings.json): SQL Server connection settings

---

## 🔧 Troubleshooting

### Common Issues

- **Cannot connect to the backend**: Make sure the ASP.NET Core app is running and CORS is enabled. Confirm the React client origin matches the configured CORS policy in `Program.cs`.
- **SQL Server connection failed**: Verify the server, database name, and credentials in `appsettings.json`. For localdb or named instances, adjust the `Server` value accordingly. If your SQL Server uses a self-signed certificate, keep `TrustServerCertificate=True` for local development.
- **No data appears**: Confirm the `dbo.salesdata` table exists in the `salesdb` database and contains records.
- **Frontend build error**: Run `npm install` again and ensure Node.js is up to date.
- **Identity insert error on Insert**: Make sure the `orderid` column is an `IDENTITY` column. Dapper returns the new value via `OUTPUT INSERTED.orderid`, so the column must auto-generate.

### Useful checks

```sql
SELECT name FROM sys.databases;
GO
SELECT TOP 10 * FROM dbo.salesdata;
```

```bash
dotnet build
```

---

## 📜 License

This sample is intended for demo and development use. Please review the Syncfusion licensing terms before using it in production.

import { useState, useEffect } from 'react';
import { DrillThrough, FieldList, Inject, PivotViewComponent } from '@syncfusion/ej2-react-pivotview';
import { DataManager, UrlAdaptor } from '@syncfusion/ej2-data';
import './App.css';

function App() {
  const [stats, setStats] = useState({
    totalSales: 0,
    totalOrders: 0,
    avgOrderValue: 0,
    totalQuantity: 0
  });

  let pivotObj: PivotViewComponent;

  // Initialize DataManager with the Web API endpoint
  let data: DataManager = new DataManager({
    url: 'https://localhost:7086/api/Sales',                    // Data retrieval endpoint
    insertUrl: 'https://localhost:7086/api/Sales/Insert',       // Called when user adds a new record
    updateUrl: 'https://localhost:7086/api/Sales/Update',       // Called when user edits an existing record
    removeUrl: 'https://localhost:7086/api/Sales/Remove',       // Called when user deletes a record
    adaptor: new UrlAdaptor                                    // Uses the standard URL adaptor for HTTP communication
  });

  // Configure the Pivot Table data structure
  const dataSourceSettings = {
    dataSource: data,
    expandAll: true,
    rows: [{ name: 'country', caption: 'Country' }],
    columns: [{ name: 'productCategory', caption: 'Product Category' }],
    values: [{ name: 'quantity', caption: 'Quantity' }, { name: 'totalAmount', caption: 'Total Amount' }],
    filters: [],
    formatSettings: [{ name: 'orderDate', format: 'dd/MM/yyyy-hh:mm', type: 'date' }],
    fieldMapping: [{ name: 'orderDate', caption: 'Order Date' }, { name: 'orderID', caption: 'Order ID' }, { name: 'customerName', caption: 'Customer Name' }, { name: 'region', caption: 'Region' }, { name: 'salesPerson', caption: 'Sales Person' }, { name: 'productName', caption: 'Product Name' }, { name: 'unitPrice', caption: 'Unit Price' }]
  }

  // Enable editing functionality
  const editSettings = {
    allowEditing: true,    // Enables the Edit button and allows users to modify existing records
    allowAdding: true,     // Enables the Add button and allows users to create new records
    allowDeleting: true,   // Enables the Delete button and allows users to remove records
    mode: 'Normal'         // Uses Normal mode (popup dialog) for editing; other options: 'Dialog', 'Batch'
  } as any;

  // Configure beginDrillThrough event to set the primary key for CRUD operations
  function beginDrillThrough(args: any) {
    // Iterate through all columns in the drill-through grid
    for (var i = 0; i < args.gridObj.columns.length; i++) {
      // Check if the current column is the primary key column
      if (args.gridObj.columns[i].field == "orderID") {
        // Mark this column as the primary key
        // This tells DataManager to use this column's value to uniquely identify records
        args.gridObj.columns[i].isPrimaryKey = true;
      } else {
        // Make all other columns visible so users can view and edit them
        args.gridObj.columns[i].visible = true;
        // Configure the edit type for date field to use a date picker for editing
        if (args.gridObj.columns[i].field === 'orderDate') {
          args.gridObj.columns[i].editType = 'datetimepickeredit';
        }
      }
    }
  }

  // Fetch and calculate statistics
  const fetchStatistics = async () => {
    try {
      const response = await fetch('https://localhost:7086/api/Sales');
      const result = await response.json();

      if (result && Array.isArray(result) && result.length > 0) {
        const orders = result;
        const totalSales = orders.reduce((sum: number, order: any) => sum + (order.totalAmount || 0), 0);
        const totalQty = orders.reduce((sum: number, order: any) => sum + (order.quantity || 0), 0);

        setStats({
          totalSales: parseFloat(totalSales.toFixed(2)),
          totalOrders: orders.length,
          avgOrderValue: parseFloat((totalSales / orders.length).toFixed(2)),
          totalQuantity: totalQty
        });
      }
    } catch (error) {
      console.error('Error fetching statistics:', error);
    }
  };

  // Update the statistics cards whenever the pivot table data is bound/refreshed
  // (including after performing any CRUD operations).
  function dataBound(_args: any) {
    fetchStatistics();
  }

  useEffect(() => {
    fetchStatistics();
  }, []);

  return (
    <div className="dashboard-container">
      {/* Header Section */}
      <div className="dashboard-header">
        <div className="header-content">
          <h1 className="dashboard-title">📊 Sales Analytics Dashboard</h1>
          <p className="dashboard-subtitle">Interactive Pivot Table with Real-time CRUD Operations</p>
        </div>
      </div>

      {/* Statistics Cards Section */}
      <div className="stats-section">
        <div className="stat-card">
          <div className="stat-icon">💰</div>
          <div className="stat-content">
            <p className="stat-label">Total Sales</p>
            <p className="stat-value">${stats.totalSales.toLocaleString()}</p>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon">📦</div>
          <div className="stat-content">
            <p className="stat-label">Total Orders</p>
            <p className="stat-value">{stats.totalOrders}</p>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon">📈</div>
          <div className="stat-content">
            <p className="stat-label">Avg Order Value</p>
            <p className="stat-value">${stats.avgOrderValue.toLocaleString()}</p>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon">🎯</div>
          <div className="stat-content">
            <p className="stat-label">Total Quantity</p>
            <p className="stat-value">{stats.totalQuantity.toLocaleString()}</p>
          </div>
        </div>
      </div>

      {/* Main Content Section */}
      <div className="dashboard-content">
        <div className="pivot-section">
          <div className="pivot-header">
            <h2>Sales Pivot Analysis</h2>
            <p className="pivot-subtitle">Drag fields to customize your analysis • Double-click cells to edit</p>
          </div>

          <div className="pivot-table-wrapper">
            <PivotViewComponent
              id='PivotView' 
              ref={(scope: any) => { pivotObj = scope; }} 
              height='100%'
              dataSourceSettings={dataSourceSettings} 
              editSettings={editSettings}
              dataBound={dataBound} 
              beginDrillThrough={beginDrillThrough} 
              showFieldList={true}
              gridSettings={{ columnWidth: 120 }}
            >
              <Inject services={[FieldList, DrillThrough]} />
            </PivotViewComponent>
          </div>
        </div>
      </div>

      {/* Footer Section */}
      <div className="dashboard-footer">
        <p>&copy; 2026 Sales Analytics Dashboard | MySQL + React + ASP.NET Core</p>
      </div>
    </div>
  );
}

export default App;

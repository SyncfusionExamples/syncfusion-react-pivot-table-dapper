using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Dapper;
using Microsoft.Data.SqlClient;

namespace PivotTable_Dapper.Server.Controllers
{
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly string _connectionString;

        public SalesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SalesDb")
                ?? throw new InvalidOperationException(
                    "Connection string 'SalesDb' is not configured.");
        }

        /// <summary>
        /// Handles the DataManager request and returns data to the client.
        /// </summary>
        /// <param name="DataManagerRequest">Contains the details of the data operation requested.</param>
        /// <returns>Returns the data records along with the total count.</returns>
        [HttpPost]
        [Route("api/[controller]")]
        public async Task<object> Post([FromBody] object DataManagerRequest)
        {
            // Retrieve data from the data source (database).
            List<SalesData> salesData = await GetSalesData();
            IQueryable<SalesData> DataSource = salesData.AsQueryable();
            int totalRecordsCount = DataSource.Count();
            // Return data based on the request.
            return new { result = DataSource, count = totalRecordsCount };
        }

        /// <summary>
        /// Retrieves the sales data from the database using Dapper.
        /// </summary>
        /// <returns>Returns a list of SalesData records fetched from the database.</returns>
        [HttpGet]
        [Route("api/[controller]")]
        public async Task<List<SalesData>> GetSalesData()
        {
            const string Query = @"SELECT * FROM dbo.salesdata ORDER BY orderid;";

            using var Connection = new SqlConnection(_connectionString);
            await Connection.OpenAsync();

            // Dapper's QueryAsync<T> maps query results directly to the SalesData model.
            var DataSource = (await Connection.QueryAsync<SalesData>(Query)).ToList();
            return DataSource;
        }

        /// <summary>
        /// Data model that represents the structure of a sales record.
        /// This class maps to the columns in the 'salesdata' table in MySQL.
        /// </summary>
        public class SalesData
        {
            /// <summary>
            /// Unique identifier for each order (Primary Key).
            /// The [Key] attribute marks this as the primary key for CRUD operations.
            /// </summary>
            [Key]
            public int? OrderID { get; set; }

            /// <summary>
            /// Name of the customer who placed the order.
            /// </summary>
            public string? CustomerName { get; set; }

            /// <summary>
            /// Geographic region where the customer is located.
            /// Useful for regional analysis in the pivot table.
            /// </summary>
            public string? Region { get; set; }

            /// <summary>
            /// Country where the order was placed.
            /// </summary>
            public string? Country { get; set; }

            /// <summary>
            /// Category of the product (e.g., Electronics, Furniture).
            /// Used as a dimension in the pivot table analysis.
            /// </summary>
            public string? ProductCategory { get; set; }

            /// <summary>
            /// Name of the specific product ordered.
            /// </summary>
            public string? ProductName { get; set; }

            /// <summary>
            /// Date when the order was placed.
            /// </summary>
            public DateTime? OrderDate { get; set; }

            /// <summary>
            /// Number of units ordered.
            /// </summary>
            public int? Quantity { get; set; }

            /// <summary>
            /// Price per unit of the product.
            /// </summary>
            public decimal UnitPrice { get; set; }

            /// <summary>
            /// Total cost of the order (typically quantity × unitprice).
            /// Used as a measure/aggregate value in pivot table analysis.
            /// </summary>
            public decimal TotalAmount { get; set; }

            /// <summary>
            /// Name of the sales representative who handled the order.
            /// </summary>
            public string? SalesPerson { get; set; }
        }

        /// <summary>
        /// Generic model for handling CRUD operations from the Pivot Table.
        /// The Pivot Table uses this structure to send data to Insert, Update, and Remove endpoints.
        /// </summary>
        /// <typeparam name="T">The data type (e.g., SalesData)</typeparam>
        public class CRUDModel<T> where T : class
        {
            /// <summary>
            /// Action being performed (e.g., "insert", "update", "remove").
            /// </summary>
            public string? Action { get; set; }

            /// <summary>
            /// Primary key column name (e.g., "orderid").
            /// </summary>
            public string? KeyColumn { get; set; }

            /// <summary>
            /// The primary key value (e.g., the OrderID).
            /// </summary>
            public object? Key { get; set; }

            /// <summary>
            /// The single record being operated on (for Insert, Update operations).
            /// </summary>
            public T? Value { get; set; }

            /// <summary>
            /// Additional parameters sent by the client.
            /// </summary>
            public IDictionary<string, object>? Params { get; set; }
        }

        /// <summary>
        /// Inserts a new sales record into the database using Dapper.
        /// This method is called when a new row is added in the Pivot Table.
        /// </summary>
        /// <param name="model">Contains the new sales data to insert.</param>
        /// <returns>Returns the inserted record with its new OrderID.</returns>
        [HttpPost]
        [Route("api/[controller]/Insert")]
        public async Task<IActionResult> Insert([FromBody] CRUDModel<SalesData> model)
        {
            if (model?.Value == null)
                return BadRequest("A sales record is required.");

            if (string.IsNullOrWhiteSpace(model.Value.CustomerName) ||
                string.IsNullOrWhiteSpace(model.Value.Country) ||
                model.Value.OrderDate == null ||
                model.Value.Quantity == null ||
                model.Value.Quantity <= 0 ||
                model.Value.UnitPrice < 0)
            {
                return BadRequest("Required fields, a positive quantity, and a non-negative unit price are required.");
            }

            try
            {
                model.Value.TotalAmount =
                    model.Value.Quantity.Value * model.Value.UnitPrice;
                const string sql = @"
            INSERT INTO dbo.salesdata
            (customername, region, country, productcategory, productname, orderdate, quantity, unitprice, totalamount, salesperson)
            OUTPUT INSERTED.orderid
            VALUES
            (@CustomerName, @Region, @Country, @ProductCategory, @ProductName, @OrderDate, @Quantity, @UnitPrice, @TotalAmount, @SalesPerson);
        ";

                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                // Dapper's ExecuteScalarAsync<T> returns the first column of the
                // first row from the executed query, which in this case is the
                // newly generated identity value.
                var newId = await conn.ExecuteScalarAsync<int>(sql, model.Value);

                // Update the model with the new ID
                model.Value.OrderID = newId;

                // UrlAdaptor expects { key, value, action } on insert.
                return Ok(new { key = newId, value = model.Value, action = "insert" });
            }
            catch (Exception)
            {
                // Log the exception in a real application. Returning a generic
                // message avoids leaking database or stack details to the client.
                return StatusCode(500, new { error = "Insert failed." });
            }
        }

        /// <summary>
        /// Updates an existing sales record in the database using Dapper.
        /// This method is called when a row is edited in the Pivot Table.
        /// </summary>
        /// <param name="model">Contains the updated sales data.</param>
        /// <returns>Returns the updated record.</returns>
        [HttpPost]
        [Route("api/[controller]/Update")]
        public async Task<IActionResult> Update([FromBody] CRUDModel<SalesData> model)
        {
            if (model?.Value == null)
                return BadRequest("A sales record is required.");

            if (model.Value.OrderID == null)
                return BadRequest("OrderID is required.");

            if (string.IsNullOrWhiteSpace(model.Value.CustomerName) ||
                string.IsNullOrWhiteSpace(model.Value.Country) ||
                model.Value.OrderDate == null ||
                model.Value.Quantity == null ||
                model.Value.Quantity <= 0 ||
                model.Value.UnitPrice < 0)
            {
                return BadRequest("Required fields, a positive quantity, and a non-negative unit price are required.");
            }

            try
            {
                model.Value.TotalAmount =
                    model.Value.Quantity.Value * model.Value.UnitPrice;
                const string sql = @"
            UPDATE dbo.salesdata
            SET customername    = @CustomerName,
                region          = @Region,
                country         = @Country,
                productcategory = @ProductCategory,
                productname     = @ProductName,
                orderdate       = @OrderDate,
                quantity        = @Quantity,
                unitprice       = @UnitPrice,
                totalamount     = @TotalAmount,
                salesperson     = @SalesPerson
            WHERE orderid = @OrderID;
        ";

                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                // Dapper's Execute maps the SalesData properties to the SQL
                // parameters automatically and returns the number of affected rows.
                await conn.ExecuteAsync(sql, model.Value);

                // UrlAdaptor expects { key, value, action } on update.
                return Ok(new { key = model.Value.OrderID, value = model.Value, action = "update" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Update failed." });
            }
        }

        /// <summary>
        /// Deletes a sales record from the database using Dapper.
        /// This method is called when a row is deleted in the Pivot Table.
        /// </summary>
        /// <param name="model">Contains the OrderID of the record to delete.</param>
        /// <returns>Returns the deleted key.</returns>
        [HttpPost]
        [Route("api/[controller]/Remove")]
        public async Task<IActionResult> Remove([FromBody] CRUDModel<SalesData> model)
        {
            if (model?.Key == null)
                return BadRequest("Missing key.");

            if (!int.TryParse(model.Key.ToString(), out var id))
                return BadRequest("Invalid OrderID.");

            try
            {
                const string sql = @"DELETE FROM dbo.salesdata WHERE orderid = @OrderID;";

                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                // Dapper's Execute runs the parameterized DELETE and returns the
                // number of rows affected.
                await conn.ExecuteAsync(sql, new { OrderID = id });

                // UrlAdaptor expects { key, action } on remove.
                return Ok(new { key = id, action = "remove" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Delete failed." });
            }
        }
    }
}

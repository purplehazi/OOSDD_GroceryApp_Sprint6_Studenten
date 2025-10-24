using Grocery.Core.Data.Helpers;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;

namespace Grocery.Core.Data.Repositories
{
    // Deze class bewaart alle producten in een database
    public class ProductRepository : DatabaseConnection, IProductRepository
    {
        public ProductRepository()
        {
            string createTableQuery = @"CREATE TABLE IF NOT EXISTS Product (
                            [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            [Name] NVARCHAR(100) NOT NULL,
                            [Stock] INTEGER NOT NULL,
                            [ShelfLife] DATE NOT NULL,
                            [Price] DECIMAL(10,2) NOT NULL)";

            CreateTable(createTableQuery);

            List<string> insertQueries = new List<string>();
            insertQueries.Add("INSERT OR IGNORE INTO Product(Id, Name, Stock, ShelfLife, Price) VALUES(1, 'Melk', 300, '2025-09-25', 0.95)");
            insertQueries.Add("INSERT OR IGNORE INTO Product(Id, Name, Stock, ShelfLife, Price) VALUES(2, 'Kaas', 100, '2025-09-30', 7.98)");
            insertQueries.Add("INSERT OR IGNORE INTO Product(Id, Name, Stock, ShelfLife, Price) VALUES(3, 'Brood', 400, '2025-09-12', 2.19)");
            insertQueries.Add("INSERT OR IGNORE INTO Product(Id, Name, Stock, ShelfLife, Price) VALUES(4, 'Cornflakes', 0, '2025-12-31', 1.48)");

            InsertMultipleWithTransaction(insertQueries);
        }

        public List<Product> GetAll()
        {
            List<Product> products = new List<Product>();

            string query = "SELECT Id, Name, Stock, date(ShelfLife), Price FROM Product";

            OpenConnection();

            SqliteCommand command = new SqliteCommand(query, Connection);
            SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int id = reader.GetInt32(0);                    
                string name = reader.GetString(1);              
                int stock = reader.GetInt32(2);                 
                DateTime shelfLifeDateTime = reader.GetDateTime(3); // Column 3 = ShelfLife
                decimal price = reader.GetDecimal(4);

                // Change from date + time to date only
                DateOnly shelfLife = DateOnly.FromDateTime(shelfLifeDateTime);

                Product product = new Product(id, name, stock, shelfLife, price);
                products.Add(product);
            }

            CloseConnection();
            return products;
        }

        public Product? Get(int id)
        {
            Product? product = null;
            string query = "SELECT Id, Name, Stock, date(ShelfLife), Price FROM Product WHERE Id = " + id;

            OpenConnection();

            SqliteCommand command = new SqliteCommand(query, Connection);
            SqliteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                int productId = reader.GetInt32(0);
                string name = reader.GetString(1);
                int stock = reader.GetInt32(2);
                DateTime shelfLifeDateTime = reader.GetDateTime(3);
                decimal price = reader.GetDecimal(4);

                DateOnly shelfLife = DateOnly.FromDateTime(shelfLifeDateTime);

                product = new Product(productId, name, stock, shelfLife, price);
            }

            CloseConnection();
            return product;
        }

        public Product Add(Product item)
        {
            string query = @"INSERT INTO Product(Name, Stock, ShelfLife, Price) 
                            VALUES(@Name, @Stock, @ShelfLife, @Price) 
                            RETURNING Id;";

            OpenConnection();

            SqliteCommand command = new SqliteCommand(query, Connection);

            command.Parameters.AddWithValue("@Name", item.Name);
            command.Parameters.AddWithValue("@Stock", item.Stock);
            command.Parameters.AddWithValue("@ShelfLife", item.ShelfLife.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@Price", item.Price);

            object result = command.ExecuteScalar();
            item.Id = Convert.ToInt32(result);

            CloseConnection();

            return item;
        }

        public Product? Update(Product item)
        {
            string query = @"UPDATE Product SET Name = @Name, Stock = @Stock, ShelfLife = @ShelfLife, Price = @Price WHERE Id = @Id;";

            OpenConnection();

            SqliteCommand command = new SqliteCommand(query, Connection);
            command.Parameters.AddWithValue("@Name", item.Name);
            command.Parameters.AddWithValue("@Stock", item.Stock);
            command.Parameters.AddWithValue("@ShelfLife", item.ShelfLife.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@Price", item.Price);
            command.Parameters.AddWithValue("@Id", item.Id);

            command.ExecuteNonQuery();

            CloseConnection();
            return item;
        }

        public Product? Delete(Product item)
        {
            string query = "DELETE FROM Product WHERE Id = " + item.Id;

            OpenConnection();
            Connection.ExecuteNonQuery(query);
            CloseConnection();

            return item;
        }
    }
}
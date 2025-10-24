using Grocery.Core.Data.Helpers;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;

namespace Grocery.Core.Data.Repositories
{
    public class GroceryListItemsRepository : DatabaseConnection, IGroceryListItemsRepository
    {
        // Constructor for the repository.
        // It ensures the GroceryListItem table exists in the database
        // and inserts some initial example data if the table is empty.
        public GroceryListItemsRepository()
        {
            // Step 1: Make the table in the database (if it doesn't exist yet), this code gives the design of the table
            // Made a verbatim string (with @) to make it more readable
            string createTableQuery = @"CREATE TABLE IF NOT EXISTS GroceryListItem (
                            [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            [GroceryListId] INTEGER NOT NULL,
                            [ProductId] INTEGER NOT NULL,
                            [Amount] INTEGER NOT NULL)";

            CreateTable(createTableQuery);

            // Step 2: Make an empty list of insert queries to fill the table with some starting data, made five items so it's easier to test
            List<string> insertQueries = new List<string>();
            insertQueries.Add("INSERT OR IGNORE INTO GroceryListItem(Id, GroceryListId, ProductId, Amount) VALUES(1, 1, 1, 3)");
            insertQueries.Add("INSERT OR IGNORE INTO GroceryListItem(Id, GroceryListId, ProductId, Amount) VALUES(2, 1, 2, 1)");
            insertQueries.Add("INSERT OR IGNORE INTO GroceryListItem(Id, GroceryListId, ProductId, Amount) VALUES(3, 1, 3, 4)");
            insertQueries.Add("INSERT OR IGNORE INTO GroceryListItem(Id, GroceryListId, ProductId, Amount) VALUES(4, 2, 1, 2)");
            insertQueries.Add("INSERT OR IGNORE INTO GroceryListItem(Id, GroceryListId, ProductId, Amount) VALUES(5, 2, 2, 5)");

            // Step 3: Execute all insert queries in a single transaction
            InsertMultipleWithTransaction(insertQueries);
        }

        // This method creates a new list every time it's called
        // including any new products that were added to the database
        public List<GroceryListItem> GetAll()
        {
            List<GroceryListItem> items = new List<GroceryListItem>();

            // SQL query specifies which columns to get
            string query = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem";

            // Connect to the database
            OpenConnection();

            // Make an SQL command and execute it
            SqliteCommand command = new SqliteCommand(query, Connection);
            SqliteDataReader reader = command.ExecuteReader();

            // While there are still rows to read, go through them one by one
            while (reader.Read())
            {
                // Grab the values from each column
                int id = reader.GetInt32(0);              // First column
                int groceryListId = reader.GetInt32(1);   // Second column
                int productId = reader.GetInt32(2);       // Third column
                int amount = reader.GetInt32(3);          // Fourth column

                // Make a new GroceryListItem object with the values and add it to the list
                GroceryListItem item = new GroceryListItem(id, groceryListId, productId, amount);
                items.Add(item);
            }

            // Disconnect from database
            CloseConnection();

            return items;
        }

        // This method allows people to have different grocery lists.
        // It retrieves all products that belong to the grocery list with the given GroceryListId
        // and returns them together as a new list.
        public List<GroceryListItem> GetAllOnGroceryListId(int groceryListId)
        {
            List<GroceryListItem> items = new List<GroceryListItem>();

            // SQL query with WHERE to filter on GroceryListId
            string query = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem WHERE GroceryListId = " + groceryListId;

            OpenConnection();

            SqliteCommand command = new SqliteCommand(query, Connection);
            SqliteDataReader reader = command.ExecuteReader();

            // Read all rows that matches the string query and add new items to the list
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                int listId = reader.GetInt32(1);
                int productId = reader.GetInt32(2);
                int amount = reader.GetInt32(3);

                GroceryListItem item = new GroceryListItem(id, listId, productId, amount);
                items.Add(item);
            }

            CloseConnection();
            return items;
        }

        // Adds a new GroceryListItem to the database and returns it with its new Id.
        public GroceryListItem Add(GroceryListItem item)
        {
            // SQL query to add one new GroceryListItem to the database and create a new row + Id
            string query = "INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(@GroceryListId, @ProductId, @Amount) RETURNING Id;";

            OpenConnection();

            SqliteCommand command = new SqliteCommand(query, Connection);

            // Change the placeholders to the real values
            command.Parameters.AddWithValue("@GroceryListId", item.GroceryListId);
            command.Parameters.AddWithValue("@ProductId", item.ProductId);
            command.Parameters.AddWithValue("@Amount", item.Amount);

            // Execute the command and get the Id of the newly added item
            // ExecuteScalar is used because a value has to be returned
            object result = command.ExecuteScalar();
            item.Id = Convert.ToInt32(result);

            CloseConnection();
            return item;
        }

        // Gets a single GroceryListItem from the database by its Id.
        // Useful when you need to view, edit, or delete one specific item without loading all items.
        public GroceryListItem? Get(int id)
        {
            GroceryListItem? item = null;

            string query = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem WHERE Id = " + id;

            OpenConnection();

            SqliteCommand command = new SqliteCommand(query, Connection);
            SqliteDataReader reader = command.ExecuteReader();

            // Only read one row because Ids are unique
            if (reader.Read())
            {
                int itemId = reader.GetInt32(0);
                int groceryListId = reader.GetInt32(1);
                int productId = reader.GetInt32(2);
                int amount = reader.GetInt32(3);

                // Create the GroceryListItem object
                item = new GroceryListItem(itemId, groceryListId, productId, amount);
            }

            CloseConnection();
            // Idem is either the found item or null
            return item;
        }

        // Use this method when you need to change the details of an existing grocery list item.
        // It updates the database row that matches the item's Id with the new values for GroceryListId, ProductId, and Amount.
        public GroceryListItem? Update(GroceryListItem item)
        {
            // SQL query to update an existing GroceryListItem in the database.
            // Without the WHERE, all rows would be updated
            string query = "UPDATE GroceryListItem SET GroceryListId = @GroceryListId, ProductId = @ProductId, Amount = @Amount WHERE Id = @Id;";

            OpenConnection();

            SqliteCommand command = new SqliteCommand(query, Connection);
            // Give values to the placeholders in the query
            command.Parameters.AddWithValue("@GroceryListId", item.GroceryListId);
            command.Parameters.AddWithValue("@ProductId", item.ProductId);
            command.Parameters.AddWithValue("@Amount", item.Amount);
            command.Parameters.AddWithValue("@Id", item.Id);

            // ExecuteNonQuery is used because no value has to be returned
            command.ExecuteNonQuery();

            CloseConnection();
            return item;
        }
        // Use this method when you want to remove an item from the grocery list permanently.
        // It permanenty deletes the row in the database that matches the item's Id.
        public GroceryListItem? Delete(GroceryListItem item)
        {
            string query = "DELETE FROM GroceryListItem WHERE Id = " + item.Id;

            OpenConnection();
            Connection.ExecuteNonQuery(query);
            CloseConnection();

            return item;
        }
    }
}
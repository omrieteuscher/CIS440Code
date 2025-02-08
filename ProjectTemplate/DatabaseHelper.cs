using System;
using MySql.Data.MySqlClient;
using System.Configuration;

public class DatabaseHelper
{
    private string connectionString;

    public DatabaseHelper()
    {
        // Fetch the connection string from web.config
        connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
    }

    public MySqlConnection GetConnection()
    {
        return new MySqlConnection(connectionString);
    }

    public void TestConnection()
    {
        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                Console.WriteLine("✅ Database connection successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error connecting to database: " + ex.Message);
            }
        }
    }

    // ➤ NEW METHOD: Test Login Function
    public void TestLogin(string email, string password)
    {
        using (MySqlConnection conn = GetConnection())
        {
            try
            {
                conn.Open();
                string query = "SELECT * FROM Users WHERE email = @email AND password = @password";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@password", password);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            Console.WriteLine("✅ Login successful!");
                        else
                            Console.WriteLine("❌ Login failed!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error during login: " + ex.Message);
            }
        }
    }
}

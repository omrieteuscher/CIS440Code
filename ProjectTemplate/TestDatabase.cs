using System;

class TestDatabase
{
    static void Main(string[] args)
    {
        Console.WriteLine("🔍 Testing database connection...");

        // Create an instance of ProjectServices
        ProjectTemplate.ProjectServices service = new ProjectTemplate.ProjectServices();

        // Test database connection
        string connectionResult = service.TestConnection();
        Console.WriteLine("📡 Connection Test: " + connectionResult);

        // Test Login (Replace email & password with real user data from your DB)
        string loginResult = service.Login("john@example.com", "securepass");
        Console.WriteLine("🔑 Login Test: " + loginResult);

        Console.WriteLine("✅ Done!");
        Console.ReadLine();
    }
}

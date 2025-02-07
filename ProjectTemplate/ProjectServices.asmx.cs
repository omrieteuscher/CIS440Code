using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;

namespace ProjectTemplate
{
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	[System.Web.Script.Services.ScriptService]

	public class ProjectServices : System.Web.Services.WebService
	{
		////////////////////////////////////////////////////////////////////////
		///replace the values of these variables with your database credentials
		////////////////////////////////////////////////////////////////////////
		private string dbID = "cis440springA2025team1";
		private string dbPass = "cis440springA2025team1";
		private string dbName = "cis440springA2025team1";
		////////////////////////////////////////////////////////////////////////
		
		////////////////////////////////////////////////////////////////////////
		///call this method anywhere that you need the connection string!
		////////////////////////////////////////////////////////////////////////
		private string getConString() {
			return "SERVER=107.180.1.16; PORT=3306; DATABASE=" + dbName+"; UID=" + dbID + "; PASSWORD=" + dbPass;
		}
		////////////////////////////////////////////////////////////////////////



		/////////////////////////////////////////////////////////////////////////
		//don't forget to include this decoration above each method that you want
		//to be exposed as a web service!
		[WebMethod(EnableSession = true)]
		/////////////////////////////////////////////////////////////////////////
		public string TestConnection()
		{
			try
			{
				string testQuery = "select * from users";

				////////////////////////////////////////////////////////////////////////
				///here's an example of using the getConString method!
				////////////////////////////////////////////////////////////////////////
				MySqlConnection con = new MySqlConnection(getConString());
				////////////////////////////////////////////////////////////////////////

				MySqlCommand cmd = new MySqlCommand(testQuery, con);
				MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
				DataTable table = new DataTable();
				adapter.Fill(table);
				return "Success!";
			}
			catch (Exception e)
			{
				return "Something went wrong, please check your credentials and db name and try again.  Error: "+e.Message;
			}
		}

        [WebMethod(EnableSession = true)]
        public string CreateAccount(string email, string password, string firstName, string lastName)
        {
            string sqlConnectString = getConString();

            // Normalize email to lowercase for case-insensitive handling
            string normalizedEmail = email.ToLower();

            // Check if email already exists
            string checkEmailQuery = "SELECT COUNT(*) FROM users WHERE LOWER(email) = @Email";

            string insertQuery = "INSERT INTO users (email, password, first_name, last_name, status, role) " +
                                 "VALUES (@Email, @Password, @FirstName, @LastName, 1, 1); SELECT LAST_INSERT_ID();";

            using (MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString))
            {
                sqlConnection.Open();

                using (MySqlCommand checkCommand = new MySqlCommand(checkEmailQuery, sqlConnection))
                {
                    checkCommand.Parameters.AddWithValue("@Email", normalizedEmail);
                    int existingCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                    if (existingCount > 0)
                    {
                        return "Error: Email already registered.";
                    }
                }

                using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, sqlConnection))
                {
                    insertCommand.Parameters.AddWithValue("@Email", normalizedEmail);
                    insertCommand.Parameters.AddWithValue("@Password", HttpUtility.UrlDecode(password));
                    insertCommand.Parameters.AddWithValue("@FirstName", HttpUtility.UrlDecode(firstName));
                    insertCommand.Parameters.AddWithValue("@LastName", HttpUtility.UrlDecode(lastName));

                    try
                    {
                        int newUserId = Convert.ToInt32(insertCommand.ExecuteScalar());
                        return "Success! Your new account has been created.";
                    }
                    catch (Exception ex)
                    {
                        return "Error: " + ex.Message;
                    }
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public string Login(string email, string password)
        {
            string sqlConnectString = getConString();

            // Normalize email to lowercase for case-insensitive handling
            string normalizedEmail = email.ToLower();

            // SQL query to check for a matching user
            string sqlSelect = "SELECT user_id, role FROM users WHERE LOWER(email) = @Email AND password = @Password";

            using (MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString))
            {
                sqlConnection.Open();

                using (MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@Email", normalizedEmail);
                    sqlCommand.Parameters.AddWithValue("@Password", password);

                    MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                    DataTable sqlDt = new DataTable();
                    sqlDa.Fill(sqlDt);

                    if (sqlDt.Rows.Count > 0)
                    {
                        // Store user ID and role in session
                        Session["user_id"] = sqlDt.Rows[0]["user_id"];
                        Session["role"] = sqlDt.Rows[0]["role"];

                        return "Login successful";
                    }
                    else
                    {
                        return "Invalid email or password. Please try again.";
                    }
                }
            }
        }

    }
}

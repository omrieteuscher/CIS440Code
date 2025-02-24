using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using System.Web.Script.Services;

namespace ProjectTemplate
{
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	[System.Web.Script.Services.ScriptService]

    public class FeedbackRequest
    {
        public int RequestId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string RequestText { get; set; }
        public string DueDate { get; set; } // Formatted as "YYYY-MM-DD"
    }

    public class FeedbackResponse
    {
        public int ResponseId { get; set; }
        public int RequestId { get; set; }
        public int ResponderId { get; set; }
        public string ResponseText { get; set; }
    }


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

        [WebMethod(EnableSession = true)]
        public string EditAccount(string newPassword, string newPreferredName)
        {
            // Ensure the user is logged in
            if (Session["user_id"] == null)
            {
                return "Error: User not logged in.";
            }

            if (string.IsNullOrEmpty(newPassword) && string.IsNullOrEmpty(newPreferredName))
            {
                return "No changes detected. Please update at least one field.";
            }

            string sqlConnectString = getConString();
            int userId = Convert.ToInt32(Session["user_id"]);

            // Construct the SQL query dynamically based on provided fields
            List<string> updates = new List<string>();
            if (!string.IsNullOrEmpty(newPassword))
            {
                updates.Add("password = @NewPassword");
            }
            if (!string.IsNullOrEmpty(newPreferredName))
            {
                updates.Add("preferred_name = @NewPreferredName");
            }

            string sqlUpdate = "UPDATE users SET " + string.Join(", ", updates) + " WHERE user_id = @UserId";

            using (MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString))
            {
                sqlConnection.Open();
                using (MySqlCommand sqlCommand = new MySqlCommand(sqlUpdate, sqlConnection))
                {
                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        sqlCommand.Parameters.AddWithValue("@NewPassword", newPassword);
                    }
                    if (!string.IsNullOrEmpty(newPreferredName))
                    {
                        sqlCommand.Parameters.AddWithValue("@NewPreferredName", newPreferredName);
                    }
                    sqlCommand.Parameters.AddWithValue("@UserId", userId);

                    try
                    {
                        int rowsAffected = sqlCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return "Account updated successfully.";
                        }
                        else
                        {
                            return "Error: No changes made.";
                        }
                    }
                    catch (Exception ex)
                    {
                        return "Error: " + ex.Message;
                    }
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public string DeleteAccount(int userId)
        {
            // Ensure the user is logged in and has the right role
            if (Session["role"] == null || Convert.ToInt32(Session["role"]) < 2)
            {
                return "Error: You do not have permission to delete this user.";
            }

            string sqlConnectString = getConString();

            // Check if the user exists before attempting deletion
            string checkUserQuery = "SELECT COUNT(*) FROM users WHERE user_id = @UserId";
            string deleteUserQuery = "DELETE FROM users WHERE user_id = @UserId";

            using (MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString))
            {
                sqlConnection.Open();

                using (MySqlCommand checkCommand = new MySqlCommand(checkUserQuery, sqlConnection))
                {
                    checkCommand.Parameters.AddWithValue("@UserId", userId);
                    int userCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                    if (userCount == 0)
                    {
                        return "Error: User does not exist.";
                    }
                }

                using (MySqlCommand deleteCommand = new MySqlCommand(deleteUserQuery, sqlConnection))
                {
                    deleteCommand.Parameters.AddWithValue("@UserId", userId);

                    try
                    {
                        int rowsAffected = deleteCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return "Account successfully deleted.";
                        }
                        else
                        {
                            return "Error: Deletion failed.";
                        }
                    }
                    catch (Exception ex)
                    {
                        return "Error: " + ex.Message;
                    }
                }
            }
        }

        [WebMethod(EnableSession = true)]
        public string RequestFeedback(string receiverEmail, string requestText)
        {
            // Ensure user is logged in
            if (Session["user_id"] == null)
            {
                return "Error: User not logged in.";
            }

            string sqlConnectString = getConString();
            int senderId = Convert.ToInt32(Session["user_id"]); // Get sender ID from session
            int receiverId;

            // SQL query to find receiver's user_id based on email
            string getReceiverQuery = "SELECT user_id FROM users WHERE email = @ReceiverEmail";

            using (MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString))
            {
                sqlConnection.Open();

                using (MySqlCommand getReceiverCmd = new MySqlCommand(getReceiverQuery, sqlConnection))
                {
                    getReceiverCmd.Parameters.AddWithValue("@ReceiverEmail", receiverEmail);
                    object result = getReceiverCmd.ExecuteScalar();

                    if (result == null)
                    {
                        return "Error: Receiver email not found.";
                    }

                    receiverId = Convert.ToInt32(result);
                }

                // Set due date to 7 days from today
                DateTime dueDate = DateTime.Now.AddDays(7);

                // SQL query to insert feedback request
                string insertFeedbackQuery = "INSERT INTO feedbackRequests (sender_id, receiver_id, request, due_date) " +
                                             "VALUES (@SenderId, @ReceiverId, @RequestText, @DueDate)";

                using (MySqlCommand insertCmd = new MySqlCommand(insertFeedbackQuery, sqlConnection))
                {
                    insertCmd.Parameters.AddWithValue("@SenderId", senderId);
                    insertCmd.Parameters.AddWithValue("@ReceiverId", receiverId);
                    insertCmd.Parameters.AddWithValue("@RequestText", requestText);
                    insertCmd.Parameters.AddWithValue("@DueDate", dueDate.ToString("yyyy-MM-dd")); // Format for MySQL date

                    try
                    {
                        insertCmd.ExecuteNonQuery();
                        return "Success: Feedback request sent.";
                    }
                    catch (Exception ex)
                    {
                        return "Error: Unable to process your request at this time.";
                    }
                }
            }
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)] // Ensures JSON response format
        public FeedbackRequest[] GetFeedback()
        {
            // Ensure user is logged in
            if (Session["user_id"] == null)
            {
                return new FeedbackRequest[0]; // Return empty array if not logged in
            }

            DataTable sqlDt = new DataTable("feedbackRequests");

            string sqlConnectString = getConString();
            string sqlSelect = @"
        SELECT request_id, sender_id, receiver_id, request, due_date
        FROM feedbackRequests
        WHERE receiver_id = @UserId";

            using (MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString))
            {
                sqlConnection.Open();
                using (MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@UserId", Session["user_id"]);

                    MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                    sqlDa.Fill(sqlDt);
                }
            }

            List<FeedbackRequest> feedbackList = new List<FeedbackRequest>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                feedbackList.Add(new FeedbackRequest
                {
                    RequestId = Convert.ToInt32(sqlDt.Rows[i]["request_id"]),
                    SenderId = Convert.ToInt32(sqlDt.Rows[i]["sender_id"]),
                    ReceiverId = Convert.ToInt32(sqlDt.Rows[i]["receiver_id"]),
                    RequestText = sqlDt.Rows[i]["request"].ToString(),
                    DueDate = Convert.ToDateTime(sqlDt.Rows[i]["due_date"]).ToString("yyyy-MM-dd")
                });
            }

            return feedbackList.ToArray();
        }

        [WebMethod(EnableSession = true)]
        public string RespondToFeedback(int requestId, string responseText)
        {
            if (Session["user_id"] == null)
            {
                return "Error: User not logged in.";
            }

            int responderId = Convert.ToInt32(Session["user_id"]);
            string sqlConnectString = getConString();

            // Check if the feedback request exists and is assigned to the logged-in user
            string checkQuery = "SELECT receiver_id FROM feedbackRequests WHERE request_id = @RequestId AND status = 'Pending'";

            using (MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString))
            {
                sqlConnection.Open();
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, sqlConnection))
                {
                    checkCmd.Parameters.AddWithValue("@RequestId", requestId);
                    object result = checkCmd.ExecuteScalar();

                    if (result == null || Convert.ToInt32(result) != responderId)
                    {
                        return "Error: Invalid feedback request or already responded.";
                    }
                }

                // Insert response into feedbackResponses
                string insertQuery = "INSERT INTO feedbackResponses (request_id, responder_id, response) VALUES (@RequestId, @ResponderId, @ResponseText)";
                using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, sqlConnection))
                {
                    insertCmd.Parameters.AddWithValue("@RequestId", requestId);
                    insertCmd.Parameters.AddWithValue("@ResponderId", responderId);
                    insertCmd.Parameters.AddWithValue("@ResponseText", responseText);
                    insertCmd.ExecuteNonQuery();
                }

                // Update feedbackRequests to mark as "Responded"
                string updateQuery = "UPDATE feedbackRequests SET status = 'Responded' WHERE request_id = @RequestId";
                using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, sqlConnection))
                {
                    updateCmd.Parameters.AddWithValue("@RequestId", requestId);
                    updateCmd.ExecuteNonQuery();
                }
            }

            return "Success: Feedback response submitted.";
        }

    }
}

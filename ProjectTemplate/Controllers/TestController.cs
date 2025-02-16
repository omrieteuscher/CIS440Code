using System;
using System.Collections.Generic;
using System.Web.Http;
using MySql.Data.MySqlClient;

namespace ProjectTemplate.Controllers
{
    [RoutePrefix("api/test")]
    public class TestController : ApiController
    {
        // ✅ Test Database Connection
        [HttpGet]
        [Route("db")]
        public IHttpActionResult TestDbConnection()
        {
            try
            {
                DatabaseHelper dbHelper = new DatabaseHelper();
                dbHelper.TestConnection();
                return Ok("✅ Database connection attempted. Check console for results.");
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("❌ Database connection failed: " + ex.Message));
            }
        }

        // ✅ Data Models
        public class Feedback
        {
            public int Id { get; set; }
            public string Message { get; set; }
            public string Status { get; set; }
        }

        public class Notification
        {
            public int Id { get; set; }
            public string Message { get; set; }
        }

        public class User
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        // ✅ Fetch User Feedback
        [HttpGet]
        [Route("feedback")]
        public IHttpActionResult GetFeedback()
        {
            var feedbackList = new List<Feedback>
            {
                new Feedback { Id = 1, Message = "Great job on the project!", Status = "Responded" },
                new Feedback { Id = 2, Message = "Can you provide more details?", Status = "Not Responded" },
                new Feedback { Id = 3, Message = "I really liked your design!", Status = "Responded" }
            };

            return Ok(feedbackList);
        }

        // ✅ Fetch User Notifications
        [HttpGet]
        [Route("notifications")]
        public IHttpActionResult GetNotifications()
        {
            var notifications = new List<Notification>
            {
                new Notification { Id = 1, Message = "New feedback received!" },
                new Notification { Id = 2, Message = "Your request has been approved." }
            };

            return Ok(notifications);
        }

        // ✅ Login API
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(User user)
        {
            using (MySqlConnection conn = new DatabaseHelper().GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT user_id, Email FROM Users WHERE Email = @Email AND Password = @Password LIMIT 1";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@Password", user.Password); // Ensure passwords match (hash if needed)

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // If a user is found
                            {
                                int userId = reader.GetInt32(0); // Fetch user_id (not Id)
                                string userEmail = reader.GetString(1);

                                return Ok(new
                                {
                                    success = true,
                                    message = "✅ Login successful!",
                                    userId = userId,  // Ensure this uses user_id
                                    email = userEmail
                                });
                            }
                            else
                            {
                                return BadRequest("❌ No matching user found. Check email and password.");
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine("❌ MySQL Exception: " + ex.Message);
                    return InternalServerError(new Exception("❌ MySQL Error: " + ex.Message));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("❌ General Error: " + ex.Message);
                    return InternalServerError(new Exception("❌ General Error: " + ex.Message));
                }

            }
        }


        // ✅ Register API
        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(User user)
        {
            using (MySqlConnection conn = new DatabaseHelper().GetConnection())
            {
                try
                {
                    conn.Open();

                    // ✅ Check if email already exists
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", user.Email);
                        int userCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (userCount > 0)
                        {
                            return BadRequest("❌ Email already in use.");
                        }
                    }

                    // ✅ Insert new user into the database
                    string insertQuery = "INSERT INTO Users (FirstName, LastName, Email, Password) VALUES (@FirstName, @LastName, @Email, @Password)";

                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", user.LastName);
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@Password", HashPassword(user.Password)); // Hash password

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return Ok(new { message = "✅ Account created successfully!" });
                        }
                        else
                        {
                            return BadRequest("❌ Registration failed.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(new Exception("❌ Database error: " + ex.Message));
                }
            }
        }


        // ✅ Password Hashing Function
        public static string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}

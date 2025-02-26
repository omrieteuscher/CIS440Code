using System;
using System.Collections.Generic;
using System.Web.Http;
using MySql.Data.MySqlClient;

namespace ProjectTemplate.Controllers
{
    public class User
    {
        public int Id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class FeedbackResponseModel
    {
        public int feedback_id { get; set; }
        public string responseMessage { get; set; }
    }

    [RoutePrefix("api/test")]
    public class TestController : ApiController
    {
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
                    string checkQuery = "SELECT COUNT(*) FROM users WHERE email = @Email";
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
                    string insertQuery = "INSERT INTO users (first_name, last_name, email, password) VALUES (@first_name, @last_name, @Email, @Password)";

                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@first_name", user.first_name);
                        cmd.Parameters.AddWithValue("@last_name", user.last_name);
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@Password", user.Password); // Store as plaintext

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

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(User user)
        {
            using (MySqlConnection conn = new DatabaseHelper().GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT user_id, Email FROM users WHERE Email = @Email AND Password = @Password";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@Password", user.Password); // Store as plaintext

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // If a user is found
                            {
                                int userId = reader.GetInt32(0);
                                string userEmail = reader.GetString(1);

                                return Ok(new
                                {
                                    success = true,
                                    message = "✅ Login successful!",
                                    userId = userId,
                                    email = userEmail
                                });
                            }
                            else
                            {
                                return BadRequest("❌ Invalid email or password.");
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    return InternalServerError(new Exception("❌ MySQL Error: " + ex.Message));
                }
                catch (Exception ex)
                {
                    return InternalServerError(new Exception("❌ General Error: " + ex.Message));
                }
            }
        }
        [HttpGet]
        [Route("getProfile")]
        public IHttpActionResult GetProfile(int user_id)
        {
            using (MySqlConnection conn = new DatabaseHelper().GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT first_name, last_name, email FROM users WHERE user_id = @user_id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user_id", user_id);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // If a user is found
                            {
                                var userProfile = new
                                {
                                    first_name = reader.GetString(0),
                                    last_name = reader.GetString(1),
                                    email = reader.GetString(2)
                                };

                                return Ok(userProfile);
                            }
                            else
                            {
                                return BadRequest("❌ User not found.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(new Exception("❌ Database error: " + ex.Message));
                }
            }
        }
        [HttpPost]
        [Route("updateProfile")]
        public IHttpActionResult UpdateProfile(User user)
        {
            using (MySqlConnection conn = new DatabaseHelper().GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE users SET first_name = @first_name, last_name = @last_name, email = @Email WHERE user_id = @user_id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@first_name", user.first_name);
                        cmd.Parameters.AddWithValue("@last_name", user.last_name);
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@user_id", user.Id);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return Ok(new { success = true, message = "✅ Profile updated successfully!" });
                        }
                        else
                        {
                            return BadRequest("❌ Update failed. No changes were made.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(new Exception("❌ Database error: " + ex.Message));
                }
            }
        }
        [HttpGet]
        [Route("getUsers")]
        public IHttpActionResult GetUsers()
        {
            using (MySqlConnection conn = new DatabaseHelper().GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT user_id, first_name, last_name FROM users";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<object> users = new List<object>();

                            while (reader.Read())
                            {
                                users.Add(new
                                {
                                    user_id = reader.GetInt32(0),
                                    full_name = reader.GetString(1) + " " + reader.GetString(2)
                                });
                            }

                            return Ok(users);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(new Exception("❌ Database error: " + ex.Message));
                }
            }
        }
        [HttpPost]
        [Route("submitFeedback")]
        public IHttpActionResult SubmitFeedback(Feedback feedback)
        {
            using (MySqlConnection conn = new DatabaseHelper().GetConnection())
            {
                try
                {
                    conn.Open();

                    string insertQuery = "INSERT INTO feedback (sender_id, receiver_id, message, status) VALUES (@SenderId, @ReceiverId, @Message, 'Pending')";

                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SenderId", feedback.sender_id);
                        cmd.Parameters.AddWithValue("@ReceiverId", feedback.receiver_id);
                        cmd.Parameters.AddWithValue("@Message", feedback.message);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return Ok(new { success = true, message = "✅ Feedback submitted successfully!" });
                        }
                        else
                        {
                            return BadRequest("❌ Feedback submission failed.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(new Exception("❌ Database error: " + ex.Message));
                }
            }
        }

        // Feedback model
        public class Feedback
        {
            public int sender_id { get; set; }
            public int receiver_id { get; set; }
            public string message { get; set; }
        }

        [HttpGet]
        [Route("feedback")]
        public IHttpActionResult GetFeedback()
        {
            using (MySqlConnection conn = new DatabaseHelper().GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT feedback_id, sender_id, receiver_id, message, status FROM feedback";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<object> feedbackList = new List<object>();

                            while (reader.Read())
                            {
                                feedbackList.Add(new
                                {
                                    feedback_id = reader.GetInt32(0),
                                    sender_id = reader.GetInt32(1),
                                    receiver_id = reader.GetInt32(2),
                                    message = reader.GetString(3),
                                    status = reader.GetString(4)
                                });
                            }

                            return Ok(feedbackList);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(new Exception("❌ Database error: " + ex.Message));
                }
            }
        }
        [HttpGet]
        [Route("feedback/{user_id}")]
        public IHttpActionResult GetFeedback(int user_id)
        {
            using (MySqlConnection conn = new DatabaseHelper().GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = @"
                SELECT f.feedback_id, f.sender_id, f.receiver_id, f.message, f.status, f.response,
                       sender.first_name AS sender_name, receiver.first_name AS receiver_name
                FROM feedback f
                JOIN users sender ON f.sender_id = sender.user_id
                JOIN users receiver ON f.receiver_id = receiver.user_id
                WHERE f.sender_id = @user_id OR f.receiver_id = @user_id
                ORDER BY f.feedback_id DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user_id", user_id);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<object> feedbackList = new List<object>();

                            while (reader.Read())
                            {
                                feedbackList.Add(new
                                {
                                    feedback_id = reader.GetInt32(0),
                                    sender_id = reader.GetInt32(1),
                                    receiver_id = reader.GetInt32(2),
                                    message = reader.GetString(3),
                                    status = reader.GetString(4),
                                    response = reader.IsDBNull(5) ? "No response given." : reader.GetString(5),
                                    sender_name = reader.GetString(6),
                                    receiver_name = reader.GetString(7)
                                });
                            }

                            return Ok(feedbackList);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(new Exception("❌ Database error: " + ex.Message));
                }
            }
        }

        [HttpPost]
        [Route("markAsResponded/{feedbackId}")]
        public IHttpActionResult MarkAsResponded(int feedbackId)
        {
            using (MySqlConnection conn = new DatabaseHelper().GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE feedback SET status = 'Responded' WHERE feedback_id = @FeedbackId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FeedbackId", feedbackId);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok(new { success = true, message = "✅ Feedback marked as responded!" });
                        }
                        else
                        {
                            return BadRequest("❌ Failed to update feedback.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(new Exception("❌ Database error: " + ex.Message));
                }
            }
        }
        [HttpPost]
        [Route("respondToFeedback")]
        public IHttpActionResult RespondToFeedback(FeedbackResponseModel model)
        {
            using (MySqlConnection conn = new DatabaseHelper().GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE feedback SET status = 'Responded', response = @ResponseMessage WHERE feedback_id = @FeedbackId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ResponseMessage", model.responseMessage);
                        cmd.Parameters.AddWithValue("@FeedbackId", model.feedback_id);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return Ok(new { success = true, message = "✅ Response submitted successfully!" });
                        }
                        else
                        {
                            return BadRequest("❌ Failed to submit response.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(new Exception("❌ Database error: " + ex.Message));
                }
            }
        }


        [HttpGet]
        [Route("discussion")]
        public IHttpActionResult GetDiscussionPosts()
        {
            using (MySqlConnection conn = new DatabaseHelper().GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = @"
                SELECT d.post_id, d.user_id, d.message, d.created_at, 
                       u.first_name, u.last_name 
                FROM discussionPosts d
                JOIN users u ON d.user_id = u.user_id
                ORDER BY d.created_at DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<object> posts = new List<object>();

                            while (reader.Read())
                            {
                                posts.Add(new
                                {
                                    post_id = reader.GetInt32(0),
                                    user_id = reader.GetInt32(1),
                                    message = reader.GetString(2),
                                    created_at = reader.GetDateTime(3).ToString("yyyy-MM-dd HH:mm:ss"),
                                    user_name = reader.GetString(4) + " " + reader.GetString(5)
                                });
                            }

                            return Ok(posts);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(new Exception("❌ Database error: " + ex.Message));
                }
            }
        }


        [HttpPost]
        [Route("discussion")]
        public IHttpActionResult PostDiscussionMessage([FromBody] DiscussionPost newPost)
        {

            if (newPost == null)
            {
                return BadRequest("❌ Received null request.");
            }

            if (newPost.UserId == null || newPost.UserId <= 0)
            {
                return BadRequest("❌ User ID is missing or invalid.");
            }

            if (string.IsNullOrWhiteSpace(newPost.Message))
            {
                return BadRequest("❌ Message cannot be empty.");
            }

            using (MySqlConnection conn = new DatabaseHelper().GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO discussionPosts (user_id, message) VALUES (@user_id, @message)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user_id", newPost.UserId);
                        cmd.Parameters.AddWithValue("@message", newPost.Message);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return Ok(new { success = true, message = "✅ Message posted successfully!" });
                        }
                        else
                        {
                            return BadRequest("❌ Failed to post message.");
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    return InternalServerError(new Exception($"❌ MySQL Error: {ex.Message}"));
                }
                catch (Exception ex)
                {
                    return InternalServerError(new Exception($"❌ General Error: {ex.Message}"));
                }
            }
        }

        [HttpPost]
        [Route("discussion/comment")]
        public IHttpActionResult PostComment([FromBody] DiscussionComment newComment)
        {
            if (newComment == null || string.IsNullOrWhiteSpace(newComment.CommentText))
            {
                return BadRequest("❌ Comment cannot be empty.");
            }

            using (MySqlConnection conn = new DatabaseHelper().GetConnection())
            {
                try
                {
                    conn.Open();


                    string checkUser = "SELECT COUNT(*) FROM users WHERE user_id = @user_id";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkUser, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@user_id", newComment.UserId);
                        int userExists = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (userExists == 0)
                        {
                            return BadRequest("❌ User does not exist.");
                        }
                    }


                    string query = "INSERT INTO discussionComments (post_id, user_id, comment_text) VALUES (@post_id, @user_id, @comment_text)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@post_id", newComment.PostId);
                        cmd.Parameters.AddWithValue("@user_id", newComment.UserId);
                        cmd.Parameters.AddWithValue("@comment_text", newComment.CommentText);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return Ok(new { success = true, message = "✅ Comment posted successfully!" });
                        }
                        else
                        {
                            return BadRequest("❌ Failed to post comment.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(new Exception("❌ Database error: " + ex.Message));
                }
            }
        }

        [HttpGet]
        [Route("discussion/comments/{post_id}")]
        public IHttpActionResult GetComments(int post_id)
        {
            using (MySqlConnection conn = new DatabaseHelper().GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = @"
                SELECT c.comment_id, c.post_id, c.user_id, c.comment_text, c.created_at, 
                       u.first_name, u.last_name 
                FROM discussionComments c
                JOIN users u ON c.user_id = u.user_id
                WHERE c.post_id = @post_id
                ORDER BY c.created_at ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@post_id", post_id);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<object> comments = new List<object>();

                            while (reader.Read())
                            {
                                comments.Add(new
                                {
                                    comment_id = reader.GetInt32(0),
                                    post_id = reader.GetInt32(1),
                                    user_id = reader.GetInt32(2),
                                    comment_text = reader.GetString(3),
                                    created_at = reader.GetDateTime(4).ToString("yyyy-MM-dd HH:mm:ss"),
                                    user_name = reader.GetString(5) + " " + reader.GetString(6)
                                });
                            }

                            return Ok(comments);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(new Exception("❌ Database error: " + ex.Message));
                }
            }
        }


    }


    public class DiscussionPost
    {
        public int? UserId { get; set; }
        public string Message { get; set; }
    }

    public class DiscussionComment
    {
        public int? PostId { get; set; }
        public int? UserId { get; set; }
        public string CommentText { get; set; }
    }


}



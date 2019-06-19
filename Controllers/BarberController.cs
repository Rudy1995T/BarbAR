using System;
using System.Collections;
using Microsoft.AspNetCore.Mvc;
using BarbarAPI.Models;
using System.Configuration;
using System.Diagnostics;
//using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Text;
using System.Windows;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;


namespace BarbarAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarberController : ControllerBase
    {
        // GET api/barber
        [HttpGet]
        public ArrayList Get()
        {
            ArrayList barbers = new ArrayList();


            SqlConnection conn = null;
            SqlCommand comm;
            SqlDataReader reader = null;

            String connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            String query = "SELECT * FROM barber;";

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                Debug.Write("CONNECTION SUCCESSFUL ");

                conn = new SqlConnection(connectionString);
                conn.Open();
                Debug.Write("CONNECTION SUCCESSFUL");
            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO CONNECT: " + e);
            }

            try
            {
                comm = new SqlCommand(query, conn);

                comm.ExecuteNonQuery();
                reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    Barber barber = new Barber();
                    barber.id = reader.GetInt32(0);
                    barber.username = reader.GetString(1);
                    barber.firstname = reader.GetString(2);
                    barber.surname = reader.GetString(3);
                    barber.email = reader.GetString(4);
                    barber.password = reader.GetString(5);
                    barber.avatar = reader.GetString(6);
                    barber.login_code = reader.GetString(7);
                    barbers.Add(barber);
                }

                Debug.Write("INSERT SUCCESSFUL");
                conn.Close();
                Debug.Write("CONNECTION CLOSED");
            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO INSERT: " + e);
            }

            return barbers;
        }

        // GET api/barber/5
        [HttpGet("{id}")]
        public Barber Get(int id)
        {
            Barber barber = new Barber();

            SqlConnection conn = null;
            SqlCommand comm;
            SqlDataReader reader = null;

            String connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            String query = "SELECT * FROM barber WHERE barber_id = '" + id + "';";

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                Debug.Write("CONNECTION SUCCESSFUL ");
            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO CONNECT: " + e);
            }

            try
            {

                comm = new SqlCommand(query, conn);

                comm.ExecuteNonQuery();
                reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    barber.id = reader.GetInt32(0);
                    barber.username = reader.GetString(1);
                    barber.firstname = reader.GetString(2);
                    barber.surname = reader.GetString(3);
                    barber.email = reader.GetString(4);
                    barber.password = reader.GetString(5);
                    barber.avatar = reader.GetString(6);
                    barber.login_code = reader.GetString(7);
                }

                Debug.Write("INSERT SUCCESSFUL ");
                conn.Close();
                Debug.Write("CONNECTION CLOSED ");

            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO INSERT: " + e);
            }

            return barber;
        }

        //GET api/barber/barberLogIn
        [HttpPost("logIn")]
        public JObject PostLogIn([FromBody] Barber barber)
        {
            JObject responseObj = new JObject();
            UserController userController = new UserController();

            SqlConnection conn = null;
            SqlCommand checkCredentials;
            SqlDataReader reader = null;

            String connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            string hPassword = userController.ComputeHash(barber.password, new SHA256CryptoServiceProvider());


            String query = "Select * from barber where barber_username = '" + barber.username + "' and barber_password = '" + hPassword +
                            "'and login_code = '" + barber.login_code + "';";

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                Debug.Write("CONNECTION SUCCESSFUL ");
            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO CONNECT: " + e);
            }

            try
            {
                checkCredentials = new SqlCommand(query, conn);
                int count = Convert.ToInt32(checkCredentials.ExecuteScalar());

                Debug.Write("INTEGER RETURNED: " + count);

                if (count > 0)
                {
                    reader = checkCredentials.ExecuteReader();

                    while (reader.Read())
                    {

                        barber.id = reader.GetInt32(0);
                        barber.username = reader.GetString(1);
                        barber.firstname = reader.GetString(2);
                        barber.surname = reader.GetString(3);
                        barber.email = reader.GetString(4);
                        barber.password = reader.GetString(5);
                        barber.avatar = reader.GetString(6);
                        barber.login_code = reader.GetString(7);
                    }
                    Debug.Write("LOGIN SUCCESSFUL ");
                    conn.Close();
                    Debug.Write("CONNECTION CLOSED ");

                    responseObj.Add("message", "success");
                    responseObj.Add("data", barber.toJson());
                }
                else
                {
                    Debug.Write("FAILED TO LOG IN INCORRECT CRDENTIALS");


                    responseObj.Add("message", "failed");
                    responseObj.Add("data", barber.toJson());
                    responseObj.Add("error", "Failed to log in");
                }
            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO LOGIN: " + e);

                responseObj.Add("message", "failed");
                responseObj.Add("data", barber.toJson());
                responseObj.Add("error", e.GetBaseException().ToString());
            }

            return responseObj;
        }

        [HttpPost("resetLogInCode")]
        public JObject UpdateLogInCode([FromBody] Barber barber)
        {
            JObject responseObj = new JObject();
            UserController userController = new UserController();

            SqlConnection conn = null;
            SqlCommand comm;

            String connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            Guid g;
            g = Guid.NewGuid();
            Debug.Write(g);

            string logInCode = g.ToString().Split("-")[0].Substring(0, 5);

            String query = "Update barber SET login_code = '" + logInCode + "' WHERE barber_id = '" + barber.id + "'";

            String emailMessage = "Hi " + barber.username + "," + "<br> <br>" + "Below is your new log-in code that you will need " +
                      "to enter to access the BarbAR app. <br> <br> This code can be reset at anytime from the log-in page of the app." +
                      "<br> <br>" + " Log-in Code: <b>" + logInCode + "</b> <br> <br> Thank You, <br> BarbAR Team.";

            String subject = "Barber Log-in Code";

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                Debug.Write("CONNECTION SUCCESSFUL ");
            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO CONNECT: " + e);
            }

            try
            {
                comm = new SqlCommand(query, conn);
                comm.ExecuteNonQuery();
                Debug.Write("UPDATE LOGIN CODE SUCESSFUL ");

                responseObj.Add("message", "success");
                responseObj.Add("data", barber.toJson());

                userController.SendEmail(barber.username, barber.email, logInCode, emailMessage, subject);

                conn.Close();
                Debug.Write("CONNECTION CLOSED ");

            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO UPDATE: " + e);

                responseObj.Add("message", "failed");
                responseObj.Add("data", barber.toJson());
                responseObj.Add("error", e.GetBaseException().ToString());

            }
            return responseObj;
        }

        // POST api/barber/signUp
        [HttpPost("signUp")]
        public JObject PostSignUp([FromBody] Barber barber)
        {
            JObject responseObj = new JObject();
            UserController userController = new UserController();

            SqlConnection conn = null;
            SqlCommand comm;
            SqlCommand checkEmail;
            SqlCommand checkUsername;

            String subject = "Barber Log-in Code";

            String connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            //Automatically generating the Log-in code
            Guid g;
            g = Guid.NewGuid();
            Debug.Write(g);

            string logInCode = g.ToString().Split("-")[0].Substring(0, 5);

            string hPassword = userController.ComputeHash(barber.password, new SHA256CryptoServiceProvider());


            String query = "INSERT INTO barber(barber_username,barber_name,barber_surname,barber_email," +
                "barber_password,barber_avatar,login_code) VALUES ('" + barber.username + "','" +
                barber.firstname + "','" + barber.surname + "','" + barber.email + "','" + hPassword + "','" +
                barber.avatar + "','" + logInCode + "'); ";

            String query2 = "Select * from barber where barber_email = '" + barber.email + "';";

            String query3 = "Select * from barber where barber_username = '" + barber.username + "';";

            String emailMessage = "Hi " + barber.username + "," + "<br> <br>" + "Below is the code that you will need " +
                      "to enter to access the BarbAR app. <br> <br> This code can be reset at anytime from the log-in page of the app." +
                      "<br> <br>" + " Log-in Code: <b>" + logInCode + "</b> <br> <br> Thank You, <br> BarbAR Team.";

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                Debug.Write("CONNECTION SUCCESSFUL ");
            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO CONNECT: " + e);
            }

            try
            {

                comm = new SqlCommand(query, conn);

                checkEmail = new SqlCommand(query2, conn);
                int count = Convert.ToInt32(checkEmail.ExecuteScalar());

                checkUsername = new SqlCommand(query3, conn);
                int count1 = Convert.ToInt32(checkUsername.ExecuteScalar());

                if (count > 0)
                {
                    Debug.Write("USER ALREADY EXSISTS");

                    responseObj.Add("message", "failed");
                    responseObj.Add("data", barber.toJson());
                    responseObj.Add("error", "Email already taken");

                }
                else if (count1 > 0)
                {
                    Debug.Write("USER ALREADY EXSISTS");

                    responseObj.Add("message", "failed");
                    responseObj.Add("data", barber.toJson());
                    responseObj.Add("error", "Username already taken");

                }
                else
                {
                    comm.ExecuteNonQuery();
                    Debug.Write("INSERT SUCCESSFUL");

                    userController.SendEmail(barber.username, barber.email, logInCode, emailMessage, subject);

                    responseObj.Add("message", "success");
                    responseObj.Add("data", barber.toJson());
                }
                conn.Close();
                Debug.Write("CONNECTION CLOSED ");

            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO INSERT: " + e);

                responseObj.Add("message", "failed");
                responseObj.Add("data", barber.toJson());
                responseObj.Add("error", e.GetBaseException().ToString());
            }
            return responseObj;

        }

        [HttpPost("traineeFeedbackAssement")]
        public JObject PostFeedbackAssement([FromBody] JObject feedbackAssement)
        {
            JObject responseObj = new JObject();

            SqlConnection conn = null;
            SqlCommand comm;

            String connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            int sessionID = 0;
            int barberID = 0;
            string barberFeedback = "";

            if (feedbackAssement.GetValue("sessionID") != null && !feedbackAssement.GetValue("sessionID").Equals(""))
            {
                sessionID = Int32.Parse(feedbackAssement.GetValue("sessionID").ToString());
            }

            if (feedbackAssement.GetValue("barberID") != null && !feedbackAssement.GetValue("barberID").Equals(""))
            {
                barberID = Int32.Parse(feedbackAssement.GetValue("barberID").ToString());
            }

            if (feedbackAssement.GetValue("barberFeedback") != null && !feedbackAssement.GetValue("barberFeedback").Equals(""))
            {
                barberFeedback = feedbackAssement.GetValue("barberFeedback").ToString();
            }

            string query = "INSERT INTO barber_assessment (session_id, barber_id, barber_assessment)" +
                " VALUES ('" + sessionID + "', '" + barberID + "', '" + barberFeedback + "');";

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                Debug.Write("CONNECTION SUCCESSFUL ");
            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO CONNECT: " + e);
            }


            try
            {
                comm = new SqlCommand(query, conn);
                comm.ExecuteNonQuery();

                Debug.Write("SELF ASSESSMENT UPDATED");
                conn.Close();
                Debug.Write("CONNECTION CLOSED");

                responseObj.Add("message", "success");
                responseObj.Add("data", feedbackAssement);

            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO UPDATE BARBER ASSESSMENT " + e);
                responseObj.Add("message", "failed");
                responseObj.Add("data", feedbackAssement);
                responseObj.Add("error", e.GetBaseException().ToString());
            }

            return responseObj;
        }

        [HttpPost("updateBarberAvatar")]
        public JObject UpdateTraineeAvatar([FromBody] Barber barber)
        {
            JObject responseObj = new JObject();
            JArray session_data_list = new JArray();

            SqlConnection conn = null;
            SqlCommand comm;

            string connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            string query = "UPDATE barber SET barber_avatar = '" + barber.avatar + "' WHERE barber.barber_id = " + barber.id + ";";

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                Debug.Write("CONNECTION SUCCESSFUL");
            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO CONNECT: " + e);
            }

            try
            {

                comm = new SqlCommand(query, conn);
                comm.ExecuteNonQuery();


                responseObj.Add("message", "success");
                responseObj.Add("data", session_data_list);

                Debug.Write("SUCCESSFUL");
                conn.Close();
                Debug.Write("CONNECTION CLOSED");

            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO UPDATE AVATAR " + e);
                responseObj.Add("message", "failed");
                responseObj.Add("data", barber.ToString());
                responseObj.Add("error", e.GetBaseException().ToString());
            }
            return responseObj;
        }


        // PUT api/barber/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/barber/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }


}




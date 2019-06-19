using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
//using MySql.Data.MySqlClient;
using System.Configuration;
using System.Net.Mail;
using System.Diagnostics;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Cors;
using System.Security.Cryptography;
using System.Text;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

// Common controller for barber + trainee
namespace BarbarAPI.Controllers
{
    [Route("api/[controller]")]
    //[EnableCors(origins: "http://mywebclient.azurewebsites.net", headers: "*", methods: "*")]
    public class UserController : ControllerBase
    {
        [HttpPost("resetPassword")]
        public JObject ResetPassword([FromBody] JObject objectData)
        {
            JObject responseObj = new JObject();

            Boolean barber = true;
            String password = "";
            String email = "";
            String username = "";

            SqlConnection conn = null;
            SqlCommand comm;

            String connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            if (objectData.GetValue("password") != null && !objectData.GetValue("password").Equals(""))
            {
                password = objectData.GetValue("password").ToString();
            }

            if (objectData.GetValue("email") != null && !objectData.GetValue("email").Equals(""))
            {
                email = objectData.GetValue("email").ToString();
            }

            if (objectData.GetValue("username") != null && !objectData.GetValue("username").Equals(""))
            {
                username = objectData.GetValue("username").ToString();
            }

            string hPassword = ComputeHash(password, new SHA256CryptoServiceProvider());


            String query = "Update barber SET password = '" + hPassword + "' WHERE barber_username = '" + username + "'and WHERE barber_email = '" + email + "'";

            String query1 = "Update trainee SET password = '" + hPassword + "' WHERE trainee_username = '" + username + "'and WHERE barber_email = '" + email + "'";

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
                if (!barber)
                {
                    comm = new SqlCommand(query1, conn);
                    comm.ExecuteNonQuery();

                    responseObj.Add("message", "success");
                    responseObj.Add("data", objectData);
                }
                else
                {
                    comm = new SqlCommand(query, conn);
                    comm.ExecuteNonQuery();

                    responseObj.Add("message", "success");
                    responseObj.Add("data", objectData);
                }


            }
            catch (Exception e)
            {
                Debug.Write("PASSWORD IN DATABASE NOT UPDATED: " + e);

                responseObj.Add("message", "failed");
                responseObj.Add("data", objectData);
                responseObj.Add("error", e.GetBaseException().ToString());
            }
            return responseObj;

        }

        [HttpPost("sendEmail")]
        public JObject SendEmail([FromBody] JObject objectData)
        {
            JObject responseObj = new JObject();

            SqlConnection conn = null;
            SqlCommand checkCredentials;

            String connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            String userName = "";
            String email = "";
            String subject = "Reset Password";

            if (objectData.GetValue("userName") != null && !objectData.GetValue("userName").Equals(""))
            {
                userName = objectData.GetValue("userName").ToString();
            }

            if (objectData.GetValue("email") != null && !objectData.GetValue("email").Equals(""))
            {
                email = objectData.GetValue("email").ToString();
            }

            String query = "Select from barber where barber_username = '" + userName + "' and barber_password = '" + email + "';";

            String query1 = "Select from trainee where trainee_username = '" + userName + "' and trainee_password = '" + email + "';";

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

                checkCredentials = new SqlCommand(query1, conn);
                int count1 = Convert.ToInt32(checkCredentials.ExecuteScalar());

                if (count > 0)
                {
                    Debug.Write("USER DOESN'T EXSIST");

                    responseObj.Add("message", "failed");
                    responseObj.Add("data", objectData);
                    responseObj.Add("error", "Email and Username don't match for a user");

                }
                else if (count1 > 0)
                {
                    Debug.Write("USER DOESN'T EXSIST");

                    responseObj.Add("message", "failed");
                    responseObj.Add("data", objectData);
                    responseObj.Add("error", "Email and Username don't match for a user");
                }
                else
                {
                    String emailMessage = "Hi " + userName + "," + "<br> <br>" + "Click the link below to reset yor password " +
                   "<br> <br>" + "Reset password link: " + "<a href=\"https://barbarweb.000webhostapp.com/index.html" + "\">" + "https://barbarweb.000webhostapp.com/index.html" + "</a> <br> <br> Thank You, <br> BarbAR Team.";

                    SendEmail(userName, email, "", emailMessage, subject);

                    responseObj.Add("message", "success");
                    responseObj.Add("data", objectData);
                }
            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO SEND: " + e);

                responseObj.Add("message", "failed");
                responseObj.Add("data", objectData);
                responseObj.Add("error", e.GetBaseException().ToString());
            }
            return objectData;
        }

        [HttpPost("getFeedback")]
        public JObject GetFeedback([FromBody]JObject objectData)
        {
            JObject responseObj = new JObject();
            JObject feedbackObject = new JObject();

            SqlConnection conn = null;
            SqlCommand comm;
            SqlCommand checkFields;
            SqlDataReader reader = null;

            int sessionID = 0;
            string traineeAssessment = "";
            int traineeAssessmentID = 0;
            int rating = 0;
            int barberAssessmentID = 0;
            string barberAssessment = "";

            String connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            if (objectData.GetValue("sessionID") != null && !objectData.GetValue("sessionID").Equals(""))
            {
                sessionID = Int32.Parse(objectData.GetValue("sessionID").ToString());
            }

            string query = "SELECT trainee_assessment_id, self_assessment, rating FROM trainee_assessment WHERE session_id = '" + sessionID + "';";

            string query1 = "SELECT barber_assessment_id, barber_assessment FROM barber_assessment WHERE session_id = '" + sessionID + "';";

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

                checkFields = new SqlCommand(query, conn);
                int count = Convert.ToInt32(checkFields.ExecuteScalar());

                if (count > 0)
                {
                    reader = checkFields.ExecuteReader();
                    while (reader.Read())
                    {
                        traineeAssessmentID = reader.GetInt32(0);
                        traineeAssessment = reader.GetString(1);
                        rating = reader.GetInt32(2);

                        feedbackObject.Add("traineeAssessmentID", traineeAssessmentID);
                        feedbackObject.Add("traineeAssessment", traineeAssessment);
                        feedbackObject.Add("rating", rating);
                    }
                    reader.Close();
                }
                else
                {
                    Debug.Write("FAILED TO ADD TO OBJECT");

                    responseObj.Add("message", "fail");
                    responseObj.Add("data", objectData);
                    responseObj.Add("error", "Failed to add to object");
                }

                comm = new SqlCommand(query1, conn);
                comm.ExecuteNonQuery();

                reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    barberAssessmentID = reader.GetInt32(0);
                    barberAssessment = reader.GetString(1);

                    feedbackObject.Add("barberAssessmentID", barberAssessmentID);
                    feedbackObject.Add("barberAssessment", barberAssessment);
                }
                reader.Close();

                responseObj.Add("message", "Success");
                responseObj.Add("data", feedbackObject);

                Debug.Write("SUCCESSFUL");
                conn.Close();
                Debug.Write("CONNECTION CLOSED");

            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO INSERT: " + e);

                responseObj.Add("message", "failed");
                responseObj.Add("data", objectData);
                responseObj.Add("error", e.GetBaseException().ToString());
            }

            return responseObj;
        }

        [HttpPost("startHaircutSession")]
        public JObject StartHircutSession([FromBody] JObject objectData)
        {
            JObject responseObj = new JObject();

            SqlConnection conn = null;
            SqlCommand comm;
            string haircutName = "";
            string userName = "";
            string code = "";

            String connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            if (objectData.GetValue("username") != null && !objectData.GetValue("username").Equals(""))
            {
                userName = objectData.GetValue("username").ToString();
            }

            if (objectData.GetValue("haircutname") != null && !objectData.GetValue("haircutname").Equals(""))
            {
                haircutName = objectData.GetValue("haircutname").ToString();
            }

            if (objectData.GetValue("code") != null && !objectData.GetValue("code").Equals(""))
            {
                code = objectData.GetValue("code").ToString();
            }


            String query = "INSERT INTO haircut_session_code(code, username, haircut_name, isFinished) VALUES"
                + "('" + code + "','" + userName + "','" + haircutName + "',0); ";

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

                Debug.Write("INSERT SUCCESSFUL ");
                conn.Close();
                Debug.Write("CONNECTION CLOSED ");

                responseObj.Add("message", "Success");
                responseObj.Add("data", objectData);

            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO INSERT: " + e);

                responseObj.Add("message", "failed");
                responseObj.Add("data", objectData);
                responseObj.Add("error", e.GetBaseException().ToString());
            }

            return responseObj;
        }

        public void SendEmail(String userName, String email, String logInCode, String message, String subject)
        {
            try
            {
                MailMessage objeto_mail = new MailMessage();
                objeto_mail.IsBodyHtml = true;
                SmtpClient client = new SmtpClient();
                client.Port = 587;
                client.EnableSsl = true;
                client.Host = "smtp.gmail.com";
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential("barbarportal", "dkit2019!");
                objeto_mail.From = new MailAddress("barbarportal@gmail.com");
                objeto_mail.To.Add(new MailAddress(email));
                objeto_mail.Subject = subject;
                objeto_mail.Body = message;
                client.Send(objeto_mail);

                Debug.Write("EMAIL SENT ");
            }
            catch (Exception ex)
            {
                Debug.Write("FAILED TO SEND: " + ex);
            }
        }

        public string ComputeHash(string input, HashAlgorithm algorithm)
        {
            Byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);

            return BitConverter.ToString(hashedBytes);
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
    }
}

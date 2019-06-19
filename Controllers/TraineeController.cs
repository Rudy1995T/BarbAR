using System;
using Microsoft.AspNetCore.Mvc;
using BarbarAPI.Models;
using System.Diagnostics;
using System.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Security.Cryptography;


namespace BarbarAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TraineeController : ControllerBase
    {
        //GET api/trainee/traineeLogIn
        [HttpPost("logIn")]
        public JObject PostLogIn([FromBody] Trainee trainee)
        {
            JObject responseObj = new JObject();
            UserController userController = new UserController();

            SqlConnection conn = null;
            SqlDataReader reader = null;
            SqlCommand checkCredentials;

            String connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            string hPassword = userController.ComputeHash(trainee.password, new SHA256CryptoServiceProvider());

            String query = "Select * from trainee where trainee_username = '" + trainee.username + "' and trainee_password = '" + hPassword + "';";

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

                        trainee.id = reader.GetInt32(0);
                        trainee.username = reader.GetString(1);
                        trainee.firstname = reader.GetString(2);
                        trainee.surname = reader.GetString(3);
                        trainee.password = reader.GetString(4);
                        trainee.email = reader.GetString(5);
                        trainee.avatar = reader.GetString(6);
                    }
                    Debug.Write("LOGIN SUCCESSFUL ");
                    Debug.Write("CONNECTION CLOSED ");

                    responseObj.Add("message", "Success");
                    responseObj.Add("data", trainee.toJson());
                }
                else
                {
                    Debug.Write("FAILED TO LOG IN INCORRECT CRDENTIALS");


                    responseObj.Add("message", "failed");
                    responseObj.Add("data", trainee.toJson());
                    responseObj.Add("error", "Failed to log in");
                }
                conn.Close();
                Debug.Write("CONNECTION CLOSED ");


            }
            catch (Exception e)
            {
                Debug.Write("LOGIN FAILED: " + e);

                responseObj.Add("message", "failed");
                responseObj.Add("data", trainee.toJson());
                responseObj.Add("error", e.GetBaseException().ToString());

            }
            return responseObj;
        }

        // POST api/trainee/traineeSignUp
        [HttpPost("signUp")]
        public JObject PostSignUp([FromBody] Trainee trainee)
        {
            JObject responseObj = new JObject();
            UserController userController = new UserController();

            SqlConnection conn = null;
            SqlCommand comm;
            SqlCommand checkEmail;
            SqlCommand checkUsername;

            String connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            string hPassword = userController.ComputeHash(trainee.password, new SHA256CryptoServiceProvider());

            String query = "INSERT INTO trainee(trainee_username,trainee_name,trainee_surname,trainee_email," +
                "trainee_password,trainee_avatar) VALUES ('" + trainee.username + "','" +
                trainee.firstname + "','" + trainee.surname + "','" + trainee.email + "','" + hPassword + "','" +
                trainee.avatar + "'); ";

            String query2 = "Select * from trainee where trainee_email = '" + trainee.email + "';";

            String query3 = "Select * from trainee where trainee_username = '" + trainee.username + "';";

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

                checkEmail = new SqlCommand(query2, conn);
                int count = Convert.ToInt32(checkEmail.ExecuteScalar());

                checkUsername = new SqlCommand(query3, conn);
                int count1 = Convert.ToInt32(checkUsername.ExecuteScalar());
                if (count > 0)
                {
                    Debug.Write("USER ALREADY EXSISTS ");
                    responseObj.Add("message", "failed");
                    responseObj.Add("data", trainee.toJson());
                    responseObj.Add("error", "Email already taken");
                }
                else if (count1 > 0)
                {
                    Debug.Write("USER ALREADY EXSISTS ");
                    responseObj.Add("message", "failed");
                    responseObj.Add("data", trainee.toJson());
                    responseObj.Add("error", "Username already taken");
                }
                else
                {
                    comm.ExecuteNonQuery();
                    Debug.Write("SIGNUP: INSERT SUCCESSFUL");
                    responseObj.Add("message", "success");
                    responseObj.Add("data", trainee.toJson());
                }
                conn.Close();
                Debug.Write("CONNECTION CLOSED ");
            }
            catch (Exception e)
            {
                Debug.Write("SIGNUP: FAILED TO INSERT: " + e);

                responseObj.Add("message", "failed");
                responseObj.Add("data", trainee.toJson());
                responseObj.Add("error", e.GetBaseException().ToString());
            }

            return responseObj;
        }

        // POST api/trainee/selfAssesment
        [HttpPost("selfAssessment")]
        public JObject PostSelfAssesment([FromBody] JObject selfAssessment)
        {
            JObject responseObj = new JObject();

            SqlConnection conn = null;
            SqlCommand comm;

            string connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;
            int session_id = 0;
            int trainee_id = 0;
            string self_assessment = "";
            string rating = "";

            if (selfAssessment.GetValue("session_id") != null && !selfAssessment.GetValue("session_id").Equals(""))
            {
                session_id = Int32.Parse(selfAssessment.GetValue("session_id").ToString());
            }
            if (selfAssessment.GetValue("trainee_id") != null && !selfAssessment.GetValue("trainee_id").Equals(""))
            {
                trainee_id = Int32.Parse(selfAssessment.GetValue("trainee_id").ToString());
            }
            if (selfAssessment.GetValue("self_assessment") != null && !selfAssessment.GetValue("self_assessment").Equals(""))
            {
                self_assessment = selfAssessment.GetValue("self_assessment").ToString();
            }
            if (selfAssessment.GetValue("rating") != null && !selfAssessment.GetValue("rating").Equals(""))
            {
                rating = selfAssessment.GetValue("rating").ToString();
            }
            string query = "INSERT INTO trainee_assessment (session_id, trainee_id, self_assessment, rating)" +
                " VALUES ('" + session_id + "', '" + trainee_id + "', '" + self_assessment + "', '" + rating + "');";

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

                Debug.Write("SELF ASSESSMENT UPDATED");
                conn.Close();
                Debug.Write("CONNECTION CLOSED");

                responseObj.Add("message", "success");
                responseObj.Add("data", selfAssessment);

            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO UPDATE SELF ASSESSMENT " + e);
                responseObj.Add("message", "failed");
                responseObj.Add("data", selfAssessment);
                responseObj.Add("error", e.GetBaseException().ToString());
            }
            return responseObj;
        }

        // POST api/trainee/customerFeedback
        // [HttpPost("customerFeedback")]
        ////     public JObject CustomerFeedback([FromBody] JObject customerFeedback)
        //     {
        //         JObject responseObj = new JObject();

        //         MySqlConnection conn = null;
        //         MySqlCommand comm;

        //         string connectionString = ConfigurationManager.ConnectionStrings["localhost"].ConnectionString;
        //         int session_id = 0;
        //         int trainee_id = 0;
        //         string self_assessment = "";

        //         if (customerFeedback.GetValue("session_id") != null && !customerFeedback.GetValue("session_id").Equals(""))
        //         {
        //             session_id = Int32.Parse(customerFeedback.GetValue("session_id").ToString());
        //         }
        //         if (customerFeedback.GetValue("trainee_id") != null && !customerFeedback.GetValue("trainee_id").Equals(""))
        //         {
        //             trainee_id = Int32.Parse(customerFeedback.GetValue("trainee_id").ToString());
        //         }
        //         if (customerFeedback.GetValue("customer_feedback") != null && !customerFeedback.GetValue("customer_feedback").Equals(""))
        //         {
        //             self_assessment = customerFeedback.GetValue("customer_feedback").ToString();
        //         }
        //         string query = "INSERT INTO trainee_assessment (session_id, trainee_id, customer_feedback)" +
        //             " VALUES ('" + session_id + "', '" + trainee_id + "', '" + self_assessment + "');";

        //         try
        //         {
        //             conn = new MySqlConnection(connectionString);
        //             conn.Open();
        //             Debug.Write("CONNECTION SUCCESSFUL");
        //         }
        //         catch (Exception e)
        //         {
        //             Debug.Write("FAILED TO CONNECT: " + e);
        //         }

        //         try
        //         {
        //             comm = new MySqlCommand(query, conn);
        //             comm.ExecuteNonQuery();

        //             Debug.Write("SELF ASSESSMENT UPDATED");
        //             conn.Close();
        //             Debug.Write("CONNECTION CLOSED");

        //             responseObj.Add("message", "success");
        //             responseObj.Add("data", customerFeedback);

        //         }
        //         catch (Exception e)
        //         {
        //             Debug.Write("FAILED TO UPDATE SELF ASSESSMENT " + e);
        //             responseObj.Add("message", "failed");
        //             responseObj.Add("data", customerFeedback);
        //             responseObj.Add("error", e.GetBaseException().ToString());
        //         }
        //         return responseObj;
        //     }

        //POST api/trainee/HaircutInfo
        [HttpPost("haircutInfo")]
        public JObject GetHaircutInfo([FromBody] Trainee trainee)
        {
            JObject responseObj = new JObject();
            JObject session_data = null;
            JArray session_data_list = new JArray();

            SqlConnection conn = null;
            SqlCommand comm;
            SqlDataReader reader = null;

            string time_taken = "";
            int haircut_id = 0;
            int session_id = 0;

            string connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            string query = "SELECT  time_taken, haircut_id, session_id FROM haircut_session WHERE trainee_id = '" + trainee.id + "';";

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

                reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    JObject haircut_details = new JObject();


                    time_taken = reader.GetString(0);
                    haircut_id = reader.GetInt32(1);
                    session_id = reader.GetInt32(2);

                    session_data = new JObject();

                    session_data.Add("time_taken", time_taken);
                    session_data.Add("session_id", session_id);
                    haircut_details = GetHaircutDetails(haircut_id);
                    session_data.Add("haircut_details", haircut_details);

                    session_data_list.Add(session_data);
                }

                responseObj.Add("message", "success");
                responseObj.Add("data", session_data_list);

                Debug.Write("SUCCESSFUL");
                conn.Close();
                Debug.Write("CONNECTION CLOSED");

            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO GET HAIRCUT HISTORY " + e);
                responseObj.Add("message", "failed");
                responseObj.Add("data", trainee.ToString());
                responseObj.Add("error", e.GetBaseException().ToString());
            }
            return responseObj;
        }

        public JObject GetHaircutDetails(int haircut_id)
        {
            JObject haircut_data = new JObject();
            SqlConnection conn = null;
            SqlCommand comm;
            SqlDataReader reader = null;


            string haricut_url = "";
            string haircut_name = "";


            string connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            string query = "Select haircut_image_url, haircut_name from object_data where object_data_id = " + haircut_id + ";";
           

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

                reader = comm.ExecuteReader();

                while (reader.Read())
                {

                    haricut_url = reader.GetString(0);
                    haircut_name = reader.GetString(1);

                    haircut_data.Add("haircut_url", haricut_url);
                    haircut_data.Add("haircut_name", haircut_name);
                }


                conn.Close();

            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO GET HAIRCUT HISTORY " + e);

            }
            return haircut_data;
        }


        [HttpPost("updateTraineeAvatar")]
        public JObject UpdateTraineeAvatar([FromBody] Trainee trainee)
        {
            JObject responseObj = new JObject();
            JArray session_data_list = new JArray();

            SqlConnection conn = null;
            SqlCommand comm;

            string connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            string query = "UPDATE trainee SET trainee_avatar = '"+ trainee.avatar + "' WHERE trainee.trainee_id = "+ trainee.id +";";

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
                responseObj.Add("data", trainee.ToString());
                responseObj.Add("error", e.GetBaseException().ToString());
            }
            return responseObj;
        }

        [HttpGet("GetTraineeInfo")]
        public JObject GetTraineeInfo()
        {
            JObject responseObj = new JObject();
            JArray session_data_list = new JArray();

            SqlConnection conn = null;
            SqlCommand comm;
            SqlDataReader reader = null;

            String connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;


            string query = "SELECT trainee_id,trainee_username,trainee_name,trainee_surname,trainee_email,trainee_avatar FROM trainee";

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
                int trainee_id = 0;
                string trainee_username = "";
                string trainee_name = "";
                string trainee_surname = "";
                string trainee_email = "";
                string trainee_avatar = "";

                while (reader.Read())
                {
                    trainee_id = reader.GetInt32(0);
                    trainee_username = reader.GetString(1);
                    trainee_name = reader.GetString(2);
                    trainee_surname = reader.GetString(3);
                    trainee_email = reader.GetString(4);
                    trainee_avatar = reader.GetString(5);


                    JObject session_data = new JObject();
                    session_data.Add("trainee_id", trainee_id);
                    session_data.Add("trainee_username", trainee_username);
                    session_data.Add("trainee_name", trainee_name);
                    session_data.Add("trainee_surname", trainee_surname);
                    session_data.Add("trainee_email", trainee_email);
                    session_data.Add("trainee_avatar", trainee_avatar);

                    session_data_list.Add(session_data);

                }


                responseObj.Add("message", "success");
                responseObj.Add("data", session_data_list);

                Debug.Write("SUCCESSFUL");
                conn.Close();
                Debug.Write("CONNECTION CLOSED ");

            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO GET SESSION ID " + e);
                responseObj.Add("message", "failed");
                responseObj.Add("data", responseObj.ToString());
                responseObj.Add("error", e.GetBaseException().ToString());
            }

            return responseObj;
        }
    }

}



using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using BarbarAPI.Models;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BarbarAPI.Controllers
{
    [Route("api/[controller]")]
    public class HaircutController : Controller
    {
        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/haircut/session/complete
        [HttpPost("session/complete")]
        public JObject InsertSessionData([FromBody] JObject obj)
        {
            JObject responseObj = new JObject();

            string connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            SqlConnection conn = null;
            SqlCommand comm1, comm2;

            string code = "";
            string trainee_id = "";
            string time_taken = "";
            string date = "";

            if (obj.GetValue("code") != null && !obj.GetValue("code").Equals(""))
            {
                code = obj.GetValue("code").ToString();
            }

            if (obj.GetValue("trainee_id") != null && !obj.GetValue("trainee_id").Equals(""))
            {
                trainee_id = obj.GetValue("trainee_id").ToString();
            }
            if (obj.GetValue("time_taken") != null && !obj.GetValue("time_taken").Equals(""))
            {
                time_taken = obj.GetValue("time_taken").ToString();
            }
            if (obj.GetValue("date") != null && !obj.GetValue("date").Equals(""))
            {
                date = obj.GetValue("date").ToString();
            }

            string query1 = "INSERT INTO haircut_session(trainee_id,time_taken, haircutDate) VALUES ('" + trainee_id + "','" +
                time_taken + "','" + date + "'); ";

            string query2 = "UPDATE haircut_session_code SET isFinished  = 1 WHERE code= '" + code + "';";

            if (!trainee_id.Equals("") && !time_taken.Equals(""))
            {
                try
                {
                    conn = new SqlConnection(connectionString);
                    conn.Open();
                    Debug.Write("CONNECTION SUCCESSFUL");

                    comm1 = new SqlCommand(query1, conn);
                    comm1.ExecuteNonQuery();
                    Debug.Write("INSERT SUCCESSFUL");

                    comm2 = new SqlCommand(query2, conn);
                    comm2.ExecuteNonQuery();
                    Debug.Write("UPDATE SUCCESSFUL");

                    responseObj.Add("message", "success");
                    responseObj.Add("data", obj);

                    return responseObj;

                }
                catch (Exception e)
                {
                    Debug.Write("FAILED TO INSERT: " + e);

                    responseObj.Add("message", "failed");
                    responseObj.Add("data", obj);
                    responseObj.Add("error", e.GetBaseException().ToString());

                    return responseObj;
                }
            }
            else
            {
                responseObj.Add("message", "failed");
                responseObj.Add("data", obj);
                responseObj.Add("error", "failed to retrieve trainee id or time taken");

                return responseObj;
            }

        }

        //GET api/haircut/validateCode
        [HttpPost("validateCode")]
        public JObject ValidateCode([FromBody] JObject codeInput)
        {
            SqlConnection conn = null;
            SqlCommand comm;
            SqlDataReader reader = null;

            JObject responseObj = new JObject();
            JObject codeObj = new JObject();

            string code = codeInput.GetValue("code").ToString();

            String connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            String query = "SELECT * FROM haircut_session_code WHERE code = '" + code + "'; ";

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
                conn = new SqlConnection(connectionString);
                conn.Open();
                Debug.Write("CONNECTION SUCCESSFUL");

                comm = new SqlCommand(query, conn);
                int db_data = Convert.ToInt32(comm.ExecuteScalar());
                reader = comm.ExecuteReader();

                Debug.Write("DB: " + db_data);

                if (db_data > 0)
                {
                    while (reader.Read())
                    {
                        codeObj.Add("id", reader.GetInt32(0));
                        codeObj.Add("code", reader.GetString(1));
                        codeObj.Add("username", reader.GetString(2));
                        codeObj.Add("haircut", reader.GetString(3));
                    }

                    responseObj.Add("message", "Success");
                    responseObj.Add("data", codeObj);
                }
                else
                {
                    responseObj.Add("message", "failed");
                    responseObj.Add("data", code);
                    responseObj.Add("error", "code incorrect");
                }
                conn.Close();
                Debug.Write("CONNECTION CLOSED");

            }
            catch (Exception e)
            {
                Debug.Write(e);

                responseObj.Add("message", "failed");
                responseObj.Add("data", code);
                responseObj.Add("error", e.GetBaseException().ToString());
            }
            return responseObj;
        }

        //GET api/barber/code
        [HttpPost("session/checkStatus")]
        public JObject checkSessionStatus([FromBody] JObject codeObj)
        {
            SqlConnection conn = null;
            SqlCommand comm;
            SqlDataReader reader = null;

            JObject responseObj = new JObject();

            int isFinished = 2;

            string code = "";

            if (codeObj.GetValue("code") != null && !codeObj.GetValue("code").Equals(""))
            {
                code = codeObj.GetValue("code").ToString();
            }

            String connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            String query = "SELECT code_id, isFinished FROM haircut_session_code WHERE code = '" + code + "'; ";

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                Debug.Write("CONNECTION SUCCESSFUL");

                comm = new SqlCommand(query, conn);
                int db_data = Convert.ToInt32(comm.ExecuteScalar());
                reader = comm.ExecuteReader();

                if (db_data > 0)
                {
                    while (reader.Read())
                    {
                        isFinished = reader.GetInt32(1);
                    }

                    if (isFinished == 0 || isFinished == 1)
                    {
                        responseObj.Add("message", "Success");
                        responseObj.Add("data", isFinished);
                    }
                    else
                    {
                        responseObj.Add("message", "failed");
                        responseObj.Add("data", code);
                        responseObj.Add("error", "session not found");
                    }

                }
                else
                {
                    responseObj.Add("message", "failed");
                    responseObj.Add("data", code);
                    responseObj.Add("error", "session not found");
                }
                conn.Close();
                Debug.Write("CONNECTION CLOSED");

            }
            catch (Exception e)
            {
                Debug.Write(e);

                responseObj.Add("message", "failed");
                responseObj.Add("data", code);
                responseObj.Add("error", e.GetBaseException().ToString());
            }

            return responseObj;
        }

        //POST http://barbarservice.azurewebsites.net/api/haircut/haircutInformation
        //Gets haircut name,description,image and object id
        [HttpGet("haircutInformation")]
        public JObject GetHaircutInformation()
        {
            JObject responseObj = new JObject();
            JObject haircut_data = null;
            JArray haircut_object_list = new JArray();

            SqlConnection conn = null;
            SqlCommand comm;
            SqlDataReader reader = null;

            string haircut_name = "";
            string haircut_url = "";
            string haircut_description = "";
            int object_data_id = 0;

            string connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            string query = "SELECT haircut_name, haircut_image_url, haircut_description, object_data_id FROM object_data";
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
                    haircut_name = reader.GetString(0);
                    haircut_url = reader.GetString(1);
                    haircut_description = reader.GetString(2);
                    object_data_id = reader.GetInt32(3);


                    haircut_data = new JObject();
                    haircut_data.Add("haircut_name", haircut_name);
                    haircut_data.Add("haircut_url", haircut_url);
                    haircut_data.Add("haircut_description", haircut_description);
                    haircut_data.Add("object_data_id", object_data_id);

                    haircut_object_list.Add(haircut_data);

                }
                responseObj.Add("message", "success");
                responseObj.Add("data", haircut_object_list);

                Debug.Write("INSERT SUCCESSFUL ");
                conn.Close();
                Debug.Write("CONNECTION CLOSED ");

            }

            catch (Exception e)
            {
                Debug.Write("FAILED TO GET HAIRCUT DETAILS " + e);
                responseObj.Add("message", "failed");
                responseObj.Add("error", e.GetBaseException().ToString());
            }
            return responseObj;
        }

        //gets average time of haircuts
        [HttpPost("HaircutAverage")]
        public JObject GetHaircutAverageTime([FromBody]Trainee trainee)
        {
            JObject responseObj = new JObject();
            JObject session_data = null;
            JArray session_data_list = new JArray();

            SqlConnection conn = null;
            SqlCommand comm;
            SqlDataReader reader = null;

            string time_taken = "";
            int haircut_id = 0;

            string connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            string query = "SELECT time_taken, haircut_id FROM haircut_session WHERE trainee_id = '" + trainee.id + "';";


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

                var time_taken_list = new List<TimeSpan>();

                while (reader.Read())
                {
                    time_taken = reader.GetString(0);
                    haircut_id = reader.GetInt32(1);

                    TimeSpan time = TimeSpan.Parse(time_taken);
                    time_taken_list.Add(time);

                    session_data = new JObject();

                    session_data.Add("time_taken", time_taken);

                    session_data_list.Add(session_data);
                }
                var average = time_taken_list.Average(x => x.TotalMilliseconds);

                var averageTime = TimeSpan.FromMilliseconds(average);

                responseObj.Add("message", "success");
                responseObj.Add("data", averageTime);

                Debug.Write("SUCCESSFUL");
                conn.Close();
                Debug.Write("CONNECTION CLOSED");

            }
            catch (Exception e)
            {
                Debug.Write("FAILED TO GET HAIRCUT AVERAGE TIME " + e);
                responseObj.Add("message", "failed");
                responseObj.Add("data", trainee.ToString());
                responseObj.Add("error", e.GetBaseException().ToString());
            }
            return responseObj;
        }

        [HttpGet("getSessionId")]
        public JObject GetSessionId()
        {
            JObject responseObj = new JObject();

            SqlConnection conn = null;
            SqlCommand comm;
            SqlDataReader reader = null;

            String connectionString = ConfigurationManager.ConnectionStrings["azure"].ConnectionString;

            int session_id = 0;

            string query = "SELECT TOP 1 session_id FROM haircut_session ORDER BY session_id DESC;";

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
                    session_id = reader.GetInt32(0);
                }

                responseObj.Add("message", "success");
                responseObj.Add("data", session_id);

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

using System;
using Newtonsoft.Json.Linq;

namespace BarbarAPI.Models
{
    public class Trainee
    {
        public long id { get; set; }
        public String username { get; set; }
        public String firstname { get; set; }
        public String surname { get; set; }
        public String email { get; set; }
        public String password { get; set; }
        public String avatar { get; set; }

        public JObject toJson()
        {
            JObject trainee = new JObject();

            trainee.Add("id", id);
            trainee.Add("username", username);
            trainee.Add("firstname", firstname);
            trainee.Add("surname", surname);
            trainee.Add("email", email);
            trainee.Add("password", password);
            trainee.Add("avatar", avatar);

            return trainee;
        }

    }


}

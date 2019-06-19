using System;
using Newtonsoft.Json.Linq;

namespace BarbarAPI.Models
{
    public class Barber
    {
        public long id { get; set; }
        public String username { get; set; }
        public String firstname { get; set; }
        public String surname { get; set; }
        public String email { get; set; }
        public String password { get; set; }
        public String avatar { get; set; }
        public String login_code { get; set; }

        public JObject toJson()
        {
            JObject barber = new JObject();

            barber.Add("id", id);
            barber.Add("username", username);
            barber.Add("firstname", firstname);
            barber.Add("surname", surname);
            barber.Add("email", email);
            barber.Add("password", password);
            barber.Add("avatar", avatar);
            barber.Add("login_code", login_code);

            return barber;
        }

    }


}
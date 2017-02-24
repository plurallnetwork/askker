using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Models
{
    public class UserLoginModel
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public UserLoginModel(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(this.Username) && !string.IsNullOrWhiteSpace(this.Password);
        }
    }
}

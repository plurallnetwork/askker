using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Models
{
    public class UserFriendModel
    {
        public string id { get; set; }

        public string name { get; set; }

        public string profilePicture { get; set; }

        public UserFriendModel(string id, string name, string profilePicture)
        {
            this.id = id;
            this.name = name;
            this.profilePicture = profilePicture;
        }
    }
}

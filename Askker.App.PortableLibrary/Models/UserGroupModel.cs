using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Models
{
    public class UserGroupModel
    {
        public string id { get; set; }

        public string searchName { get; set; }

        public string profilePicture { get; set; }

        public UserGroupModel(string id, string searchName, string profilePicture)
        {
            this.id = id;
            this.searchName = searchName;
            this.profilePicture = profilePicture;
        }
    }
}

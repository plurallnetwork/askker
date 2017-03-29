using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Models
{
    public class UserFriendsModel
    {
        public string userId { get; set; }

        public Friends friends { get; set; }
    }

    public class Friends
    {
        public List<string> ids { get; set; }

        public List<string> names { get; set; }

        public List<string> profilePictures { get; set; }
    }
}

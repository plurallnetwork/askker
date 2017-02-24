using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Models
{
    public class TokenModel
    {
        public string Access_Token { get; set; }

        public string Token_Type { get; set; }

        public int Expires_In { get; set; }

        public DateTime Issued { get; set; }

        public DateTime Expires { get; set; }

        public string UserName { get; set; }

        public string Name { get; set; }

        public string Id { get; set; }

        public string ProfilePicturePath { get; set; }

        public bool IsShowingTour { get; set; }

        public bool IsStillValid(DateTime currentDateTime)
        {
            return currentDateTime.CompareTo(this.Expires) == -1;
        }
    }
}

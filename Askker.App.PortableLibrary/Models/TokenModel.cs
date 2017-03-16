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
        public string access_token { get; set; }

        public string token_type { get; set; }

        public int expires_in { get; set; }

        public DateTime issued { get; set; }

        public DateTime expires { get; set; }

        public string userName { get; set; }

        public string name { get; set; }

        public string id { get; set; }

        public string profilePicturePath { get; set; }

        public bool isShowingTour { get; set; }

        public bool isStillValid(DateTime currentDateTime)
        {
            return currentDateTime.CompareTo(this.expires) == -1;
        }
    }
}

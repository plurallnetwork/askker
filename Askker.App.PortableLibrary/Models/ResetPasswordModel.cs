using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Models
{
    public class ResetPasswordModel
    {
        public string username { get; set; }

        public string code { get; set; }

        public ResetPasswordModel(string username, string code)
        {
            this.username = username;
            this.code = code;
        }
    }
}

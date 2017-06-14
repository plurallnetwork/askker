using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Models
{
    public class SetPasswordModel
    {
        public string Email { get; set; }

        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }

        public SetPasswordModel(string email, string newPwd, string confirmPwd)
        {
            this.Email = email;
            this.NewPassword = newPwd;
            this.ConfirmPassword = confirmPwd;
        }
    }
}

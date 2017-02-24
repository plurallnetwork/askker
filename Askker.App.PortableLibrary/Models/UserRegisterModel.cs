using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Models
{
    public class UserRegisterModel
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public bool IsAgreePrivacyPolicy { get; set; }

        public UserRegisterModel(string Name, string Email, string Password, string ConfirmPassword, bool IsAgreePrivacyPolicy)
        {
            this.Name = Name;
            this.Email = Email;
            this.Password = Password;
            this.ConfirmPassword = ConfirmPassword;
            this.IsAgreePrivacyPolicy = IsAgreePrivacyPolicy;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(this.Name) && 
                   !string.IsNullOrWhiteSpace(this.Email) && 
                   !string.IsNullOrWhiteSpace(this.Password) && 
                   !string.IsNullOrWhiteSpace(this.ConfirmPassword) && 
                   this.IsAgreePrivacyPolicy;
        }
    }
}

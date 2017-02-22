using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Models
{
    public sealed class UserRegisterModel
    {
        public readonly string Name;
        public readonly string Email;
        public readonly string Password;
        public readonly string ConfirmPassword;
        public readonly bool IsAgreePrivacyPolicy;

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

using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Auth;
using System.Linq;
using Askker.App.PortableLibrary.Services;
using Askker.App.PortableLibrary.Models;

namespace Askker.App.iOS
{
    public class CredentialsService : ICredentialsService
    {
        public CredentialsService() { }

        public string access_token
        {
            get
            {
                var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
                return (account != null) ? account.Properties["access_token"] : null;
            }
        }

        public string token_type
        {
            get
            {
                var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
                return (account != null) ? account.Properties["token_type"] : null;
            }
        }

        public int expires_in
        {
            get
            {
                var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
                string result = (account != null) ? account.Properties["expires_in"] : null;
                int x = 0;

                if (Int32.TryParse(result, out x))
                {
                    return x;
                }

                return x;
            }
        }

        public DateTime issued
        {
            get
            {
                var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
                string result = (account != null) ? account.Properties["issued"] : null;
                if(result != null)
                {
                    return DateTime.Parse(result);
                }else
                {
                    return DateTime.MinValue;
                }
            }
        }

        public DateTime expires
        {
            get
            {
                var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
                string result = (account != null) ? account.Properties["expires"] : null;
                if (result != null)
                {
                    return DateTime.Parse(result);
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }

        public string userName
        {
            get
            {                
                var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
                return (account != null) ? account.Username : null;
            }
        }

        public string name
        {
            get
            {
                var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
                return (account != null) ? account.Properties["name"] : null;
            }
        }

        public string id
        {
            get
            {
                var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
                return (account != null) ? account.Properties["id"] : null;
            }
        }

        public string profilePicturePath
        {
            get
            {
                var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
                return (account != null) ? account.Properties["profilePicturePath"] : null;
            }
        }

        public bool isShowingTour
        {
            get
            {
                var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
                string result = (account != null) ? account.Properties["isShowingTour"] : null;
                if (result == null)
                {
                    return false;
                }
                else
                {
                    return result.Equals("True");
                }
            }
        }

        public bool isStillValid
        {
            get
            {
                var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
                string result = (account != null) ? account.Properties["isStillValid"] : null;
                if(result == null)
                {
                    return false;
                }else
                {
                    return result.Equals("True");
                }
            }
        }

        public TokenModel GetTokenModel()
        {
            TokenModel model = new TokenModel();

            model.Access_Token = this.access_token;
            model.Expires = this.expires;
            model.Expires_In = this.expires_in;
            model.Id = this.id;
            model.IsShowingTour = this.isShowingTour;
            model.Issued = this.issued;
            model.Name = this.name;
            model.ProfilePicturePath = this.profilePicturePath;
            model.Token_Type = this.token_type;
            model.UserName = this.userName;

            return model;
        }

        public void SaveCredentials(TokenModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.UserName) && !string.IsNullOrWhiteSpace(model.Access_Token))
            {
                Account account = new Account
                {
                    Username = model.UserName
                };
                account.Properties.Add("access_token", model.Access_Token);
                account.Properties.Add("expires", model.Expires.ToString());
                account.Properties.Add("expires_in", model.Expires_In.ToString());
                account.Properties.Add("id", model.Id);
                account.Properties.Add("isShowingTour", Convert.ToString(model.IsShowingTour));
                account.Properties.Add("IsStillValid", Convert.ToString(model.IsStillValid(DateTime.Now)));
                account.Properties.Add("issued", model.Issued.ToString());
                account.Properties.Add("name", model.Name);
                account.Properties.Add("profilePicturePath", model.ProfilePicturePath);
                account.Properties.Add("token_type", model.Token_Type);
                AccountStore.Create().Save(account, AppDelegate.AppName);
            }
        }

        public void DeleteCredentials()
        {
            var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
            if (account != null)
            {
                AccountStore.Create().Delete(account, AppDelegate.AppName);
            }
        }

        public bool DoCredentialsExist()
        {
            return AccountStore.Create().FindAccountsForService(AppDelegate.AppName).Any() ? true : false;
        }
    }
}

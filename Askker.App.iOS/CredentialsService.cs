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

            model.access_token = this.access_token;
            model.expires = this.expires;
            model.expires_in = this.expires_in;
            model.id = this.id;
            model.isShowingTour = this.isShowingTour;
            model.issued = this.issued;
            model.name = this.name;
            model.profilePicturePath = this.profilePicturePath;
            model.token_type = this.token_type;
            model.userName = this.userName;

            return model;
        }

        public void SaveCredentials(TokenModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.userName) && !string.IsNullOrWhiteSpace(model.access_token))
            {
                Account account = new Account
                {
                    Username = model.userName
                };
                account.Properties.Add("access_token", model.access_token);
                account.Properties.Add("expires", model.expires.ToString());
                account.Properties.Add("expires_in", model.expires_in.ToString());
                account.Properties.Add("id", model.id);
                account.Properties.Add("isShowingTour", Convert.ToString(model.isShowingTour));
                account.Properties.Add("IsStillValid", Convert.ToString(model.IsStillValid(DateTime.Now)));
                account.Properties.Add("issued", model.issued.ToString());
                account.Properties.Add("name", model.name);
                account.Properties.Add("profilePicturePath", model.profilePicturePath);
                account.Properties.Add("token_type", model.token_type);
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

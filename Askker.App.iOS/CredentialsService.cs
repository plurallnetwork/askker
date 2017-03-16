using System;
using Xamarin.Auth;
using System.Linq;
using Askker.App.PortableLibrary.Models;

namespace Askker.App.iOS
{
    public class CredentialsService
    {
        public CredentialsService() { }

        public static string access_token
        {
            get
            {
                var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
                return (account != null) ? account.Properties["access_token"] : null;
            }
        }

        public static string token_type
        {
            get
            {
                var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
                return (account != null) ? account.Properties["token_type"] : null;
            }
        }

        public static int expires_in
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

        public static DateTime issued
        {
            get
            {
                var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
                string result = (account != null) ? account.Properties["issued"] : null;
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

        public static DateTime expires
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

        public static string userName
        {
            get
            {                
                var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
                return (account != null) ? account.Username : null;
            }
        }

        //public static string name
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["name"] : null;
        //    }
        //}

        //public static string id
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["id"] : null;
        //    }
        //}

        //public static int age
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        string result = (account != null) ? account.Properties["age"] : null;
        //        int x = 0;

        //        if (Int32.TryParse(result, out x))
        //        {
        //            return x;
        //        }

        //        return x;
        //    }
        //}

        //public static string gender
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["gender"] : null;
        //    }
        //}

        //public static string city
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["city"] : null;
        //    }
        //}

        //public static string state
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["state"] : null;
        //    }
        //}

        //public static string country
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["country"] : null;
        //    }
        //}

        //public static string education
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["education"] : null;
        //    }
        //}

        //public static string profilePicturePath
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["profilePicturePath"] : null;
        //    }
        //}

        //public static bool isActive
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        string result = (account != null) ? account.Properties["isActive"] : null;
        //        if (result == null)
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            return result.Equals("True");
        //        }
        //    }
        //}

        //public static bool isShowingTour
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        string result = (account != null) ? account.Properties["isShowingTour"] : null;
        //        if (result == null)
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            return result.Equals("True");
        //        }
        //    }
        //}

        //public static string description
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["description"] : null;
        //    }
        //}

        //public static string descriptionPrivacyLevel
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["descriptionPrivacyLevel"] : null;
        //    }
        //}

        //public static string profilePicturePrivacyLevel
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["profilePicturePrivacyLevel"] : null;
        //    }
        //}

        //public static string namePrivacyLevel
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["namePrivacyLevel"] : null;
        //    }
        //}

        //public static string genderPrivacyLevel
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["genderPrivacyLevel"] : null;
        //    }
        //}

        //public static string emailPrivacyLevel
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["emailPrivacyLevel"] : null;
        //    }
        //}

        //public static string agePrivacyLevel
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["agePrivacyLevel"] : null;
        //    }
        //}

        //public static string questionsMadePrivacyLevel
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["questionsMadePrivacyLevel"] : null;
        //    }
        //}

        //public static string answersGivenPrivacyLevel
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["answersGivenPrivacyLevel"] : null;
        //    }
        //}

        //public static string influencedByPrivacyLevel
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["influencedByPrivacyLevel"] : null;
        //    }
        //}

        //public static string influenceOverPrivacyLevel
        //{
        //    get
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
        //        return (account != null) ? account.Properties["influenceOverPrivacyLevel"] : null;
        //    }
        //}

        //public static bool isStillValid(DateTime currentDateTime)
        //{
        //    return currentDateTime.CompareTo(expires) == -1;
        //}

        public static TokenModel GetTokenModel()
        {
            TokenModel tokenModel = new TokenModel();

            tokenModel.access_token = access_token;
            tokenModel.token_type = token_type;
            tokenModel.expires_in = expires_in;
            tokenModel.issued = issued;
            tokenModel.expires = expires;

            return tokenModel;
        }

        public static UserModel GetUserModel()
        {
            UserModel userModel = new UserModel();

            userModel.userName = userName;
            //userModel.id = id;
            //userModel.name = name;
            //userModel.age = age;
            //userModel.gender = gender;
            //userModel.city = city;
            //userModel.state = state;
            //userModel.country = country;
            //userModel.education = education;
            //userModel.profilePicturePath = profilePicturePath;
            //userModel.isActive = isActive;
            //userModel.isShowingTour = isShowingTour;
            //userModel.description = description;
            //userModel.descriptionPrivacyLevel = descriptionPrivacyLevel;
            //userModel.profilePicturePrivacyLevel = profilePicturePrivacyLevel;
            //userModel.namePrivacyLevel = namePrivacyLevel;
            //userModel.genderPrivacyLevel = genderPrivacyLevel;
            //userModel.emailPrivacyLevel = emailPrivacyLevel;
            //userModel.agePrivacyLevel = agePrivacyLevel;
            //userModel.questionsMadePrivacyLevel = questionsMadePrivacyLevel;
            //userModel.answersGivenPrivacyLevel = answersGivenPrivacyLevel;
            //userModel.influencedByPrivacyLevel = influencedByPrivacyLevel;
            //userModel.influenceOverPrivacyLevel = influenceOverPrivacyLevel;

            return userModel;
        }

        public static void SaveCredentials(TokenModel tokenModel, UserModel userModel)
        {
            if (!string.IsNullOrWhiteSpace(userModel.userName) && !string.IsNullOrWhiteSpace(tokenModel.access_token))
            {
                Account account = new Account
                {
                    Username = userModel.userName
                };

                // Token
                account.Properties.Add("access_token", tokenModel.access_token);

                if (tokenModel.token_type != null)
                {
                    account.Properties.Add("token_type", tokenModel.token_type);
                }
                else
                {
                    account.Properties.Add("token_type", "null");
                }

                account.Properties.Add("expires_in", tokenModel.expires_in.ToString());
                account.Properties.Add("issued", tokenModel.issued.ToString());
                account.Properties.Add("expires", tokenModel.expires.ToString());

                // User
                //account.Properties.Add("id", userModel.id);
                //account.Properties.Add("name", userModel.name);
                //account.Properties.Add("age", userModel.age.ToString());
                //account.Properties.Add("gender", userModel.gender);
                //account.Properties.Add("city", userModel.city);
                //account.Properties.Add("state", userModel.state);
                //account.Properties.Add("country", userModel.country);
                //account.Properties.Add("education", userModel.education);
                //account.Properties.Add("profilePicturePath", userModel.profilePicturePath);
                //account.Properties.Add("isActive", Convert.ToString(userModel.isActive));
                //account.Properties.Add("isShowingTour", Convert.ToString(userModel.isShowingTour));
                //account.Properties.Add("description", userModel.description);

                //account.Properties.Add("descriptionPrivacyLevel", userModel.descriptionPrivacyLevel);
                //account.Properties.Add("profilePicturePrivacyLevel", userModel.profilePicturePrivacyLevel);
                //account.Properties.Add("namePrivacyLevel", userModel.namePrivacyLevel);
                //account.Properties.Add("genderPrivacyLevel", userModel.genderPrivacyLevel);
                //account.Properties.Add("emailPrivacyLevel", userModel.emailPrivacyLevel);
                //account.Properties.Add("agePrivacyLevel", userModel.agePrivacyLevel);
                //account.Properties.Add("questionsMadePrivacyLevel", userModel.questionsMadePrivacyLevel);
                //account.Properties.Add("answersGivenPrivacyLevel", userModel.answersGivenPrivacyLevel);
                //account.Properties.Add("influencedByPrivacyLevel", userModel.influencedByPrivacyLevel);
                //account.Properties.Add("influenceOverPrivacyLevel", userModel.influenceOverPrivacyLevel);

                AccountStore.Create().Save(account, AppDelegate.AppName);
            }
        }

        //public static void UpdateCredentials(UserModel model)
        //{
        //    if (!string.IsNullOrWhiteSpace(model.userName))
        //    {
        //        var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();

        //        if (account != null)
        //        {
        //            // User
        //            account.Properties["name"] = model.name;
        //            account.Properties["age"] = model.age.ToString();
        //            account.Properties["gender"] = model.gender;
        //            account.Properties["city"] = model.city;
        //            account.Properties["state"] = model.state;
        //            account.Properties["country"] = model.country;
        //            account.Properties["education"] = model.education;
        //            account.Properties["profilePicturePath"] = model.profilePicturePath;
        //            account.Properties["isActive"] = Convert.ToString(model.isActive);
        //            account.Properties["isShowingTour"] = Convert.ToString(model.isShowingTour);
        //            account.Properties["description"] = model.description;

        //            account.Properties["descriptionPrivacyLevel"] = model.descriptionPrivacyLevel;
        //            account.Properties["profilePicturePrivacyLevel"] = model.profilePicturePrivacyLevel;
        //            account.Properties["namePrivacyLevel"] = model.namePrivacyLevel;
        //            account.Properties["genderPrivacyLevel"] = model.genderPrivacyLevel;
        //            account.Properties["emailPrivacyLevel"] = model.emailPrivacyLevel;
        //            account.Properties["agePrivacyLevel"] = model.agePrivacyLevel;
        //            account.Properties["questionsMadePrivacyLevel"] = model.questionsMadePrivacyLevel;
        //            account.Properties["answersGivenPrivacyLevel"] = model.answersGivenPrivacyLevel;
        //            account.Properties["influencedByPrivacyLevel"] = model.influencedByPrivacyLevel;
        //            account.Properties["influenceOverPrivacyLevel"] = model.influenceOverPrivacyLevel;

        //            AccountStore.Create().Save(account, AppDelegate.AppName);
        //        }
        //    }
        //}

        public static void DeleteCredentials()
        {
            var account = AccountStore.Create().FindAccountsForService(AppDelegate.AppName).FirstOrDefault();
            if (account != null)
            {
                AccountStore.Create().Delete(account, AppDelegate.AppName);
            }
        }

        public static bool DoCredentialsExist()
        {
            return AccountStore.Create().FindAccountsForService(AppDelegate.AppName).Any() ? true : false;
        }
    }
}

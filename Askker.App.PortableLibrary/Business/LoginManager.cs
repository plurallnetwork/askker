using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Business
{
    public class LoginManager
    {
        public async Task<TokenModel> GetAuthorizationToken(UserLoginModel userLoginModel)
        {
            try
            {
                LoginService loginService = new LoginService();

                var response = await loginService.GetAuthorizationToken(userLoginModel);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<TokenModel>(json);
                }
                else
                {
                    if (!json.Equals(""))
                    {
                        throw new Exception(JObject.Parse(json).SelectToken("$.error").ToString());
                    }
                    else
                    {
                        throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<UserModel> GetUserById(string authenticationToken)
        {
            LoginService loginService = new LoginService();

            var response = await loginService.GetUserById(authenticationToken);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<UserModel>(json);
            }
            else
            {
                if (!json.Equals(""))
                {
                    throw new Exception(JObject.Parse(json).SelectToken("$.error").ToString());
                }
                else
                {
                    throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                }
            }
        }

        public async Task<UserModel> GetUserById(string authenticationToken, string userId)
        {
            LoginService loginService = new LoginService();

            var response = await loginService.GetUserById(authenticationToken, userId);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<UserModel>(json);
            }
            else
            {
                if (!json.Equals(""))
                {
                    throw new Exception(JObject.Parse(json).SelectToken("$.error").ToString());
                }
                else
                {
                    throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                }
            }
        }

        public async Task Update(string authenticationToken, UserModel userModel, byte[] profileImage, string profileImageName)
        {
            try
            {
                LoginService loginService = new LoginService();

                var response = await loginService.Update(authenticationToken, userModel, profileImage, profileImageName);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

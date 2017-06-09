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
        public LoginService loginService { get; set; }

        public LoginManager()
        {
            loginService = new LoginService();
        }

        public async Task<TokenModel> GetAuthorizationToken(UserLoginModel userLoginModel)
        {
            try
            {
                var response = await loginService.GetAuthorizationToken(userLoginModel);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TokenModel token = JsonConvert.DeserializeObject<TokenModel>(json);

                    // Set the expiration date locally, because of the difference between the timezone of the client and server
                    // Removed 10 seconds of the expiration time to avoid communication delay problems
                    token.expires = DateTime.Now.AddSeconds(token.expires_in - 10);

                    return token;
                }
                else
                {
                    if (!json.Equals(""))
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            throw new Exception("Unauthorized");
                        }
                        else
                        {
                            //JObject.Parse(json).SelectToken("$.error") != null
                            throw new Exception(JObject.Parse(json).SelectToken("$.error").ToString());
                        }
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

        public UserModel GetUserByTokenSync(string authenticationToken)
        {
            var response = loginService.GetUserByTokenSync(authenticationToken);
            var json = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<UserModel>(json);
            }
            else
            {
                if (!json.Equals(""))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Unauthorized");
                    }
                    else
                    {
                        throw new Exception(JObject.Parse(json).SelectToken("$.error").ToString());
                    }
                }
                else
                {
                    throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                }
            }
        }

        public async Task<UserModel> GetUserById(string authenticationToken)
        {
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
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Unauthorized");
                    }
                    else
                    {
                        //JObject.Parse(json).SelectToken("$.error") != null
                        throw new Exception(JObject.Parse(json).SelectToken("$.error").ToString());
                    }
                }
                else
                {
                    throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                }
            }
        }

        public async Task<UserModel> GetUserById(string authenticationToken, string userId)
        {
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
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Unauthorized");
                    }
                    else
                    {
                        //JObject.Parse(json).SelectToken("$.error") != null
                        throw new Exception(JObject.Parse(json).SelectToken("$.error").ToString());
                    }
                }
                else
                {
                    throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                }
            }
        }

        public async Task<List<UserModel>> SearchUsersByName(string authenticationToken, string name)
        {
            var response = await loginService.SearchUsersByName(authenticationToken, name);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<UserModel>>(json);
            }
            else
            {
                if (!json.Equals(""))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Unauthorized");
                    }
                    else
                    {
                        //JObject.Parse(json).SelectToken("$.error") != null
                        throw new Exception(JObject.Parse(json).SelectToken("$.error").ToString());
                    }
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
                var response = await loginService.Update(authenticationToken, userModel, profileImage, profileImageName);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Unauthorized");
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

        public async Task UpdateUserInformation(UserModel userModel, string authenticationToken)
        {
            try
            {
                var response = await loginService.UpdateUserInformation(userModel, authenticationToken);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Unauthorized");
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

        public async Task SendEmailResetPassword(string email)
        {
            try
            {
                var response = await loginService.SendEmailResetPassword(email);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (!json.Equals(""))
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            throw new Exception("Unauthorized");
                        }
                        else
                        {
                            throw new Exception(json);
                        }
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
    }
}

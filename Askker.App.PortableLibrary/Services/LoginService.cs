using Askker.App.PortableLibrary.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Services
{
    public class LoginService
    {
        public async Task<HttpResponseMessage> GetAuthorizationToken(UserLoginModel userLoginModel)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var formContent = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>("username", userLoginModel.Username),
                        new KeyValuePair<string, string>("password", userLoginModel.Password)
                    });

                    return await client.PostAsync("https://ec2-52-27-214-166.us-west-2.compute.amazonaws.com:44322/Token", formContent);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> GetUserById(string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);

                    return await client.GetAsync("https://ec2-52-27-214-166.us-west-2.compute.amazonaws.com:44322/api/Account/GetUserById");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

using Askker.App.PortableLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Services
{
    public class LoginService
    {
        public async Task<string> GetAuthorizationToken(UserLoginModel loginModel)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var formContent = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>("username", loginModel.Username),
                        new KeyValuePair<string, string>("password", loginModel.Password)
                    });

                    var postResponse = await client.PostAsync("https://ec2-52-27-214-166.us-west-2.compute.amazonaws.com:44321/Token", formContent);
                    //postResponse.EnsureSuccessStatusCode();

                    return await postResponse.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

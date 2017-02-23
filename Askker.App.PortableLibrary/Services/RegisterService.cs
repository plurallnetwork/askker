using Askker.App.PortableLibrary.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Services
{
    public class RegisterService
    {
        public async Task<string> RegisterUser(UserRegisterModel userRegisterModel)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var formContent = new StringContent(JsonConvert.SerializeObject(userRegisterModel), Encoding.UTF8, "application/json");

                    var postResponse = await client.PostAsync("https://ec2-52-27-214-166.us-west-2.compute.amazonaws.com:44322/api/Account/Register", formContent);

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

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
        public async Task<HttpResponseMessage> RegisterUser(UserRegisterModel userRegisterModel)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var formContent = new StringContent(JsonConvert.SerializeObject(userRegisterModel), Encoding.UTF8, "application/json");

                    return await client.PostAsync("https://askker.io:44322/api/Account/Register", formContent);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Business
{
    public class LoginManager
    {
        public async Task<TokenModel> GetAuthorizationToken(UserLoginModel loginModel)
        {
            try
            {
                LoginService loginService = new LoginService();

                var response = await loginService.GetAuthorizationToken(loginModel);
                return JsonConvert.DeserializeObject<TokenModel>(response);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

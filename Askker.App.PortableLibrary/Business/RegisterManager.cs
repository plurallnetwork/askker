using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Business
{
    public class RegisterManager
    {
        public async Task<string> RegisterUser(UserRegisterModel userRegisterModel)
        {
            try
            {
                RegisterService registerService = new RegisterService();

                return await registerService.RegisterUser(userRegisterModel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

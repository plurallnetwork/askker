using Askker.App.PortableLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Business
{
    public class UserManager
    {
        public async Task BlockUser(string userId, string blockedUserId, string authenticationToken)
        {
            try
            {
                UserService userService = new UserService();

                var response = await userService.BlockUser(userId, blockedUserId, authenticationToken);

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

        public async Task UnblockUser(string userId, string blockedUserId, string authenticationToken)
        {
            try
            {
                UserService userService = new UserService();

                var response = await userService.UnblockUser(userId, blockedUserId, authenticationToken);

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

        public async Task<bool> IsUserBlocked (string blockedUserId, string authenticationToken)
        {
            try
            {
                UserService userService = new UserService();
                bool isUserBlocked = false;

                var response = await userService.IsUserBlocked(blockedUserId, authenticationToken);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(result))
                    {
                        Boolean.TryParse(result, out isUserBlocked);

                        return isUserBlocked;
                    }
                    else
                    {
                        return isUserBlocked;
                    }
                }
                else
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
    }
}

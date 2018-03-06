using Askker.App.iOS;
using Askker.App.PortableLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Services
{
    public class FriendService
    {
        public async Task<HttpResponseMessage> GetFriends(string userId, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format(EnvironmentConstants.getServerUrl() + "api/survey/getfriends/{0}", userId));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    return await client.GetAsync(uri);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> GetUserRelationshipStatus(string authenticationToken, string friendId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format(EnvironmentConstants.getServerUrl() + "api/Account/GetUserRelationshipStatus/{0}", friendId));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    return await client.GetAsync(uri);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> AddFriend(string authenticationToken, string friendId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format(EnvironmentConstants.getServerUrl() + "api/Account/AddUserFriend/{0}", friendId));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    return await client.PostAsync(uri, null);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> UpdateUserRelationshipStatus(string authenticationToken, string friendId, RelationshipStatus relationshipStatus)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format(EnvironmentConstants.getServerUrl() + "api/Account/UpdateUserRelationshipStatus/{0}/{1}", friendId, relationshipStatus.ToString()));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    return await client.PostAsync(uri, null);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

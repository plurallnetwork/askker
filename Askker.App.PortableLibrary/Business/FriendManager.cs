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
    public class FriendManager
    {
        public async Task<List<UserFriendModel>> GetFriends(string userId, string authenticationToken)
        {
            try
            {
                FriendService friendService = new FriendService();
                var userFriends = new List<UserFriendModel>();

                var response = await friendService.GetFriends(userId, authenticationToken);

                if (response.IsSuccessStatusCode)
                {
                    var json = JObject.Parse(await response.Content.ReadAsStringAsync());

                    if (json.HasValues)
                    {
                        var ids = json.SelectToken("$.friends.ids").ToObject<JArray>();
                        var names = json.SelectToken("$.friends.names").ToObject<JArray>();
                        var profilePictures = json.SelectToken("$.friends.profilePictures").ToObject<JArray>();

                        for (int i = 0; i < ids.Count; i++)
                        {
                            userFriends.Add(new UserFriendModel(ids[i].ToString(), names[i].ToString(), profilePictures[i].ToString()));
                        }
                    }

                    return userFriends;
                }
                else
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

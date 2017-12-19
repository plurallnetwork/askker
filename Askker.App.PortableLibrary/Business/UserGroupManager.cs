using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Business
{
    public class UserGroupManager
    {
        public async Task<List<UserGroupModel>> GetGroups(string userId, string authenticationToken)
        {
            try
            {
                UserGroupService groupService = new UserGroupService();
                var groups = new List<UserGroupModel>();

                var response = await groupService.GetGroups(userId, authenticationToken);

                if (response.IsSuccessStatusCode)
                {
                    var json = JObject.Parse(await response.Content.ReadAsStringAsync());

                    if (json.HasValues)
                    {
                        var ids = json.SelectToken("$.groups.ids").ToObject<JArray>();
                        var names = json.SelectToken("$.groups.names").ToObject<JArray>();
                        var profilePictures = json.SelectToken("$.groups.profilePictures").ToObject<JArray>();

                        for (int i = 0; i < ids.Count; i++)
                        {
                            groups.Add(new UserGroupModel(ids[i].ToString(), names[i].ToString(), profilePictures[i].ToString()));
                        }
                    }

                    return groups;
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

        public async Task<UserGroupRelationshipStatus> GetGroupRelationshipStatus(string authenticationToken, string groupId)
        {
            try
            {
                UserGroupService groupService = new UserGroupService();
                UserGroupRelationshipStatus groupRelationshipStatus = new UserGroupRelationshipStatus();

                var response = await groupService.GetGroupRelationshipStatus(authenticationToken, groupId);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(result))
                    {
                        Enum.TryParse(result, out groupRelationshipStatus);

                        return groupRelationshipStatus;
                    }
                    else
                    {
                        return UserGroupRelationshipStatus.NotInGroup;
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

        public async Task AddGroup(string authenticationToken, string groupId)
        {
            try
            {
                UserGroupService groupService = new UserGroupService();

                var response = await groupService.AddGroup(authenticationToken, groupId);

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

        public async Task UpdateGroupRelationshipStatus(string authenticationToken, string groupId, UserGroupRelationshipStatus groupRelationshipStatus)
        {
            try
            {
                UserGroupService groupService = new UserGroupService();

                var response = await groupService.UpdateGroupRelationshipStatus(authenticationToken, groupId, groupRelationshipStatus);

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

        public async Task<List<UserGroupModel>> SearchGroupsByName(string authenticationToken, string name)
        {
            var response = await new UserGroupService().SearchGroupsByName(authenticationToken, name);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<UserGroupModel>>(json);
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
    }
}

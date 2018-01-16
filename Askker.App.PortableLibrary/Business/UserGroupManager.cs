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
                    var jsonGroups = JArray.Parse(await response.Content.ReadAsStringAsync());

                    if (jsonGroups.HasValues)
                    {
                        foreach (var group in jsonGroups)
                        {
                            groups.Add(new UserGroupModel(group["userId"].ToString(), group["creationDate"].ToString(), group["name"].ToString(), group["profilePicture"].ToString()));
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

        public async Task<UserGroupRelationshipStatus> GetGroupRelationshipStatus(string authenticationToken, string groupId, string userId)
        {
            try
            {
                UserGroupService groupService = new UserGroupService();
                UserGroupRelationshipStatus groupRelationshipStatus = new UserGroupRelationshipStatus();

                var response = await groupService.GetGroupRelationshipStatus(authenticationToken, groupId, userId);

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
                        return UserGroupRelationshipStatus.NotMember;
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

        public async Task RequestPermissionToGroup(string authenticationToken, string groupId, string userId)
        {
            try
            {
                UserGroupService groupService = new UserGroupService();

                var response = await groupService.RequestPermissionToGroup(authenticationToken, groupId, userId);

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

        public async Task UpdateGroupRelationshipStatus(string authenticationToken, string groupId, string userId, UserGroupRelationshipStatus groupRelationshipStatus)
        {
            try
            {
                UserGroupService groupService = new UserGroupService();

                var response = await groupService.UpdateGroupRelationshipStatus(authenticationToken, groupId, userId, groupRelationshipStatus);

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

        public async Task<UserGroupModel> GetGroupById(string authenticationToken, string groupId)
        {
            var response = await new UserGroupService().GetGroupById(authenticationToken, groupId);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<UserGroupModel>(json);
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

        public async Task<List<UserGroupMemberModel>> GetGroupMembers(string authenticationToken, string groupId)
        {
            var response = await new UserGroupService().GetGroupMembers(authenticationToken, groupId);
            var json = await response.Content.ReadAsStringAsync();
            var members = new List<UserGroupMemberModel>();

            if (response.IsSuccessStatusCode)
            {
                var jsonMembers = JArray.Parse(await response.Content.ReadAsStringAsync());

                if (jsonMembers.HasValues)
                {
                    foreach (var member in jsonMembers)
                    {
                        members.Add(new UserGroupMemberModel(member["id"].ToString(), member["relationshipStatus"].ToString(), member["name"].ToString(), member["requestDate"].ToString(), member["acceptDate"].ToString(), member["profilePicture"].ToString()));
                    }
                }

                return members;
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

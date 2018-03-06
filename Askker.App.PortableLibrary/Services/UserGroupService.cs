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
    public class UserGroupService
    {
        public async Task<HttpResponseMessage> GetGroups(string userId, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format(EnvironmentConstants.getServerUrl() + "api/survey/getusergroups/{0}", userId));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    return await client.GetAsync(uri);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> GetGroupRelationshipStatus(string authenticationToken, string groupId, string userId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format(EnvironmentConstants.getServerUrl() + "api/Account/GetGroupRelationshipStatus/{0}/{1}", groupId, userId));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    return await client.GetAsync(uri);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> RequestPermissionToGroup(string authenticationToken, string groupId, string userId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format(EnvironmentConstants.getServerUrl() + "api/Account/RequestPermissionToGroup/{0}/{1}", groupId, userId));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    return await client.PostAsync(uri, null);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> UpdateGroupRelationshipStatus(string authenticationToken, string groupId, string userId, UserGroupRelationshipStatus groupRelationshipStatus)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format(EnvironmentConstants.getServerUrl() + "api/Account/UpdateUserRelationshipStatusGroup/{0}/{1}/{2}", groupId, userId, groupRelationshipStatus.ToString()));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);
                    return await client.PostAsync(uri, null);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> SearchGroupsByName(string authenticationToken, string name)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format(EnvironmentConstants.getServerUrl() + "api/Account/SearchGroupsByName/{0}", name));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);

                    return await client.GetAsync(uri);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> GetGroupById(string authenticationToken, string groupId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format(EnvironmentConstants.getServerUrl() + "api/survey/GetGroup/{0}", groupId));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);

                    return await client.GetAsync(uri);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> GetGroupMembers(string authenticationToken, string groupId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format(EnvironmentConstants.getServerUrl() + "api/survey/GetGroupMembers/{0}", groupId));

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);

                    return await client.GetAsync(uri);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

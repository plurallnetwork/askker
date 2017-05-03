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
    public class NotificationService
    {
        public async Task<HttpResponseMessage> SetUserNotification(UserNotificationModel userNotificationModel, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var formContent = new StringContent(JsonConvert.SerializeObject(userNotificationModel, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);

                    return await client.PostAsync("https://blinq-development.com:44322/api/survey/setusernotification", formContent);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> SetUserNotificationDismissed(UserNotificationModel userNotificationModel, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var formContent = new StringContent(JsonConvert.SerializeObject(userNotificationModel, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + authenticationToken);

                    return await client.PostAsync("https://blinq-development.com:44322/api/survey/setusernotificationdismissed", formContent);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> GetUserNotifications(string userId, string authenticationToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri(string.Format("https://blinq-development.com:44322/api/survey/getusernotifications/{0}", userId));

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

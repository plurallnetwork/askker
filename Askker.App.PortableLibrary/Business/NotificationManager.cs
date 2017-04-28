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
    public class NotificationManager
    {
        public async Task SetUserNotification(UserNotificationModel userNotificationModel, string authenticationToken)
        {
            try
            {
                NotificationService notificationService = new NotificationService();

                var response = await notificationService.SetUserNotification(userNotificationModel, authenticationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task SetUserNotificationDismissed(UserNotificationModel userNotificationModel, string authenticationToken)
        {
            try
            {
                NotificationService notificationService = new NotificationService();

                var response = await notificationService.SetUserNotificationDismissed(userNotificationModel, authenticationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<UserNotificationModel>> GetUserNotifications(string userId, string authenticationToken)
        {
            try
            {
                NotificationService notificationService = new NotificationService();

                var response = await notificationService.GetUserNotifications(userId, authenticationToken);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<UserNotificationModel>>(json);
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

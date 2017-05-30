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

        public async Task SetUserNotificationDismissed(UserNotificationModel userNotificationModel, string authenticationToken)
        {
            try
            {
                NotificationService notificationService = new NotificationService();

                var response = await notificationService.SetUserNotificationDismissed(userNotificationModel, authenticationToken);

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

        public async Task<int> GetUserUnreadNotificationsCount(string userId, string authenticationToken)
        {
            try
            {
                NotificationService notificationService = new NotificationService();

                var response = await notificationService.GetUserUnreadNotificationsCount(userId, authenticationToken);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return Int32.Parse(json);
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

        public async Task SetUserNotificationsRead(string userId, string authenticationToken)
        {
            try
            {
                NotificationService notificationService = new NotificationService();

                var response = await notificationService.SetUserNotificationsRead(userId, authenticationToken);

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
    }
}

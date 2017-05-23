using Askker.App.PortableLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Models
{
    public class UserNotificationModel
    {
        public string userId { get; set; }

        public string notificationDate { get; set; }

        public UserFriendModel notificationUser { get; set; }

        public string type { get; set; }

        public string text { get; set; }

        public string link { get; set; }

        public int isRead { get; set; }

        public int isDismissed { get; set; }
    }
}

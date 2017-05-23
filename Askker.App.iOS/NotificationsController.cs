using Foundation;
using System;
using UIKit;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using CoreFoundation;

namespace Askker.App.iOS
{
    public partial class NotificationsController : UITableViewController
    {
        public static NSString notificationCellId = new NSString("notificationCellId");

        List<UserNotificationModel> notifications { get; set; }
        public UIActivityIndicatorView indicator;
        public UIRefreshControl refreshControl;

        public NotificationsController (IntPtr handle) : base (handle)
        {
            indicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
            indicator.Frame = new CoreGraphics.CGRect(0.0, 0.0, 80.0, 80.0);
            indicator.Center = this.View.Center;
            Add(indicator);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RegisterClassForCellReuse(typeof(NotificationsTableViewCell), notificationCellId);
            TableView.SeparatorInset = new UIEdgeInsets(0, 10, 0, 10);

            notifications = new List<UserNotificationModel>();

            refreshControl = new UIRefreshControl();
            refreshControl.ValueChanged += (sender, e) =>
            {
                refreshControl.BeginRefreshing();
                fetchNotifications();
            };

            TableView.Add(refreshControl);

            indicator.StartAnimating();
            fetchNotifications();
        }

        private async void fetchNotifications()
        {
            try
            {
                notifications = (await new NotificationManager().GetUserNotifications(LoginController.userModel.id, LoginController.tokenModel.access_token)).OrderByDescending(q => q.notificationDate).ToList();

                indicator.StopAnimating();
                refreshControl.EndRefreshing();
                TableView.ReloadData();
            }
            catch (Exception ex)
            {
                indicator.StopAnimating();
                refreshControl.EndRefreshing();
                Utils.HandleException(ex);
            }
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return notifications.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var notificationCell = tableView.DequeueReusableCell(notificationCellId) as NotificationsTableViewCell;

            if (notifications[indexPath.Row].notificationUser.profilePicture != null)
            {
                var url = new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + notifications[indexPath.Row].notificationUser.profilePicture);

                var task = NSUrlSession.SharedSession.CreateDataTask(url, (data, response, error) =>
                {
                    if (response == null)
                    {
                        notificationCell.profileImageView.Image = UIImage.FromBundle("Profile");
                    }
                    else
                    {
                        try
                        {
                            DispatchQueue.MainQueue.DispatchAsync(() =>
                            {
                                notificationCell.profileImageView.Image = UIImage.LoadFromData(data);
                            });
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                });
                task.Resume();
            }

            var attributedText = new NSMutableAttributedString(notifications[indexPath.Row].notificationUser.name, UIFont.BoldSystemFontOfSize(14));
            attributedText.Append(new NSAttributedString(" " + notifications[indexPath.Row].text, UIFont.SystemFontOfSize(14)));

            notificationCell.notificationLabel.AttributedText = attributedText;

            if (notifications[indexPath.Row].isDismissed == 0)
            {
                notificationCell.BackgroundColor = UIColor.FromWhiteAlpha(nfloat.Parse("0.95"), 1);
                notificationCell.notificationLabel.BackgroundColor = UIColor.FromWhiteAlpha(nfloat.Parse("0.95"), 1);
            }
            else
            {
                notificationCell.BackgroundColor = UIColor.White;
                notificationCell.notificationLabel.BackgroundColor = UIColor.White;
            }

            notificationCell.userId = notifications[indexPath.Row].userId;
            notificationCell.notificationDate = notifications[indexPath.Row].notificationDate;
            notificationCell.type = notifications[indexPath.Row].type;
            notificationCell.link = notifications[indexPath.Row].link;

            return notificationCell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            setNotificationDismissed(notifications[indexPath.Row]);

            var notificationCell = tableView.CellAt(indexPath) as NotificationsTableViewCell;
            if (notificationCell != null)
            {
                notificationCell.BackgroundColor = UIColor.White;
                notificationCell.notificationLabel.BackgroundColor = UIColor.White;
            }

            UIAlertView alert = new UIAlertView()
            {
                Title = "Notification",
                Message = "Notification Dismissed !"
            };
            alert.AddButton("OK");
            alert.Show();
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 60;
        }

        private async void setNotificationDismissed(UserNotificationModel userNotificationModel)
        {
            await new NotificationManager().SetUserNotificationDismissed(userNotificationModel, LoginController.tokenModel.access_token);
        }
    }

    public partial class NotificationsTableViewCell : UITableViewCell
    {
        public UIImageView profileImageView { get; set; }
        public UILabel notificationLabel { get; set; }

        public string userId { get; set; }
        public string notificationDate { get; set; }
        public string type { get; set; }
        public string link { get; set; }

        protected NotificationsTableViewCell(IntPtr handle) : base(handle)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            profileImageView = new UIImageView();
            profileImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            profileImageView.Layer.CornerRadius = 22;
            profileImageView.Layer.MasksToBounds = true;
            profileImageView.TranslatesAutoresizingMaskIntoConstraints = false;
            profileImageView.Image = UIImage.FromBundle("Profile");

            notificationLabel = new UILabel();
            notificationLabel.Lines = 2;
            notificationLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            ContentView.Add(profileImageView);
            ContentView.Add(notificationLabel);

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-8-[v0(44)]-8-[v1]-8-|", new NSLayoutFormatOptions(), "v0", profileImageView, "v1", notificationLabel));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(44)]", new NSLayoutFormatOptions(), "v0", profileImageView));
            AddConstraint(NSLayoutConstraint.Create(notificationLabel, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1f, 0f));
        }
    }
}
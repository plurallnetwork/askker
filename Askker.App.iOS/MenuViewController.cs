using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Enums;
using Foundation;
using SidebarNavigation;
using System;
using System.Drawing;
using UIKit;

namespace Askker.App.iOS
{
    public partial class MenuViewController : UIViewController
    {
        public static SidebarController sidebarController { get; private set; }
        public static FeedMenuView feedMenu = FeedMenuView.Create();
        private NSObject closeMenuObserver;
        private NSObject updateUnreadNotificationsCountObserver;
        public int unreadNotifications { get; set; }
        public UILabel badge;

        FeedController content;

        public MenuViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            if(closeMenuObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(closeMenuObserver);
            }

            if (updateUnreadNotificationsCountObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(updateUnreadNotificationsCountObserver);
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            closeMenuObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("CloseSideMenu"), CloseMessageRecieved);
            updateUnreadNotificationsCountObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UpdateUnreadNotificationsCount"), UpdateUnreadNotificationsMessageRecieved);

            feedMenu.Hidden = true;
            
            feedMenu.CancelButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                feedMenu.Layer.AddAnimation(new CoreAnimation.CATransition
                {
                    Duration = 0.2,
                    Type = CoreAnimation.CAAnimation.TransitionPush,
                    Subtype = CoreAnimation.CAAnimation.TransitionFromBottom
                }, "hideMenu");

                feedMenu.Hidden = true;
                sidebarController.View.Alpha = 1f;
            };

            var mainWindow = UIApplication.SharedApplication.KeyWindow;
            var viewController = mainWindow?.RootViewController;
            while (viewController?.PresentedViewController != null)
            {
                viewController = viewController.PresentedViewController;
            }
            if (viewController == null)
                viewController = this;

            feedMenu.Frame = viewController.View.Frame;
            viewController.View.AddSubview(feedMenu);

            content = this.Storyboard.InstantiateViewController("FeedController") as FeedController;
            content.menuViewController = this;
            content.filterMine = false;
            content.filterForMe = false;
            content.filterFinished = false;

            sidebarController = new SidebarController(this, content, new SideMenuController(this));
            sidebarController.MenuLocation = MenuLocations.Left;

            UIBarButtonItem notificationsButton = GetNotificationsButton();

            this.NavigationItem.SetLeftBarButtonItem(
                new UIBarButtonItem(UIImage.FromBundle("assets/img/threelines")
                        , UIBarButtonItemStyle.Plain
                        , (sender, args) =>
                        {
                            sidebarController.ToggleMenu();
                        })
            , true);

            this.NavigationItem.SetRightBarButtonItems(
                new UIBarButtonItem[] {
                    notificationsButton,
                    new UIBarButtonItem(UIBarButtonSystemItem.Add, (sender, args) =>
                    {
                        var CreateSurveyController = this.Storyboard.InstantiateViewController("CreateSurveyController") as CreateSurveyController;
                        if (CreateSurveyController != null)
                        {
                            CreateSurveyController.ScreenState = ScreenState.Create.ToString();

                            var rootController = this.Storyboard.InstantiateViewController("CreateSurveyNavController");
                            if (rootController != null)
                            {
                                this.PresentViewController(rootController, true, null);
                            }
                        }
                    }) }
            , true);

            this.NavigationItem.LeftBarButtonItem.TintColor = UIColor.Black;
            this.NavigationItem.RightBarButtonItem.TintColor = UIColor.Black;

            this.NavigationItem.BackBarButtonItem = new UIBarButtonItem("", UIBarButtonItemStyle.Plain, null);

            this.NavigationController.NavigationBar.TintColor = UIColor.Black;
        }

        private UIBarButtonItem GetNotificationsButton()
        {
            var composeButton = new UIButton(new RectangleF(0, 0, 24, 24));
            composeButton.SetBackgroundImage(UIImage.FromBundle("assets/img/notification"), UIControlState.Normal);
            composeButton.AddTarget((sender, args) => {
                var notificationsController = this.Storyboard.InstantiateViewController("NotificationsController") as NotificationsController;
                this.NavigationController.PushViewController(notificationsController, true);
            }, UIControlEvent.TouchUpInside);

            // Notifications Badge
            nfloat badgeSize = 16;
            nfloat badgeOriginX = 12; // Half of the icon width
            nfloat badgeOriginY = -6;

            this.badge = new UILabel();
            this.badge.TextColor = UIColor.White;
            this.badge.BackgroundColor = UIColor.Red;
            this.badge.Font = UIFont.SystemFontOfSize(11);
            this.badge.TextAlignment = UITextAlignment.Center;
            this.badge.Frame = CoreGraphics.CGRect.FromLTRB(badgeOriginX, badgeOriginY, badgeOriginX + badgeSize, badgeOriginY + badgeSize);
            this.badge.Layer.CornerRadius = badgeSize / 2;
            this.badge.Layer.MasksToBounds = true;

            GetUserUnreadNotificationsCount(composeButton, badge);

            return new UIBarButtonItem(composeButton);
        }

        private void CloseMessageRecieved(NSNotification notification)
        {
            sidebarController.CloseMenu();
        }

        public void changeContentView(UIViewController viewController)
        {
            sidebarController.ChangeContentView(viewController);
        }

        public async void GetUserUnreadNotificationsCount(UIButton composeButton, UILabel badge)
        {
            unreadNotifications = await new NotificationManager().GetUserUnreadNotificationsCount(LoginController.userModel.id, LoginController.tokenModel.access_token);

            if (unreadNotifications > 0)
            {
                badge.Text = this.unreadNotifications.ToString();
                composeButton.AddSubview(badge);
            }
        }

        private void UpdateUnreadNotificationsMessageRecieved(NSNotification notification)
        {
            if (badge != null)
            {
                badge.RemoveFromSuperview();

            }
        }
    }
}
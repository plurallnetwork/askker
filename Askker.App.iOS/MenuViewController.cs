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
        }

        public override void ViewDidLoad()
        {

            base.ViewDidLoad();

            closeMenuObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("CloseSideMenu"), CloseMessageRecieved);

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
            content.filterMine = false;
            content.filterForMe = false;
            content.filterFinished = false;

            sidebarController = new SidebarController(this, content, new SideMenuController(this));
            sidebarController.MenuLocation = MenuLocations.Left;
            

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
                    new UIBarButtonItem(UIBarButtonSystemItem.Organize, (sender, args) =>
                    {
                        var notificationsController = this.Storyboard.InstantiateViewController("NotificationsController") as NotificationsController;
                        this.NavigationController.PushViewController(notificationsController, true);
                    }),
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

        private void CloseMessageRecieved(NSNotification notification)
        {
            sidebarController.CloseMenu();
        }

        public void changeContentView(UIViewController viewController)
        {
            sidebarController.ChangeContentView(viewController);
        }
    }
}
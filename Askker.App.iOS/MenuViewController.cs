﻿using Foundation;
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

        public MenuViewController(IntPtr handle) : base(handle)
        {
        }        

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            feedMenu.Hidden = true;
            
            feedMenu.CancelButton.TouchUpInside += (object sender, EventArgs e) =>
            {
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
            viewController.View.AddSubview(feedMenu);

            var content = this.Storyboard.InstantiateViewController("FeedController") as FeedController;
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

            this.NavigationItem.LeftBarButtonItem.TintColor = UIColor.Black;

            this.NavigationItem.BackBarButtonItem = new UIBarButtonItem("", UIBarButtonItemStyle.Plain, null);

            this.NavigationController.NavigationBar.TintColor = UIColor.Black;
        }

        public void changeContentView(UIViewController viewController)
        {
            sidebarController.ChangeContentView(viewController);
        }
    }
}
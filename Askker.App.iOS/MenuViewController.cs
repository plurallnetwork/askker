using Foundation;
using SidebarNavigation;
using System;
using System.Drawing;
using UIKit;

namespace Askker.App.iOS
{
    public partial class MenuViewController : UIViewController
    {
        public SidebarController sidebarController { get; private set; }

        public MenuViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var content = this.Storyboard.InstantiateViewController("FeedController") as FeedController;
            content.filter = "nofilter";

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
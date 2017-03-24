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

            //var image = UIImage.FromBundle("assets/img/threelines");
            //var button = new UIButton();
            //button = UIButton.FromType(UIButtonType.Custom);
            //button.SetBackgroundImage(image, UIControlState.Normal);
            //button.Frame = new RectangleF(0, 0, (float)image.Size.Width, (float)image.Size.Height);
            //NavigationItem.LeftBarButtonItem = new UIBarButtonItem(button);
        }

        public void changeContentView(UIViewController viewController)
        {
            sidebarController.ChangeContentView(viewController);
        }
    }
}
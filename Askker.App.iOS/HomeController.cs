using Foundation;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class HomeController : UIViewController
    {
        public HomeController (IntPtr handle) : base (handle)
        {
        }

        partial void BtnMenu_TouchUpInside(UIButton sender)
        {
            var rootController = this.Storyboard.InstantiateViewController("MenuNavController");
            if (rootController != null)
            {
                this.PresentViewController(rootController, true, null);
            }
        }
    }
}
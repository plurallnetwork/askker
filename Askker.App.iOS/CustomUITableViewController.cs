using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Askker.App.iOS
{
    public class CustomUITableViewController : UITableViewController
    {
        private UIInterfaceOrientationMask orientationMask = UIInterfaceOrientationMask.Portrait;

        public CustomUITableViewController(IntPtr handle) : base(handle)
        {
        }

        public CustomUITableViewController() : base()
        {
        }

        public override bool ShouldAutorotate()
        {
            return false;
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return UIInterfaceOrientationMask.Portrait;
        }

        public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
        {
            return UIInterfaceOrientation.Portrait;
        }

        protected void RestrictRotation(UIInterfaceOrientationMask restriction)
        {
            AppDelegate app = (AppDelegate)UIApplication.SharedApplication.Delegate;
            app.ScreenOrientation = restriction;
            orientationMask = restriction;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            this.RestrictRotation(orientationMask);
        }
    }
}

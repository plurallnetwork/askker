using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace Askker.App.iOS
{
    public class CustomUIViewController : UIViewController
    {
        private UIInterfaceOrientationMask orientationMask = UIInterfaceOrientationMask.Portrait;

        public CustomUIViewController(IntPtr handle) : base(handle)
        {
        }

        public CustomUIViewController() : base()
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
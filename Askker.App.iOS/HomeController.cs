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

        public override void ViewDidLoad()
        {
            profileOtherButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                var profileOtherController = this.Storyboard.InstantiateViewController("ProfileOtherController") as ProfileOtherController;
                if (profileOtherController != null)
                {
                    profileOtherController.UserId = "75e4441c-4414-4fb2-8966-62c53d8ef854";
                    this.PresentViewController(profileOtherController, true, null);
                }
            };
        }

        private void ProfileOtherButton_TouchUpInside(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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
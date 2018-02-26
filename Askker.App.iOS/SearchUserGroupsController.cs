using Askker.App.iOS.TableControllers;
using Askker.App.PortableLibrary.Enums;
using Foundation;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class SearchUserGroupsController : CustomUIViewController
    {
        public ProfileSearchTableViewController tableController { get; set; }
        private NSObject changeBackBtnText;

        public SearchUserGroupsController(IntPtr handle) : base(handle)
        {            
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            if (changeBackBtnText != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(changeBackBtnText);
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            Title = "Find Groups";

            base.ViewWillAppear(animated);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);
            changeBackBtnText = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("ChangeBackBtnText"), ChangeBackBtnText);
            // Perform any additional setup after loading the view, typically from a nib.

            tableController = new ProfileSearchTableViewController(ProfileType.FindGroups, NavigationController);
            Add(tableController.TableView);

            tableController.TableView.ReloadData();
        }

        private void ChangeBackBtnText(NSNotification notification)
        {
            Title = "";
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
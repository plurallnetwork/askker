using Askker.App.iOS.TableControllers;
using Askker.App.PortableLibrary.Enums;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class SearchAllController : CustomUIViewController
    {
        public ProfileSearchTableViewController tableController { get; set; }

        public SearchAllController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);
            // Perform any additional setup after loading the view, typically from a nib.

            tableController = new ProfileSearchTableViewController(ProfileType.FindFriends, NavigationController);
            Add(tableController.TableView);

            tableController.TableView.ReloadData();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
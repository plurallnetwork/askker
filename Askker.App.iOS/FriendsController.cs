using Askker.App.iOS.TableControllers;
using Askker.App.PortableLibrary.Enums;
using BigTed;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class FriendsController : ProfileListTableViewController
    {
        public FriendsController (IntPtr handle) : base (handle)
        {        
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            profileType = ProfileType.ListFriends;

            NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = UIColor.FromRGB(90, 89, 89)
            };

            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            fetchTableItems();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
using Askker.App.iOS.TableControllers;
using Askker.App.PortableLibrary.Enums;
using BigTed;
using Foundation;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class GroupsController : ProfileListTableViewController
    {
        private NSObject changeBackBtnText;

        public GroupsController(IntPtr handle) : base(handle)
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
            Title = "My Groups";

            base.ViewWillAppear(animated);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            profileType = ProfileType.ListGroups;
            changeBackBtnText = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("ChangeBackBtnTextGroups"), ChangeBackBtnText);

            NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = UIColor.FromRGB(90, 89, 89)
            };

            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            fetchTableItems();
        }

        private void ChangeBackBtnText(NSNotification notification)
        {
            Title = "";
        }
    }
}
using Askker.App.iOS.TableControllers;
using Askker.App.PortableLibrary.Enums;
using BigTed;
using CoreGraphics;
using Foundation;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class UserGroupMembersAdminController : ProfileListTableViewController
    {
        //public string groupProfilePicture { get; set; }

        UIBarButtonItem edit, done;
        UIButton groupProfileBtn;

        public UserGroupMembersAdminController(IntPtr handle) : base (handle)
        {
        }

        [Export("GroupProfileBtn:")]
        private void GroupProfileBtn(UIFeedButton button)
        {
            Utils.OpenGroupProfile(this.NavigationController, groupId);
        }

        public override void ViewWillAppear(bool animated)
        {   
            base.ViewWillAppear(animated);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            profileType = ProfileType.ListGroupMembers;

            NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = UIColor.FromRGB(90, 89, 89)
            };

            Title = "";

            groupProfileBtn = new UIButton(UIButtonType.System);
            groupProfileBtn.Frame = new CGRect(0, 0, 100, 40);
            groupProfileBtn.BackgroundColor = UIColor.White;
            groupProfileBtn.SetTitleColor(UIColor.FromRGB(90, 89, 89), UIControlState.Normal);
            //groupProfileBtn.ImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            //groupProfileBtn.ImageView.Layer.CornerRadius = 22;
            //groupProfileBtn.ImageView.Layer.MasksToBounds = true;
            //groupProfileBtn.ImageView.Layer.Frame = new CGRect(0, 0, 44, 44);
            groupProfileBtn.SetImage(UIImage.FromBundle("MyGroup"), UIControlState.Normal);
            //Utils.SetImageFromNSUrlSession(groupProfilePicture, groupProfileBtn.ImageView, this, PictureType.Profile);
            groupProfileBtn.SetTitle("View Group Details", UIControlState.Normal);
            groupProfileBtn.TitleEdgeInsets = new UIEdgeInsets(0, groupProfileBtn.ImageView.Frame.Size.Width/5, 0, -groupProfileBtn.ImageView.Frame.Size.Width);
            groupProfileBtn.ImageEdgeInsets = new UIEdgeInsets(0, -groupProfileBtn.TitleLabel.Frame.Size.Width/5, 0, groupProfileBtn.TitleLabel.Frame.Size.Width);
            groupProfileBtn.AddTarget(Self, new ObjCRuntime.Selector("GroupProfileBtn:"), UIControlEvent.TouchUpInside);
            NavigationItem.TitleView = groupProfileBtn;

            done = new UIBarButtonItem(UIImage.FromBundle("DoneEditProfile"), UIBarButtonItemStyle.Plain, (s, e) => {
                TableView.SetEditing(false, true);
                NavigationItem.RightBarButtonItem = edit;
            });

            edit = new UIBarButtonItem(UIImage.FromBundle("EditProfile"), UIBarButtonItemStyle.Plain, (s, e) => {
                if (TableView.Editing)
                    TableView.SetEditing(false, true); // if we've half-swiped a row

                TableView.SetEditing(true, true);
                NavigationItem.LeftBarButtonItem = null;
                NavigationItem.RightBarButtonItem = done;

            });

            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            NavigationItem.RightBarButtonItem = edit;

            fetchTableItems();
        }
    }
}
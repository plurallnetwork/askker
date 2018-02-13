using Askker.App.iOS.TableControllers;
using Askker.App.PortableLibrary.Enums;
using BigTed;
using CoreGraphics;
using Foundation;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class UserGroupMembersOtherController : ProfileListTableViewController
    {
        //public string groupProfilePicture { get; set; }

        UIButton groupProfileBtn;

        public UserGroupMembersOtherController(IntPtr handle) : base (handle)
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

            Title = "";

            NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = UIColor.FromRGB(90, 89, 89)
            };

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
            groupProfileBtn.TitleEdgeInsets = new UIEdgeInsets(0, groupProfileBtn.ImageView.Frame.Size.Width / 4, 0, -groupProfileBtn.ImageView.Frame.Size.Width);
            groupProfileBtn.ImageEdgeInsets = new UIEdgeInsets(0, -groupProfileBtn.TitleLabel.Frame.Size.Width / 4, 0, groupProfileBtn.TitleLabel.Frame.Size.Width);
            groupProfileBtn.AddTarget(Self, new ObjCRuntime.Selector("GroupProfileBtn:"), UIControlEvent.TouchUpInside);
            NavigationItem.TitleView = groupProfileBtn;

            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            fetchTableItems();
        }
    }
}
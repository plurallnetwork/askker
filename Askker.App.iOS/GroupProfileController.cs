using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
using BigTed;
using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace Askker.App.iOS
{
    public partial class GroupProfileController : CustomUIViewController
    {
        public static NSCache imageCache = new NSCache();

        public string groupId { get; set; }
        public UserGroupModel groupModel { get; set; }

        public UserGroupRelationshipStatus relationshipStatus { get; set; }

        public GroupProfileController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad()
        {
            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            base.ViewDidLoad();

            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);

            imageCache.RemoveAllObjects();

            profileImageView.ClipsToBounds = true;
            profileImageView.Image = null;

            groupModel = await new UserGroupManager().GetGroupById(LoginController.tokenModel.access_token, groupId);

            lblName.Text = groupModel.name;
            lblName.TextColor = UIColor.FromRGB(90, 89, 89);

            textDescription.Text = groupModel.description;
            textDescription.TextColor = UIColor.FromRGB(90, 89, 89);

            if (groupModel.profilePicture != null)
            {
                Utils.SetImageFromNSUrlSession(groupModel.profilePicture, profileImageView, this, PictureType.Group);
            }
            else
            {
                profileImageView.Image = UIImage.FromBundle("Group");
            }

            relationshipStatus = await new UserGroupManager().GetGroupRelationshipStatus(LoginController.tokenModel.access_token, groupId, LoginController.userModel.id);

            LoadRelationshipButton();
            btnRelationship.TouchUpInside += BtnRelationship_TouchUpInside;

            if (groupId.Contains(LoginController.userModel.id))
            {
                btnRelationship.Hidden = true;
            }
            else
            {
                btnRelationship.Hidden = false;
            }

            BTProgressHUD.Dismiss();
        }

        private async void BtnRelationship_TouchUpInside(object sender, EventArgs e)
        {
            try
            {
                if (relationshipStatus == UserGroupRelationshipStatus.NotMember)
                {
                    relationshipStatus = UserGroupRelationshipStatus.PendingYourApproval;

                    await new UserGroupManager().RequestPermissionToGroup(LoginController.tokenModel.access_token, groupId, LoginController.userModel.id);
                }
                else
                {
                    switch (relationshipStatus)
                    {
                        case UserGroupRelationshipStatus.PendingMemberApproval:
                            relationshipStatus = UserGroupRelationshipStatus.Member;
                            break;
                        case UserGroupRelationshipStatus.RejectedByMember:
                            relationshipStatus = UserGroupRelationshipStatus.Member;
                            break;
                        case UserGroupRelationshipStatus.Unmembered:
                            relationshipStatus = UserGroupRelationshipStatus.PendingYourApproval;
                            break;
                        case UserGroupRelationshipStatus.Member:
                            relationshipStatus = UserGroupRelationshipStatus.Unmembered;
                            break;
                        default:
                            relationshipStatus = UserGroupRelationshipStatus.PendingYourApproval;
                            break;
                    }

                    await new UserGroupManager().UpdateGroupRelationshipStatus(LoginController.tokenModel.access_token, groupId, LoginController.userModel.id,  relationshipStatus);
                }

                LoadRelationshipButton();
            }
            catch (Exception ex)
            {
                Utils.HandleException(ex);
            }
        }

        private void LoadRelationshipButton()
        {
            switch (relationshipStatus)
            {
                case UserGroupRelationshipStatus.NotMember:
                    btnRelationship.SetTitle(" Request Membership ", UIControlState.Normal);
                    btnRelationship.BackgroundColor = UIColor.FromRGB(0, 134, 255);
                    btnRelationship.Enabled = true;
                    break;
                case UserGroupRelationshipStatus.Member:
                    btnRelationship.SetTitle(" Quit Membership", UIControlState.Normal);
                    btnRelationship.BackgroundColor = UIColor.FromRGB(0, 134, 255);
                    btnRelationship.Enabled = true;
                    break;
                case UserGroupRelationshipStatus.PendingYourApproval:
                    btnRelationship.SetTitle(" Pending Approval ", UIControlState.Disabled);
                    btnRelationship.BackgroundColor = UIColor.Orange;
                    btnRelationship.Enabled = false;
                    break;
                case UserGroupRelationshipStatus.PendingMemberApproval:
                    btnRelationship.SetTitle(" Accept ", UIControlState.Normal);
                    btnRelationship.BackgroundColor = UIColor.FromRGB(70, 230, 130);
                    btnRelationship.Enabled = true;
                    break;
                case UserGroupRelationshipStatus.RejectedByMember:
                    btnRelationship.SetTitle(" Request Membership ", UIControlState.Normal);
                    btnRelationship.BackgroundColor = UIColor.FromRGB(0, 134, 255);
                    btnRelationship.Enabled = true;
                    break;
                case UserGroupRelationshipStatus.RejectedByYou:
                    btnRelationship.SetTitle(" Pending Approval ", UIControlState.Disabled);
                    btnRelationship.BackgroundColor = UIColor.Orange;
                    btnRelationship.Enabled = false;
                    break;
                case UserGroupRelationshipStatus.Unmembered:
                    btnRelationship.SetTitle(" Request Membership ", UIControlState.Normal);
                    btnRelationship.BackgroundColor = UIColor.FromRGB(0, 134, 255);
                    btnRelationship.Enabled = true;
                    break;
                default:
                    btnRelationship.Enabled = true;
                    break;
            }
        }
    }
}
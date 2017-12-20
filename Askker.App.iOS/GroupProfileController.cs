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

            lblName.Text = groupModel.searchName;
            textDescription.Text = groupModel.description;

            if (groupModel.profilePicture != null)
            {
                Utils.SetImageFromNSUrlSession(groupModel.profilePicture, profileImageView, this, PictureType.Profile);
            }
            else
            {
                profileImageView.Image = UIImage.FromBundle("Group");
            }

            LoadRelationshipButton();
            btnRelationship.TouchUpInside += BtnRelationship_TouchUpInside;
            BTProgressHUD.Dismiss();
        }

        private async void BtnRelationship_TouchUpInside(object sender, EventArgs e)
        {
            try
            {
                if (relationshipStatus == UserGroupRelationshipStatus.NotInGroup)
                {
                    relationshipStatus = UserGroupRelationshipStatus.PendingGroupApproval;

                    await new UserGroupManager().AddGroup(LoginController.tokenModel.access_token, groupId);
                }
                else
                {
                    switch (relationshipStatus)
                    {
                        case UserGroupRelationshipStatus.PendingYourApproval:
                            relationshipStatus = UserGroupRelationshipStatus.InGroup;
                            break;
                        case UserGroupRelationshipStatus.RejectedByYou:
                            relationshipStatus = UserGroupRelationshipStatus.InGroup;
                            break;
                        case UserGroupRelationshipStatus.UnGrouped:
                            relationshipStatus = UserGroupRelationshipStatus.PendingGroupApproval;
                            break;
                        case UserGroupRelationshipStatus.InGroup:
                            relationshipStatus = UserGroupRelationshipStatus.UnGrouped;
                            break;
                        default:
                            relationshipStatus = UserGroupRelationshipStatus.PendingGroupApproval;
                            break;
                    }

                    await new UserGroupManager().UpdateGroupRelationshipStatus(LoginController.tokenModel.access_token, groupId, relationshipStatus);
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
                case UserGroupRelationshipStatus.NotInGroup:
                    btnRelationship.SetTitle(" Add Friend ", UIControlState.Normal);
                    btnRelationship.BackgroundColor = UIColor.FromRGB(0, 134, 255);
                    btnRelationship.Enabled = true;
                    break;
                case UserGroupRelationshipStatus.InGroup:
                    btnRelationship.SetTitle(" Unfriend ", UIControlState.Normal);
                    btnRelationship.BackgroundColor = UIColor.FromRGB(0, 134, 255);
                    btnRelationship.Enabled = true;
                    break;
                case UserGroupRelationshipStatus.PendingGroupApproval:
                    btnRelationship.SetTitle(" Pending Approval ", UIControlState.Disabled);
                    btnRelationship.BackgroundColor = UIColor.Orange;
                    btnRelationship.Enabled = false;
                    break;
                case UserGroupRelationshipStatus.PendingYourApproval:
                    btnRelationship.SetTitle(" Accept ", UIControlState.Normal);
                    btnRelationship.BackgroundColor = UIColor.FromRGB(70, 230, 130);
                    btnRelationship.Enabled = true;
                    break;
                case UserGroupRelationshipStatus.RejectedByYou:
                    btnRelationship.SetTitle(" Add Friend ", UIControlState.Normal);
                    btnRelationship.BackgroundColor = UIColor.FromRGB(0, 134, 255);
                    btnRelationship.Enabled = true;
                    break;
                case UserGroupRelationshipStatus.RejectedByGroup:
                    btnRelationship.SetTitle(" Pending Approval ", UIControlState.Disabled);
                    btnRelationship.BackgroundColor = UIColor.Orange;
                    btnRelationship.Enabled = false;
                    break;
                case UserGroupRelationshipStatus.UnGrouped:
                    btnRelationship.SetTitle(" Add Friend ", UIControlState.Normal);
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
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Enums;
using Foundation;
using System;
using UIKit;
using System.Collections.Generic;
using System.Globalization;
using BigTed;

namespace Askker.App.iOS
{
    public partial class ProfileOtherController : CustomUIViewController
    {
        string fileName;
        public static NSCache imageCache = new NSCache();

        string groupId { get; set; }
        public string friendUserId { get; set; }
        public UserModel friendUserModel { get; set; }

        public RelationshipStatus relationshipStatus { get; set; }
        public UserGroupRelationshipStatus groupRelationshipStatus { get; set; }

        public ProfileOtherController (IntPtr handle) : base (handle)
        {
        }
        
        public override async void ViewDidLoad()
        {
            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            base.ViewDidLoad();

            this.EdgesForExtendedLayout = UIRectEdge.None;
            this.ExtendedLayoutIncludesOpaqueBars = false;
            this.AutomaticallyAdjustsScrollViewInsets = false;
            this.NavigationController.NavigationBar.Translucent = false;

            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);

            imageCache.RemoveAllObjects();

            try
            {
                btnRelationship.SetTitle("", UIControlState.Normal);
                btnGroupRelationship.SetTitle("", UIControlState.Normal);
                btnRelationship.Hidden = true;
                btnGroupRelationship.Hidden = true;
                profileImageView.Layer.MasksToBounds = true;

                friendUserModel = await new LoginManager().GetUserById(LoginController.tokenModel.access_token, friendUserId);

                nameText.Text = friendUserModel.name;
                emailText.Text = friendUserModel.userName;
                ageText.Text = friendUserModel.age.ToString();

                nameText.TextColor = UIColor.FromRGB(90, 89, 89);
                emailText.TextColor = UIColor.FromRGB(90, 89, 89);
                ageText.TextColor = UIColor.FromRGB(90, 89, 89);
                genderText.TextColor = UIColor.FromRGB(90, 89, 89);

                if ("male".Equals(friendUserModel.gender) || "female".Equals(friendUserModel.gender))
                {
                    genderText.Text = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(friendUserModel.gender);
                }

                if (friendUserModel.profilePicturePath != null)
                {
                    fileName = friendUserModel.profilePicturePath;
                    Utils.SetImageFromNSUrlSession(fileName, profileImageView, this, PictureType.Profile);
                }

                //get groups from logged user
                List<UserGroupModel> userGroups = await new UserGroupManager().GetGroups(LoginController.userModel.id, LoginController.tokenModel.access_token);

                //check if logged user is admin of one these groups
                foreach(UserGroupModel userGroup in userGroups)
                {
                    if (LoginController.userModel.id.Equals(userGroup.userId))
                    {                        
                        List<UserGroupMemberModel> members = await new UserGroupManager().GetGroupMembers(LoginController.tokenModel.access_token, userGroup.userId + userGroup.creationDate);                        

                        //check is profile user is part of ths group
                        foreach(UserGroupMemberModel member in members)
                        {
                            if (member.id.Equals(friendUserId))
                            {
                                groupId = userGroup.userId + userGroup.creationDate;
                            }
                        }
                    }
                }

                relationshipStatus = await new FriendManager().GetUserRelationshipStatus(LoginController.tokenModel.access_token, friendUserId);
                LoadRelationshipButton();

                if (groupId != null)
                {
                    groupRelationshipStatus = await new UserGroupManager().GetGroupRelationshipStatus(LoginController.tokenModel.access_token, groupId, friendUserId);
                    LoadGroupRelationshipButton();
                }
                else
                {
                    btnGroupRelationship.Hidden = true;
                }
            }
            catch (Exception ex)
            {
                BTProgressHUD.Dismiss();
                Utils.HandleException(ex);
            }

            btnRelationship.TouchUpInside += BtnRelationship_TouchUpInside;
            btnGroupRelationship.TouchUpInside += BtnGroupRelationship_TouchUpInside;
            BTProgressHUD.Dismiss();
        }

        private async void BtnGroupRelationship_TouchUpInside(object sender, EventArgs e)
        {
            try
            {
                btnGroupRelationship.LoadingIndicatorButton(true);

                if (groupRelationshipStatus == UserGroupRelationshipStatus.PendingYourApproval)
                {
                    groupRelationshipStatus = UserGroupRelationshipStatus.Member;

                    await new UserGroupManager().UpdateGroupRelationshipStatus(LoginController.tokenModel.access_token, groupId, friendUserId, groupRelationshipStatus);
                }

                btnGroupRelationship.LoadingIndicatorButton(false);
                LoadGroupRelationshipButton();
            }
            catch (Exception ex)
            {
                btnGroupRelationship.LoadingIndicatorButton(false);
                Utils.HandleException(ex);
            }
        }

        private async void BtnRelationship_TouchUpInside(object sender, EventArgs e)
        {
            try
            {
                btnRelationship.LoadingIndicatorButton(true);

                if (relationshipStatus == RelationshipStatus.NotFriends)
                {
                    relationshipStatus = RelationshipStatus.PendingFriendApproval;

                    var userOwnerAndFriend = new List<UserModel>();
                    userOwnerAndFriend.Add(LoginController.userModel);
                    userOwnerAndFriend.Add(friendUserModel);
                    await new FriendManager().AddFriend(LoginController.tokenModel.access_token, friendUserId);
                }
                else
                {
                    switch (relationshipStatus)
                    {
                        case RelationshipStatus.PendingYourApproval:
                            relationshipStatus = RelationshipStatus.Friend;
                            break;
                        case RelationshipStatus.RejectedByYou:
                            relationshipStatus = RelationshipStatus.Friend;
                            break;
                        case RelationshipStatus.Unfriended:
                            relationshipStatus = RelationshipStatus.PendingFriendApproval;
                            break;
                        case RelationshipStatus.Friend:
                            relationshipStatus = RelationshipStatus.Unfriended;
                            break;
                        default:
                            relationshipStatus = RelationshipStatus.PendingFriendApproval;
                            break;
                    }

                    await new FriendManager().UpdateUserRelationshipStatus(LoginController.tokenModel.access_token, friendUserId, relationshipStatus);
                }

                btnRelationship.LoadingIndicatorButton(false);
                LoadRelationshipButton();
            }
            catch (Exception ex)
            {
                btnRelationship.LoadingIndicatorButton(false);
                Utils.HandleException(ex);
            }
        }

        private void LoadRelationshipButton()
        {
            switch (relationshipStatus)
            {
                case RelationshipStatus.NotFriends:
                    btnRelationship.SetTitle(" Add Friend ", UIControlState.Normal);
                    btnRelationship.BackgroundColor = UIColor.FromRGB(0, 134, 255);
                    btnRelationship.Enabled = true;
                    btnRelationship.Hidden = false;
                    break;
                case RelationshipStatus.Friend:
                    btnRelationship.SetTitle(" Unfriend ", UIControlState.Normal);
                    btnRelationship.BackgroundColor = UIColor.FromRGB(0, 134, 255);
                    btnRelationship.Enabled = true;
                    btnRelationship.Hidden = false;
                    break;
                case RelationshipStatus.PendingFriendApproval:
                    btnRelationship.SetTitle(" Pending Approval ", UIControlState.Disabled);
                    btnRelationship.BackgroundColor = UIColor.Orange;
                    btnRelationship.Enabled = false;
                    btnRelationship.Hidden = false;
                    break;
                case RelationshipStatus.PendingYourApproval:
                    btnRelationship.SetTitle(" Accept ", UIControlState.Normal);
                    btnRelationship.BackgroundColor = UIColor.FromRGB(70, 230, 130);
                    btnRelationship.Enabled = true;
                    btnRelationship.Hidden = false;
                    break;
                case RelationshipStatus.RejectedByYou:
                    btnRelationship.SetTitle(" Add Friend ", UIControlState.Normal);
                    btnRelationship.BackgroundColor = UIColor.FromRGB(0, 134, 255);
                    btnRelationship.Enabled = true;
                    btnRelationship.Hidden = false;
                    break;
                case RelationshipStatus.RejectedByFriend:
                    btnRelationship.SetTitle(" Pending Approval ", UIControlState.Disabled);
                    btnRelationship.BackgroundColor = UIColor.Orange;
                    btnRelationship.Enabled = false;
                    btnRelationship.Hidden = false;
                    break;
                case RelationshipStatus.Unfriended:
                    btnRelationship.SetTitle(" Add Friend ", UIControlState.Normal);
                    btnRelationship.BackgroundColor = UIColor.FromRGB(0, 134, 255);
                    btnRelationship.Enabled = true;
                    btnRelationship.Hidden = false;
                    break;
                default:
                    btnRelationship.Enabled = true;
                    btnRelationship.Hidden = false;
                    break;
            }
        }

        private void LoadGroupRelationshipButton()
        {
            switch (groupRelationshipStatus)
            {
                case UserGroupRelationshipStatus.Member:
                    btnGroupRelationship.SetTitle(" Accepted ", UIControlState.Normal);
                    btnGroupRelationship.BackgroundColor = UIColor.FromRGB(0, 134, 255);
                    btnGroupRelationship.Enabled = false;
                    btnGroupRelationship.Hidden = false;
                    break;
                case UserGroupRelationshipStatus.PendingYourApproval:
                    btnGroupRelationship.SetTitle(" Accept ", UIControlState.Normal);
                    btnGroupRelationship.BackgroundColor = UIColor.Orange;
                    btnGroupRelationship.Enabled = true;
                    btnGroupRelationship.Hidden = false;
                    break;
                default:
                    btnGroupRelationship.Hidden = true;
                    break;
            }
        }
    }
}
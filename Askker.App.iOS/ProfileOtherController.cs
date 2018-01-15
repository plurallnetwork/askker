using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Enums;
using CoreFoundation;
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

        public string friendUserId { get; set; }
        public UserModel friendUserModel { get; set; }

        public RelationshipStatus relationshipStatus { get; set; }

        public ProfileOtherController (IntPtr handle) : base (handle)
        {
        }
        
        public override async void ViewDidLoad()
        {
            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            base.ViewDidLoad();

            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);

            imageCache.RemoveAllObjects();

            try
            {
                btnRelationship.SetTitle("", UIControlState.Normal);
                profileImageView.ClipsToBounds = true;

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

                relationshipStatus = await new FriendManager().GetUserRelationshipStatus(LoginController.tokenModel.access_token, friendUserId);

                LoadRelationshipButton();
            }
            catch (Exception ex)
            {
                BTProgressHUD.Dismiss();
                Utils.HandleException(ex);
            }

            btnRelationship.TouchUpInside += BtnRelationship_TouchUpInside;
            BTProgressHUD.Dismiss();
        }

        private async void BtnRelationship_TouchUpInside(object sender, EventArgs e)
        {
            try
            {
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
                case RelationshipStatus.NotFriends:
                    btnRelationship.SetTitle(" Add Friend ", UIControlState.Normal);
                    btnRelationship.BackgroundColor = UIColor.FromRGB(0, 134, 255);
                    btnRelationship.Enabled = true;
                    break;
                case RelationshipStatus.Friend:
                    btnRelationship.SetTitle(" Unfriend ", UIControlState.Normal);
                    btnRelationship.BackgroundColor = UIColor.FromRGB(0, 134, 255);
                    btnRelationship.Enabled = true;
                    break;
                case RelationshipStatus.PendingFriendApproval:
                    btnRelationship.SetTitle(" Pending Approval ", UIControlState.Disabled);
                    btnRelationship.BackgroundColor = UIColor.Orange;
                    btnRelationship.Enabled = false;
                    break;
                case RelationshipStatus.PendingYourApproval:
                    btnRelationship.SetTitle(" Accept ", UIControlState.Normal);
                    btnRelationship.BackgroundColor = UIColor.FromRGB(70, 230, 130);
                    btnRelationship.Enabled = true;
                    break;
                case RelationshipStatus.RejectedByYou:
                    btnRelationship.SetTitle(" Add Friend ", UIControlState.Normal);
                    btnRelationship.BackgroundColor = UIColor.FromRGB(0, 134, 255);
                    btnRelationship.Enabled = true;
                    break;
                case RelationshipStatus.RejectedByFriend:
                    btnRelationship.SetTitle(" Pending Approval ", UIControlState.Disabled);
                    btnRelationship.BackgroundColor = UIColor.Orange;
                    btnRelationship.Enabled = false;
                    break;
                case RelationshipStatus.Unfriended:
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
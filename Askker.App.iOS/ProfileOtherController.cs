using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Enums;
using CoreFoundation;
using Foundation;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class ProfileOtherController : UIViewController
    {
        string fileName;

        public string UserId { get; set; }

        public RelationshipStatus relationshipStatus { get; set; }

        public ProfileOtherController (IntPtr handle) : base (handle)
        {
        }
        
        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            try
            {
                profileImageView.ClipsToBounds = true;

                UserModel userModel = await new LoginManager().GetUserById(LoginController.tokenModel.access_token, UserId);

                nameText.Text = userModel.name;
                emailText.Text = userModel.userName;
                ageText.Text = userModel.age.ToString();
                if ("male".Equals(userModel.gender) || "female".Equals(userModel.gender))
                {
                    genderText.Text = userModel.gender;
                }

                if (userModel.profilePicturePath != null)
                {
                    fileName = userModel.profilePicturePath;
                    var url = new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + userModel.profilePicturePath);

                    var imageFromCache = (UIImage)FeedController.imageCache.ObjectForKey(NSString.FromObject(url.AbsoluteString));
                    if (imageFromCache != null)
                    {
                        profileImageView.Image = imageFromCache;
                    }
                    else
                    {
                        var task = NSUrlSession.SharedSession.CreateDataTask(url, (data, response, error) =>
                        {
                            if (response == null)
                            {
                                profileImageView.Image = UIImage.FromBundle("Profile");
                            }
                            else
                            {
                                try
                                {
                                    DispatchQueue.MainQueue.DispatchAsync(() =>
                                    {
                                        var imageToCache = UIImage.LoadFromData(data);

                                        profileImageView.Image = imageToCache;

                                        if (imageToCache != null)
                                        {
                                            FeedController.imageCache.SetObjectforKey(imageToCache, NSString.FromObject(url.AbsoluteString));
                                        }
                                    });
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(ex.Message);
                                }
                            }
                        });
                        task.Resume();
                    }
                }

                relationshipStatus = await new FriendManager().GetUserRelationshipStatus(LoginController.tokenModel.access_token, UserId);

                LoadRelationshipButton();
            }
            catch (Exception ex)
            {
                Utils.HandleException(ex);
            }

            btnRelationship.TouchUpInside += BtnRelationship_TouchUpInside;
        }

        private async void BtnRelationship_TouchUpInside(object sender, EventArgs e)
        {
            try
            {
                if (relationshipStatus == RelationshipStatus.NotFriends)
                {
                    relationshipStatus = RelationshipStatus.PendingFriendApproval;

                    await new FriendManager().AddFriend(LoginController.tokenModel.access_token, UserId);
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
                            relationshipStatus = RelationshipStatus.Friend;
                            break;
                        case RelationshipStatus.Friend:
                            relationshipStatus = RelationshipStatus.Unfriended;
                            break;
                        default:
                            relationshipStatus = RelationshipStatus.PendingFriendApproval;
                            break;
                    }

                    await new FriendManager().UpdateUserRelationshipStatus(LoginController.tokenModel.access_token, UserId, relationshipStatus);
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
                    btnRelationship.SetTitle("Add Friend", UIControlState.Normal);
                    btnRelationship.Enabled = true;
                    break;
                case RelationshipStatus.Friend:
                    btnRelationship.SetTitle("Unfriend", UIControlState.Normal);
                    btnRelationship.Enabled = true;
                    break;
                case RelationshipStatus.PendingFriendApproval:
                    btnRelationship.SetTitle("Pending Approval", UIControlState.Disabled);
                    btnRelationship.Enabled = false;
                    break;
                case RelationshipStatus.PendingYourApproval:
                    btnRelationship.SetTitle("Accept", UIControlState.Normal);
                    btnRelationship.Enabled = true;
                    break;
                case RelationshipStatus.RejectedByYou:
                    btnRelationship.SetTitle("Add Friend", UIControlState.Normal);
                    btnRelationship.Enabled = true;
                    break;
                case RelationshipStatus.RejectedByFriend:
                    btnRelationship.SetTitle("Pending Approval", UIControlState.Disabled);
                    btnRelationship.Enabled = false;
                    break;
                case RelationshipStatus.Unfriended:
                    btnRelationship.SetTitle("Add Friend", UIControlState.Normal);
                    btnRelationship.Enabled = true;
                    break;
                default:
                    btnRelationship.Enabled = true;
                    break;
            }
        }
    }
}
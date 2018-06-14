using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Enums;
using Foundation;
using System;
using UIKit;
using System.Collections.Generic;
using System.Globalization;
using BigTed;
using System.Linq;

namespace Askker.App.iOS
{
    public partial class ProfileOtherController : CustomUIViewController
    {
        string fileName;
        public static NSCache imageCache = new NSCache();

        public string friendUserId { get; set; }
        public UserModel friendUserModel { get; set; }

        public RelationshipStatus relationshipStatus { get; set; }
        public bool isUserBlocked { get; set; }

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
                btnRelationship.Hidden = true;
                btnBlock.SetTitle("", UIControlState.Normal);
                btnBlock.Hidden = true;

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

                relationshipStatus = await new FriendManager().GetUserRelationshipStatus(LoginController.tokenModel.access_token, friendUserId);
                LoadRelationshipButton();

                isUserBlocked = await new UserManager().IsUserBlocked(friendUserId, LoginController.tokenModel.access_token);
                LoadBlockButton();

                var adminGroups = (await new UserGroupManager().GetGroupsWithMembers(LoginController.userModel.id, LoginController.tokenModel.access_token))
                                                               .Where(g => g.userId == LoginController.userModel.id && g.members.Any(m => m.id == friendUserId)).ToList();
                if (adminGroups != null && adminGroups.Count > 0)
                {
                    groupsTableView.RegisterClassForCellReuse(typeof(GroupsTableViewCell), "cellId");
                    groupsTableView.Source = new GroupsTableViewSource(adminGroups, friendUserId);
                    View.AddSubview(groupsTableView);
                    groupsTableView.ReloadData();
                    groupsTableView.TableFooterView = new UIView();
                    groupsTableView.Hidden = false;
                    groupsTitleLabel.Hidden = false;
                }
            }
            catch (Exception ex)
            {
                BTProgressHUD.Dismiss();
                Utils.HandleException(ex);
            }

            btnRelationship.TouchUpInside += BtnRelationship_TouchUpInside;
            btnBlock.TouchUpInside += BtnBlock_TouchUpInside;
            BTProgressHUD.Dismiss();
        }

        private async void BtnBlock_TouchUpInside(object sender, EventArgs e)
        {
            try
            {
                btnBlock.LoadingIndicatorButton(true);

                if (!isUserBlocked)
                {
                    isUserBlocked = true;

                    var userOwnerAndFriend = new List<UserModel>();
                    userOwnerAndFriend.Add(LoginController.userModel);
                    userOwnerAndFriend.Add(friendUserModel);
                    await new UserManager().BlockUser(LoginController.userModel.id, friendUserId, LoginController.tokenModel.access_token);
                }
                else
                {
                    switch (isUserBlocked)
                    {
                        case true:
                            isUserBlocked = false;
                            break;
                        default:
                            isUserBlocked = false;
                            break;
                    }

                    await new UserManager().UnblockUser(LoginController.userModel.id, friendUserId, LoginController.tokenModel.access_token);
                }

                btnBlock.LoadingIndicatorButton(false);
                LoadBlockButton();
            }
            catch (Exception ex)
            {
                btnBlock.LoadingIndicatorButton(false);
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

        private void LoadBlockButton()
        {
            switch (isUserBlocked)
            {
                case false:
                    btnBlock.SetTitle(" Block ", UIControlState.Normal);
                    btnBlock.BackgroundColor = UIColor.Red;
                    btnBlock.Enabled = true;
                    btnBlock.Hidden = false;
                    break;
                case true:
                    btnBlock.SetTitle(" Unblock ", UIControlState.Normal);
                    btnBlock.BackgroundColor = UIColor.FromRGB(0, 134, 255);
                    btnBlock.Enabled = true;
                    btnBlock.Hidden = false;
                    break;
                default:
                    btnBlock.Enabled = true;
                    btnBlock.Hidden = false;
                    break;
            }
        }
    }

    public class GroupsTableViewSource : UITableViewSource
    {
        public List<UserGroupModel> groups { get; set; }
        public string friendUserId { get; set; }

        public GroupsTableViewSource(List<UserGroupModel> groups, string friendUserId)
        {
            this.groups = groups;
            this.friendUserId = friendUserId;
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return groups.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var groupCell = tableView.DequeueReusableCell("cellId", indexPath) as GroupsTableViewCell;

            ConfigureCell(groupCell, groups[indexPath.Row]);

            return groupCell;
        }

        protected void ConfigureCell(GroupsTableViewCell cell, UserGroupModel group)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            cell.groupId = group.userId + group.creationDate;
            cell.friendUserId = friendUserId;

            var groupRelationshipStatus = new UserGroupRelationshipStatus();
            Enum.TryParse(group.members.Where(m => m.id == friendUserId).Select(f => f.relationshipStatus).FirstOrDefault(), out groupRelationshipStatus);
            cell.groupRelationshipStatus = groupRelationshipStatus;
            cell.LoadGroupRelationshipButton();

            cell.profileImageView.Image = UIImage.FromBundle("Group");

            if (!string.IsNullOrEmpty(group.profilePicture))
            {
                Utils.SetImageFromNSUrlSession(group.profilePicture, cell.profileImageView, this, PictureType.Group);
            }

            cell.nameLabel.Text = group.name;
        }
    }

    public partial class GroupsTableViewCell : UITableViewCell
    {
        public UIImageView profileImageView { get; set; }
        public UILabel nameLabel { get; set; }
        public UIButton groupRelationshipButton { get; set; }

        public string groupId { get; set; }
        public string friendUserId { get; set; }
        public UserGroupRelationshipStatus groupRelationshipStatus { get; set; }

        protected GroupsTableViewCell(IntPtr handle) : base(handle)
        {
            profileImageView = new UIImageView();
            profileImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            profileImageView.Layer.CornerRadius = 18;
            profileImageView.Layer.MasksToBounds = true;
            profileImageView.TranslatesAutoresizingMaskIntoConstraints = false;

            nameLabel = new UILabel();
            nameLabel.Font = UIFont.SystemFontOfSize(14);
            nameLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            nameLabel.TextColor = UIColor.FromRGB(90, 89, 89);

            groupRelationshipButton = new UIButton();
            groupRelationshipButton.Layer.CornerRadius = 5;
            groupRelationshipButton.Font = UIFont.SystemFontOfSize(15, UIFontWeight.Regular);
            groupRelationshipButton.TouchUpInside += GroupRelationshipButton_TouchUpInside;
            groupRelationshipButton.TranslatesAutoresizingMaskIntoConstraints = false;

            ContentView.Add(profileImageView);
            ContentView.Add(nameLabel);
            ContentView.Add(groupRelationshipButton);

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-12-[v0(36)]-8-[v1]-8-[v2(74)]-4-|", new NSLayoutFormatOptions(), "v0", profileImageView, "v1", nameLabel, "v2", groupRelationshipButton));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-4-[v0(36)]", new NSLayoutFormatOptions(), "v0", profileImageView));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-12-[v0(20)]", new NSLayoutFormatOptions(), "v0", nameLabel));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0]", new NSLayoutFormatOptions(), "v0", groupRelationshipButton));
        }

        public void LoadGroupRelationshipButton()
        {
            switch (groupRelationshipStatus)
            {
                case UserGroupRelationshipStatus.Member:
                    groupRelationshipButton.SetTitle(" Accepted ", UIControlState.Normal);
                    groupRelationshipButton.BackgroundColor = UIColor.FromRGB(0, 134, 255);
                    break;
                case UserGroupRelationshipStatus.PendingYourApproval:
                    groupRelationshipButton.SetTitle(" Accept ", UIControlState.Normal);
                    groupRelationshipButton.BackgroundColor = UIColor.Orange;
                    break;
                default:
                    groupRelationshipButton.Hidden = true;
                    break;
            }
        }

        private async void GroupRelationshipButton_TouchUpInside(object sender, EventArgs e)
        {
            try
            {
                groupRelationshipButton.LoadingIndicatorButton(true);

                if (groupRelationshipStatus == UserGroupRelationshipStatus.PendingYourApproval)
                {
                    groupRelationshipStatus = UserGroupRelationshipStatus.Member;

                    await new UserGroupManager().UpdateGroupRelationshipStatus(LoginController.tokenModel.access_token, groupId, friendUserId, groupRelationshipStatus);
                }

                groupRelationshipButton.LoadingIndicatorButton(false);
                LoadGroupRelationshipButton();
            }
            catch (Exception ex)
            {
                groupRelationshipButton.LoadingIndicatorButton(false);
                Utils.HandleException(ex);
            }
        }
    }
}
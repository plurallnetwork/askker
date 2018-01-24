using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
using BigTed;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace Askker.App.iOS
{
    public partial class UserGroupMembersAdminController : GroupMemberBaseTableViewController
    {
        public List<UserGroupMemberModel> members { get; set; }
        public string groupId { get; set; }
        public string groupProfilePicture { get; set; }

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

            UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes
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

            members = new List<UserGroupMemberModel>();
            fetchMembers();

            TableView.Source = new MemberResultsTableController(members, this, groupId);
            TableView.ReloadData();

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
        }

        public async void fetchMembers()
        {
            try
            {
                members = await new UserGroupManager().GetGroupMembers(LoginController.tokenModel.access_token, groupId);
                TableView.Source = new MemberResultsTableController(members, this, groupId);
                TableView.ReloadData();
            }
            catch (Exception ex)
            {
                Utils.HandleException(ex);
            }

            BTProgressHUD.Dismiss();

            TableView.ReloadData();
        }

        [Export("searchBarSearchButtonClicked:")]
        public virtual void SearchButtonClicked(UISearchBar searchBar)
        {
            searchBar.ResignFirstResponder();
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return members.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(cellIdentifier) as MembersTableViewCell;

            ConfigureCell(cell, members[indexPath.Row]);

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            Utils.OpenUserProfile(this.NavigationController, members[indexPath.Row].id);
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 60;
        }
                
        List<UserGroupMemberModel> PerformSearch(string searchString)
        {
            searchString = searchString.Trim();
            string[] searchItems = string.IsNullOrEmpty(searchString)
                ? new string[0]
                : searchString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var filteredMembers = new List<UserGroupMemberModel>();

            foreach (var item in searchItems)
            {
                IEnumerable<UserGroupMemberModel> query =
                    from aGroup in members
                    where aGroup.name.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0
                    orderby aGroup.name
                    select aGroup;

                filteredMembers.AddRange(query);
            }

            return filteredMembers.Distinct().ToList();
        }
    }

    public class GroupMemberBaseTableViewController : CustomUITableViewController
    {
        protected const string cellIdentifier = "cellId";

        public GroupMemberBaseTableViewController()
        {
        }

        public GroupMemberBaseTableViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);
            TableView.RegisterClassForCellReuse(typeof(MembersTableViewCell), cellIdentifier);
            TableView.SeparatorInset = new UIEdgeInsets(0, 10, 0, 10);
        }

        protected void ConfigureCell(MembersTableViewCell cell, UserGroupMemberModel member)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            if (member.profilePicture != null)
            {
                Utils.SetImageFromNSUrlSession(member.profilePicture, cell.profileImageView, this, PictureType.Profile);
            }

            cell.nameLabel.Text = member.name;
        }
    }

    public partial class MembersTableViewCell : UITableViewCell
    {
        public UIImageView profileImageView { get; set; }
        public UILabel nameLabel { get; set; }

        protected MembersTableViewCell(IntPtr handle) : base(handle)
        {
            profileImageView = new UIImageView();
            profileImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            profileImageView.Layer.CornerRadius = 22;
            profileImageView.Layer.MasksToBounds = true;
            profileImageView.TranslatesAutoresizingMaskIntoConstraints = false;
            profileImageView.Image = UIImage.FromBundle("Group");

            nameLabel = new UILabel();
            nameLabel.Font = UIFont.SystemFontOfSize(14);
            nameLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            nameLabel.TextColor = UIColor.FromRGB(90, 89, 89);

            ContentView.Add(profileImageView);
            ContentView.Add(nameLabel);

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-8-[v0(44)]-8-[v1]-8-|", new NSLayoutFormatOptions(), "v0", profileImageView, "v1", nameLabel));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(44)]", new NSLayoutFormatOptions(), "v0", profileImageView));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-18-[v0(22)]", new NSLayoutFormatOptions(), "v0", nameLabel));
        }
    }

    public class MemberResultsTableController : UITableViewSource
    {
        public List<UserGroupMemberModel> filteredMembers { get; set; }
        protected const string cellIdentifier = "cellId";
        UIViewController viewController;
        public string groupId { get; set; }

        public MemberResultsTableController(List<UserGroupMemberModel> items, UIViewController viewController, string groupId)
        {
            filteredMembers = items;
            this.viewController = viewController;
            this.groupId = groupId;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return filteredMembers.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(cellIdentifier) as MembersTableViewCell;

            var userGroup = filteredMembers[indexPath.Row];

            //ConfigureCell(cell, userGroup);

            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            if (userGroup.profilePicture != null)
            {
                Utils.SetImageFromNSUrlSession(userGroup.profilePicture, cell.profileImageView, this, PictureType.Profile);
            }

            cell.nameLabel.Text = userGroup.name;

            return cell;
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 60;
        }

        /// <summary>
        /// Called by the table view to determine whether or not the row is editable
        /// </summary>
        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true; // return false if you wish to disable editing for a specific indexPath or for all rows
        }
        
        /// <summary>
        /// Custom text for delete button
        /// </summary>
        public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
        {
            return "Remove member";
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            Utils.OpenUserProfile(this.viewController.NavigationController, filteredMembers[indexPath.Row].id);
        }

        public override async void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            switch (editingStyle)
            {
                case UITableViewCellEditingStyle.Delete:

                    var member = filteredMembers.ElementAt(indexPath.Row);

                    if (!groupId.Contains(LoginController.userModel.id))
                    {
                        // remove the item from the underlying data source
                        filteredMembers.RemoveAt(indexPath.Row);
                        // delete the row from the table
                        tableView.DeleteRows(new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);

                        await new UserGroupManager().UpdateGroupRelationshipStatus(LoginController.tokenModel.access_token, groupId, member.id, UserGroupRelationshipStatus.NotMember);
                    }
                    else
                    {
                        Utils.ShowToast("Can't remove group admin", 3000);
                    }
                    break;

                case UITableViewCellEditingStyle.Insert:

                    Console.WriteLine("CommitEditingStyle:Insert called");
                    break;

                case UITableViewCellEditingStyle.None:
                    Console.WriteLine("CommitEditingStyle:None called");
                    break;
            }
        }

        /// <summary>
        /// Called by the table view to determine whether the editing control should be an insert
        /// or a delete.
        /// </summary>
        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return UITableViewCellEditingStyle.Delete;
        }
    }
}
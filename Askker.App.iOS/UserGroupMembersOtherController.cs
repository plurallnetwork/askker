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
    public partial class UserGroupMembersOtherController : GroupMemberOtherBaseTableViewController
    {
        public List<UserGroupMemberModel> members { get; set; }
        public string groupId { get; set; }
        public string groupProfilePicture { get; set; }

        MemberResultsOtherTableController resultsTableController;
        UISearchController searchController;
        bool searchControllerWasActive;
        bool searchControllerSearchFieldWasFirstResponder;

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
            groupProfileBtn.TitleEdgeInsets = new UIEdgeInsets(0, groupProfileBtn.ImageView.Frame.Size.Width / 4, 0, -groupProfileBtn.ImageView.Frame.Size.Width);
            groupProfileBtn.ImageEdgeInsets = new UIEdgeInsets(0, -groupProfileBtn.TitleLabel.Frame.Size.Width / 4, 0, groupProfileBtn.TitleLabel.Frame.Size.Width);
            groupProfileBtn.AddTarget(Self, new ObjCRuntime.Selector("GroupProfileBtn:"), UIControlEvent.TouchUpInside);
            NavigationItem.TitleView = groupProfileBtn;

            members = new List<UserGroupMemberModel>();

            resultsTableController = new MemberResultsOtherTableController
            {
                filteredMembers = new List<UserGroupMemberModel>()
            };

            searchController = new UISearchController(resultsTableController)
            {
                WeakDelegate = this,
                DimsBackgroundDuringPresentation = false,
                WeakSearchResultsUpdater = this
            };

            searchController.SearchBar.SizeToFit();
            TableView.TableHeaderView = searchController.SearchBar;
            TableView.SetContentOffset(new CGPoint(0, 56), true);

            resultsTableController.TableView.WeakDelegate = this;
            searchController.SearchBar.WeakDelegate = this;

            DefinesPresentationContext = true;

            if (searchControllerWasActive)
            {
                searchController.Active = searchControllerWasActive;
                searchControllerWasActive = false;

                if (searchControllerSearchFieldWasFirstResponder)
                {
                    searchController.SearchBar.BecomeFirstResponder();
                    searchControllerSearchFieldWasFirstResponder = false;
                }
            }

            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            fetchMembers();            
        }

        public async void fetchMembers()
        {
            try
            {
                members = await new UserGroupManager().GetGroupMembers(LoginController.tokenModel.access_token, groupId);
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
            var cell = tableView.DequeueReusableCell(cellIdentifier) as MembersOtherTableViewCell;

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

        [Export("updateSearchResultsForSearchController:")]
        public virtual void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            var tableController = (MemberResultsOtherTableController)searchController.SearchResultsController;
            tableController.filteredMembers = PerformSearch(searchController.SearchBar.Text);
            tableController.TableView.ReloadData();
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

    public class GroupMemberOtherBaseTableViewController : CustomUITableViewController
    {
        protected const string cellIdentifier = "cellId";

        public GroupMemberOtherBaseTableViewController()
        {
        }

        public GroupMemberOtherBaseTableViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);
            TableView.RegisterClassForCellReuse(typeof(MembersOtherTableViewCell), cellIdentifier);
            TableView.SeparatorInset = new UIEdgeInsets(0, 10, 0, 10);
        }

        protected void ConfigureCell(MembersOtherTableViewCell cell, UserGroupMemberModel member)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            if (member.profilePicture != null)
            {
                Utils.SetImageFromNSUrlSession(member.profilePicture, cell.profileImageView, this, PictureType.Profile);
            }

            cell.nameLabel.Text = member.name;
        }
    }

    public partial class MembersOtherTableViewCell : UITableViewCell
    {
        public UIImageView profileImageView { get; set; }
        public UILabel nameLabel { get; set; }

        protected MembersOtherTableViewCell(IntPtr handle) : base(handle)
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

    public class MemberResultsOtherTableController : GroupMemberOtherBaseTableViewController
    {
        public List<UserGroupMemberModel> filteredMembers { get; set; }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return filteredMembers.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(cellIdentifier) as MembersOtherTableViewCell;

            var userGroup = filteredMembers[indexPath.Row];

            ConfigureCell(cell, userGroup);

            return cell;
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 60;
        }
    }
}
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
    public partial class GroupsController : GroupBaseTableViewController
    {
        public List<UserGroupModel> userGroups { get; set; }

        GroupResultsTableController resultsTableController;
        UISearchController searchController;
        bool searchControllerWasActive;
        bool searchControllerSearchFieldWasFirstResponder;

        public GroupsController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            userGroups = new List<UserGroupModel>();

            resultsTableController = new GroupResultsTableController
            {
                filteredGroups = new List<UserGroupModel>()
            };

            searchController = new UISearchController(resultsTableController)
            {
                WeakDelegate = this,
                DimsBackgroundDuringPresentation = false,
                WeakSearchResultsUpdater = this
            };

            searchController.SearchBar.SizeToFit();
            TableView.TableHeaderView = searchController.SearchBar;
            TableView.SetContentOffset(new CGPoint(0, 44), true);

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
            fetchGroups();
        }

        public async void fetchGroups()
        {
            try
            {
                userGroups = await new UserGroupManager().GetGroups(LoginController.userModel.id, LoginController.tokenModel.access_token);
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
            return userGroups.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(cellIdentifier) as MyGroupsTableViewCell;

            ConfigureCell(cell, userGroups[indexPath.Row]);

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            Utils.OpenUserProfile(this.NavigationController, userGroups[indexPath.Row].id);
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 60;
        }

        [Export("updateSearchResultsForSearchController:")]
        public virtual void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            var tableController = (GroupResultsTableController)searchController.SearchResultsController;
            tableController.filteredGroups = PerformSearch(searchController.SearchBar.Text);
            tableController.TableView.ReloadData();
        }

        List<UserGroupModel> PerformSearch(string searchString)
        {
            searchString = searchString.Trim();
            string[] searchItems = string.IsNullOrEmpty(searchString)
                ? new string[0]
                : searchString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var filteredGroups = new List<UserGroupModel>();

            foreach (var item in searchItems)
            {
                IEnumerable<UserGroupModel> query =
                    from aGroup in userGroups
                    where aGroup.searchName.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0
                    orderby aGroup.searchName
                    select aGroup;

                filteredGroups.AddRange(query);
            }

            return filteredGroups.Distinct().ToList();
        }
    }

    public class GroupBaseTableViewController : CustomUITableViewController
    {
        protected const string cellIdentifier = "cellId";

        public GroupBaseTableViewController()
        {
        }

        public GroupBaseTableViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);
            TableView.RegisterClassForCellReuse(typeof(MyGroupsTableViewCell), cellIdentifier);
            TableView.SeparatorInset = new UIEdgeInsets(0, 10, 0, 10);
        }

        protected void ConfigureCell(MyGroupsTableViewCell cell, UserGroupModel userGroup)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            if (userGroup.profilePicture != null)
            {
                Utils.SetImageFromNSUrlSession(userGroup.profilePicture, cell.profileImageView, this, PictureType.Profile);
            }

            cell.nameLabel.Text = userGroup.searchName;
        }
    }

    public partial class MyGroupsTableViewCell : UITableViewCell
    {
        public UIImageView profileImageView { get; set; }
        public UILabel nameLabel { get; set; }

        protected MyGroupsTableViewCell(IntPtr handle) : base(handle)
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

            ContentView.Add(profileImageView);
            ContentView.Add(nameLabel);

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-8-[v0(44)]-8-[v1]-8-|", new NSLayoutFormatOptions(), "v0", profileImageView, "v1", nameLabel));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(44)]", new NSLayoutFormatOptions(), "v0", profileImageView));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-18-[v0(22)]", new NSLayoutFormatOptions(), "v0", nameLabel));
        }
    }

    public class GroupResultsTableController : GroupBaseTableViewController
    {
        public List<UserGroupModel> filteredGroups { get; set; }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return filteredGroups.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(cellIdentifier) as MyGroupsTableViewCell;

            var userGroup = filteredGroups[indexPath.Row];

            ConfigureCell(cell, userGroup);

            return cell;
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 60;
        }
    }
}
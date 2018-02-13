using Askker.App.iOS.CustomViewComponents;
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
        private NSObject changeBackBtnText;

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            if (changeBackBtnText != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(changeBackBtnText);
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            Title = "My Groups";

            base.ViewWillAppear(animated);
        }

        public GroupsController (IntPtr handle) : base (handle)
        {            
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            changeBackBtnText = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("ChangeBackBtnTextGroups"), ChangeBackBtnText);

            UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = UIColor.FromRGB(90, 89, 89)
            };

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
            searchController.HidesNavigationBarDuringPresentation = false;
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

            if (userGroups.Count > 0)
            {
                TableView.BackgroundView = null;
                TableView.ReloadData();
            }
            else
            {
                TableView.BackgroundView = Utils.GetSystemWarningImage("MyGroupsEmpty");
            }
        }

        [Export("searchBarSearchButtonClicked:")]
        public virtual void SearchButtonClicked(UISearchBar searchBar)
        {
            searchBar.ResignFirstResponder();
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            if (userGroups.Count > 0)
            {
                tableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
            }
            else
            {
                tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            }

            return userGroups.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(cellIdentifier) as ProfileTableViewCell;

            ConfigureCell(cell, userGroups[indexPath.Row]);

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("ChangeBackBtnText"), null);
            NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("ChangeBackBtnTextGroups"), null);
            Utils.OpenGroupMembers(this.NavigationController, userGroups[indexPath.Row].userId, userGroups[indexPath.Row].userId + userGroups[indexPath.Row].creationDate, userGroups[indexPath.Row].profilePicture);
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
                    where aGroup.name.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0
                    orderby aGroup.name
                    select aGroup;

                filteredGroups.AddRange(query);
            }

            return filteredGroups.Distinct().ToList();
        }

        private void ChangeBackBtnText(NSNotification notification)
        {
            Title = "";
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
            TableView.RegisterClassForCellReuse(typeof(ProfileTableViewCell), cellIdentifier);
            TableView.SeparatorInset = new UIEdgeInsets(0, 10, 0, 10);
        }

        protected void ConfigureCell(ProfileTableViewCell cell, UserGroupModel userGroup)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            cell.profileImageView.Image = UIImage.FromBundle("Group");

            if (userGroup.profilePicture != null)
            {
                Utils.SetImageFromNSUrlSession(userGroup.profilePicture, cell.profileImageView, this, PictureType.Profile);
            }

            cell.nameLabel.Text = userGroup.name;
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
            var cell = tableView.DequeueReusableCell(cellIdentifier) as ProfileTableViewCell;

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
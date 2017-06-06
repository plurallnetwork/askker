using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace Askker.App.iOS
{
    public partial class FriendsController : BaseTableViewController
    {
        public List<UserFriendModel> userFriends { get; set; }
        public UIActivityIndicatorView indicator;

        ResultsTableController resultsTableController;
        UISearchController searchController;
        bool searchControllerWasActive;
        bool searchControllerSearchFieldWasFirstResponder;

        public FriendsController (IntPtr handle) : base (handle)
        {
            indicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
            indicator.Frame = new CoreGraphics.CGRect(0.0, 0.0, 80.0, 80.0);
            indicator.Center = this.View.Center;
            Add(indicator);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            userFriends = new List<UserFriendModel>();

            resultsTableController = new ResultsTableController
            {
                filteredFriends = new List<UserFriendModel>()
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

            indicator.StartAnimating();
            fetchFriends();
        }

        public async void fetchFriends()
        {
            try
            {
                userFriends = await new FriendManager().GetFriends(LoginController.userModel.id, LoginController.tokenModel.access_token);
            }
            catch (Exception ex)
            {
                Utils.HandleException(ex);
            }

            indicator.StopAnimating();

            TableView.ReloadData();
        }

        [Export("searchBarSearchButtonClicked:")]
        public virtual void SearchButtonClicked(UISearchBar searchBar)
        {
            searchBar.ResignFirstResponder();
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return userFriends.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(cellIdentifier) as MyFriendsTableViewCell;

            ConfigureCell(cell, userFriends[indexPath.Row]);

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            Utils.OpenUserProfile(this.NavigationController, userFriends[indexPath.Row].id);
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 60;
        }

        [Export("updateSearchResultsForSearchController:")]
        public virtual void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            var tableController = (ResultsTableController)searchController.SearchResultsController;
            tableController.filteredFriends = PerformSearch(searchController.SearchBar.Text);
            tableController.TableView.ReloadData();
        }

        List<UserFriendModel> PerformSearch(string searchString)
        {
            searchString = searchString.Trim();
            string[] searchItems = string.IsNullOrEmpty(searchString)
                ? new string[0]
                : searchString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var filteredFriends = new List<UserFriendModel>();

            foreach (var item in searchItems)
            {
                IEnumerable<UserFriendModel> query =
                    from friend in userFriends
                    where friend.name.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0
                    orderby friend.name
                    select friend;

                filteredFriends.AddRange(query);
            }

            return filteredFriends.Distinct().ToList();
        }
    }

    public class BaseTableViewController : UITableViewController
    {
        protected const string cellIdentifier = "cellId";

        public BaseTableViewController()
        {
        }

        public BaseTableViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.RegisterClassForCellReuse(typeof(MyFriendsTableViewCell), cellIdentifier);
            TableView.SeparatorInset = new UIEdgeInsets(0, 10, 0, 10);
        }

        protected void ConfigureCell(MyFriendsTableViewCell cell, UserFriendModel userFriend)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            if (userFriend.profilePicture != null)
            {
                Utils.SetImageFromNSUrlSession(userFriend.profilePicture, cell.profileImageView, this);
            }

            cell.nameLabel.Text = userFriend.name;
        }
    }

    public partial class MyFriendsTableViewCell : UITableViewCell
    {
        public UIImageView profileImageView { get; set; }
        public UILabel nameLabel { get; set; }

        protected MyFriendsTableViewCell(IntPtr handle) : base(handle)
        {
            profileImageView = new UIImageView();
            profileImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            profileImageView.Layer.CornerRadius = 22;
            profileImageView.Layer.MasksToBounds = true;
            profileImageView.TranslatesAutoresizingMaskIntoConstraints = false;
            profileImageView.Image = UIImage.FromBundle("Profile");

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

    public class ResultsTableController : BaseTableViewController
    {
        public List<UserFriendModel> filteredFriends { get; set; }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return filteredFriends.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(cellIdentifier) as MyFriendsTableViewCell;

            var userFriend = filteredFriends[indexPath.Row];

            ConfigureCell(cell, userFriend);

            return cell;
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 60;
        }
    }
}
﻿using Askker.App.iOS.CustomViewComponents;
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
    public partial class FriendsController : BaseTableViewController
    {
        public List<UserFriendModel> userFriends { get; set; }
        
        ResultsTableController resultsTableController;
        UISearchController searchController;
        bool searchControllerWasActive;
        bool searchControllerSearchFieldWasFirstResponder;

        public FriendsController (IntPtr handle) : base (handle)
        {        
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = UIColor.FromRGB(90, 89, 89)
            };

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

            BTProgressHUD.Dismiss();
            
            if (userFriends.Count > 0)
            {
                TableView.BackgroundView = null;
                TableView.ReloadData();
            }
            else
            {
                TableView.BackgroundView = Utils.GetSystemWarningImage("MyFriendsEmpty");
            }
        }

        [Export("searchBarSearchButtonClicked:")]
        public virtual void SearchButtonClicked(UISearchBar searchBar)
        {
            searchBar.ResignFirstResponder();
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            if (userFriends.Count > 0)
            {
                tableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
            }
            else
            {
                tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            }

            return userFriends.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(cellIdentifier) as ProfileTableViewCell;

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

    public class BaseTableViewController : CustomUITableViewController
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
            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);
            TableView.RegisterClassForCellReuse(typeof(ProfileTableViewCell), cellIdentifier);
            TableView.SeparatorInset = new UIEdgeInsets(0, 10, 0, 10);
        }

        protected void ConfigureCell(ProfileTableViewCell cell, UserFriendModel userFriend)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            cell.profileImageView.Image = UIImage.FromBundle("Profile");

            if (userFriend.profilePicture != null)
            {
                Utils.SetImageFromNSUrlSession(userFriend.profilePicture, cell.profileImageView, this, PictureType.Profile);
            }

            cell.nameLabel.Text = userFriend.name;
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
            var cell = tableView.DequeueReusableCell(cellIdentifier) as ProfileTableViewCell;

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
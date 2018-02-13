using Askker.App.iOS.CustomViewComponents;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Enums;
using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace Askker.App.iOS.TableControllers
{
    public class SearchProfileTableViewController : CustomUITableViewController
    {
        protected const string cellIdentifier = "cellId";
        public List<SearchProfileTableItem> tableItems { get; set; }
        public SearchProfileType searchProfileType { get; set; }
        public UINavigationController navigationController { get; set; }

        public SearchProfileTableViewController()
        {
        }

        public SearchProfileTableViewController(SearchProfileType searchProfileType, UINavigationController navigationController)
        {
            this.searchProfileType = searchProfileType;
            this.navigationController = navigationController;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);
            TableView.RegisterClassForCellReuse(typeof(ProfileTableViewCell), cellIdentifier);
            TableView.SeparatorInset = new UIEdgeInsets(0, 10, 0, 10);

            tableItems = new List<SearchProfileTableItem>();
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            if (tableItems.Count > 0)
            {
                tableView.BackgroundView = null;
                tableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
            }
            else
            {
                if (SearchProfileType.Friends.Equals(searchProfileType))
                {
                    tableView.BackgroundView = Utils.GetSystemWarningImage("FindFriendsEmpty");
                }
                else if (SearchProfileType.Groups.Equals(searchProfileType))
                {
                    tableView.BackgroundView = Utils.GetSystemWarningImage("FindGroupsEmpty");
                }

                tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            }

            return tableItems.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(cellIdentifier) as ProfileTableViewCell;

            ConfigureCell(cell, tableItems[indexPath.Row]);

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            if (SearchProfileType.Friends.Equals(searchProfileType))
            {
                Utils.OpenUserProfile(this.navigationController, this.tableItems[indexPath.Row].Id);
            }
            else if (SearchProfileType.Groups.Equals(searchProfileType))
            {
                Utils.OpenGroupProfile(this.navigationController, this.tableItems[indexPath.Row].Id);
            }
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 60;
        }

        protected void ConfigureCell(ProfileTableViewCell cell, SearchProfileTableItem tableItem)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            if (SearchProfileType.Friends.Equals(searchProfileType))
            {
                cell.profileImageView.Image = UIImage.FromBundle("Profile");

                if (tableItem.ProfilePicture != null)
                {
                    Utils.SetImageFromNSUrlSession(tableItem.ProfilePicture, cell.profileImageView, this, PictureType.Profile);
                }
            }
            else if (SearchProfileType.Groups.Equals(searchProfileType))
            {
                cell.profileImageView.Image = UIImage.FromBundle("Group");

                if (tableItem.ProfilePicture != null)
                {
                    Utils.SetImageFromNSUrlSession(tableItem.ProfilePicture, cell.profileImageView, this, PictureType.Group);
                }
            }

            cell.nameLabel.Text = tableItem.Name;
        }

        public async void PerformSearch(string searchText)
        {
            searchText = searchText.ToLower();
            if (!string.IsNullOrEmpty(searchText.Trim()))
            {
                try
                {
                    tableItems = new List<SearchProfileTableItem>();

                    if (SearchProfileType.Friends.Equals(searchProfileType))
                    {
                        var users = await new LoginManager().SearchUsersByName(LoginController.tokenModel.access_token, searchText);

                        foreach (var user in users)
                        {
                            tableItems.Add(new SearchProfileTableItem(user.id, user.name, user.profilePicturePath));
                        }
                    }
                    else if (SearchProfileType.Groups.Equals(searchProfileType))
                    {
                        var groups = await new UserGroupManager().SearchGroupsByName(LoginController.tokenModel.access_token, searchText);

                        foreach (var group in groups)
                        {
                            tableItems.Add(new SearchProfileTableItem(group.userId + group.creationDate, group.name, group.profilePicture));
                        }
                    }

                    TableView.ReloadData();
                }
                catch (Exception ex)
                {
                    Utils.HandleException(ex);
                }
            }
        }
    }
}

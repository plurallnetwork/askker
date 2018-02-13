using Askker.App.iOS.CustomViewComponents;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Enums;
using BigTed;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace Askker.App.iOS.TableControllers
{
    public class ProfileListTableViewController : CustomUITableViewController
    {
        protected const string cellIdentifier = "cellId";
        public List<ProfileTableItem> tableItems { get; set; }
        public List<ProfileTableItem> fetchedTableItems { get; set; }
        public UISearchBar searchBar { get; set; }
        public ProfileType profileType { get; set; }
        public string groupId { get; set; }

        public ProfileListTableViewController()
        {
        }

        public ProfileListTableViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);

            tableItems = new List<ProfileTableItem>();
            fetchedTableItems = new List<ProfileTableItem>();

            searchBar = new UISearchBar();
            searchBar.SizeToFit();
            searchBar.AutocorrectionType = UITextAutocorrectionType.No;
            searchBar.AutocapitalizationType = UITextAutocapitalizationType.None;
            searchBar.Placeholder = "Search";
            searchBar.OnEditingStarted += (sender, e) =>
            {
                searchBar.ShowsCancelButton = true;
            };
            searchBar.OnEditingStopped += (sender, e) =>
            {
                searchBar.ShowsCancelButton = false;
            };
            searchBar.CancelButtonClicked += (sender, e) =>
            {
                cleanTable();
                searchBar.ShowsCancelButton = false;
                searchBar.Text = "";
                searchBar.ResignFirstResponder();
            };
            searchBar.TextChanged += (sender, e) =>
            {
                searchTable();
            };
            searchBar.SearchButtonClicked += (sender, e) =>
            {
                searchBar.ResignFirstResponder();
            };

            foreach (UIView subView in searchBar.Subviews)
            {
                foreach (UIView secondLevelSubview in subView.Subviews)
                {
                    if (secondLevelSubview is UITextField)
                    {
                        UITextField searchBarTextField = (UITextField)secondLevelSubview;

                        searchBarTextField.TextColor = UIColor.FromRGB(90, 89, 89);
                        break;
                    }
                }
            }

            TableView.RegisterClassForCellReuse(typeof(ProfileTableViewCell), cellIdentifier);
            TableView.SeparatorInset = new UIEdgeInsets(0, 10, 0, 10);
            TableView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height - 20);
            TableView.TableHeaderView = searchBar;
            TableView.SetContentOffset(new CGPoint(0, 56), true);
        }

        private void cleanTable()
        {
            tableItems = fetchedTableItems;
            TableView.ReloadData();
        }

        private void searchTable()
        {
            tableItems = PerformSearch(searchBar.Text);
            TableView.ReloadData();
        }

        protected async void fetchTableItems()
        {
            try
            {
                fetchedTableItems = new List<ProfileTableItem>();

                if (ProfileType.ListFriends.Equals(profileType))
                {
                    var userFriends = await new FriendManager().GetFriends(LoginController.userModel.id, LoginController.tokenModel.access_token);

                    foreach (var user in userFriends)
                    {
                        fetchedTableItems.Add(new ProfileTableItem(user.id, user.name, user.profilePicture));
                    }
                }
                else if (ProfileType.ListGroups.Equals(profileType))
                {
                    var userGroups = await new UserGroupManager().GetGroups(LoginController.userModel.id, LoginController.tokenModel.access_token);

                    foreach (var group in userGroups)
                    {
                        fetchedTableItems.Add(new ProfileTableItem(group.userId + group.creationDate, group.name, group.profilePicture));
                    }
                }
                else if (ProfileType.ListGroupMembers.Equals(profileType))
                {
                    var groupMembers = await new UserGroupManager().GetGroupMembers(LoginController.tokenModel.access_token, groupId);

                    foreach (var member in groupMembers)
                    {
                        fetchedTableItems.Add(new ProfileTableItem(member.id, member.name, member.profilePicture));
                    }
                }

                tableItems = fetchedTableItems;
            }
            catch (Exception ex)
            {
                Utils.HandleException(ex);
            }

            BTProgressHUD.Dismiss();

            if (tableItems.Count > 0)
            {
                TableView.BackgroundView = null;
                TableView.ReloadData();
            }
            else
            {
                if (ProfileType.ListFriends.Equals(profileType))
                {
                    TableView.BackgroundView = Utils.GetSystemWarningImage("MyFriendsEmpty");
                }
                else if (ProfileType.ListGroups.Equals(profileType))
                {
                    TableView.BackgroundView = Utils.GetSystemWarningImage("MyGroupsEmpty");
                }
            }
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            if (tableItems.Count > 0)
            {
                tableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
            }
            else
            {
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
            if (ProfileType.ListFriends.Equals(profileType) || ProfileType.ListGroupMembers.Equals(profileType))
            {
                Utils.OpenUserProfile(NavigationController, this.tableItems[indexPath.Row].Id);
            }
            else if (ProfileType.ListGroups.Equals(profileType))
            {
                NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("ChangeBackBtnText"), null);
                NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("ChangeBackBtnTextGroups"), null);
                Utils.OpenGroupMembers(NavigationController, tableItems[indexPath.Row].Id.ToString().Substring(0, 36), tableItems[indexPath.Row].Id, tableItems[indexPath.Row].ProfilePicture);
            }
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 60;
        }

        protected void ConfigureCell(ProfileTableViewCell cell, ProfileTableItem tableItem)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            
            if (ProfileType.ListFriends.Equals(profileType) || ProfileType.ListGroupMembers.Equals(profileType))
            {
                cell.profileImageView.Image = UIImage.FromBundle("Profile");

                if (tableItem.ProfilePicture != null)
                {
                    Utils.SetImageFromNSUrlSession(tableItem.ProfilePicture, cell.profileImageView, this, PictureType.Profile);
                }
            }
            else if (ProfileType.ListGroups.Equals(profileType))
            {
                cell.profileImageView.Image = UIImage.FromBundle("Group");

                if (tableItem.ProfilePicture != null)
                {
                    Utils.SetImageFromNSUrlSession(tableItem.ProfilePicture, cell.profileImageView, this, PictureType.Group);
                }
            }

            cell.nameLabel.Text = tableItem.Name;
        }

        protected List<ProfileTableItem> PerformSearch(string searchString)
        {
            searchString = searchString.Trim();
            string[] searchItems = string.IsNullOrEmpty(searchString)
                ? new string[0]
                : searchString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var filteredTableItems = new List<ProfileTableItem>();

            foreach (var item in searchItems)
            {
                IEnumerable<ProfileTableItem> query =
                    from tableItem in fetchedTableItems
                    where tableItem.Name.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0
                    orderby tableItem.Name
                    select tableItem;

                filteredTableItems.AddRange(query);
            }

            if (filteredTableItems.Count == 0 && string.IsNullOrWhiteSpace(searchString))
            {
                return fetchedTableItems;
            }
            else
            {
                return filteredTableItems.Distinct().ToList();
            }
        }

        /// <summary>
        /// Called by the table view to determine whether or not the row is editable
        /// </summary>
        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            // return false if you wish to disable editing for a specific indexPath or for all rows
            if (ProfileType.ListGroupMembers.Equals(profileType) && groupId.ToString().Substring(0, 36).Equals(LoginController.userModel.id))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Custom text for delete button
        /// </summary>
        public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
        {
            return "Remove member";
        }

        public override async void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            switch (editingStyle)
            {
                case UITableViewCellEditingStyle.Delete:

                    var member = tableItems.ElementAt(indexPath.Row);

                    if (!member.Id.Equals(LoginController.userModel.id))
                    {
                        // remove the item from the underlying data source
                        tableItems.RemoveAt(indexPath.Row);
                        // delete the row from the table
                        tableView.DeleteRows(new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);

                        await new UserGroupManager().UpdateGroupRelationshipStatus(LoginController.tokenModel.access_token, groupId, member.Id, UserGroupRelationshipStatus.Unmembered);
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

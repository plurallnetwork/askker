using Askker.App.iOS.CustomViewComponents;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Enums;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace Askker.App.iOS.TableControllers
{
    public class ProfileSearchTableViewController : CustomUITableViewController
    {
        protected const string cellIdentifier = "cellId";
        public List<ProfileTableItem> tableItems { get; set; }
        public UISearchBar searchBar { get; set; }
        public ProfileType profileType { get; set; }
        public UINavigationController navigationController { get; set; }

        public ProfileSearchTableViewController()
        {
        }

        public ProfileSearchTableViewController(ProfileType profileType, UINavigationController navigationController)
        {
            this.profileType = profileType;
            this.navigationController = navigationController;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);

            tableItems = new List<ProfileTableItem>();

            //Declare the search bar and add it to the header of the table
            searchBar = new UISearchBar();
            searchBar.SizeToFit();
            searchBar.AutocorrectionType = UITextAutocorrectionType.No;
            searchBar.AutocapitalizationType = UITextAutocapitalizationType.None;
            searchBar.Placeholder = "Type at least 3 characters";
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
                //this is the method that is called when the user searches
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

                        //set font color here
                        searchBarTextField.TextColor = UIColor.FromRGB(90, 89, 89);
                        break;
                    }
                }
            }

            TableView.RegisterClassForCellReuse(typeof(ProfileTableViewCell), cellIdentifier);
            TableView.SeparatorInset = new UIEdgeInsets(0, 10, 0, 10);
            TableView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height - 20);
            TableView.TableHeaderView = searchBar;
        }

        private void cleanTable()
        {
            tableItems = new List<ProfileTableItem>();
            TableView.ReloadData();
        }

        private void searchTable()
        {
            //perform the search, and refresh the table with the results
            if (searchBar.Text.Length >= 3)
            {
                PerformSearch(searchBar.Text);
            }
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
                if (ProfileType.FindFriends.Equals(profileType))
                {
                    tableView.BackgroundView = Utils.GetSystemWarningImage("FindFriendsEmpty");
                }
                else if (ProfileType.FindGroups.Equals(profileType))
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
            if (ProfileType.FindFriends.Equals(profileType))
            {
                Utils.OpenUserProfile(this.navigationController, this.tableItems[indexPath.Row].Id);
            }
            else if (ProfileType.FindGroups.Equals(profileType))
            {
                NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("ChangeBackBtnText"), null);
                NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("ChangeBackBtnTextGroups"), null);
                Utils.OpenGroupProfile(this.navigationController, this.tableItems[indexPath.Row].Id);
            }
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 60;
        }

        protected void ConfigureCell(ProfileTableViewCell cell, ProfileTableItem tableItem)
        {
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            if (ProfileType.FindFriends.Equals(profileType))
            {
                cell.profileImageView.Image = UIImage.FromBundle("Profile");

                if (tableItem.ProfilePicture != null)
                {
                    Utils.SetImageFromNSUrlSession(tableItem.ProfilePicture, cell.profileImageView, this, PictureType.Profile);
                }
            }
            else if (ProfileType.FindGroups.Equals(profileType))
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
                    if (ProfileType.FindFriends.Equals(profileType))
                    {
                        var users = await new LoginManager().SearchUsersByName(LoginController.tokenModel.access_token, searchText);

                        tableItems = new List<ProfileTableItem>();

                        foreach (var user in users)
                        {
                            tableItems.Add(new ProfileTableItem(user.id, user.name, user.profilePicturePath));
                        }
                    }
                    else if (ProfileType.FindGroups.Equals(profileType))
                    {
                        var groups = await new UserGroupManager().SearchGroupsByName(LoginController.tokenModel.access_token, searchText);

                        tableItems = new List<ProfileTableItem>();

                        foreach (var group in groups)
                        {
                            tableItems.Add(new ProfileTableItem(group.userId + group.creationDate, group.name, group.profilePicture));
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

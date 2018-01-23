using Askker.App.iOS.CustomViewComponents;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Askker.App.iOS.TableControllers
{
    public class SearchUserGroupsTableSource : UITableViewSource
    {
        private List<SearchUserGroupsTableItem> tableItems = new List<SearchUserGroupsTableItem>();
        private List<SearchUserGroupsTableItem> searchItems = new List<SearchUserGroupsTableItem>();
        protected NSString cellIdentifier = new NSString("TableCell");
        public static NSCache imageCache = new NSCache();
        public UINavigationController navigationController = new UINavigationController();

        public SearchUserGroupsTableSource(List<SearchUserGroupsTableItem> items, UINavigationController navigationController)
        {
            this.tableItems = items;
            this.searchItems = items;
            this.navigationController = navigationController;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return searchItems.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // request a recycled cell to save memory
            SearchUserGroupsCustomCell cell = tableView.DequeueReusableCell(cellIdentifier) as SearchUserGroupsCustomCell;

            // if there are no cells to reuse, create a new one
            if (cell == null)
            {
                cell = new SearchUserGroupsCustomCell(cellIdentifier);
            }

            var imageView = cell.GetImageView();
            imageView.Image = UIImage.FromBundle("Group");

            if (!string.IsNullOrEmpty(searchItems[indexPath.Row].ImageName))
            {
                Utils.SetImageFromNSUrlSession(searchItems[indexPath.Row].ImageName, imageView, this, PictureType.Group);
            }

            cell.UpdateCell(searchItems[indexPath.Row].Title);
            return cell;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            var numOfSections = 0;

            if(tableItems.Count > 0)
            {
                numOfSections = 1;
                tableView.BackgroundView = null;
                tableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
            }
            else
            {
                UILabel label = new UILabel(new CoreGraphics.CGRect(0, 0, tableView.Bounds.Size.Width, tableView.Bounds.Size.Height));
                label.Text = "No results found";
                label.TextColor = UIColor.Red;
                label.TextAlignment = UITextAlignment.Center;
                tableView.BackgroundView = label;
                tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            }

            return numOfSections;
        }

        public async void PerformSearch(string searchText)
        {
            searchText = searchText.ToLower();
            if (!string.IsNullOrEmpty(searchText.Trim()))
            {
                try
                {
                    List<UserGroupModel> groups = await new UserGroupManager().SearchGroupsByName(LoginController.tokenModel.access_token, searchText);
                    
                    tableItems = new List<SearchUserGroupsTableItem>();

                    foreach (var group in groups)
                    {
                        tableItems.Add(new SearchUserGroupsTableItem(group.userId+group.creationDate, group.name, group.profilePicture));
                    }

                    SearchUserGroupsController.table.Source = new SearchUserGroupsTableSource(tableItems, this.navigationController);
                    SearchUserGroupsController.table.ReloadData();
                }
                catch (Exception ex)
                {
                    tableItems = new List<SearchUserGroupsTableItem>();

                    SearchUserGroupsController.table.Source = new SearchUserGroupsTableSource(tableItems, this.navigationController);
                    SearchUserGroupsController.table.ReloadData();

                    Utils.HandleException(ex);
                }
            }
            else
            {
                tableItems = new List<SearchUserGroupsTableItem>();

                SearchUserGroupsController.table.Source = new SearchUserGroupsTableSource(tableItems, this.navigationController);
                SearchUserGroupsController.table.ReloadData();
            }
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("ChangeBackBtnText"), null);
            NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("ChangeBackBtnTextGroups"), null);
            Utils.OpenGroupProfile(this.navigationController, this.tableItems[indexPath.Row].Id);
        }
    }
}

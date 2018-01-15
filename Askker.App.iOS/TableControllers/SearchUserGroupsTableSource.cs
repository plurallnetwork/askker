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
            return 1;
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
            Utils.OpenGroupMembers(this.navigationController, this.tableItems[indexPath.Row].Id, this.tableItems[indexPath.Row].Id, this.tableItems[indexPath.Row].ImageName);
        }
    }
}

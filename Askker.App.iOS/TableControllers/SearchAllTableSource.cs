using Askker.App.iOS.CustomViewComponents;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UIKit;

namespace Askker.App.iOS.TableControllers
{
    public class SearchAllTableSource : UITableViewSource
    {
        private List<SearchAllTableItem> tableItems = new List<SearchAllTableItem>();
        private List<SearchAllTableItem> searchItems = new List<SearchAllTableItem>();
        protected NSString cellIdentifier = new NSString("TableCell");
        public static NSCache imageCache = new NSCache();
        public UINavigationController navigationController = new UINavigationController();

        public SearchAllTableSource(List<SearchAllTableItem> items, UINavigationController navigationController)
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
            SearchAllCustomCell cell = tableView.DequeueReusableCell(cellIdentifier) as SearchAllCustomCell;

            // if there are no cells to reuse, create a new one
            if (cell == null)
            {
                cell = new SearchAllCustomCell(cellIdentifier);
            }

            var imageView = cell.GetImageView();
            imageView.Image = UIImage.FromBundle("Profile");

            if (!string.IsNullOrEmpty(searchItems[indexPath.Row].ImageName))
            {
                Utils.SetImageFromNSUrlSession(searchItems[indexPath.Row].ImageName, imageView, this, PictureType.Profile);
            }

            cell.UpdateCell(searchItems[indexPath.Row].Title);
            return cell;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            var numOfSections = 0;

            if (tableItems.Count > 0)
            {
                numOfSections = 1;
                tableView.BackgroundView = null;
                tableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
            }
            else
            {
                tableView.BackgroundView = Utils.GetSystemWarningImage("FindFriendsEmpty");
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
                    //this.searchItems = tableItems.Where(x => x.Title.ToLower().Contains(searchText)).ToList();
                    List<UserModel> users = await new LoginManager().SearchUsersByName(LoginController.tokenModel.access_token, searchText);
                    //Console.WriteLine(users.Count);

                    tableItems = new List<SearchAllTableItem>();

                    foreach (var user in users)
                    {
                        tableItems.Add(new SearchAllTableItem(user.id, user.name, user.profilePicturePath));
                    }

                    SearchAllController.table.Source = new SearchAllTableSource(tableItems, this.navigationController);
                    SearchAllController.table.ReloadData();
                }
                catch (Exception ex)
                {
                    tableItems = new List<SearchAllTableItem>();

                    SearchAllController.table.Source = new SearchAllTableSource(tableItems, this.navigationController);
                    SearchAllController.table.ReloadData();

                    Utils.HandleException(ex);
                }
            }
            else
            {
                tableItems = new List<SearchAllTableItem>();
                
                SearchAllController.table.Source = new SearchAllTableSource(tableItems, this.navigationController);
                SearchAllController.table.ReloadData();
            }
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            Utils.OpenUserProfile(this.navigationController, this.tableItems[indexPath.Row].Id);
        }
    }
}

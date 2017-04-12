using Askker.App.iOS.CustomViewComponents;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
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

        public SearchAllTableSource(List<SearchAllTableItem> items)
        {
            this.tableItems = items;
            this.searchItems = items;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return searchItems.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // request a recycled cell to save memory
            SearchAllCustomCell cell = tableView.DequeueReusableCell(cellIdentifier) as SearchAllCustomCell;
            UIImage image; 

            var cellStyle = UITableViewCellStyle.Default;

            // if there are no cells to reuse, create a new one
            if (cell == null)
            {
                cell = new SearchAllCustomCell(cellIdentifier);
            }

            //cell.textLabel.Text = searchItems[indexPath.Row].Title;
            //cell.ImageView.Image = UIImage.FromFile("Images/" + searchItems[indexPath.Row].ImageName);

            

            if (string.IsNullOrEmpty(searchItems[indexPath.Row].ImageName))
            {
                image = UIImage.FromBundle("Profile");
            }
            else
            {
                image = UIImage.LoadFromData(NSData.FromUrl(new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + searchItems[indexPath.Row].ImageName)));
            }
            

            cell.UpdateCell(searchItems[indexPath.Row].Title, image);
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
                //this.searchItems = tableItems.Where(x => x.Title.ToLower().Contains(searchText)).ToList();
                List<UserModel> users = await new LoginManager().SearchUsersByName(LoginController.tokenModel.access_token, searchText);
                //Console.WriteLine(users.Count);

                tableItems = new List<SearchAllTableItem>();

                foreach (var user in users)
                {
                    tableItems.Add(new SearchAllTableItem(user.name, user.profilePicturePath));
                }

                SearchAllController.table.Source = new SearchAllTableSource(tableItems);
                SearchAllController.table.ReloadData();
            }
            else
            {
                tableItems = new List<SearchAllTableItem>();
                
                SearchAllController.table.Source = new SearchAllTableSource(tableItems);
                SearchAllController.table.ReloadData();
            }
        }
    }
}

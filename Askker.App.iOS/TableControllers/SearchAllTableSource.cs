using Askker.App.PortableLibrary.Business;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace Askker.App.iOS.TableControllers
{
    public class SearchAllTableSource : UITableViewSource
    {
        private List<SearchAllTableItem> tableItems = new List<SearchAllTableItem>();
        private List<SearchAllTableItem> searchItems = new List<SearchAllTableItem>();
        protected string cellIdentifier = "TableCell";

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
            UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);


            var cellStyle = UITableViewCellStyle.Default;

            // if there are no cells to reuse, create a new one
            if (cell == null)
            {
                cell = new UITableViewCell(cellStyle, cellIdentifier);
            }

            cell.TextLabel.Text = searchItems[indexPath.Row].Title;
            cell.ImageView.Image = UIImage.FromFile("Images/" + searchItems[indexPath.Row].ImageName);

            return cell;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return 1;
        }

        public async void PerformSearch(string searchText)
        {
            searchText = searchText.ToLower();
            //this.searchItems = tableItems.Where(x => x.Title.ToLower().Contains(searchText)).ToList();
            await new LoginManager().SearchUsersByName(LoginController.tokenModel.access_token, searchText);
        }
    }
}

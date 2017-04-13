using Askker.App.iOS.CustomViewComponents;
using Askker.App.PortableLibrary.Business;
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
            UIImage image = null;  

            // if there are no cells to reuse, create a new one
            if (cell == null)
            {
                cell = new SearchAllCustomCell(cellIdentifier);
            }

           
            if (string.IsNullOrEmpty(searchItems[indexPath.Row].ImageName))
            {
                image = UIImage.FromBundle("Profile");
            }
            else
            {
                var url = new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + searchItems[indexPath.Row].ImageName);
                var imageFromCache = (UIImage)imageCache.ObjectForKey(NSString.FromObject(url.AbsoluteString));
                if (imageFromCache != null)
                {
                    image = imageFromCache;
                }
                else
                {
                    var task = NSUrlSession.SharedSession.CreateDataTask(url, (data, response, error) =>
                    {
                        if (response == null)
                        {
                            image = UIImage.FromBundle("Profile");
                        }
                        else
                        {
                            try
                            {
                                DispatchQueue.MainQueue.DispatchAsync(() => {
                                    var imageToCache = UIImage.LoadFromData(data);
                    
                                    image = imageToCache;

                                    if (imageToCache != null)
                                    {
                                        imageCache.SetObjectforKey(imageToCache, NSString.FromObject(url.AbsoluteString));
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message);
                            }
                        }
                    });
                    task.Resume();
                }
                    
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

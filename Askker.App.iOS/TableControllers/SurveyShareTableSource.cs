using Askker.App.iOS.CustomViewComponents;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using CoreFoundation;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Askker.App.iOS.TableControllers
{
    public class SurveyShareTableSource : UITableViewSource
    {
        private List<SurveyShareTableItem> tableItems = new List<SurveyShareTableItem>();
        protected NSString cellIdentifier = new NSString("TableCell");
        public static NSCache imageCache = new NSCache();

        public SurveyShareTableSource(List<SurveyShareTableItem> items)
        {
            this.tableItems = items;        
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return tableItems.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // request a recycled cell to save memory
            SurveyShareCustomCell cell = tableView.DequeueReusableCell(cellIdentifier) as SurveyShareCustomCell;
            UIImage image = null;

            // if there are no cells to reuse, create a new one
            if (cell == null)
            {
                cell = new SurveyShareCustomCell(cellIdentifier);
            }

            if (string.IsNullOrEmpty(tableItems[indexPath.Row].ImageName))
            {
                image = UIImage.FromBundle("Profile");
            }
            else
            {
                var url = new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + tableItems[indexPath.Row].ImageName);
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
                                        cell.UpdateCell(tableItems[indexPath.Row].Name, image);
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


            cell.UpdateCell(tableItems[indexPath.Row].Name, image);
            return cell;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return 1;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = tableView.CellAt(indexPath);
            if (indexPath.Row >= 0)
            {
                if (cell.Accessory == UITableViewCellAccessory.Checkmark)
                {
                    cell.Accessory = UITableViewCellAccessory.None;

                    var index = CreateSurveyController.SurveyModel.targetAudienceUsers.ids.IndexOf(tableItems[indexPath.Row].Id);

                    if(index >= 0)
                    {
                        CreateSurveyController.SurveyModel.targetAudienceUsers.ids.RemoveAt(index);
                        CreateSurveyController.SurveyModel.targetAudienceUsers.names.RemoveAt(index);
                    }                    
                }
                else
                {
                    cell.Accessory = UITableViewCellAccessory.Checkmark;

                    if(CreateSurveyController.SurveyModel.targetAudienceUsers == null)
                    {
                        CreateSurveyController.SurveyModel.targetAudienceUsers = new AudienceUsers();
                        CreateSurveyController.SurveyModel.targetAudienceUsers.ids = new List<string>();
                        CreateSurveyController.SurveyModel.targetAudienceUsers.names = new List<string>();
                    }

                    CreateSurveyController.SurveyModel.targetAudienceUsers.ids.Add(tableItems[indexPath.Row].Id);
                    CreateSurveyController.SurveyModel.targetAudienceUsers.names.Add(tableItems[indexPath.Row].Name);
                }
            }
            else
            {
                cell.Accessory = UITableViewCellAccessory.None;
            }

            tableView.DeselectRow(indexPath, true);
        }
    }
}

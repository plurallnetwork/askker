using Askker.App.iOS.CustomViewComponents;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Enums;
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

            // if there are no cells to reuse, create a new one
            if (cell == null)
            {
                cell = new SurveyShareCustomCell(cellIdentifier);
            }

            var imageView = cell.GetImageView();
            imageView.Image = UIImage.FromBundle("Profile");

            if (!string.IsNullOrEmpty(tableItems[indexPath.Row].ImageName))
            {
                Utils.SetImageFromNSUrlSession(tableItems[indexPath.Row].ImageName, imageView, imageCache);
            }

            if (CreateSurveyController.ScreenState == ScreenState.Edit.ToString() && CreateSurveyController.SurveyModel.targetAudience == TargetAudience.Private.ToString())
            {
                if(CreateSurveyController.SurveyModel.targetAudienceUsers != null && CreateSurveyController.SurveyModel.targetAudienceUsers.ids.Count > 0)
                {
                    foreach (var id in CreateSurveyController.SurveyModel.targetAudienceUsers.ids)
                    {
                        if (id.Equals(tableItems[indexPath.Row].Id))
                        {
                            cell.Accessory = UITableViewCellAccessory.Checkmark;
                        }
                    }
                }                
            }

            cell.UpdateCell(tableItems[indexPath.Row].Name);
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

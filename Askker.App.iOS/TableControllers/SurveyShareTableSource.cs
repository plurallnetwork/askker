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

            cell.Accessory = UITableViewCellAccessory.None;
            cell.UpdateCell(null, null);
            
            //var imageView = cell.GetCustomImageView();
            //imageView.Image = UIImage.FromBundle("Profile");
            UIImageView varImageView = new UIImageView();
            varImageView.Image = UIImage.FromBundle("Profile");

            if (!string.IsNullOrEmpty(tableItems[indexPath.Row].ImageName))
            {
                Utils.SetImageFromNSUrlSession(tableItems[indexPath.Row].ImageName, varImageView, this, PictureType.OptionImage);
            }

            if (CreateSurveyController.SurveyModel.targetAudience == TargetAudience.Private.ToString())
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
            else
            if (CreateSurveyController.SurveyModel.targetAudience == TargetAudience.Groups.ToString())
            {
                if (CreateSurveyController.SurveyModel.targetAudienceGroups != null && CreateSurveyController.SurveyModel.targetAudienceGroups.ids.Count > 0)
                {
                    foreach (var id in CreateSurveyController.SurveyModel.targetAudienceGroups.ids)
                    {
                        if (id.Equals(tableItems[indexPath.Row].Id))
                        {
                            cell.Accessory = UITableViewCellAccessory.Checkmark;
                        }
                    }
                }
            }

            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            cell.UpdateCell(tableItems[indexPath.Row].Name, varImageView.Image);
            cell.LayoutSubviews();
            return cell;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return 1;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.CellAt(indexPath) as SurveyShareCustomCell;
            if (CreateSurveyController.SurveyModel.targetAudience == TargetAudience.Private.ToString())
            {
                if (indexPath.Row >= 0)
                {
                    if (cell.Accessory == UITableViewCellAccessory.Checkmark)
                    {
                        cell.Accessory = UITableViewCellAccessory.None;

                        var index = CreateSurveyController.SurveyModel.targetAudienceUsers.ids.IndexOf(tableItems[indexPath.Row].Id);

                        if (index >= 0)
                        {
                            CreateSurveyController.SurveyModel.targetAudienceUsers.ids.RemoveAt(index);
                            CreateSurveyController.SurveyModel.targetAudienceUsers.names.RemoveAt(index);
                        }
                    }
                    else
                    {
                        cell.Accessory = UITableViewCellAccessory.Checkmark;

                        if (CreateSurveyController.SurveyModel.targetAudienceUsers == null)
                        {
                            CreateSurveyController.SurveyModel.targetAudienceUsers = new AudienceUsers();
                            CreateSurveyController.SurveyModel.targetAudienceUsers.ids = new List<string>();
                            CreateSurveyController.SurveyModel.targetAudienceUsers.names = new List<string>();
                        }

                        CreateSurveyController.SurveyModel.targetAudienceUsers.ids.Add(tableItems[indexPath.Row].Id);
                        CreateSurveyController.SurveyModel.targetAudienceUsers.names.Add(tableItems[indexPath.Row].Name);
                    }

                    if (CreateSurveyController.SurveyModel != null && CreateSurveyController.SurveyModel.targetAudienceUsers != null &&
                        CreateSurveyController.SurveyModel.targetAudienceUsers.ids != null && CreateSurveyController.SurveyModel.targetAudienceUsers.ids.Count > 0)
                    {
                        CreateSurveyController._askButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                        CreateSurveyController._askButton.BackgroundColor = UIColor.FromRGB(70, 230, 130);
                    }
                    else
                    {
                        CreateSurveyController._askButton.SetTitleColor(UIColor.FromRGB(220, 220, 220), UIControlState.Normal);
                        CreateSurveyController._askButton.BackgroundColor = UIColor.White;
                    }
                }
                else
                {
                    cell.Accessory = UITableViewCellAccessory.None;
                }
            } else if (CreateSurveyController.SurveyModel.targetAudience == TargetAudience.Groups.ToString())
            {
                if (indexPath.Row >= 0)
                {
                    if (cell.Accessory == UITableViewCellAccessory.Checkmark)
                    {
                        cell.Accessory = UITableViewCellAccessory.None;

                        var index = CreateSurveyController.SurveyModel.targetAudienceGroups.ids.IndexOf(tableItems[indexPath.Row].Id);

                        if (index >= 0)
                        {
                            CreateSurveyController.SurveyModel.targetAudienceGroups.ids.RemoveAt(index);
                            CreateSurveyController.SurveyModel.targetAudienceGroups.names.RemoveAt(index);
                        }
                    }
                    else
                    {
                        cell.Accessory = UITableViewCellAccessory.Checkmark;

                        if (CreateSurveyController.SurveyModel.targetAudienceGroups == null)
                        {
                            CreateSurveyController.SurveyModel.targetAudienceGroups = new AudienceGroups();
                            CreateSurveyController.SurveyModel.targetAudienceGroups.ids = new List<string>();
                            CreateSurveyController.SurveyModel.targetAudienceGroups.names = new List<string>();
                        }

                        CreateSurveyController.SurveyModel.targetAudienceGroups.ids.Add(tableItems[indexPath.Row].Id);
                        CreateSurveyController.SurveyModel.targetAudienceGroups.names.Add(tableItems[indexPath.Row].Name);
                    }

                    if (CreateSurveyController.SurveyModel != null && CreateSurveyController.SurveyModel.targetAudienceGroups != null &&
                        CreateSurveyController.SurveyModel.targetAudienceGroups.ids != null && CreateSurveyController.SurveyModel.targetAudienceGroups.ids.Count > 0)
                    {
                        CreateSurveyController._askButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                        CreateSurveyController._askButton.BackgroundColor = UIColor.FromRGB(70, 230, 130);
                    }
                    else
                    {
                        CreateSurveyController._askButton.SetTitleColor(UIColor.FromRGB(220, 220, 220), UIControlState.Normal);
                        CreateSurveyController._askButton.BackgroundColor = UIColor.White;
                    }
                }
                else
                {
                    cell.Accessory = UITableViewCellAccessory.None;
                }
            }

            tableView.DeselectRow(indexPath, true);

            cell.LayoutSubviews();
        }

        public void DeselectAll(UITableView tableView) {
            for (int i = 0; i < tableItems.Count; i++)
            {
                var cell = tableView.CellAt(NSIndexPath.FromRowSection((nint)i, (nint)0));

                if (cell != null)
                {
                    if (cell.Accessory == UITableViewCellAccessory.Checkmark)
                    {
                        cell.Accessory = UITableViewCellAccessory.None;
                    }
                    cell.LayoutSubviews();
                }
            }
        }        
    }
}

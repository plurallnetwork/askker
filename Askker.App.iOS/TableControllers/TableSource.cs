using AssetsLibrary;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Askker.App.iOS.TableControllers
{
    public class TableSource : UITableViewSource
    {
        List<TableItem> tableItems;
        UIViewController viewController;
        string cellIdentifier = "TableCell";
        UIImagePickerController imagePicker;

        public TableSource(List<TableItem> items, UIViewController viewController)
        {
            tableItems = items;
            this.viewController = viewController;
        }

        public List<TableItem> GetTableItems()
        {
            return tableItems;
        }

        /// <summary>
        /// Called by the TableView to determine how many cells to create for that particular section.
        /// </summary>
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return tableItems.Count;
        }

        /// <summary>
        /// Called when a row is touched
        /// </summary>
        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            new UIAlertView("Row Selected"
                , tableItems[indexPath.Row].Heading, null, "OK", null).Show();
            tableView.DeselectRow(indexPath, true);
        }

        /// <summary>
        /// Called by the TableView to get the actual UITableViewCell to render for the particular row
        /// </summary>
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // request a recycled cell to save memory
            UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);

            // UNCOMMENT one of these to use that style
            //			var cellStyle = UITableViewCellStyle.Default;
            var cellStyle = UITableViewCellStyle.Subtitle;
            //			var cellStyle = UITableViewCellStyle.Value1;
            //			var cellStyle = UITableViewCellStyle.Value2;

            // if there are no cells to reuse, create a new one
            if (cell == null)
            {
                cell = new UITableViewCell(cellStyle, cellIdentifier);
            }

            cell.TextLabel.Text = tableItems[indexPath.Row].Heading;

            // Default style doesn't support Subtitle
            if (cellStyle == UITableViewCellStyle.Subtitle
               || cellStyle == UITableViewCellStyle.Value1
               || cellStyle == UITableViewCellStyle.Value2)
            {
                cell.DetailTextLabel.Text = tableItems[indexPath.Row].SubHeading;
            }

            // Value2 style doesn't support an image
            if (cellStyle != UITableViewCellStyle.Value2)
            {
                cell.ImageView.Image = tableItems[indexPath.Row].Image;
            }
                
            //cell.ImageView.image = UIImage.FromFile("Images/" + tableItems[indexPath.Row].ImageName);

            return cell;
        }

        #region -= editing methods =-

        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            switch (editingStyle)
            {
                case UITableViewCellEditingStyle.Delete:
                    // remove the item from the underlying data source
                    tableItems.RemoveAt(indexPath.Row);
                    // delete the row from the table
                    tableView.DeleteRows(new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
                    break;

                case UITableViewCellEditingStyle.Insert:

                    if ("text".Equals(CreateSurveyController.SurveyModel.type))
                    {

                        UIAlertView alert = new UIAlertView();
                        alert.Title = "text Option";
                        alert.AddButton("Done");
                        alert.Message = "Please enter an option description:";
                        alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
                        alert.Clicked += (object s, UIButtonEventArgs ev) =>
                        {
                            // user input will be in alert.GetTextField(0).text;
                            //---- create a new item and add it to our underlying data
                            tableItems.Insert(indexPath.Row, new TableItem(alert.GetTextField(0).Text));
                            //---- insert a new row in the table
                            tableView.InsertRows(new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
                        };

                        alert.Show();
                    }
                    else if ("image".Equals(CreateSurveyController.SurveyModel.type))
                    {
                        imagePicker = new UIImagePickerController();
                        imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;

                        imagePicker.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);

                        //imagePicker.FinishedPickingMedia += Handle_FinishedPickingMedia;

                        imagePicker.FinishedPickingMedia += delegate (object sender, UIImagePickerMediaPickedEventArgs e) { Handle_FinishedPickingMedia(sender, e, tableView, indexPath); };

                        imagePicker.Canceled += Handle_Canceled;

                        var mainWindow = UIApplication.SharedApplication.KeyWindow;
                        var viewController = mainWindow?.RootViewController;
                        while (viewController?.PresentedViewController != null)
                        {
                            viewController = viewController.PresentedViewController;
                        }
                        if (viewController == null)
                            viewController = this.viewController;
                        imagePicker.View.Frame = viewController.View.Frame;
                        viewController.PresentModalViewController(imagePicker, true);
                    }

                    break;

                case UITableViewCellEditingStyle.None:
                    Console.WriteLine("CommitEditingStyle:None called");
                    break;
            }
        }

        private void Handle_Canceled(object sender, EventArgs e)
        {
            imagePicker.DismissModalViewController(true);
        }

        protected void Handle_FinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs e, UITableView tableView, NSIndexPath indexPath)
        {
            // determine what was selected, video or image
            bool isImage = false;
            switch (e.Info[UIImagePickerController.MediaType].ToString())
            {
                case "public.image":
                    isImage = true;
                    break;

                case "public.video":
                    break;
            }

            // get common info (shared between images and video)
            NSUrl referenceURL = e.Info[new NSString("UIImagePickerControllerReferenceURL")] as NSUrl;

            Console.WriteLine("image path :" + referenceURL.Path.ToString());
            Console.WriteLine("image relative path :" + referenceURL.RelativePath.ToString());

            ALAssetsLibrary assetsLibrary = new ALAssetsLibrary();
            assetsLibrary.AssetForUrl(referenceURL, delegate (ALAsset asset) 
            {
                ALAssetRepresentation representation = asset.DefaultRepresentation;
                if (representation == null)
                {
                    return;
                }
                else
                {
                    string fileName = representation.Filename;
                    Console.WriteLine("image Filename :" + fileName);
                }
            }, delegate (NSError error) {
                Console.WriteLine("User denied access to photo Library... {0}", error);
            });


            // if it was an image, get the other image info
            if (isImage)
            {

                // get the original image
                UIImage originalImage = e.Info[UIImagePickerController.OriginalImage] as UIImage;
                if (originalImage != null)
                {
                    // do something with the image
                    Console.WriteLine("got the original image");
                    //imageView.image = originalImage;

                    UIAlertView alert = new UIAlertView();
                    alert.Title = "image Option";
                    alert.AddButton("Done");
                    alert.Message = "Please enter an option description:";
                    alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
                    alert.Clicked += (object s, UIButtonEventArgs ev) =>
                    {
                        // user input will be in alert.GetTextField(0).text;
                        //---- create a new item and add it to our underlying data
                        tableItems.Insert(indexPath.Row, new TableItem(alert.GetTextField(0).Text, originalImage));
                        //---- insert a new row in the table
                        tableView.InsertRows(new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
                    };

                    alert.Show();
                }

                // get the edited image
                UIImage editedImage = e.Info[UIImagePickerController.EditedImage] as UIImage;
                if (editedImage != null)
                {
                    // do something with the image
                    Console.WriteLine("got the edited image");
                    //imageView.image = editedImage;
                }

                //- get the image metadata
                NSDictionary imageMetadata = e.Info[UIImagePickerController.MediaMetadata] as NSDictionary;
                if (imageMetadata != null)
                {
                    // do something with the metadata
                    Console.WriteLine("got image metadata");
                }

            }
            // if it's a video
            else
            {
                // get video url
                NSUrl mediaURL = e.Info[UIImagePickerController.MediaURL] as NSUrl;
                if (mediaURL != null)
                {
                    //
                    Console.WriteLine(mediaURL.ToString());
                }
            }

            // dismiss the picker
            imagePicker.DismissModalViewController(true);
        }

        /// <summary>
        /// Called by the table view to determine whether or not the row is editable
        /// </summary>
        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true; // return false if you wish to disable editing for a specific indexPath or for all rows
        }

        /// <summary>
        /// Called by the table view to determine whether or not the row is moveable
        /// </summary>
        public override bool CanMoveRow(UITableView tableView, NSIndexPath indexPath)
        {
            return indexPath.Row < tableView.NumberOfRowsInSection(0) - 1;
        }

        /// <summary>
        /// Custom text for delete button
        /// </summary>
        public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
        {
            return "Trash option";
        }

        /// <summary>
        /// Called by the table view to determine whether the editing control should be an insert
        /// or a delete.
        /// </summary>
        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
        {
            if (tableView.Editing)
            {
                if (indexPath.Row == tableView.NumberOfRowsInSection(0) - 1)
                    return UITableViewCellEditingStyle.Insert;
                else
                    return UITableViewCellEditingStyle.Delete;
            }
            else  // not in editing mode, enable swipe-to-delete for all rows
                return UITableViewCellEditingStyle.Delete;
        }
        public override NSIndexPath CustomizeMoveTarget(UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath proposedIndexPath)
        {
            var numRows = tableView.NumberOfRowsInSection(0) - 1; // less the (add new) one
            Console.WriteLine(proposedIndexPath.Row + " " + numRows);
            if (proposedIndexPath.Row >= numRows)
                return NSIndexPath.FromRowSection(numRows - 1, 0);
            else
                return proposedIndexPath;
        }
        /// <summary>
        /// called by the table view when a row is moved.
        /// </summary>
        public override void MoveRow(UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
        {
            //---- get a reference to the item
            var item = tableItems[sourceIndexPath.Row];
            var deleteAt = sourceIndexPath.Row;
            var insertAt = destinationIndexPath.Row;

            //---- if we're moving within the same section, and we're inserting it before
            if ((sourceIndexPath.Section == destinationIndexPath.Section) && (destinationIndexPath.Row < sourceIndexPath.Row))
            {
                //---- add one to where we delete, because we're increasing the index by inserting
                deleteAt += 1;
            }
            else
            {
                insertAt += 1;
            }

            //---- copy the item to the new location
            tableItems.Insert(destinationIndexPath.Row, item);

            //---- remove from the old
            tableItems.RemoveAt(deleteAt);
        }
        /// <summary>
        /// Called manually when the table goes into edit mode
        /// </summary>
        public void WillBeginTableEditing(UITableView tableView)
        {
            //---- start animations
            tableView.BeginUpdates();

            //---- insert a new row in the table
            tableView.InsertRows(new NSIndexPath[] {
                    NSIndexPath.FromRowSection (tableView.NumberOfRowsInSection (0), 0)
                }, UITableViewRowAnimation.Fade);
            //---- create a new item and add it to our underlying data
            tableItems.Add(new TableItem("<- Add new option"));

            //---- end animations
            tableView.EndUpdates();
        }

        /// <summary>
        /// Called manually when the table leaves edit mode
        /// </summary>
        public void DidFinishTableEditing(UITableView tableView)
        {
            //---- start animations
            tableView.BeginUpdates();
            //---- remove our row from the underlying data
            tableItems.RemoveAt((int)tableView.NumberOfRowsInSection(0) - 1); // zero based :)
                                                                              //---- remove the row from the table
            tableView.DeleteRows(new NSIndexPath[] { NSIndexPath.FromRowSection(tableView.NumberOfRowsInSection(0) - 1, 0) }, UITableViewRowAnimation.Fade);
            //---- finish animations
            tableView.EndUpdates();
        }
        #endregion
    }
}

using Askker.App.iOS.HorizontalSwipe;
using Askker.App.iOS.TableControllers;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
using BigTed;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace Askker.App.iOS
{
    public partial class CreateSurveyShareStep : CustomUIViewController, IMultiStepProcessStep
    {
        public ShareStepView _shareStepView;
        public SurveyShareTableSource tableSource;

        public List<SurveyShareTableItem> tableItems;
        public List<SurveyShareTableItem> tableItemsGroups;

        public CreateSurveyShareStep (IntPtr handle) : base (handle)
        {
        }

        public CreateSurveyShareStep()
        {
        }

        public override void LoadView()
        {
            View = new UIView();
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);

            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            _shareStepView = ShareStepView.Create();
            _shareStepView.QuestionText.TextColor = UIColor.FromRGB(90, 89, 89);
            View.AddSubview(_shareStepView);

            try
            {
                var targetAudienceItems = new SurveyTargetAudiencesModel().TargetAudienceItems;
                new TargetAudienceTableViewController(_shareStepView.ShareOptions, targetAudienceItems, this);

                tableItems = new List<SurveyShareTableItem>();
                tableItemsGroups = new List<SurveyShareTableItem>();

                
                List<UserFriendModel> friends = await new FriendManager().GetFriends(LoginController.userModel.id, LoginController.tokenModel.access_token);

                foreach (var friend in friends)
                {
                    tableItems.Add(new SurveyShareTableItem(friend.name, friend.profilePicture, friend.id));
                }

                List<UserGroupModel> groups = await new UserGroupManager().GetGroups(LoginController.userModel.id, LoginController.tokenModel.access_token);

                foreach (var group in groups)
                {
                    tableItemsGroups.Add(new SurveyShareTableItem(group.name, group.profilePicture, group.userId+group.creationDate));
                }
                
                if (CreateSurveyController.ScreenState == ScreenState.Edit.ToString())
                {
                    if (CreateSurveyController.SurveyModel.targetAudience == TargetAudience.Private.ToString())
                    {
                        tableSource = new SurveyShareTableSource(tableItems);
                        _shareStepView.ShareTable.Hidden = false;
                    }
                    else if (CreateSurveyController.SurveyModel.targetAudience == TargetAudience.Groups.ToString())
                    {
                        tableSource = new SurveyShareTableSource(tableItemsGroups);
                        _shareStepView.ShareTable.Hidden = false;
                    }
                    else
                    {
                        _shareStepView.ShareTable.Hidden = true;
                    }

                    CreateSurveyController._askButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                    CreateSurveyController._askButton.BackgroundColor = UIColor.FromRGB(70, 230, 130);
                }

                _shareStepView.ShareTable.BackgroundColor = UIColor.Clear;
                _shareStepView.ShareTable.SeparatorInset = new UIEdgeInsets(0, 10, 0, 10);
                _shareStepView.ShareTable.Source = tableSource;
                _shareStepView.ShareTable.ReloadData();
            }
            catch (Exception ex)
            {
                BTProgressHUD.Dismiss();
                Utils.HandleException(ex);
            }

            BTProgressHUD.Dismiss();
        }

        public override void ViewDidLayoutSubviews()
        {
            _shareStepView.Frame = View.Bounds;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            _shareStepView.QuestionText.Text = CreateSurveyController.SurveyModel.question.text;
            StepActivated?.Invoke(this, new MultiStepProcessStepEventArgs { Index = StepIndex });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);            
            StepDeactivated?.Invoke(this, new MultiStepProcessStepEventArgs { Index = StepIndex });
        }

        public int StepIndex { get; set; }

        public event EventHandler<MultiStepProcessStepEventArgs> StepActivated;
        public event EventHandler<MultiStepProcessStepEventArgs> StepDeactivated;
    }

    public class TargetAudienceTableViewController : UITableViewController
    {
        static NSString CellId = new NSString("TargetAudienceCell");

        public TargetAudienceTableViewController(UITableView targetAudienceTableView, List<TargetAudienceModel> targetAudienceItems, CreateSurveyShareStep createSurveyShareStep)
        {
            targetAudienceTableView.RegisterClassForCellReuse(UITableViewCellStyle.Subtitle.GetType(), CellId);
            targetAudienceTableView.Source = new TargetAudienceTableViewDataSource(targetAudienceItems, createSurveyShareStep);
        }

        class TargetAudienceTableViewDataSource : UITableViewSource
        {
            List<TargetAudienceModel> targetAudienceItems;
            CreateSurveyShareStep createSurveyShareStep;

            public TargetAudienceTableViewDataSource(List<TargetAudienceModel> targetAudienceItems, CreateSurveyShareStep createSurveyShareStep)
            {
                this.targetAudienceItems = targetAudienceItems;
                this.createSurveyShareStep = createSurveyShareStep;
            }

            public override nint RowsInSection(UITableView tableView, nint section)
            {
                return targetAudienceItems.Count;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(CellId);
                
                if (cell == null)
                {
                    cell = new UITableViewCell(UITableViewCellStyle.Subtitle, CellId);
                }
                
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                cell.BackgroundColor = UIColor.Clear;
                cell.ContentView.BackgroundColor = UIColor.Clear;
                cell.SeparatorInset = new UIEdgeInsets(0, 10, 0, 10);

                cell.ImageView.Image = UIImage.FromBundle(targetAudienceItems[indexPath.Row].ImageName);
                
                cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(14);
                cell.TextLabel.Text = targetAudienceItems[indexPath.Row].Title;
                cell.TextLabel.TextColor = UIColor.FromRGB(90, 89, 89);

                cell.DetailTextLabel.Text = targetAudienceItems[indexPath.Row].Text;
                cell.DetailTextLabel.TextColor = UIColor.FromRGB(90, 89, 89);

                if (CreateSurveyController.SurveyModel.targetAudience == targetAudienceItems[indexPath.Row].TargetAudience.ToString())
                {
                    cell.AccessoryView = new UIImageView(new CGRect(0, 0, 38, 38)) { Image = UIImage.FromBundle("OptionCheck") };
                    tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);
                }
                else
                {
                    cell.AccessoryView = new UIImageView(new CGRect(0, 0, 38, 38)) { Image = UIImage.FromBundle("EmptyCircleText") };
                }

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);

                CreateSurveyController.SurveyModel.targetAudience = targetAudienceItems[indexPath.Row].TargetAudience.ToString();

                if(createSurveyShareStep.tableSource != null)
                {
                    createSurveyShareStep.tableSource.DeselectAll(createSurveyShareStep._shareStepView.ShareTable);
                }

                if (targetAudienceItems[indexPath.Row].TargetAudience == TargetAudience.Private)
                {
                    createSurveyShareStep.tableSource = new SurveyShareTableSource(createSurveyShareStep.tableItems);
                    createSurveyShareStep._shareStepView.ShareTable.BackgroundColor = UIColor.Clear;
                    createSurveyShareStep._shareStepView.ShareTable.SeparatorInset = new UIEdgeInsets(0, 10, 0, 10);
                    createSurveyShareStep._shareStepView.ShareTable.Source = createSurveyShareStep.tableSource;
                    createSurveyShareStep._shareStepView.ShareTable.ReloadData();

                    this.createSurveyShareStep._shareStepView.ShareTable.Hidden = false;

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
                } else if (targetAudienceItems[indexPath.Row].TargetAudience == TargetAudience.Groups)
                {
                    createSurveyShareStep.tableSource = new SurveyShareTableSource(createSurveyShareStep.tableItemsGroups);
                    createSurveyShareStep._shareStepView.ShareTable.BackgroundColor = UIColor.Clear;
                    createSurveyShareStep._shareStepView.ShareTable.SeparatorInset = new UIEdgeInsets(0, 10, 0, 10);
                    createSurveyShareStep._shareStepView.ShareTable.Source = createSurveyShareStep.tableSource;
                    createSurveyShareStep._shareStepView.ShareTable.ReloadData();

                    this.createSurveyShareStep._shareStepView.ShareTable.Hidden = false;

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
                    this.createSurveyShareStep._shareStepView.ShareTable.Hidden = true;

                    CreateSurveyController._askButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                    CreateSurveyController._askButton.BackgroundColor = UIColor.FromRGB(70, 230, 130);
                }

                cell.AccessoryView = new UIImageView(new CGRect(0, 0, 38, 38)) { Image = UIImage.FromBundle("OptionCheck") };
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);

                if (cell == null)
                {
                    cell = this.GetCell(tableView, indexPath);
                }

                cell.AccessoryView = new UIImageView(new CGRect(0, 0, 38, 38)) { Image = UIImage.FromBundle("EmptyCircleText") };
            }
        }
    }
}
using Askker.App.iOS.HorizontalSwipe;
using Askker.App.iOS.TableControllers;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
using BigTed;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using UIKit;

namespace Askker.App.iOS
{
    public partial class CreateSurveyShareStep : CustomUIViewController, IMultiStepProcessStep
    {
        public ShareStepView _shareStepView;
        SurveyShareTableSource tableSource;

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
            View.AddSubview(_shareStepView);

            try
            {
                var targetAudienceItems = new SurveyTargetAudiencesModel().TargetAudienceItems;
                new TargetAudienceTableViewController(_shareStepView.ShareOptions, targetAudienceItems, this);

                List<SurveyShareTableItem> tableItems = new List<SurveyShareTableItem>();

                List<UserFriendModel> friends = await new FriendManager().GetFriends(LoginController.userModel.id, LoginController.tokenModel.access_token);

                foreach (var friend in friends)
                {
                    tableItems.Add(new SurveyShareTableItem(friend.name, friend.profilePicture, friend.id));
                }

                tableSource = new SurveyShareTableSource(tableItems);

                if (CreateSurveyController.ScreenState == ScreenState.Edit.ToString())
                {
                    if (CreateSurveyController.SurveyModel.targetAudience == TargetAudience.Public.ToString())
                    {
                        publicButtonLogic();
                    }
                    else if (CreateSurveyController.SurveyModel.targetAudience == TargetAudience.Friends.ToString())
                    {
                        friendsButtonLogic();
                    }
                    else if (CreateSurveyController.SurveyModel.targetAudience == TargetAudience.Private.ToString())
                    {
                        privateButtonLogic();
                    }
                }
                else
                {
                    //_shareStepView.PublicButton.Enabled = false;
                    //_shareStepView.FriendsButton.Enabled = true;
                    //_shareStepView.PrivateButton.Enabled = true;

                    //_shareStepView.ShareMessageLabel.Text = "This question will be visible to everybody!";
                    _shareStepView.ShareTable.Hidden = true;

                    CreateSurveyController.SurveyModel.targetAudience = TargetAudience.Public.ToString();
                }


                _shareStepView.ShareTable.Source = tableSource;
                _shareStepView.ShareTable.ReloadData();
            }
            catch (Exception ex)
            {
                BTProgressHUD.Dismiss();
                Utils.HandleException(ex);
            }

            //_shareStepView.PublicButton.TouchUpInside += (sender, e) =>
            //{
            //    publicButtonLogic();

            //    CreateSurveyController.SurveyModel.targetAudience = TargetAudience.Public.ToString();
            //};

            //_shareStepView.FriendsButton.TouchUpInside += (sender, e) =>
            //{
            //    friendsButtonLogic();

            //    CreateSurveyController.SurveyModel.targetAudience = TargetAudience.Friends.ToString();

            //};

            //_shareStepView.PrivateButton.TouchUpInside += (sender, e) =>
            //{
            //    privateButtonLogic();

            //    CreateSurveyController.SurveyModel.targetAudience = TargetAudience.Private.ToString();

            //};
            BTProgressHUD.Dismiss();
        }

        public void publicButtonLogic()
        {
            //_shareStepView.PublicButton.Enabled = false;
            //_shareStepView.FriendsButton.Enabled = true;
            //_shareStepView.PrivateButton.Enabled = true;

            //_shareStepView.ShareMessageLabel.Text = "This question will be visible to everybody!";
            _shareStepView.ShareTable.Hidden = true;
        }

        public void friendsButtonLogic()
        {
            //_shareStepView.PublicButton.Enabled = true;
            //_shareStepView.FriendsButton.Enabled = false;
            //_shareStepView.PrivateButton.Enabled = true;

            //_shareStepView.ShareMessageLabel.Text = "This question will be visible to all your friends!";
            _shareStepView.ShareTable.Hidden = true;
        }

        public void privateButtonLogic()
        {
            //_shareStepView.PublicButton.Enabled = true;
            //_shareStepView.FriendsButton.Enabled = true;
            //_shareStepView.PrivateButton.Enabled = false;

            //_shareStepView.ShareMessageLabel.Text = "This question will be visible to the selected friends below:";
            _shareStepView.ShareTable.Hidden = false;
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
                cell.SeparatorInset = new UIEdgeInsets(0.0f, 0.0f, 0.0f, cell.Bounds.Size.Width);

                cell.ImageView.Image = UIImage.FromBundle(targetAudienceItems[indexPath.Row].ImageName);

                cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(12);
                cell.TextLabel.Text = targetAudienceItems[indexPath.Row].Title;

                cell.DetailTextLabel.Font = UIFont.SystemFontOfSize(10);
                cell.DetailTextLabel.Text = targetAudienceItems[indexPath.Row].Text;

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath) as MenuTableViewCell;

                CreateSurveyController.SurveyModel.targetAudience = targetAudienceItems[indexPath.Row].TargetAudience.ToString();

                if (targetAudienceItems[indexPath.Row].TargetAudience == TargetAudience.Private)
                {
                    this.createSurveyShareStep._shareStepView.ShareTable.Hidden = false;
                }
                else
                {
                    this.createSurveyShareStep._shareStepView.ShareTable.Hidden = true;
                }
            }

            //public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            //{
            //    var menuItem = menuItems[indexPath.Row].MenuItem;
            //    if (menuItem == MenuItem.Mine || menuItem == MenuItem.ToYou || menuItem == MenuItem.Public)
            //    {
            //        var cell = tableView.CellAt(indexPath) as MenuTableViewCell;

            //        if (cell == null)
            //        {
            //            cell = this.GetCell(tableView, indexPath) as MenuTableViewCell;
            //        }

            //        cell.menuTitleLabel.TextColor = UIColor.Black;
            //    }
            //}
        }
    }
}
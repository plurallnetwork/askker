using Askker.App.iOS.HorizontalSwipe;
using Askker.App.iOS.TableControllers;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using UIKit;

namespace Askker.App.iOS
{
    public partial class CreateSurveyShareStep : UIViewController, IMultiStepProcessStep
    {
        private ShareStepView _shareStepView;
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
            _shareStepView = ShareStepView.Create();
            View.AddSubview(_shareStepView);
            List<SurveyShareTableItem> tableItems = new List<SurveyShareTableItem>();

            List<UserFriendModel> friends = await new FriendManager().GetFriends(LoginController.userModel.id, LoginController.tokenModel.access_token);

            foreach (var friend in friends)
            {
                tableItems.Add(new SurveyShareTableItem(friend.name, friend.profilePicture, friend.id));
            }

            tableSource = new SurveyShareTableSource(tableItems);
            _shareStepView.PublicView.Frame = new RectangleF(0, 295, (float)View.Frame.Width, 370);
            _shareStepView.ShareTable.Frame = new RectangleF(0, 0, 0, 0);
            _shareStepView.FriendsView.Frame = new RectangleF(0, 0, 0, 0);
            _shareStepView.ShareView.Frame = new RectangleF(0, 0, 0, 0);

            _shareStepView.PublicButton.Enabled = false;
            _shareStepView.FriendsButton.Enabled = true;
            _shareStepView.PrivateButton.Enabled = true;

            _shareStepView.PublicView.Hidden = false;
            _shareStepView.ShareTable.Hidden = true;
            _shareStepView.FriendsView.Hidden = true;
            _shareStepView.ShareView.Hidden = true;

            CreateSurveyController.SurveyModel.targetAudience = TargetAudience.Public.ToString();

            _shareStepView.ShareTable.Source = tableSource;
            _shareStepView.ShareTable.ReloadData();

            _shareStepView.PublicButton.TouchUpInside += (sender, e) =>
            {
                _shareStepView.PublicView.Frame = new RectangleF(0, 295, (float)View.Frame.Width, 370);
                _shareStepView.ShareTable.Frame = new RectangleF(0, 0, 0, 0);
                _shareStepView.FriendsView.Frame = new RectangleF(0, 0, 0, 0);
                _shareStepView.ShareView.Frame = new RectangleF(0, 0, 0, 0);

                _shareStepView.PublicButton.Enabled = false;
                _shareStepView.FriendsButton.Enabled = true;
                _shareStepView.PrivateButton.Enabled = true;

                _shareStepView.PublicView.Hidden = false;
                _shareStepView.ShareTable.Hidden = true;
                _shareStepView.FriendsView.Hidden = true;
                _shareStepView.ShareView.Hidden = true;

                CreateSurveyController.SurveyModel.targetAudience = TargetAudience.Public.ToString();

            };

            _shareStepView.PrivateButton.TouchUpInside += (sender, e) =>
            {
                _shareStepView.PublicView.Frame = new RectangleF(0, 0, 0, 0);
                _shareStepView.ShareTable.Frame = new RectangleF(0, 355, (float)View.Frame.Width, 310);
                _shareStepView.FriendsView.Frame = new RectangleF(0, 0, 0, 0);
                _shareStepView.ShareView.Frame = new RectangleF(0, 295, (float)View.Frame.Width, 60);

                _shareStepView.PublicButton.Enabled = true;
                _shareStepView.FriendsButton.Enabled = true;
                _shareStepView.PrivateButton.Enabled = false;

                _shareStepView.PublicView.Hidden = true;
                _shareStepView.ShareTable.Hidden = false;
                _shareStepView.FriendsView.Hidden = true;
                _shareStepView.ShareView.Hidden = false;

                CreateSurveyController.SurveyModel.targetAudience = TargetAudience.Private.ToString();

            };

            _shareStepView.FriendsButton.TouchUpInside += (sender, e) =>
            {
                _shareStepView.PublicView.Frame = new RectangleF(0, 0, 0, 0);
                _shareStepView.ShareTable.Frame = new RectangleF(0, 0, 0, 0);
                _shareStepView.FriendsView.Frame = new RectangleF(0, 295, (float)View.Frame.Width, 370);
                _shareStepView.ShareView.Frame = new RectangleF(0, 0, 0, 0);

                _shareStepView.PublicButton.Enabled = true;
                _shareStepView.FriendsButton.Enabled = false;
                _shareStepView.PrivateButton.Enabled = true;

                _shareStepView.PublicView.Hidden = true;
                _shareStepView.ShareTable.Hidden = true;
                _shareStepView.FriendsView.Hidden = false
                ;
                _shareStepView.ShareView.Hidden = true;

                CreateSurveyController.SurveyModel.targetAudience = TargetAudience.Friends.ToString();

            };
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
}
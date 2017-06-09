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
            base.ViewDidLoad();

            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);

            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            _shareStepView = ShareStepView.Create();
            View.AddSubview(_shareStepView);

            try
            {
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
                    _shareStepView.PublicButton.Enabled = false;
                    _shareStepView.FriendsButton.Enabled = true;
                    _shareStepView.PrivateButton.Enabled = true;

                    _shareStepView.ShareMessageLabel.Text = "This question will be visible to everybody!";
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

            _shareStepView.PublicButton.TouchUpInside += (sender, e) =>
            {
                publicButtonLogic();

                CreateSurveyController.SurveyModel.targetAudience = TargetAudience.Public.ToString();
            };

            _shareStepView.FriendsButton.TouchUpInside += (sender, e) =>
            {
                friendsButtonLogic();

                CreateSurveyController.SurveyModel.targetAudience = TargetAudience.Friends.ToString();

            };

            _shareStepView.PrivateButton.TouchUpInside += (sender, e) =>
            {
                privateButtonLogic();

                CreateSurveyController.SurveyModel.targetAudience = TargetAudience.Private.ToString();

            };
            BTProgressHUD.Dismiss();
        }

        public void publicButtonLogic()
        {
            _shareStepView.PublicButton.Enabled = false;
            _shareStepView.FriendsButton.Enabled = true;
            _shareStepView.PrivateButton.Enabled = true;

            _shareStepView.ShareMessageLabel.Text = "This question will be visible to everybody!";
            _shareStepView.ShareTable.Hidden = true;
        }

        public void friendsButtonLogic()
        {
            _shareStepView.PublicButton.Enabled = true;
            _shareStepView.FriendsButton.Enabled = false;
            _shareStepView.PrivateButton.Enabled = true;

            _shareStepView.ShareMessageLabel.Text = "This question will be visible to all your friends!";
            _shareStepView.ShareTable.Hidden = true;
        }

        public void privateButtonLogic()
        {
            _shareStepView.PublicButton.Enabled = true;
            _shareStepView.FriendsButton.Enabled = true;
            _shareStepView.PrivateButton.Enabled = false;

            _shareStepView.ShareMessageLabel.Text = "This question will be visible to the selected friends below:";
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
}
using Askker.App.iOS.HorizontalSwipe;
using Askker.App.iOS.TableControllers;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using Foundation;
using System;
using System.Collections.Generic;
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
                tableItems.Add(new SurveyShareTableItem(friend.name, friend.profilePicture));
            }

            tableSource = new SurveyShareTableSource(tableItems);
            _shareStepView.ShareTable.Source = tableSource;
            _shareStepView.ShareTable.ReloadData();

            //_shareStepView.GroupsButton.TouchUpInside += (sender, e) =>
            //{
            //    if (_shareStepView.ShareTable.Editing)
            //        _shareStepView.ShareTable.SetEditing(false, true);
            //    tableSource.WillBeginTableEditing(_shareStepView.ShareTable);
            //    _shareStepView.ShareTable.SetEditing(true, true);

            //    _shareStepView.GroupsButton.Hidden = true;
            //    _shareStepView.FriendsButton.Hidden = true;
            //    _shareStepView.DoneButton.Hidden = false;
            //};

            //_shareStepView.DoneButton.TouchUpInside += (sender, e) =>
            //{
            //    _shareStepView.ShareTable.SetEditing(false, true);
            //    tableSource.DidFinishTableEditing(_shareStepView.ShareTable);

            //    _shareStepView.GroupsButton.Hidden = false;
            //    _shareStepView.FriendsButton.Hidden = false;
            //    _shareStepView.DoneButton.Hidden = true;
            //};

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
            //List<TableItem> items = tableSource.GetTableItems();
            //if (items.Count > 0)
            //{
            //    if (CreateSurveyController.SurveyModel.options == null)
            //    {
            //        CreateSurveyController.SurveyModel.options = new List<Option>();
            //    }
            //    int optionId = 0;
            //    items.ForEach(i =>
            //    {
            //        Option o = new Option();
            //        o.id = optionId;
            //        o.text = i.Heading;
            //        CreateSurveyController.SurveyModel.options.Add(o);
            //        optionId++;
            //    });
            //}
            StepDeactivated?.Invoke(this, new MultiStepProcessStepEventArgs { Index = StepIndex });
        }

        public int StepIndex { get; set; }

        public event EventHandler<MultiStepProcessStepEventArgs> StepActivated;
        public event EventHandler<MultiStepProcessStepEventArgs> StepDeactivated;
    }
}
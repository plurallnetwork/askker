using Askker.App.iOS.HorizontalSwipe;
using Askker.App.iOS.TableControllers;
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
        TableSource tableSource;

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

        public override void ViewDidLoad()
        {
            _shareStepView = ShareStepView.Create();
            View.AddSubview(_shareStepView);
            List<TableItem> tableItems = new List<TableItem>();
            tableSource = new TableSource(tableItems);
            _shareStepView.ShareTable.Source = tableSource;

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
            _shareStepView.QuestionText.Text = CreateSurveyController.SurveyModel.Question.Text;
            StepActivated?.Invoke(this, new MultiStepProcessStepEventArgs { Index = StepIndex });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            //List<TableItem> items = tableSource.GetTableItems();
            //if (items.Count > 0)
            //{
            //    if (CreateSurveyController.SurveyModel.Options == null)
            //    {
            //        CreateSurveyController.SurveyModel.Options = new List<Option>();
            //    }
            //    int optionId = 0;
            //    items.ForEach(i =>
            //    {
            //        Option o = new Option();
            //        o.Id = optionId;
            //        o.Text = i.Heading;
            //        CreateSurveyController.SurveyModel.Options.Add(o);
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
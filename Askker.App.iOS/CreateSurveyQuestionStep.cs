using Askker.App.iOS.CustomViewComponents;
using Askker.App.iOS.HorizontalSwipe;
using Askker.App.PortableLibrary.Models;
using Cirrious.FluentLayouts.Touch;
using Foundation;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class CreateSurveyQuestionStep : UIViewController, IMultiStepProcessStep
    {
        private QuestionStepView _questionStepView;
        
        public override void LoadView()
        {
            View = new UIView();
        }

        public override void ViewDidLoad()
        {
            _questionStepView = QuestionStepView.Create();
            View.AddSubview(_questionStepView);

            NSNotificationCenter.DefaultCenter.AddObserver(PlaceholderEnabledUITextView.TextDidChangeNotification, TextChangedEvent);
        }

        private void TextChangedEvent(NSNotification notification)
        {
            PlaceholderEnabledUITextView field = (PlaceholderEnabledUITextView)notification.Object;

            if (field == _questionStepView.QuestionText)
            {
                field.placeholderLabel.Hidden = field.Text.Length > 0;
            }
        }

        public override void ViewDidLayoutSubviews()
        {
            _questionStepView.Frame = View.Bounds;
        }

        public CreateSurveyQuestionStep (IntPtr handle) : base (handle)
        {
        }

        public CreateSurveyQuestionStep()
        {            
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            StepActivated?.Invoke(this, new MultiStepProcessStepEventArgs { Index = StepIndex });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            Question q = new Question();
            q.Text = _questionStepView.QuestionText.Text;
            if(CreateSurveyController.SurveyModel == null)
            {
                CreateSurveyController.SurveyModel = new SurveyModel();
                CreateSurveyController.SurveyModel.UserId = LoginController.tokenModel.Id;
                CreateSurveyController.SurveyModel.UserName = LoginController.tokenModel.UserName;
                CreateSurveyController.SurveyModel.profilePicture = LoginController.tokenModel.ProfilePicturePath;
                CreateSurveyController.SurveyModel.IsArchived = 0;
                CreateSurveyController.SurveyModel.ChoiceType = "UniqueChoice";
                CreateSurveyController.SurveyModel.TotalVotes = 0;
            }
            CreateSurveyController.SurveyModel.Question = q;
            StepDeactivated?.Invoke(this, new MultiStepProcessStepEventArgs { Index = StepIndex });
        }

        public int StepIndex { get; set; }

        public event EventHandler<MultiStepProcessStepEventArgs> StepActivated;
        public event EventHandler<MultiStepProcessStepEventArgs> StepDeactivated;
    }
}
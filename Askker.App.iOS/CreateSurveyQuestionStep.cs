using Askker.App.iOS.CustomViewComponents;
using Askker.App.iOS.HorizontalSwipe;
using Askker.App.PortableLibrary.Models;
using Cirrious.FluentLayouts.Touch;
using Foundation;
using System;
using System.Collections.Generic;
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

        public override void ViewWillUnload()
        {
            if (_questionStepView.QuestionText.Text.Length <= 0)
            {
                new UIAlertView("question", "Please write a question", null, "OK", null).Show();

                return;
            }

        }

        public override void ViewWillDisappear(bool animated)
        {
            

            base.ViewWillDisappear(animated);
            Question q = new Question();
            q.text = _questionStepView.QuestionText.Text;
            q.image = "";
            if(CreateSurveyController.SurveyModel == null)
            {
                CreateSurveyController.SurveyModel = new SurveyModel();                
            }
            CreateSurveyController.SurveyModel.userId = LoginController.userModel.id;
            CreateSurveyController.SurveyModel.userName = LoginController.userModel.userName;
            CreateSurveyController.SurveyModel.profilePicture = LoginController.userModel.profilePicturePath;
            CreateSurveyController.SurveyModel.isArchived = 0;
            CreateSurveyController.SurveyModel.choiceType = "UniqueChoice";
            CreateSurveyController.SurveyModel.question = q;
            CreateSurveyController.SurveyModel.columnOptions = new List<ColumnOption>();
            CreateSurveyController.SurveyModel.finishDate = "";
            CreateSurveyController.SurveyModel.creationDate = "";
            StepDeactivated?.Invoke(this, new MultiStepProcessStepEventArgs { Index = StepIndex });
        }

        public int StepIndex { get; set; }

        public event EventHandler<MultiStepProcessStepEventArgs> StepActivated;
        public event EventHandler<MultiStepProcessStepEventArgs> StepDeactivated;
    }
}
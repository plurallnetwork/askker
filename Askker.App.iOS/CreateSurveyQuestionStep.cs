using Askker.App.iOS.CustomViewComponents;
using Askker.App.iOS.HorizontalSwipe;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
using BigTed;
using Cirrious.FluentLayouts.Touch;
using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace Askker.App.iOS
{
    public partial class CreateSurveyQuestionStep : CustomUIViewController, IMultiStepProcessStep
    {
        private QuestionStepView _questionStepView;
        
        public override void LoadView()
        {
            View = new UIView();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);

            _questionStepView = QuestionStepView.Create();
            View.AddSubview(_questionStepView);

            NSNotificationCenter.DefaultCenter.AddObserver(UITextView.TextDidChangeNotification, TextChangedEvent);
        }

        private void TextChangedEvent(NSNotification notification)
        {
            UITextView field = (UITextView)notification.Object;

            if (field == _questionStepView.QuestionText)
            {
                if (CreateSurveyController.SurveyModel == null)
                {
                    CreateSurveyController.SurveyModel = new SurveyModel();
                }

                if (CreateSurveyController.SurveyModel.question == null)
                {
                    CreateSurveyController.SurveyModel.question = new Question();
                }
                CreateSurveyController.SurveyModel.question.text = _questionStepView.QuestionText.Text;
                CreateSurveyController.SurveyModel.question.image = "";                
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

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);

            try
            {
                if (CreateSurveyController.ScreenState == ScreenState.Edit.ToString())
                {
                    CreateSurveyController.SurveyModel = await new FeedManager().GetSurvey(CreateSurveyController.UserId, CreateSurveyController.CreationDate, LoginController.tokenModel.access_token);

                    //pre-cache image options
                    if (CreateSurveyController.SurveyModel.type == SurveyType.Image.ToString())
                    {
                        foreach (var option in CreateSurveyController.SurveyModel.options)
                        {
                            UIImage image = Utils.GetImageFromNSUrl(option.image);
                            if(image != null)
                            {
                                image.Dispose();
                            }                            
                        }                        
                    }
                    _questionStepView.QuestionText.Text = CreateSurveyController.SurveyModel.question.text;
                }

                StepActivated?.Invoke(this, new MultiStepProcessStepEventArgs { Index = StepIndex });
                BTProgressHUD.Dismiss();

                _questionStepView.QuestionText.BecomeFirstResponder();
            }
            catch (Exception ex)
            {
                BTProgressHUD.Dismiss();
                Utils.HandleException(ex);
            }
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

            if (CreateSurveyController.ScreenState == ScreenState.Create.ToString())
            {
                Question q = new Question();
                q.text = _questionStepView.QuestionText.Text;
                q.image = "";
                if (CreateSurveyController.SurveyModel == null)
                {
                    CreateSurveyController.SurveyModel = new SurveyModel();
                }
                CreateSurveyController.SurveyModel.userId = LoginController.userModel.id;
                CreateSurveyController.SurveyModel.userName = LoginController.userModel.name;
                CreateSurveyController.SurveyModel.profilePicture = LoginController.userModel.profilePicturePath;
                CreateSurveyController.SurveyModel.isArchived = 0;
                CreateSurveyController.SurveyModel.choiceType = "UniqueChoice";
                CreateSurveyController.SurveyModel.question = q;
                CreateSurveyController.SurveyModel.columnOptions = new List<ColumnOption>();
                CreateSurveyController.SurveyModel.finishDate = "";
                CreateSurveyController.SurveyModel.creationDate = "";
            }
            if (CreateSurveyController.ScreenState == ScreenState.Edit.ToString())
            {
                if(CreateSurveyController.SurveyModel != null) //When Cancel button is clicked the SurveyModel become null
                {
                    CreateSurveyController.SurveyModel.question.text = _questionStepView.QuestionText.Text;
                }                
            }
            StepDeactivated?.Invoke(this, new MultiStepProcessStepEventArgs { Index = StepIndex });
        }

        public int StepIndex { get; set; }

        public event EventHandler<MultiStepProcessStepEventArgs> StepActivated;
        public event EventHandler<MultiStepProcessStepEventArgs> StepDeactivated;
    }
}
using Askker.App.iOS.HorizontalSwipe;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
using Cirrious.FluentLayouts.Touch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UIKit;

namespace Askker.App.iOS
{
    public partial class CreateSurveyController : UIViewController
    {
        enum SwipeDirection
        {
            Forward,
            Backward
        }

        private MultiStepProcessHorizontal _pageViewController;
        private HorizontalSwipePageControl _pageControl;
        private UILabel _pageTitle;
        private List<IMultiStepProcessStep> _steps;
        private SwipeDirection _swipeDirection;
        private List<string> _pageTitles;
        private int _currentStepIndex;

        public static UIButton _nextButton { get; set; }
        public static UIButton _backButton { get; set; }
        public static UIButton _askButton { get; set; }

        public List<IMultiStepProcessStep> Steps => _steps ?? (_steps = GetSteps());

        public static SurveyModel SurveyModel { get; set; }
        public static Stream QuestionImage { get; set; }
        public static List<KeyValuePair<string, byte[]>> OptionImages { get; set; }

        public CreateSurveyController (IntPtr handle) : base (handle)
        {
            _swipeDirection = SwipeDirection.Forward;
            
        }

        public override void LoadView()
        {
            View = new UIView();
            _pageTitles = new List<string> { "Write your question", "Choose your options", "Who do you want to ask to?"};
            _pageTitle = new UILabel();
            _pageTitle.TextColor = UIColor.DarkGray;
            _pageTitle.Font = UIFont.FromName("Arial", 12f);

            _pageViewController = new MultiStepProcessHorizontal(new MultiStepProcessDataSource(Steps));
            _pageViewController.WillTransition += _multiStepProcessHorizontal_WillTransition;
            
            _pageControl = new HorizontalSwipePageControl
            {
                CurrentPage = 0,
                Pages = Steps.Count,
                BackgroundColor = UIColor.LightGray
            };

            _pageControl.CurrentPage = 0;

            _nextButton = new UIButton();
            _nextButton.SetTitle("   Next   ", UIControlState.Normal);
            _nextButton.SetTitleColor(UIColor.Red, UIControlState.Normal);
            _nextButton.Font = UIFont.SystemFontOfSize(12);
            _nextButton.Layer.BorderColor = UIColor.Red.CGColor;
            _nextButton.Layer.BorderWidth = 1f;

            _nextButton.TouchUpInside += NextTapped;

            _backButton = new UIButton();
            _backButton.SetTitle("   Back   ", UIControlState.Normal);
            _backButton.SetTitleColor(UIColor.Red, UIControlState.Normal);
            _backButton.Font = UIFont.SystemFontOfSize(12);
            _backButton.Layer.BorderColor = UIColor.Red.CGColor;
            _backButton.Layer.BorderWidth = 1f;

            _backButton.TouchUpInside += BackTapped;

            _askButton = new UIButton();
            _askButton.SetTitle("   Ask   ", UIControlState.Normal);
            _askButton.SetTitleColor(UIColor.Red, UIControlState.Normal);
            _askButton.Font = UIFont.SystemFontOfSize(12);
            _askButton.Layer.BorderColor = UIColor.Red.CGColor;
            _askButton.Layer.BorderWidth = 1f;
            _askButton.Hidden = true;

            _askButton.TouchUpInside += AskTapped;

            View.Add(_pageViewController.View);
            View.Add(_pageControl);
            View.Add(_nextButton);
            View.Add(_askButton);
            View.Add(_backButton);
            View.Add(_pageTitle);
            

            View.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            View.AddConstraints(
                _pageViewController.View.AtTopOf(View),
                _pageViewController.View.AtBottomOf(View),
                _pageViewController.View.AtLeftOf(View),
                _pageViewController.View.AtRightOf(View),

                _pageControl.WithSameCenterX(_pageViewController.View),
                _pageControl.AtLeftOf(View),
                _pageControl.AtRightOf(View),
                _pageControl.AtTopOf(View, UIScreen.MainScreen.Bounds.Height * 0.06f),
                _pageControl.AtBottomOf(View, UIScreen.MainScreen.Bounds.Height * 0.85f),

                _nextButton.AtRightOf(View, 22),
                _nextButton.WithSameCenterY(_pageControl),

                _backButton.AtLeftOf(View, 22),
                _backButton.WithSameCenterY(_pageControl),

                _askButton.AtRightOf(View, 22),
                _askButton.WithSameCenterY(_pageControl),

                _pageTitle.WithSameCenterX(_pageControl),
                _pageTitle.AtTopOf(View, UIScreen.MainScreen.Bounds.Height * 0.11f)
            );

            

        }

        private void NextTapped(object s, EventArgs e)
        {            
            if(_currentStepIndex == 0)
            {
                if (CreateSurveyController.SurveyModel == null || 
                    CreateSurveyController.SurveyModel.question == null ||
                    string.IsNullOrEmpty(CreateSurveyController.SurveyModel.question.text))
                {
                    new UIAlertView("Question", "Please write a question", null, "OK", null).Show();

                    return;
                }
            }else if (_currentStepIndex == 1)
            {
                if (!CreateSurveyOptionsStep._optionsStepView.DoneButton.Hidden)
                {
                    new UIAlertView("Options", "Please press \"Done\" button to go to next page", null, "OK", null).Show();

                    return;
                }

                if (CreateSurveyController.SurveyModel == null ||
                    CreateSurveyController.SurveyModel.options == null ||
                    CreateSurveyController.SurveyModel.options.Count < 2)
                {
                    new UIAlertView("Options", "Please give at least two options", null, "OK", null).Show();

                    return;
                }

                
            }

            var nextVcs = new UIViewController[] { Steps.ElementAt(_currentStepIndex + 1) as UIViewController };
            _pageViewController.SetViewControllers(nextVcs, UIPageViewControllerNavigationDirection.Forward, true, null);
        }

        private void BackTapped(object s, EventArgs e)
        {
            if (_currentStepIndex == 1)
            {
                if (!CreateSurveyOptionsStep._optionsStepView.DoneButton.Hidden)
                {
                    new UIAlertView("Options", "Please press \"Done\" button to go back", null, "OK", null).Show();

                    return;
                }
            }

            var vcs = new UIViewController[] { Steps.ElementAt(_currentStepIndex - 1) as UIViewController };
            _pageViewController.SetViewControllers(vcs, UIPageViewControllerNavigationDirection.Forward, true, null);
        }

        private async void AskTapped(object s, EventArgs e)
        {
            if (CreateSurveyController.SurveyModel.targetAudience == TargetAudience.Private.ToString() && (CreateSurveyController.SurveyModel == null ||
                    CreateSurveyController.SurveyModel.targetAudienceUsers == null ||
                    CreateSurveyController.SurveyModel.targetAudienceUsers.ids == null ||
                    CreateSurveyController.SurveyModel.targetAudienceUsers.ids.Count < 1))
            {
                new UIAlertView("Share", "Please select at least one friend to share this survey", null, "OK", null).Show();

                return;
            }

            if (CreateSurveyController.SurveyModel.targetAudience != TargetAudience.Private.ToString())
            {
                CreateSurveyController.SurveyModel.targetAudienceUsers = null;
            }

            try
            {
                await new FeedManager().SaveSurvey(SurveyModel, LoginController.tokenModel.access_token, QuestionImage, OptionImages);

                var feedController = this.Storyboard.InstantiateViewController("MenuNavController");
                if (feedController != null)
                {
                    this.PresentViewController(feedController, true, null);
                    CreateSurveyController.SurveyModel = null;
                }
            }
            catch (Exception ex)
            {
                var alert = UIAlertController.Create("Survey", ex.Message, UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
        }

        private void _multiStepProcessHorizontal_WillTransition(object sender, UIPageViewControllerTransitionEventArgs e)
        {
            var pendingStep = e.PendingViewControllers.FirstOrDefault() as IMultiStepProcessStep;            
            if (pendingStep == null)
            {
                return;
            }
            
            _swipeDirection = pendingStep.StepIndex > _pageControl.CurrentPage
                ? SwipeDirection.Forward
                : SwipeDirection.Backward;

            
        }

        private List<IMultiStepProcessStep> GetSteps()
        {
            var steps = new List<IMultiStepProcessStep>
                {
                    new CreateSurveyQuestionStep(),
                    new CreateSurveyOptionsStep(),
                    new CreateSurveyShareStep()
                };

            steps.ForEach(s =>
            {
                s.StepActivated += HandleStepActivated;
                s.StepDeactivated += HandleStepDeactivated;
            });

            return steps;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // NavigationController.NavigationBar.Hidden = true;

            this.NavigationItem.SetLeftBarButtonItem(
                new UIBarButtonItem(UIBarButtonSystemItem.Cancel, (sender, args) =>
                {
                    var rootController = this.Storyboard.InstantiateViewController("MenuNavController");
                    if (rootController != null)
                    {
                        this.PresentViewController(rootController, true, null);
                        CreateSurveyController.SurveyModel = null;
                    }
                }
                ), true);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }

        public override bool PrefersStatusBarHidden()
        {
            return true;
        }

        private void HandleStepActivated(object sender, MultiStepProcessStepEventArgs args)
        {
            var isFirstStep = args.Index == 0;
            var isLastStep = args.Index + 1 == _pageControl.Pages;
            var isGetStartedStep = args.Index == _pageControl.Pages;
            var isAbortedTransition = args.Index == _pageControl.CurrentPage;

            if (!isGetStartedStep)
            {
                _pageControl.CurrentPage = args.Index;
            }

            if (isLastStep)
            {
                _askButton.Hidden = false;
                _nextButton.Hidden = true;
            }
            else
            {
                _askButton.Hidden = true;
                _nextButton.Hidden = false;
            }

            if (isFirstStep)
            {
                _backButton.Hidden = true;
            }
            else
            {
                _backButton.Hidden = false;
            }

            //_pageControl.Hidden = isLastStep || isGetStartedStep;

            _pageControl.Alpha = 1.0f;

            _pageTitle.Text = _pageTitles.ElementAt(args.Index) as string;

            _currentStepIndex = args.Index;        
        }

        private void HandleStepDeactivated(object sender, MultiStepProcessStepEventArgs args)
        {
            var isCurrentActiveStep = args.Index == _pageControl.CurrentPage;
            var isTransitioningFromFirstStep = args.Index == 0;
            var isTransitionFromSecondStepBackwards = args.Index == 1 && _swipeDirection == SwipeDirection.Backward;
            var isTransitioningBetweenFirstTwoSteps = isTransitioningFromFirstStep || isTransitionFromSecondStepBackwards;
            var isTransitionigFromLastStep = args.Index + 1 == _pageControl.Pages;

            //if (isCurrentActiveStep && isTransitioningBetweenFirstTwoSteps)
            //{
            //    _pageControl.Alpha = 0.0f;
            //}
        }

        protected override void Dispose(bool disposing)
        {
            if (_steps == null)
            {
                return;
            }
            foreach (var s in _steps)
            {
                s.Dispose();
            }
            _steps = null;

            base.Dispose(disposing);
        }
    }
}


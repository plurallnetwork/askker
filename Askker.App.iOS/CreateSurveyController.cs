using Askker.App.iOS.HorizontalSwipe;
using Askker.App.iOS.TableControllers;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
using BigTed;
using Cirrious.FluentLayouts.Touch;
using CoreAnimation;
using CoreGraphics;
using SDWebImage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UIKit;

namespace Askker.App.iOS
{
    public partial class CreateSurveyController : CustomUIViewController
    {
        enum SwipeDirection
        {
            Forward,
            Backward
        }

        public static string ScreenState;
        public static string UserId;
        public static string CreationDate;
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
            _pageTitles = new List<string> { "Write your question", "Choose your options", "Who should see this?" };
            _pageTitle = new UILabel();
            _pageTitle.TextColor = UIColor.FromRGB(255, 200, 0);
            _pageTitle.Font = UIFont.BoldSystemFontOfSize(12);

            _pageViewController = new MultiStepProcessHorizontal(new MultiStepProcessDataSource(Steps));
            _pageViewController.WillTransition += _multiStepProcessHorizontal_WillTransition;

            _pageControl = new HorizontalSwipePageControl
            {
                CurrentPage = 0,
                Pages = Steps.Count,
                BackgroundColor = UIColor.White,
                BorderColorBottom = UIColor.LightGray,
                BorderWidthAll = 0.5f
            };            
            
            _pageControl.CurrentPage = 0;

            _nextButton = new PageControlUIButton
            {
                BorderColorBottom = UIColor.LightGray,
                BorderWidthAll = 0.5f
            };
            _nextButton.SetTitle("    Next  >  ", UIControlState.Normal);
            _nextButton.SetTitleColor(UIColor.FromRGB(220, 220, 220), UIControlState.Normal);
            _nextButton.Font = UIFont.SystemFontOfSize(16);
            _nextButton.Frame = new CoreGraphics.CGRect(0, 0, 75, 50);
            _nextButton.BackgroundColor = UIColor.White;
            
            _nextButton.TouchUpInside += NextTapped;

            _backButton = new PageControlUIButton
            {
                BorderColorBottom = UIColor.LightGray,
                BorderWidthAll = 0.5f
            };
            _backButton.SetTitle("  <  Back  ", UIControlState.Normal);
            _backButton.SetTitleColor(UIColor.FromRGB(90,89,89), UIControlState.Normal);
            _backButton.Font = UIFont.SystemFontOfSize(16);
            _backButton.Frame = new CoreGraphics.CGRect(0, 0, 75, 50);
            _backButton.BackgroundColor = UIColor.White;

            _backButton.TouchUpInside += BackTapped;

            _askButton = new PageControlUIButton
            {
                BorderColorBottom = UIColor.LightGray,
                BorderWidthAll = 0.5f
            };
            _askButton.SetTitle("  Publish  >  ", UIControlState.Normal);
            _askButton.SetTitleColor(UIColor.LightGray, UIControlState.Normal);
            _askButton.Font = UIFont.SystemFontOfSize(14);
            _askButton.Frame = new CoreGraphics.CGRect(0, 0, 75, 50);
            _askButton.BackgroundColor = UIColor.White;
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
                _pageControl.ToTopMargin(View),
                _pageControl.Height().EqualTo(50),

                _nextButton.AtRightOf(View),
                _nextButton.AtTopOf(_pageControl),
                _nextButton.WithSameCenterY(_pageControl),

                _backButton.AtLeftOf(View),
                _backButton.AtTopOf(_pageControl),
                _backButton.WithSameCenterY(_pageControl),

                _askButton.AtRightOf(View),
                _askButton.AtTopOf(_pageControl),
                _askButton.WithSameCenterY(_pageControl),
                            
                _pageTitle.WithSameCenterX(_pageControl),
                _pageTitle.AtTopOf(_pageControl, 30)
            );
        }

        private void NextTapped(object s, EventArgs e)
        {
            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            if (_currentStepIndex == 0)
            {
                if (CreateSurveyController.SurveyModel == null || 
                    CreateSurveyController.SurveyModel.question == null ||
                    string.IsNullOrEmpty(CreateSurveyController.SurveyModel.question.text))
                {
                    new UIAlertView("Question", "Please write a question", null, "OK", null).Show();
                    BTProgressHUD.Dismiss();
                    return;
                }
            }
            else if (_currentStepIndex == 1)
            {
                if (!CreateSurveyOptionsStep._optionsStepView.DoneButton.Hidden)
                {
                    new UIAlertView("Options", "Please press \"Done\" button to go to next page", null, "OK", null).Show();
                    BTProgressHUD.Dismiss();
                    return;
                }

                List<SurveyOptionTableItem> items = CreateSurveyOptionsStep.tableSource.GetTableItems();
                if (items.Count > 0)
                {
                    CreateSurveyController.SurveyModel.options = new List<Option>();

                    if (CreateSurveyController.SurveyModel.type == SurveyType.Image.ToString())
                    {
                        //if (CreateSurveyController.OptionImages == null)
                        //{
                        CreateSurveyController.OptionImages = new List<KeyValuePair<string, byte[]>>();
                        //}
                    }

                    int optionId = 0;
                    items.ForEach(i =>
                    {
                        if (!"<- Add new option".Equals(i.Text))
                        {
                            Option o = new Option();
                            o.id = optionId;
                            o.text = i.Text;
                            o.image = "";

                            if (CreateSurveyController.SurveyModel.type == SurveyType.Image.ToString() && i.Image != null)
                            {
                                CreateSurveyController.OptionImages.Add(new KeyValuePair<string, byte[]>(optionId.ToString() + i.ImageExtension, i.Image));
                            }

                            CreateSurveyController.SurveyModel.options.Add(o);


                            optionId++;
                        }
                    });
                }

                if (CreateSurveyController.SurveyModel == null ||
                    CreateSurveyController.SurveyModel.options == null ||
                    CreateSurveyController.SurveyModel.options.Count < 2)
                {
                    new UIAlertView("Options", "Please give at least two options", null, "OK", null).Show();
                    BTProgressHUD.Dismiss();
                    return;
                }
            }

            var nextVcs = new UIViewController[] { Steps.ElementAt(_currentStepIndex + 1) as UIViewController };
            _pageViewController.SetViewControllers(nextVcs, UIPageViewControllerNavigationDirection.Forward, true, null);
            BTProgressHUD.Dismiss();
        }

        private void BackTapped(object s, EventArgs e)
        {
            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            if (_currentStepIndex == 1)
            {
                if (!CreateSurveyOptionsStep._optionsStepView.DoneButton.Hidden)
                {
                    new UIAlertView("Options", "Please press \"Done\" button to go back", null, "OK", null).Show();
                    BTProgressHUD.Dismiss();
                    return;
                }

                if (CreateSurveyController.SurveyModel != null &&
                    CreateSurveyController.SurveyModel.question != null &&
                    !string.IsNullOrEmpty(CreateSurveyController.SurveyModel.question.text))
                {
                    CreateSurveyController._nextButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                    CreateSurveyController._nextButton.BackgroundColor = UIColor.FromRGB(70, 230, 130);
                }
                else
                {
                    CreateSurveyController._nextButton.SetTitleColor(UIColor.FromRGB(220, 220, 220), UIControlState.Normal);
                    CreateSurveyController._nextButton.BackgroundColor = UIColor.White;
                }
            }

            var vcs = new UIViewController[] { Steps.ElementAt(_currentStepIndex - 1) as UIViewController };
            _pageViewController.SetViewControllers(vcs, UIPageViewControllerNavigationDirection.Reverse, true, null);
            BTProgressHUD.Dismiss();
        }

        private async void AskTapped(object s, EventArgs e)
        {
            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);

            if (string.IsNullOrEmpty(CreateSurveyController.SurveyModel.targetAudience))
            {
                new UIAlertView("Share", "Please select the privacy", null, "OK", null).Show();
                BTProgressHUD.Dismiss();
                return;
            }

            if (CreateSurveyController.SurveyModel.targetAudience == TargetAudience.Private.ToString() && (CreateSurveyController.SurveyModel == null ||
                    CreateSurveyController.SurveyModel.targetAudienceUsers == null ||
                    CreateSurveyController.SurveyModel.targetAudienceUsers.ids == null ||
                    CreateSurveyController.SurveyModel.targetAudienceUsers.ids.Count < 1))
            {
                new UIAlertView("Share", "Please select at least one friend to share this survey", null, "OK", null).Show();
                BTProgressHUD.Dismiss();

                return;
            }

            if (CreateSurveyController.SurveyModel.targetAudience == TargetAudience.Groups.ToString() && (CreateSurveyController.SurveyModel == null ||
                    CreateSurveyController.SurveyModel.targetAudienceGroups == null ||
                    CreateSurveyController.SurveyModel.targetAudienceGroups.ids == null ||
                    CreateSurveyController.SurveyModel.targetAudienceGroups.ids.Count < 1))
            {
                new UIAlertView("Share", "Please select at least one group to share this survey", null, "OK", null).Show();
                BTProgressHUD.Dismiss();

                return;
            }

            if (CreateSurveyController.SurveyModel.targetAudience != TargetAudience.Private.ToString())
            {
                CreateSurveyController.SurveyModel.targetAudienceUsers = null;
            }

            if (CreateSurveyController.SurveyModel.targetAudience != TargetAudience.Groups.ToString())
            {
                CreateSurveyController.SurveyModel.targetAudienceGroups = null;
            }

            try
            {
                //remove option images from cache to load the feed with the correct images
                if (SurveyModel.type == SurveyType.Image.ToString())
                {
                    foreach (var option in SurveyModel.options)
                    {
                        Utils.RemoveImageFromCache(LoginController.userModel.id + "/" + SurveyModel.creationDate + "/optionImage-" + option.id + ".jpg");
                    }
                }

                SurveyModel.optionSelected = null;
                await new FeedManager().SaveSurvey(SurveyModel, LoginController.tokenModel.access_token, QuestionImage, OptionImages);

                var feedController = this.Storyboard.InstantiateViewController("MenuNavController");
                if (feedController != null)
                {                    
                    this.PresentViewController(feedController, true, null);
                    CreateSurveyController.SurveyModel = null;
                }
                BTProgressHUD.Dismiss();
            }
            catch (Exception ex)
            {
                BTProgressHUD.Dismiss();
                Utils.HandleException(ex);                
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

        public override bool PrefersStatusBarHidden()
        {
            return false;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);

            if (ScreenState == Askker.App.PortableLibrary.Enums.ScreenState.Edit.ToString())
            {
                Title = "Edit Survey";
            }
            else
            {
                Title = "Create Survey";
            }

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

    public class PageControlUIButton : UIButton
    {
        /* If set, overrides individual widths */
        public nfloat BorderWidthAll { get; set; }
        /* If set, overrides individual colors */
        public UIColor BorderColorAll { get; set; }

        /* For specifying individual widths */
        public UIEdgeInsets BorderWidth { get; set; }
        public UIColor BorderColorTop { get; set; }
        public UIColor BorderColorBottom { get; set; }
        public UIColor BorderColorLeft { get; set; }
        public UIColor BorderColorRight { get; set; }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            if (BorderWidthAll > 0)
            {
                BorderWidth = new UIEdgeInsets(BorderWidthAll, BorderWidthAll, BorderWidthAll, BorderWidthAll);
            }

            if (BorderColorAll != null)
            {
                BorderColorTop = BorderColorBottom = BorderColorLeft = BorderColorRight = BorderColorAll;
            }

            var xMin = rect.GetMinX();
            var xMax = rect.GetMaxX();

            var yMin = rect.GetMinY();
            var yMax = rect.GetMaxY();

            var fWidth = this.Frame.Size.Width;
            var fHeight = this.Frame.Size.Height;

            var context = UIGraphics.GetCurrentContext();

            DrawBorders(context, xMin, xMax, yMin, yMax, fWidth, fHeight);
        }

        void DrawBorders(CGContext context, nfloat xMin, nfloat xMax, nfloat yMin, nfloat yMax, nfloat fWidth, nfloat fHeight)
        {
            if (BorderColorTop != null)
            {
                context.SetFillColor(BorderColorTop.CGColor);
                context.FillRect(new CGRect(xMin, yMin, fWidth, BorderWidth.Top));
            }

            if (BorderColorLeft != null)
            {
                context.SetFillColor(BorderColorLeft.CGColor);
                context.FillRect(new CGRect(xMin, yMin, BorderWidth.Left, fHeight));
            }

            if (BorderColorRight != null)
            {
                context.SetFillColor(BorderColorRight.CGColor);
                context.FillRect(new CGRect(xMax - BorderWidth.Right, yMin, BorderWidth.Right, fHeight));
            }

            if (BorderColorBottom != null)
            {
                context.SetFillColor(BorderColorBottom.CGColor);
                context.FillRect(new CGRect(xMin, yMax - BorderWidth.Bottom, fWidth, BorderWidth.Bottom));
            }
        }
    }
}


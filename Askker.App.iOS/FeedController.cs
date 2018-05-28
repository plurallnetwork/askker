using Foundation;
using System;
using System.Collections.Generic;
using UIKit;
using CoreGraphics;
using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Util;
using Askker.App.PortableLibrary.Enums;
using ObjCRuntime;
using BigTed;
using System.Linq;
using Askker.App.iOS.PDFComponents;
using QuickLook;
using System.IO;

namespace Askker.App.iOS
{
    public partial class FeedController : CustomUICollectionViewController
    {
        public List<SurveyModel> surveys { get; set; }
        public static NSCache imageCache = new NSCache();
        public static VoteManager voteManager = new VoteManager();
        public LikeManager likeManager = new LikeManager();
        public bool filterMine { get; set; }
        public bool filterForMe { get; set; }
        public bool filterFinished { get; set; }
        public UIRefreshControl refreshControl;
        public static NSString feedCellId = new NSString("feedCell");
        public SurveyModel survey { get; set; }
        public FeedCollectionViewCell surveyCell { get; set; }
        public UIViewController viewController { get; set; }

        public FeedController (IntPtr handle) : base (handle)
        {            
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);
            imageCache.RemoveAllObjects();

            surveys = new List<SurveyModel>();

            feedCollectionView.RegisterClassForCell(typeof(FeedCollectionViewCell), feedCellId);
            feedCollectionView.BackgroundColor = UIColor.FromWhiteAlpha(nfloat.Parse("0.95"), 1);
            feedCollectionView.AlwaysBounceVertical = true;

            refreshControl = new UIRefreshControl();
            refreshControl.ValueChanged += (sender, e) =>
            {
                refreshControl.BeginRefreshing();
                fetchSurveys(filterMine, filterForMe, filterFinished);
            };
            feedCollectionView.RefreshControl = refreshControl;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            MenuViewController.feedMenu.feedView = MenuViewController.menuView;

            BTProgressHUD.Show("Refreshing feed...", -1, ProgressHUD.MaskType.Clear);
            fetchSurveys(filterMine, filterForMe, filterFinished);
        }

        [Export("FinishSelector:")]
        private async void FinishSelector(UIFeedButton but)
        {
            nint button = await Utils.ShowAlert("Close Survey", "The survey will be closed. Continue?", "Ok", "Cancel");

            try
            {
                if (button == 0)
                {
                    BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
                    await new FeedManager().FinishSurvey(survey, LoginController.tokenModel.access_token);

                    surveyCell.finishedLabel.Text = "Finished";
                    
                    if (surveys.Count > 0 && !this.filterFinished)
                    {
                        surveys.Remove(survey);
                        this.feedCollectionView.Delegate = new FeedCollectionViewDelegate(surveys);
                        this.feedCollectionView.ReloadData();
                    }
                }

                MenuViewController.feedMenu.Hidden = true;
                MenuViewController.feedMenu.feedView.Alpha = 1f;
                BTProgressHUD.Dismiss();
            }
            catch (Exception ex)
            {
                BTProgressHUD.Dismiss();
                Utils.HandleException(ex);
            }
        }

        [Export("DeleteSelector:")]
        private async void DeleteSelector(UIFeedButton but)
        {
            nint button = await Utils.ShowAlert("Delete Survey", "The survey will be deleted. Continue?", "Ok", "Cancel");
            
            try
            {
                if (button == 0)
                {
                    BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);

                    await new FeedManager().DeleteSurvey(survey.userId + survey.creationDate, LoginController.tokenModel.access_token);

                    surveys.Remove(survey);
                    this.feedCollectionView.Delegate = new FeedCollectionViewDelegate(surveys);
                    this.feedCollectionView.ReloadData();
                }

                MenuViewController.feedMenu.Hidden = true;
                MenuViewController.feedMenu.feedView.Alpha = 1f;
                BTProgressHUD.Dismiss();
            }
            catch (Exception ex)
            {
                BTProgressHUD.Dismiss();
                Utils.HandleException(ex);
            }
        }

        [Export("CleanSelector:")]
        private async void CleanSelector(UIFeedButton but)
        {
            nint button = await Utils.ShowAlert("Clean Votes", "All survey votes will be deleted. Continue?", "Ok", "Cancel");

            try
            {
                if (button == 0)
                {
                    BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
                    await new FeedManager().CleanVotes(this.survey.userId + this.survey.creationDate, LoginController.tokenModel.access_token);

                    this.survey.optionSelected = null;
                    this.survey.totalVotes = 0;
                    surveyCell.updateTotalVotes(0);
                    if (this.survey.type == SurveyType.Text.ToString())
                    {
                        surveyCell.optionsTableView.ReloadData();
                    }
                    else
                    {
                        surveyCell.optionsCollectionView.ReloadData();
                    }
                }

                MenuViewController.feedMenu.Hidden = true;
                MenuViewController.feedMenu.feedView.Alpha = 1f;
                BTProgressHUD.Dismiss();
            }
            catch (Exception ex)
            {
                BTProgressHUD.Dismiss();
                Utils.HandleException(ex);
            }
        }

        [Export("EditSelector:")]
        private void EditSelector(UIFeedButton but)
        {
            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            try
            {
                if (survey.totalVotes > 0)
                {
                    BTProgressHUD.Dismiss();
                    Utils.ShowAlertOk("Edit", "The survey have votes. Please clean the votes to edit the survey.");
                }
                else
                {
                    var CreateSurveyController = this.Storyboard.InstantiateViewController("CreateSurveyController") as CreateSurveyController;
                    if (CreateSurveyController != null)
                    {
                        CreateSurveyController.ScreenState = ScreenState.Edit.ToString();
                        CreateSurveyController.SurveyModel = survey;

                        var rootController = this.Storyboard.InstantiateViewController("CreateSurveyNavController");
                        if (rootController != null)
                        {
                            this.viewController.PresentViewController(rootController, true, null);
                        }
                    }
                }

                MenuViewController.feedMenu.Hidden = true;
                MenuViewController.feedMenu.feedView.Alpha = 1f;
                BTProgressHUD.Dismiss();
            }
            catch (Exception ex)
            {
                BTProgressHUD.Dismiss();
                Utils.HandleException(ex);
            }
        }

        public async void fetchSurveys(bool filterMine, bool filterForMe, bool filterFinished)
        {
            try
            {
                surveys = await new FeedManager().GetFeed(LoginController.userModel.id, filterMine, filterForMe, filterFinished, LoginController.tokenModel.access_token);

                foreach (var survey in surveys)
                {
                    survey.options = Common.Randomize(survey.options);
                }

                BTProgressHUD.Dismiss();
                refreshControl.EndRefreshing();

                if (surveys.Count > 0)
                {
                    feedCollectionView.BackgroundColor = UIColor.FromWhiteAlpha(nfloat.Parse("0.95"), 1);
                    feedCollectionView.BackgroundView = null;

                    feedCollectionView.Delegate = new FeedCollectionViewDelegate(surveys);
                    feedCollectionView.ReloadData();
                }
                else
                {
                    feedCollectionView.BackgroundColor = UIColor.White;

                    if (filterMine || filterForMe || filterFinished)
                        feedCollectionView.BackgroundView = Utils.GetSystemWarningImage("FeedClearFilters");
                    else
                        feedCollectionView.BackgroundView = Utils.GetSystemWarningImage("FeedNoAnswers");
                }

                NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("UpdateUnreadNotificationsCount"), new NSString("true"));
            }
            catch (Exception ex)
            {
                Utils.HandleException(ex);
            }
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return surveys.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var feedCell = collectionView.DequeueReusableCell(feedCellId, indexPath) as FeedCollectionViewCell;

            BindFeedCell(feedCell, surveys[indexPath.Row], indexPath.Row);

            return feedCell;
        }

        [Export("CommentSelector:")]
        private void CommentSelector(UIFeedButton button)
        {
            var buttonParams = button.Params.ToArray();

            var commentController = this.Storyboard.InstantiateViewController("CommentViewController") as CommentViewController;

            commentController.feedController = this;
            commentController.survey = (SurveyModel)button.Params.ToArray()[0];
            commentController.indexPathRow = (int)buttonParams[1];

            (buttonParams[2] as UINavigationController).PushViewController(commentController, true);
        }

        [Export("ResultSelector:")]
        private void ResultSelector(UIFeedButton button)
        {
            var buttonParams = button.Params.ToArray();

            var resultController = this.Storyboard.InstantiateViewController("ResultViewController") as ResultViewController;

            resultController.feedController = this;
            resultController.survey = (SurveyModel)button.Params.ToArray()[0];
            resultController.indexPathRow = (int)buttonParams[1];

            (buttonParams[2] as UINavigationController).PushViewController(resultController, true);
        }

        [Export("LikeSelector:")]
        private async void LikeSelector(UIFeedButton button)
        {
            button.LoadingIndicatorButton(true);

            var buttonParams = button.Params.ToArray();
            var surveyModel = (SurveyModel)button.Params.ToArray()[0];
            var indexPathRow = (int)buttonParams[1];
            var feedCell = (FeedCollectionViewCell)buttonParams[2];

            if ("Like".Equals(feedCell.getCurrentLikeButtonTitle()))
            {
                SurveyLikeModel surveyLikeModel = new SurveyLikeModel();
                surveyLikeModel.surveyId = surveyModel.userId + surveyModel.creationDate;
                surveyLikeModel.user = new User();
                surveyLikeModel.user.id = LoginController.userModel.id;

                await likeManager.Like(surveyLikeModel, LoginController.tokenModel.access_token);
                
                feedCell.updateLikeButtonText(false, ++surveyModel.totalLikes);

                surveyModel.userLiked = true;
            }
            else
            {
                await likeManager.Unlike(surveyModel.userId + surveyModel.creationDate, LoginController.userModel.id, LoginController.tokenModel.access_token);
                                
                feedCell.updateLikeButtonText(true, --surveyModel.totalLikes);

                surveyModel.userLiked = null;
            }

            button.LoadingIndicatorButton(false);
        }

        [Export("PreviewPdfSelector:")]
        private async void PreviewPdfSelector(UIFeedButton button)
        {
            button.LoadingIndicatorButton(true);
                        
            this.survey = (SurveyModel)button.Params.ToArray()[0];
            this.surveyCell = (FeedCollectionViewCell)button.Params.ToArray()[1];
            this.viewController = (UIViewController)button.Params.ToArray()[2];

            var filename = "questionImage.pdf";
            var fileData = await Utils.GetPDFFromNSUrl(this.survey.question.image);
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = Path.Combine(directory.ToString(), filename);
                        
            QLPreviewItemBundle prevItem = new QLPreviewItemBundle(filename, new NSUrl("file://" + filePath));
            QLPreviewController previewController = new QLPreviewController();
            previewController.DataSource = new PreviewControllerDS(prevItem);
            this.viewController.NavigationController.PushViewController(previewController, true);

            button.LoadingIndicatorButton(false);            
        }

        [Export("MoreSelector:")]
        private void MoreSelector(UIFeedButton button)
        {
            this.survey = (SurveyModel)button.Params.ToArray()[0];
            this.surveyCell = (FeedCollectionViewCell)button.Params.ToArray()[1];
            this.viewController = (UIViewController)button.Params.ToArray()[2];

            MenuViewController.feedMenu.Layer.AddAnimation(new CoreAnimation.CATransition
            {
                Duration = 0.2,
                Type = CoreAnimation.CAAnimation.TransitionPush,
                Subtype = CoreAnimation.CAAnimation.TransitionFromTop
            }, "showMenu");

            if (!string.IsNullOrEmpty(this.survey.finishDate)) //is finished
            {
                MenuViewController.feedMenu.EditButton.Enabled = false;
                MenuViewController.feedMenu.FinishButton.Enabled = false;
                MenuViewController.feedMenu.CleanButton.Enabled = false;

                MenuViewController.feedMenu.EditButton.SetTitleColor(UIColor.LightGray, UIControlState.Disabled);
                MenuViewController.feedMenu.FinishButton.SetTitleColor(UIColor.LightGray, UIControlState.Disabled);
                MenuViewController.feedMenu.CleanButton.SetTitleColor(UIColor.LightGray, UIControlState.Disabled);
            }
            else
            {
                MenuViewController.feedMenu.EditButton.Enabled = true;
                MenuViewController.feedMenu.FinishButton.Enabled = true;
                MenuViewController.feedMenu.CleanButton.Enabled = true;

                MenuViewController.feedMenu.EditButton.SetTitleColor(UIColor.FromRGB(90, 89, 89), UIControlState.Normal);
                MenuViewController.feedMenu.FinishButton.SetTitleColor(UIColor.FromRGB(90, 89, 89), UIControlState.Normal);
                MenuViewController.feedMenu.CleanButton.SetTitleColor(UIColor.FromRGB(90, 89, 89), UIControlState.Normal);
            }

            MenuViewController.feedMenu.Hidden = false;
            MenuViewController.feedMenu.feedView.Alpha = 0.5f;
        }

        [Export("TapProfilePictureSelector:")]
        public void TapProfilePictureSelector(UIFeedTapGestureRecognizer tapGesture)
        {
            Utils.OpenUserProfile((UINavigationController)tapGesture.Params.ToArray()[0], (string)tapGesture.Params.ToArray()[1]);
        }

        public static async void saveVote(SurveyModel survey, int optionId, FeedCollectionViewCell feedCell)
        {
            var incrementVote = false;

            try
            {
                if (survey.optionSelected == null)
                {
                    incrementVote = true;
                    survey.totalVotes++;
                    feedCell.updateTotalVotes(survey.totalVotes);
                }

                SurveyVoteModel surveyVoteModel = new SurveyVoteModel();
                surveyVoteModel.surveyId = survey.userId + survey.creationDate;
                surveyVoteModel.optionId = optionId;
                surveyVoteModel.user = new User();
                surveyVoteModel.user.id = LoginController.userModel.id;
                surveyVoteModel.user.gender = LoginController.userModel.gender;
                surveyVoteModel.user.age = LoginController.userModel.age;
                surveyVoteModel.user.city = LoginController.userModel.city;
                surveyVoteModel.user.country = LoginController.userModel.country;

                await voteManager.Vote(surveyVoteModel, "");

                BTProgressHUD.Dismiss();

                survey.optionSelected = optionId;

                try
                {
                    if (LoginController.userModel.id != survey.userId)
                    {
                        UserNotificationModel userNotificationModel = new UserNotificationModel();
                        userNotificationModel.notificationDate = "";
                        userNotificationModel.userId = survey.userId;
                        userNotificationModel.notificationUser = new UserFriendModel(LoginController.userModel.id, LoginController.userModel.name, LoginController.userModel.profilePicturePath);
                        userNotificationModel.type = UserNotificationType.SurveyVote.ToString();

                        if (survey.question.text.Length > 25)
                        {
                            userNotificationModel.text = "voted on \"" + survey.question.text.Substring(0, 25) + "...\"";
                        }
                        else
                        {
                            userNotificationModel.text = "voted on \"" + survey.question.text + "\"";
                        }

                        userNotificationModel.link = survey.userId + survey.creationDate;
                        userNotificationModel.isDismissed = 0;
                        userNotificationModel.isRead = 0;

                        await new NotificationManager().SetUserNotification(userNotificationModel, LoginController.tokenModel.access_token);
                    }
                }
                catch
                {
                   return;
                }
            }
            catch (Exception ex)
            {
                if (incrementVote)
                {
                    survey.totalVotes--;
                }

                if (survey.type == SurveyType.Text.ToString())
                {
                    feedCell.optionsTableView.ReloadData();
                }
                else
                {
                    feedCell.optionsCollectionView.ReloadData();
                }
                BTProgressHUD.Dismiss();
                Utils.HandleException(ex);
            }
        }
        
        public static async void deleteVote(SurveyModel survey, FeedCollectionViewCell feedCell)
        {
            try
            {
                if (survey.totalVotes > 0) {
                    survey.totalVotes--;
                }
                feedCell.updateTotalVotes(survey.totalVotes);

                await voteManager.DeleteVote(survey.userId + survey.creationDate, LoginController.userModel.id, LoginController.tokenModel.access_token);

                BTProgressHUD.Dismiss();

                survey.optionSelected = null;
            }
            catch (Exception ex)
            {
                if (survey.type == SurveyType.Text.ToString())
                {
                    feedCell.optionsTableView.ReloadData();
                }
                else
                {
                    feedCell.optionsCollectionView.ReloadData();
                }

                BTProgressHUD.Dismiss();

                Utils.HandleException(ex);
            }
        }

        public void BindFeedCell(FeedCollectionViewCell feedCell, SurveyModel survey, int indexPathRow)
        {
            Utils.BindFeedCell(feedCell, survey, indexPathRow, this);
                        
            feedCell.commentButton.AddTarget(this, new Selector("CommentSelector:"), UIControlEvent.TouchUpInside);
            List<Object> commentValues = new List<Object>();
            commentValues.Add(survey);
            commentValues.Add(indexPathRow);
            commentValues.Add(this.NavigationController);
            feedCell.commentButton.Params = commentValues;

            feedCell.resultButton.AddTarget(this, new Selector("ResultSelector:"), UIControlEvent.TouchUpInside);
            List<Object> resultValues = new List<Object>();
            resultValues.Add(survey);
            resultValues.Add(indexPathRow);
            resultValues.Add(this.NavigationController);
            feedCell.resultButton.Params = resultValues;

            feedCell.likeButton.AddTarget(this, new Selector("LikeSelector:"), UIControlEvent.TouchUpInside);
            List<Object> likeValues = new List<Object>();
            likeValues.Add(survey);
            likeValues.Add(indexPathRow);
            likeValues.Add(feedCell);
            feedCell.likeButton.Params = likeValues;

            feedCell.moreButton.AddTarget(this, new Selector("MoreSelector:"), UIControlEvent.TouchUpInside);
            List<Object> moreValues = new List<Object>();
            moreValues.Add(survey);
            moreValues.Add(feedCell);
            moreValues.Add(this);
            feedCell.moreButton.Params = moreValues;

            feedCell.previewPdfButton.AddTarget(this, new Selector("PreviewPdfSelector:"), UIControlEvent.TouchUpInside);
            List<Object> pdfValues = new List<Object>();
            pdfValues.Add(survey);
            pdfValues.Add(feedCell);
            pdfValues.Add(this);
            feedCell.previewPdfButton.Params = pdfValues;

            var feedTapGestureRecognizer = new UIFeedTapGestureRecognizer(this, new Selector("TapProfilePictureSelector:"));
            List<Object> tapProfilePictureValues = new List<Object>();
            tapProfilePictureValues.Add(this.NavigationController);
            tapProfilePictureValues.Add(survey.userId);
            feedTapGestureRecognizer.Params = tapProfilePictureValues;
            feedCell.profileImageView.AddGestureRecognizer(feedTapGestureRecognizer);

            if (MenuViewController.feedMenu.CleanButton.AllTargets.Count <= 0)
            {
                MenuViewController.feedMenu.CleanButton.AddTarget(this, new Selector("CleanSelector:"), UIControlEvent.TouchUpInside);
            }
            else if (!MenuViewController.feedMenu.CleanButton.AllTargets.IsEqual(this))
            {
                MenuViewController.feedMenu.CleanButton.RemoveTarget(null, null, UIControlEvent.AllEvents);
                MenuViewController.feedMenu.CleanButton.AddTarget(this, new Selector("CleanSelector:"), UIControlEvent.TouchUpInside);
            }

            if (MenuViewController.feedMenu.EditButton.AllTargets.Count <= 0)
            {
                MenuViewController.feedMenu.EditButton.AddTarget(this, new Selector("EditSelector:"), UIControlEvent.TouchUpInside);
            }
            else if (!MenuViewController.feedMenu.EditButton.AllTargets.IsEqual(this))
            {
                MenuViewController.feedMenu.EditButton.RemoveTarget(null, null, UIControlEvent.AllEvents);
                MenuViewController.feedMenu.EditButton.AddTarget(this, new Selector("EditSelector:"), UIControlEvent.TouchUpInside);
            }

            if (MenuViewController.feedMenu.FinishButton.AllTargets.Count <= 0)
            {
                MenuViewController.feedMenu.FinishButton.AddTarget(this, new Selector("FinishSelector:"), UIControlEvent.TouchUpInside);
            }
            else if (!MenuViewController.feedMenu.FinishButton.AllTargets.IsEqual(this))
            {
                MenuViewController.feedMenu.FinishButton.RemoveTarget(null, null, UIControlEvent.AllEvents);
                MenuViewController.feedMenu.FinishButton.AddTarget(this, new Selector("FinishSelector:"), UIControlEvent.TouchUpInside);
            }

            if (MenuViewController.feedMenu.DeleteButton.AllTargets.Count <= 0)
            {
                MenuViewController.feedMenu.DeleteButton.AddTarget(this, new Selector("DeleteSelector:"), UIControlEvent.TouchUpInside);
            }
            else if (!MenuViewController.feedMenu.DeleteButton.AllTargets.IsEqual(this))
            {
                MenuViewController.feedMenu.DeleteButton.RemoveTarget(null, null, UIControlEvent.AllEvents);
                MenuViewController.feedMenu.DeleteButton.AddTarget(this, new Selector("DeleteSelector:"), UIControlEvent.TouchUpInside);
            }
        }        
    }

    public class FeedCollectionViewDelegate : UICollectionViewDelegateFlowLayout
    {
        public List<SurveyModel> surveys { get; set; }

        public FeedCollectionViewDelegate(List<SurveyModel> surveys)
        {
            this.surveys = surveys;
        }

        public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
        {
            if (surveys[indexPath.Row].question != null && !surveys[indexPath.Row].question.text.Equals(""))
            {
                var feedCellHeight = Utils.getHeightForFeedCell(surveys[indexPath.Row], collectionView.Frame.Width);

                return new CGSize(collectionView.Frame.Width, feedCellHeight);
            }

            return new CGSize(collectionView.Frame.Width, 380);
        }
    }

    public class FeedCollectionViewCell : UICollectionViewCell
    {
        public UIImageView profileImageView { get; set; }
        public UILabel nameLabel { get; set; }
        public UILabel finishedLabel { get; set; }
        public UITextView questionText { get; set; }
        public UIFeedButton previewPdfButton { get; set; }
        public UIOptionsTableView optionsTableView { get; set; }
        public OptionsTableViewSource optionsTableViewSource { get; set; }
        public UIOptionsCollectionView optionsCollectionView { get; set; }
        public OptionsCollectionViewSource optionsCollectionViewSource { get; set; }
        public OptionsCollectionViewDelegate optionsCollectionViewDelegate { get; set; }
        public UIView dividerLineView { get; set; }
        public UIFeedButton commentButton { get; set; }
        public UIFeedButton resultButton { get; set; }
        public UIFeedButton likeButton { get; set; }
        public UIFeedButton moreButton { get; set; }
        public UIView dividerLineButtons { get; set; }
        public UIView dividerLineButtons2 { get; set; }
        public UIView contentViewButtons { get; set; }
        public UILabel badge { get; set; }
        public UILabel buttonLabel { get; set; }

        public static NSString optionCellId = new NSString("optionCell");
        
        [Export("initWithFrame:")]
        public FeedCollectionViewCell(CGRect frame) : base(frame)
        {
            ContentView.BackgroundColor = UIColor.White;

            profileImageView = new UIImageView();
            profileImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            profileImageView.Image = UIImage.FromBundle("Profile");
            profileImageView.Layer.CornerRadius = 22;
            profileImageView.Layer.MasksToBounds = true;
            profileImageView.UserInteractionEnabled = true;
            profileImageView.TranslatesAutoresizingMaskIntoConstraints = false;

            nameLabel = new UILabel();
            nameLabel.Lines = 2;
            nameLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            nameLabel.TintColor = UIColor.FromRGB(90, 89, 89);
            nameLabel.TextColor = UIColor.FromRGB(90, 89, 89);

            finishedLabel = new UILabel();
            finishedLabel.TextColor = UIColor.White;
            finishedLabel.TextAlignment = UITextAlignment.Center;
            finishedLabel.Font = UIFont.BoldSystemFontOfSize(14);
            finishedLabel.Layer.BackgroundColor = UIColor.FromRGB(250, 60, 60).CGColor;
            finishedLabel.Layer.CornerRadius = 10.0f;
            finishedLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            questionText = new UITextView();
            questionText.Font = UIFont.BoldSystemFontOfSize(14);
            questionText.BackgroundColor = UIColor.Clear;
            questionText.ScrollEnabled = false;
            questionText.Editable = false;
            questionText.TranslatesAutoresizingMaskIntoConstraints = false;
            questionText.TintColor = UIColor.FromRGB(90, 89, 89);
            questionText.TextColor = UIColor.FromRGB(90, 89, 89);

            previewPdfButton = new UIFeedButton();
            previewPdfButton.SetImage(UIImage.FromBundle("PreviewPDF"), UIControlState.Normal);
            previewPdfButton.TranslatesAutoresizingMaskIntoConstraints = false;

            optionsTableView = new UIOptionsTableView();
            optionsTableView.RegisterClassForCellReuse(typeof(OptionTableViewCell), optionCellId);

            optionsTableViewSource = new OptionsTableViewSource();

            optionsCollectionView = new UIOptionsCollectionView(new CGRect(), new UICollectionViewFlowLayout()
            {
                ScrollDirection = UICollectionViewScrollDirection.Horizontal
            });
            optionsCollectionView.BackgroundColor = UIColor.White;
            optionsCollectionView.RegisterClassForCell(typeof(OptionCollectionViewCell), optionCellId);
            optionsCollectionView.ShowsHorizontalScrollIndicator = false;

            optionsCollectionViewSource = new OptionsCollectionViewSource();

            optionsCollectionViewDelegate = new OptionsCollectionViewDelegate();

            dividerLineView = new UIView();
            dividerLineView.BackgroundColor = UIColor.FromRGBA(nfloat.Parse("0.88"), nfloat.Parse("0.89"), nfloat.Parse("0.90"), nfloat.Parse("1"));
            dividerLineView.TranslatesAutoresizingMaskIntoConstraints = false;

            resultButton = buttonForTitle(title: "Result", imageName: "result");
            commentButton = buttonForTitle(title: "Comment", imageName: "comment");
            likeButton = buttonForTitle(title: "Like", imageName: "like");
            moreButton = buttonForTitle(title: "", imageName: "More");
            
            dividerLineButtons = new UIView();
            dividerLineButtons.BackgroundColor = UIColor.FromRGBA(nfloat.Parse("0.88"), nfloat.Parse("0.89"), nfloat.Parse("0.90"), nfloat.Parse("1"));
            dividerLineButtons.TranslatesAutoresizingMaskIntoConstraints = false;

            dividerLineButtons2 = new UIView();
            dividerLineButtons2.BackgroundColor = UIColor.FromRGBA(nfloat.Parse("0.88"), nfloat.Parse("0.89"), nfloat.Parse("0.90"), nfloat.Parse("1"));
            dividerLineButtons2.TranslatesAutoresizingMaskIntoConstraints = false;

            contentViewButtons = new UIView();
            contentViewButtons.AddSubviews(resultButton, dividerLineButtons, commentButton, dividerLineButtons2, likeButton);
            contentViewButtons.TranslatesAutoresizingMaskIntoConstraints = false;

            AddSubview(profileImageView);
            AddSubview(nameLabel);
            AddSubview(moreButton);
            AddSubview(questionText);
            AddSubview(previewPdfButton);
            AddSubview(dividerLineView);
            AddSubview(contentViewButtons);

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-8-[v0(44)]-8-[v1]-[v2(40)]|", new NSLayoutFormatOptions(), "v0", profileImageView, "v1", nameLabel, "v2", moreButton));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-4-[v0]-4-|", new NSLayoutFormatOptions(), "v0", questionText));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-8-[v0]", new NSLayoutFormatOptions(), "v0", previewPdfButton));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", dividerLineView));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", contentViewButtons));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0(v4)][v1(1)][v2(v4)][v3(1)][v4]|", new NSLayoutFormatOptions(), "v0", resultButton, "v1", dividerLineButtons, "v2", commentButton, "v3", dividerLineButtons2, "v4", likeButton));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-12-[v0]", new NSLayoutFormatOptions(), "v0", nameLabel));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-12-[v0]", new NSLayoutFormatOptions(), "v0", moreButton));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0(44)]|", new NSLayoutFormatOptions(), "v0", resultButton));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0]-8-|", new NSLayoutFormatOptions(), "v0", dividerLineButtons));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0(44)]|", new NSLayoutFormatOptions(), "v0", commentButton));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0]-8-|", new NSLayoutFormatOptions(), "v0", dividerLineButtons2));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0(44)]|", new NSLayoutFormatOptions(), "v0", likeButton));
        }

        public UIFeedButton buttonForTitle(string title, string imageName)
        {
            var button = new UIFeedButton();

            if (!string.IsNullOrWhiteSpace(title))
            {
                // label
                nfloat buttonHeight = 44;
                nfloat buttonWidth = ContentView.Frame.Width / 3;
                nfloat labelWidth = buttonWidth * 0.60f;
                nfloat labelHeight = 30;
                nfloat labelOriginX = 8; // Half of the icon width
                nfloat labelOriginY = 7;

                this.buttonLabel = new UILabel();
                this.buttonLabel.TextColor = UIColor.FromRGBA(nfloat.Parse("0.56"), nfloat.Parse("0.58"), nfloat.Parse("0.63"), nfloat.Parse("1"));
                this.buttonLabel.Font = UIFont.SystemFontOfSize(14);
                this.buttonLabel.TextAlignment = UITextAlignment.Center;
                this.buttonLabel.Frame = CoreGraphics.CGRect.FromLTRB(labelOriginX, labelOriginY, labelOriginX + labelWidth, labelOriginY + labelHeight);
                this.buttonLabel.Text = title;

                button.AddSubview(this.buttonLabel);
            }

            if (!imageName.Equals(""))
            {
                button.SetImage(UIImage.FromBundle(imageName), UIControlState.Normal);
            }

            button.TranslatesAutoresizingMaskIntoConstraints = false;
                        
            return button;
        }

        public UIFeedButton insertBadge(UIFeedButton button, int count)
        {
            var oldBadge = Utils.findFirstLabelWithBkgColor(button, UIColor.FromRGBA(232, 232, 232, 255));
            if (oldBadge != null)
            {
                oldBadge.RemoveFromSuperview();
            }
            // Count Badge
            nfloat badgePct = 0.55f;
            if (button.Equals(resultButton))
            {
                badgePct = 0.6f;
            }

            if (button.Equals(commentButton))
            {
                badgePct = 0.7f;
            }

            nfloat buttonHeight = 44;
            nfloat buttonWidth = ContentView.Frame.Width / 3;
            nfloat badgeSize = 30;
            nfloat badgeOriginX = buttonWidth * badgePct; 
            nfloat badgeOriginY = 7;

            this.badge = new UILabel();
            this.badge.TextColor = UIColor.Black;
            this.badge.BackgroundColor = UIColor.FromRGBA(232, 232, 232, 255);
            this.badge.Font = UIFont.SystemFontOfSize(11);
            this.badge.TextAlignment = UITextAlignment.Center;
            this.badge.Frame = CoreGraphics.CGRect.FromLTRB(badgeOriginX, badgeOriginY, badgeOriginX + badgeSize, badgeOriginY + badgeSize);
            this.badge.Layer.CornerRadius = badgeSize / 2;
            this.badge.Layer.MasksToBounds = true;
            this.badge.Layer.BorderWidth = 1;
            this.badge.Layer.BorderColor = UIColor.White.CGColor;
            this.badge.Text = count.ToString();
            
            button.AddSubview(this.badge);

            return button;
        }

        public void updateTotalVotes(int totalVotes)
        {
            insertBadge(resultButton, totalVotes);
        }

        public void updateTotalComments(int totalComments)
        {
            insertBadge(commentButton, totalComments);
        }

        public void updateTotalLikes(int totalLikes)
        {
            insertBadge(likeButton, totalLikes);
        }

        public string getCurrentLikeButtonTitle()
        {
            UILabel label = Utils.findFirstLabelWithText(likeButton, "Unlike");
            if (label != null)
            {
                return label.Text;             
            }

            label = Utils.findFirstLabelWithText(likeButton, "Like");
            if (label != null)
            {
                return label.Text;
            }

            return null;
        }

        public void updateLikeButtonText(bool fromLike, int totalLikes)
        {
            if (fromLike)
            {
                UILabel label = Utils.findFirstLabelWithText(likeButton, "Unlike");
                if (label != null)
                {
                    label.Text = "Like";
                    label.TextColor = UIColor.FromRGBA(nfloat.Parse("0.56"), nfloat.Parse("0.58"), nfloat.Parse("0.63"), nfloat.Parse("1"));
                }
            }
            else
            {
                UILabel label = Utils.findFirstLabelWithText(likeButton, "Like");
                if (label != null)
                {
                    label.Text = "Unlike";
                    label.TextColor = UIColor.FromRGB(117, 227, 213);
                }
            }

            insertBadge(likeButton, totalLikes);
        }
    }

    public class OptionsTableViewSource : UITableViewSource
    {
        public SurveyModel survey { get; set; }

        public OptionsTableViewSource()
        {
            survey = new SurveyModel();
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return survey.options.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var optionCell = tableView.DequeueReusableCell(FeedCollectionViewCell.optionCellId, indexPath) as OptionTableViewCell;
            optionCell.SelectionStyle = UITableViewCellSelectionStyle.None;
            optionCell.Tag = survey.options[indexPath.Row].id;

            optionCell.optionLabel.Text = survey.options[indexPath.Row].text;
            optionCell.optionLabel.TextColor = UIColor.FromRGB(90, 89, 89);

            if (survey.optionSelected == survey.options[indexPath.Row].id)
            {
                optionCell.AccessoryView = optionCell.optionCheckImage;
                optionCell.isSelected = true;
                tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);
            }
            else
            {
                optionCell.AccessoryView = optionCell.optionEmptyCircle;
                optionCell.isSelected = false;
            }

            return optionCell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            var optionCell = tableView.CellAt(indexPath) as OptionTableViewCell;

            if (!optionCell.isSelected)
            {
                optionCell.AccessoryView = optionCell.optionCheckImage;
                optionCell.isSelected = true;

                FeedController.saveVote(survey, (int)optionCell.Tag, ((UIOptionsTableView)tableView).FeedCell);
            }
            else
            {
                optionCell.AccessoryView = optionCell.optionEmptyCircle;
                optionCell.isSelected = false;
                tableView.DeselectRow(indexPath, true);

                FeedController.deleteVote(survey, ((UIOptionsTableView)tableView).FeedCell);
            }
        }

        public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
        {
            var optionCell = tableView.CellAt(indexPath) as OptionTableViewCell;

            if (optionCell == null)
            {
                optionCell = this.GetCell(tableView, indexPath) as OptionTableViewCell;
            }

            optionCell.AccessoryView = optionCell.optionEmptyCircle;
            optionCell.isSelected = false;
        }

        private int GetNumberFromLabel(string label)
        {
            var num = label.Substring(0, label.IndexOf(" "));

            int result;

            if(Int32.TryParse(num.Trim(), out result))
            {
                return result;
            }

            return 0;
        }
    }

    public partial class OptionTableViewCell : UITableViewCell
    {
        public bool isSelected { get; set; }
        public UILabel optionLabel { get; set; }
        public UIImageView optionCheckImage { get; set; }
        public UIImageView optionEmptyCircle { get; set; }

        protected OptionTableViewCell(IntPtr handle) : base(handle)
        {
            isSelected = false;

            optionLabel = new UILabel();
            optionLabel.Font = UIFont.SystemFontOfSize(14);
            optionLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            optionCheckImage = new UIImageView(UIImage.FromBundle("OptionCheck"));
            optionCheckImage.Frame = new CGRect(0, 0, 38, 38);

            optionEmptyCircle = new UIImageView(UIImage.FromBundle("EmptyCircleText"));
            optionEmptyCircle.Frame = new CGRect(0, 0, 38, 38);

            ContentView.Add(optionLabel);

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-24-[v0]-8-|", new NSLayoutFormatOptions(), "v0", optionLabel));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(25)]", new NSLayoutFormatOptions(), "v0", optionLabel));
        }
    }

    public class OptionsCollectionViewSource : UICollectionViewSource
    {
        public SurveyModel survey { get; set; }
        public bool isPreview { get; set; }

        public OptionsCollectionViewSource()
        {
            survey = new SurveyModel();
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return survey.options.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var optionCell = collectionView.DequeueReusableCell(FeedCollectionViewCell.optionCellId, indexPath) as OptionCollectionViewCell;
            optionCell.BackgroundColor = UIColor.White;
            optionCell.Tag = survey.options[indexPath.Row].id;

            if (isPreview)
            {
                
                if (CreateSurveyController.OptionImages.ElementAt(survey.options[indexPath.Row].id).Value != null)
                {                    
                    optionCell.optionImageView.Image = UIImage.LoadFromData(NSData.FromArray(CreateSurveyController.OptionImages.ElementAt(survey.options[indexPath.Row].id).Value));
                }
            }
            else
            {
                if (survey.options[indexPath.Row].image != null)
                {
                    Utils.SetImageFromNSUrlSession(survey.options[indexPath.Row].image, optionCell.optionImageView, this, PictureType.OptionImage);
                }
            }

            optionCell.optionLabel.Text = survey.options[indexPath.Row].text;

            if (survey.optionSelected == survey.options[indexPath.Row].id)
            {
                optionCell.optionCheckOpacityView.Hidden = false;
                optionCell.optionCheckImageView.Hidden = false;
                optionCell.optionEmptyCircle.Hidden = true;
                optionCell.isSelected = true;
                collectionView.SelectItem(indexPath, false, UICollectionViewScrollPosition.None);
            }
            else
            {
                optionCell.optionCheckOpacityView.Hidden = true;
                optionCell.optionCheckImageView.Hidden = true;
                optionCell.optionEmptyCircle.Hidden = false;
                optionCell.isSelected = false;
            }

            optionCell.optionNumber.Text = (indexPath.Row + 1) + "/" + survey.options.Count;

            return optionCell;
        }

        public OptionCollectionViewCell GetCustomCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            return this.GetCell(collectionView, indexPath) as OptionCollectionViewCell;
        }
    }

    public class OptionsCollectionViewDelegate : UICollectionViewDelegateFlowLayout
    {
        public OptionsCollectionViewSource optionsCollectionViewSource { get; set; }
        public SurveyModel survey { get; set; }

        public OptionsCollectionViewDelegate()
        {
            optionsCollectionViewSource = new OptionsCollectionViewSource();
            survey = new SurveyModel();
        }

        public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
        {
            return new CGSize(293, 280);
        }

        public override nfloat GetMinimumLineSpacingForSection(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
        {
            return 12;
        }

        public override UIEdgeInsets GetInsetForSection(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
        {
            return new UIEdgeInsets(0, 8, 0, 8);
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            var optionCell = collectionView.CellForItem(indexPath) as OptionCollectionViewCell;

            if (!optionCell.isSelected)
            {
                optionCell.optionCheckOpacityView.Hidden = false;
                optionCell.optionCheckImageView.Hidden = false;
                optionCell.optionEmptyCircle.Hidden = true;
                optionCell.isSelected = true;

                FeedController.saveVote(survey, (int)optionCell.Tag, ((UIOptionsCollectionView)collectionView).FeedCell);
            }
            else
            {
                optionCell.optionCheckOpacityView.Hidden = true;
                optionCell.optionCheckImageView.Hidden = true;
                optionCell.optionEmptyCircle.Hidden = false;
                optionCell.isSelected = false;
                collectionView.DeselectItem(indexPath, true);

                FeedController.deleteVote(survey, ((UIOptionsCollectionView)collectionView).FeedCell);
            }
        }

        public override void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var optionCell = collectionView.CellForItem(indexPath) as OptionCollectionViewCell;

            if (optionCell == null)
            {
                optionCell = optionsCollectionViewSource.GetCustomCell(collectionView, indexPath);
            }

            optionCell.optionCheckOpacityView.Hidden = true;
            optionCell.optionCheckImageView.Hidden = true;
            optionCell.optionEmptyCircle.Hidden = false;
            optionCell.isSelected = false;
        }

        private int GetNumberFromLabel(string label)
        {
            var num = label.Substring(0, label.IndexOf(" "));

            int result;

            if (Int32.TryParse(num.Trim(), out result))
            {
                return result;
            }

            return 0;
        }
    }

    public class OptionCollectionViewCell : UICollectionViewCell
    {
        public bool isSelected { get; set; }
        public UIImageView optionImageView { get; set; }
        public UIView optionCheckOpacityView { get; set; }
        public UIView optionView { get; set; }
        public UILabel optionLabel { get; set; }
        public UIImageView optionCheckImageView { get; set; }
        public UIImageView optionEmptyCircle { get; set; }
        public UILabel optionNumber { get; set; }

        [Export("initWithFrame:")]
        public OptionCollectionViewCell(CGRect frame) : base(frame)
        {
            isSelected = false;
            
            ContentView.Layer.CornerRadius = 10.0f;
            ContentView.Layer.BorderWidth = 1.0f;
            ContentView.Layer.BorderColor = UIColor.Clear.CGColor;
            ContentView.Layer.MasksToBounds = true;

            Layer.CornerRadius = 10.0f;
            Layer.ShadowColor = UIColor.FromRGB(90, 89, 89).CGColor;
            Layer.ShadowOffset = new CGSize(width: 3, height: 3);
            Layer.ShadowRadius = 5.0f;
            Layer.ShadowOpacity = 0.5f;
            Layer.MasksToBounds = false;
            Layer.ShadowPath = UIBezierPath.FromRoundedRect(Bounds, ContentView.Layer.CornerRadius).CGPath;

            optionImageView = new UIImageView();
            optionImageView.Image = null;
            optionImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            optionImageView.TranslatesAutoresizingMaskIntoConstraints = false;

            optionCheckOpacityView = new UIView(new CGRect(0, 0, 293, 220));
            optionCheckOpacityView.BackgroundColor = UIColor.Green;
            optionCheckOpacityView.Alpha = 0.3f;
            optionCheckOpacityView.TranslatesAutoresizingMaskIntoConstraints = false;
            optionCheckOpacityView.Hidden = true;

            optionNumber = new UILabel();
            optionNumber.TextColor = UIColor.White;
            optionNumber.TextAlignment = UITextAlignment.Center;
            optionNumber.Font = UIFont.SystemFontOfSize(12);
            optionNumber.Alpha = 0.5f;
            optionNumber.Layer.BackgroundColor = UIColor.Black.CGColor;
            optionNumber.Layer.CornerRadius = 12.0f;
            optionNumber.TranslatesAutoresizingMaskIntoConstraints = false;

            optionImageView.AddSubviews(optionCheckOpacityView, optionNumber);

            optionImageView.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", optionCheckOpacityView));
            optionImageView.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[v0(45)]", new NSLayoutFormatOptions(), "v0", optionNumber));

            optionImageView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0]|", new NSLayoutFormatOptions(), "v0", optionCheckOpacityView));
            optionImageView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0]-(<=1)-[v1(25)]", NSLayoutFormatOptions.AlignAllCenterX, "v0", optionImageView, "v1", optionNumber));
            optionImageView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0]-4-|", new NSLayoutFormatOptions(), "v0", optionNumber));

            optionView = new UIView(new CGRect(0, 0, 293, 60));
            optionView.BackgroundColor = UIColor.Clear;
            optionView.TranslatesAutoresizingMaskIntoConstraints = false;

            optionLabel = new UILabel();
            optionLabel.Font = UIFont.SystemFontOfSize(14);
            optionLabel.TextColor = UIColor.FromRGB(90, 89, 89);
            optionLabel.BackgroundColor = UIColor.Clear;
            optionLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            optionView.AddSubviews(optionLabel);

            optionView.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-8-[v0]|", new NSLayoutFormatOptions(), "v0", optionLabel));
            optionView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0(60)]", new NSLayoutFormatOptions(), "v0", optionLabel));

            optionCheckImageView = new UIImageView();
            optionCheckImageView.Image = UIImage.FromBundle("OptionCheck");
            optionCheckImageView.TranslatesAutoresizingMaskIntoConstraints = false;
            optionCheckImageView.Hidden = true;

            optionEmptyCircle = new UIImageView(UIImage.FromBundle("EmptyCircleText"));
            optionEmptyCircle.Frame = new CGRect(0, 0, 36, 36);
            optionEmptyCircle.TranslatesAutoresizingMaskIntoConstraints = false;
            optionEmptyCircle.Hidden = false;

            optionView.AddSubviews(optionLabel, optionCheckImageView, optionEmptyCircle);

            optionView.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-8-[v0][v1(36)]-8-|", new NSLayoutFormatOptions(), "v0", optionLabel, "v1", optionCheckImageView));
            optionView.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-8-[v0][v1(36)]-8-|", new NSLayoutFormatOptions(), "v0", optionLabel, "v1", optionEmptyCircle));

            optionView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0(60)]", new NSLayoutFormatOptions(), "v0", optionLabel));
            optionView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-12-[v0(36)]", new NSLayoutFormatOptions(), "v0", optionCheckImageView));
            optionView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-12-[v0(36)]", new NSLayoutFormatOptions(), "v0", optionEmptyCircle));

            ContentView.AddSubviews(optionImageView, optionView);

            ContentView.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", optionImageView));
            ContentView.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", optionView));

            ContentView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0(220)][v1(60)]", new NSLayoutFormatOptions(), "v0", optionImageView, "v1", optionView));
        }
    }

    public class UIFeedButton : UIButton
    {
        public List<Object> Params { get; set; }
    }

    public class UIFeedTapGestureRecognizer : UITapGestureRecognizer
    {
        public List<Object> Params { get; set; }

        public UIFeedTapGestureRecognizer(NSObject target, Selector action) : base(target, action)
        {
        }
    }

    public class UIOptionsTableView : UITableView
    {
        public FeedCollectionViewCell FeedCell { get; set; }
    }

    public class UIOptionsCollectionView : UICollectionView
    {
        public UIOptionsCollectionView(NSCoder coder) : base(coder) { }

        public UIOptionsCollectionView(CGRect frame, UICollectionViewLayout layout) : base(frame, layout) { }

        protected UIOptionsCollectionView(NSObjectFlag t) : base(t) { }

        protected internal UIOptionsCollectionView(IntPtr handle) : base(handle) { }

        public FeedCollectionViewCell FeedCell { get; set; }
    }
}
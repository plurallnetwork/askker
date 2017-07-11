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
using CoreAnimation;

namespace Askker.App.iOS
{
    public partial class FeedController : CustomUICollectionViewController
    {
        public List<SurveyModel> surveys { get; set; }
        public static NSCache imageCache = new NSCache();
        public static VoteManager voteManager = new VoteManager();
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
            refreshControl.TranslatesAutoresizingMaskIntoConstraints = false;
            refreshControl.ValueChanged += (sender, e) =>
            {
                refreshControl.BeginRefreshing();
                fetchSurveys(filterMine, filterForMe, filterFinished);
            };

            feedCollectionView.Add(refreshControl);
            feedCollectionView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0]-(<=1)-[v1]", NSLayoutFormatOptions.AlignAllCenterX, "v0", feedCollectionView, "v1", refreshControl));
            feedCollectionView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-35-[v0]|", new NSLayoutFormatOptions(), "v0", refreshControl));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

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
                    survey.finishDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    var tempOptionSelected = survey.optionSelected;
                    survey.optionSelected = null;
                    await new FeedManager().UpdateSurvey(survey, LoginController.tokenModel.access_token);
                    survey.optionSelected = tempOptionSelected;

                    surveyCell.finishedLabel.Text = "Finished";
                    surveyCell.moreButton.Hidden = true;

                    if (surveys.Count > 0 && !this.filterFinished)
                    {
                        surveys.Remove(survey);
                        this.feedCollectionView.Delegate = new FeedCollectionViewDelegate(surveys);
                        this.feedCollectionView.ReloadData();
                    }
                }

                MenuViewController.feedMenu.Hidden = true;
                MenuViewController.sidebarController.View.Alpha = 1f;
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
                    surveyCell.totalVotesLabel.SetTitle("0 Votes", UIControlState.Normal);
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
                MenuViewController.sidebarController.View.Alpha = 1f;
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
                        CreateSurveyController.UserId = survey.userId;
                        CreateSurveyController.CreationDate = survey.creationDate;

                        var rootController = this.Storyboard.InstantiateViewController("CreateSurveyNavController");
                        if (rootController != null)
                        {
                            this.viewController.PresentViewController(rootController, true, null);
                        }
                    }
                }

                MenuViewController.feedMenu.Hidden = true;
                MenuViewController.sidebarController.View.Alpha = 1f;
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
                    feedCollectionView.Delegate = new FeedCollectionViewDelegate(surveys);
                    feedCollectionView.ReloadData();
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

            MenuViewController.feedMenu.Hidden = false;
            MenuViewController.sidebarController.View.Alpha = 0.5f;
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
            if (survey.profilePicture != null)
            {
                Utils.SetImageFromNSUrlSession(survey.profilePicture, feedCell.profileImageView, this);
            }else
            {
                feedCell.profileImageView.Image = UIImage.FromBundle("Profile");
            }

            var attributedText = new NSMutableAttributedString(survey.userName, UIFont.BoldSystemFontOfSize(14));
            attributedText.Append(new NSAttributedString("\n" + survey.targetAudience, UIFont.SystemFontOfSize(12), UIColor.FromRGBA(nfloat.Parse("0.60"), nfloat.Parse("0.63"), nfloat.Parse("0.67"), nfloat.Parse("1"))));

            var paragraphStyle = new NSMutableParagraphStyle();
            paragraphStyle.LineSpacing = 4;
            attributedText.AddAttribute(new NSString("ParagraphStyle"), paragraphStyle, new NSRange(0, attributedText.Length));

            feedCell.nameLabel.AttributedText = attributedText;

            DateTime outputDateTimeValue;
            bool finished = false;
            if (survey.finishDate != null &&
                DateTime.TryParseExact(survey.finishDate, "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out outputDateTimeValue) &&
                outputDateTimeValue < DateTime.Now)
            {

                feedCell.finishedLabel.Text = "Finished";
                feedCell.moreButton.Hidden = true;
                feedCell.optionsTableView.AllowsSelection = false;
                feedCell.optionsCollectionView.AllowsSelection = false;
                finished = true;

                feedCell.AddSubview(feedCell.finishedLabel);
                feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", feedCell.finishedLabel));
            }
            else
            {
                feedCell.finishedLabel.Text = "";
                feedCell.moreButton.Hidden = false;
                feedCell.optionsTableView.AllowsSelection = true;
                feedCell.optionsCollectionView.AllowsSelection = true;
                finished = false;
            }

            if (!survey.userId.Equals(LoginController.userModel.id))
            {
                feedCell.moreButton.Hidden = true;
            }
            else
            {
                if (finished)
                {
                    feedCell.moreButton.Hidden = true;
                }
                else
                {
                    feedCell.moreButton.Hidden = false;
                }
            }

            feedCell.questionText.Text = survey.question.text;

            if (survey.type == SurveyType.Text.ToString())
            {
                feedCell.optionsTableView.ContentMode = UIViewContentMode.ScaleAspectFill;
                feedCell.optionsTableView.Layer.MasksToBounds = true;
                feedCell.optionsTableView.TranslatesAutoresizingMaskIntoConstraints = false;
                feedCell.optionsTableView.ContentInset = new UIEdgeInsets(0, -10, 0, 0);
                feedCell.optionsTableView.Tag = indexPathRow;
                feedCell.optionsTableView.FeedCell = feedCell;

                feedCell.optionsTableViewSource.survey = survey;
                feedCell.optionsTableView.Source = feedCell.optionsTableViewSource;
                feedCell.optionsTableView.ReloadData();

                feedCell.optionsCollectionView.RemoveFromSuperview();
                feedCell.AddSubview(feedCell.optionsTableView);

                feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", feedCell.optionsTableView));

                if (finished)
                {
                    feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(44)]-8-[v1(32)]-4-[v2]-4-[v3(1)][v4]-8-[v5(24)]-8-[v6(1)][v7(44)]|", new NSLayoutFormatOptions(), "v0", feedCell.profileImageView, "v1", feedCell.finishedLabel, "v2", feedCell.questionText, "v3", feedCell.dividerLineView, "v4", feedCell.optionsTableView, "v5", feedCell.totalVotesLabel, "v6", feedCell.dividerLineView2, "v7", feedCell.contentViewButtons));
                }
                else
                {
                    feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(44)]-4-[v1]-4-[v2(1)][v3]-8-[v4(24)]-8-[v5(1)][v6(44)]|", new NSLayoutFormatOptions(), "v0", feedCell.profileImageView, "v1", feedCell.questionText, "v2", feedCell.dividerLineView, "v3", feedCell.optionsTableView, "v4", feedCell.totalVotesLabel, "v5", feedCell.dividerLineView2, "v6", feedCell.contentViewButtons));
                }
            }
            else
            {
                feedCell.optionsCollectionView.TranslatesAutoresizingMaskIntoConstraints = false;
                feedCell.optionsCollectionView.Tag = indexPathRow;
                feedCell.optionsCollectionView.FeedCell = feedCell;

                feedCell.optionsCollectionViewSource.survey = survey;
                feedCell.optionsCollectionView.Source = feedCell.optionsCollectionViewSource;
                feedCell.optionsCollectionViewDelegate.optionsCollectionViewSource = feedCell.optionsCollectionViewSource;
                feedCell.optionsCollectionViewDelegate.survey = survey;
                feedCell.optionsCollectionView.Delegate = feedCell.optionsCollectionViewDelegate;
                feedCell.optionsCollectionView.ReloadData();

                feedCell.optionsTableView.RemoveFromSuperview();
                feedCell.AddSubview(feedCell.optionsCollectionView);

                feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", feedCell.optionsCollectionView));
                
                if (finished)
                {
                    feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(44)]-8-[v1(32)]-4-[v2]-4-[v3(1)][v4(<=176)]-8-[v5(24)]-8-[v6(1)][v7(44)]|", new NSLayoutFormatOptions(), "v0", feedCell.profileImageView, "v1", feedCell.finishedLabel, "v2", feedCell.questionText, "v3", feedCell.dividerLineView, "v4", feedCell.optionsCollectionView, "v5", feedCell.totalVotesLabel, "v6", feedCell.dividerLineView2, "v7", feedCell.contentViewButtons));
                }
                else
                {
                    feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(44)]-4-[v1]-4-[v2(1)][v3(<=176)]-8-[v4(24)]-8-[v5(1)][v6(44)]|", new NSLayoutFormatOptions(), "v0", feedCell.profileImageView, "v1", feedCell.questionText, "v2", feedCell.dividerLineView, "v3", feedCell.optionsCollectionView, "v4", feedCell.totalVotesLabel, "v5", feedCell.dividerLineView2, "v6", feedCell.contentViewButtons));
                }
            }

            feedCell.updateTotalVotes(survey.totalVotes);
            feedCell.updateTotalComments(survey.totalComments);

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

            feedCell.moreButton.AddTarget(this, new Selector("MoreSelector:"), UIControlEvent.TouchUpInside);
            List<Object> moreValues = new List<Object>();
            moreValues.Add(survey);
            moreValues.Add(feedCell);
            moreValues.Add(this);
            feedCell.moreButton.Params = moreValues;

            feedCell.commentsLabel.AddTarget(this, new Selector("CommentSelector:"), UIControlEvent.TouchUpInside);
            List<Object> commentLabelValues = new List<Object>();
            commentLabelValues.Add(survey);
            commentLabelValues.Add(indexPathRow);
            commentLabelValues.Add(this.NavigationController);
            feedCell.commentsLabel.Params = commentLabelValues;

            feedCell.totalVotesLabel.AddTarget(this, new Selector("ResultSelector:"), UIControlEvent.TouchUpInside);
            List<Object> resultVotesValues = new List<Object>();
            resultVotesValues.Add(survey);
            resultVotesValues.Add(indexPathRow);
            resultVotesValues.Add(this.NavigationController);
            feedCell.totalVotesLabel.Params = resultVotesValues;

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
        }

        public static nfloat getHeightForFeedCell(SurveyModel survey, nfloat width)
        {
            var rect = new NSString(survey.question.text).GetBoundingRect(new CGSize(width, 1000), NSStringDrawingOptions.UsesFontLeading | NSStringDrawingOptions.UsesLineFragmentOrigin, new UIStringAttributes() { Font = UIFont.BoldSystemFontOfSize(16) }, null);

            var optionsHeight = 176;

            if (survey.type == SurveyType.Text.ToString())
            {
                optionsHeight = survey.options.Count * 44;
            }

            // Heights of the vertical components to format the cell dinamic height
            var knownHeight = 0;
            DateTime outputDateTimeValue;
            if (survey.finishDate != null &&
                DateTime.TryParseExact(survey.finishDate, "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out outputDateTimeValue) &&
                outputDateTimeValue < DateTime.Now)
            {

                knownHeight = 8 + 44 + 8 + 32 + 4 + 4 + optionsHeight + 8 + 24 + 8 + 44;
            }
            else
            {
                knownHeight = 8 + 44 + 4 + 4 + optionsHeight + 8 + 24 + 8 + 44;
            }

            return rect.Height + knownHeight + 25;
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
                var feedCellHeight = FeedController.getHeightForFeedCell(surveys[indexPath.Row], collectionView.Frame.Width);

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
        public UIView dividerLineView { get; set; }
        public UIOptionsTableView optionsTableView { get; set; }
        public OptionsTableViewSource optionsTableViewSource { get; set; }
        public UIOptionsCollectionView optionsCollectionView { get; set; }
        public OptionsCollectionViewSource optionsCollectionViewSource { get; set; }
        public OptionsCollectionViewDelegate optionsCollectionViewDelegate { get; set; }
        public UIFeedButton totalVotesLabel { get; set; }
        public UIFeedButton commentsLabel { get; set; }
        public UIView dividerLineView2 { get; set; }
        public UIFeedButton commentButton { get; set; }
        public UIFeedButton resultButton { get; set; }
        public UIFeedButton moreButton { get; set; }
        public UIView dividerLineButtons { get; set; }
        public UIView contentViewButtons { get; set; }

        public static NSString optionCellId = new NSString("optionCell");
        public static string[] alphabet = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };

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

            finishedLabel = new UILabel();
            finishedLabel.BackgroundColor = UIColor.FromRGB(250, 60, 60);
            finishedLabel.TextColor = UIColor.White;
            finishedLabel.TextAlignment = UITextAlignment.Center;
            finishedLabel.Font = UIFont.BoldSystemFontOfSize(14);
            finishedLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            questionText = new UITextView();
            questionText.Font = UIFont.BoldSystemFontOfSize(14);
            questionText.BackgroundColor = UIColor.Clear;
            questionText.ScrollEnabled = false;
            questionText.Editable = false;
            questionText.TranslatesAutoresizingMaskIntoConstraints = false;

            dividerLineView = new UIView();
            dividerLineView.BackgroundColor = UIColor.FromRGBA(nfloat.Parse("0.88"), nfloat.Parse("0.89"), nfloat.Parse("0.90"), nfloat.Parse("1"));
            dividerLineView.TranslatesAutoresizingMaskIntoConstraints = false;

            optionsTableView = new UIOptionsTableView();
            optionsTableView.RegisterClassForCellReuse(typeof(OptionTableViewCell), optionCellId);

            optionsTableViewSource = new OptionsTableViewSource();

            optionsCollectionView = new UIOptionsCollectionView(new CGRect(), new UICollectionViewFlowLayout()
            {
                ScrollDirection = UICollectionViewScrollDirection.Horizontal
            });
            optionsCollectionView.BackgroundColor = UIColor.White;
            optionsCollectionView.RegisterClassForCell(typeof(OptionCollectionViewCell), optionCellId);

            optionsCollectionViewSource = new OptionsCollectionViewSource();

            optionsCollectionViewDelegate = new OptionsCollectionViewDelegate();

            totalVotesLabel = new UIFeedButton();
            totalVotesLabel.Font = UIFont.SystemFontOfSize(12);
            totalVotesLabel.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
            totalVotesLabel.SetTitleColor(UIColor.FromRGBA(nfloat.Parse("0.60"), nfloat.Parse("0.63"), nfloat.Parse("0.67"), nfloat.Parse("1")), UIControlState.Normal);
            totalVotesLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            totalVotesLabel.TitleEdgeInsets = new UIEdgeInsets(0, 8, 0, 0);

            commentsLabel = new UIFeedButton();
            commentsLabel.Font = UIFont.SystemFontOfSize(12);
            commentsLabel.HorizontalAlignment = UIControlContentHorizontalAlignment.Right;
            commentsLabel.SetTitleColor(UIColor.FromRGBA(nfloat.Parse("0.60"), nfloat.Parse("0.63"), nfloat.Parse("0.67"), nfloat.Parse("1")), UIControlState.Normal);
            commentsLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            commentsLabel.TitleEdgeInsets = new UIEdgeInsets(0, 8, 0, 0);

            dividerLineView2 = new UIView();
            dividerLineView2.BackgroundColor = UIColor.FromRGBA(nfloat.Parse("0.88"), nfloat.Parse("0.89"), nfloat.Parse("0.90"), nfloat.Parse("1"));
            dividerLineView2.TranslatesAutoresizingMaskIntoConstraints = false;

            resultButton = buttonForTitle(title: "Result", imageName: "result");
            commentButton = buttonForTitle(title: "Comment", imageName: "comment");
            moreButton = buttonForTitle(title: "", imageName: "More");

            dividerLineButtons = new UIView();
            dividerLineButtons.BackgroundColor = UIColor.FromRGBA(nfloat.Parse("0.88"), nfloat.Parse("0.89"), nfloat.Parse("0.90"), nfloat.Parse("1"));
            dividerLineButtons.TranslatesAutoresizingMaskIntoConstraints = false;

            contentViewButtons = new UIView();
            contentViewButtons.AddSubviews(resultButton, dividerLineButtons, commentButton);
            contentViewButtons.TranslatesAutoresizingMaskIntoConstraints = false;

            AddSubview(profileImageView);
            AddSubview(nameLabel);
            AddSubview(moreButton);
            AddSubview(questionText);
            AddSubview(dividerLineView);
            AddSubview(totalVotesLabel);
            AddSubview(commentsLabel);
            AddSubview(dividerLineView2);
            AddSubview(contentViewButtons);

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-8-[v0(44)]-8-[v1]-[v2]-12-|", new NSLayoutFormatOptions(), "v0", profileImageView, "v1", nameLabel, "v2", moreButton));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-4-[v0]-4-|", new NSLayoutFormatOptions(), "v0", questionText));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", dividerLineView));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-12-[v0(v1)]-[v1]-12-|", NSLayoutFormatOptions.AlignAllCenterY, "v0", totalVotesLabel, "v1", commentsLabel));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", dividerLineView2));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", contentViewButtons));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0(v2)][v1(1)][v2]|", new NSLayoutFormatOptions(), "v0", resultButton, "v1", dividerLineButtons, "v2", commentButton));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-12-[v0]", new NSLayoutFormatOptions(), "v0", nameLabel));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-12-[v0]", new NSLayoutFormatOptions(), "v0", moreButton));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0(44)]|", new NSLayoutFormatOptions(), "v0", resultButton));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0]-8-|", new NSLayoutFormatOptions(), "v0", dividerLineButtons));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0(44)]|", new NSLayoutFormatOptions(), "v0", commentButton));
        }

        public UIFeedButton buttonForTitle(string title, string imageName)
        {
            var button = new UIFeedButton();
            button.SetTitle(title, UIControlState.Normal);
            button.SetTitleColor(UIColor.FromRGBA(nfloat.Parse("0.56"), nfloat.Parse("0.58"), nfloat.Parse("0.63"), nfloat.Parse("1")), UIControlState.Normal);
            button.TitleLabel.Font = UIFont.BoldSystemFontOfSize(14);

            if (!imageName.Equals(""))
            {
                button.SetImage(UIImage.FromBundle(imageName), UIControlState.Normal);
            }

            button.TitleEdgeInsets = new UIEdgeInsets(0, 8, 0, 0);
            button.TranslatesAutoresizingMaskIntoConstraints = false;
            return button;
        }

        public void updateTotalVotes(int totalVotes)
        {
            if (totalVotes == 1)
            {
                this.totalVotesLabel.SetTitle("1 Vote", UIControlState.Normal);
            }
            else if (totalVotes == 0)
            {
                this.totalVotesLabel.SetTitle("0 Votes", UIControlState.Normal);
            }
            else
            {
                this.totalVotesLabel.SetTitle(Common.FormatNumberAbbreviation(totalVotes) + " Votes", UIControlState.Normal);
            }
        }

        public void updateTotalComments(int totalComments)
        {
            if (totalComments == 1)
            {
                this.commentsLabel.SetTitle("1 Comment", UIControlState.Normal);
            }
            else if (totalComments == 0)
            {
                this.commentsLabel.SetTitle("0 Comments", UIControlState.Normal);
            }
            else
            {
                this.commentsLabel.SetTitle(Common.FormatNumberAbbreviation(totalComments) + " Comments", UIControlState.Normal);
            }
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

            optionCell.optionLetterLabel.Text = FeedCollectionViewCell.alphabet[indexPath.Row];
            optionCell.optionLabel.Text = survey.options[indexPath.Row].text;

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
        public UILabel optionLetterLabel { get; set; }
        public UILabel optionLabel { get; set; }
        public UIImageView optionCheckImage { get; set; }
        public UIImageView optionEmptyCircle { get; set; }

        protected OptionTableViewCell(IntPtr handle) : base(handle)
        {
            isSelected = false;

            optionLetterLabel = new UILabel();
            optionLetterLabel.Font = UIFont.BoldSystemFontOfSize(16);
            optionLetterLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            optionLabel = new UILabel();
            optionLabel.Font = UIFont.SystemFontOfSize(14);
            optionLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            optionCheckImage = new UIImageView(UIImage.FromBundle("OptionCheck"));
            optionCheckImage.Frame = new CGRect(0, 0, 38, 38);

            optionEmptyCircle = new UIImageView(UIImage.FromBundle("EmptyCircleText"));
            optionEmptyCircle.Frame = new CGRect(0, 0, 38, 38);

            ContentView.Add(optionLetterLabel);
            ContentView.Add(optionLabel);

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-35-[v0(25)]-10-[v1]-8-|", new NSLayoutFormatOptions(), "v0", optionLetterLabel, "v1", optionLabel));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(25)]", new NSLayoutFormatOptions(), "v0", optionLetterLabel));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(25)]", new NSLayoutFormatOptions(), "v0", optionLabel));
        }
    }

    public class OptionsCollectionViewSource : UICollectionViewSource
    {
        public SurveyModel survey { get; set; }

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

            if (survey.options[indexPath.Row].image != null)
            {
                Utils.SetImageFromNSUrlSession(survey.options[indexPath.Row].image, optionCell.optionImageView, this);
            }

            optionCell.optionLetterLabel.Text = "  " + FeedCollectionViewCell.alphabet[indexPath.Row] + "  ";
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
            return new CGSize(270, 220);
        }

        public override nfloat GetMinimumLineSpacingForSection(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
        {
            return 1;
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
        public UILabel optionLetterLabel { get; set; }
        public UILabel optionLabel { get; set; }
        public UIImageView optionCheckImageView { get; set; }
        public UIImageView optionEmptyCircle { get; set; }

        [Export("initWithFrame:")]
        public OptionCollectionViewCell(CGRect frame) : base(frame)
        {
            isSelected = false;

            optionImageView = new UIImageView();
            optionImageView.Image = null;
            optionImageView.ContentMode = UIViewContentMode.ScaleToFill;
            optionImageView.Layer.MasksToBounds = true;
            optionImageView.TranslatesAutoresizingMaskIntoConstraints = false;

            optionCheckOpacityView = new UIView(new CGRect(0, 0, 270, 220));
            optionCheckOpacityView.BackgroundColor = UIColor.Green;
            optionCheckOpacityView.Alpha = 0.3f;
            optionCheckOpacityView.TranslatesAutoresizingMaskIntoConstraints = false;
            optionCheckOpacityView.Hidden = true;

            optionImageView.AddSubview(optionCheckOpacityView);

            optionImageView.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", optionCheckOpacityView));
            optionImageView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0]|", new NSLayoutFormatOptions(), "v0", optionCheckOpacityView));

            optionView = new UIView(new CGRect(0, 0, 270, 25));
            optionView.BackgroundColor = UIColor.Clear;
            optionView.Alpha = 0.8f;
            var colorTop = UIColor.Black.CGColor;
            var colorBottom = UIColor.Clear.CGColor;
            var gradientLayer = new CAGradientLayer();
            gradientLayer.Colors = new CGColor[] { colorTop, colorBottom };
            gradientLayer.Locations = new NSNumber[] { 0.3, 0.9 };
            gradientLayer.Frame = optionView.Bounds;
            optionView.Layer.InsertSublayer(gradientLayer, 0);
            optionView.TranslatesAutoresizingMaskIntoConstraints = false;

            optionLetterLabel = new UILabel();
            optionLetterLabel.Font = UIFont.BoldSystemFontOfSize(14);
            optionLetterLabel.TextColor = UIColor.White;
            optionLetterLabel.BackgroundColor = UIColor.Clear;
            optionLetterLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            optionLabel = new UILabel();
            optionLabel.Font = UIFont.SystemFontOfSize(12);
            optionLabel.TextColor = UIColor.White;
            optionLabel.BackgroundColor = UIColor.Clear;
            optionLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            optionView.AddSubviews(optionLetterLabel, optionLabel);

            optionView.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0(25)][v1]|", new NSLayoutFormatOptions(), "v0", optionLetterLabel, "v1", optionLabel));
            optionView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0(25)]", new NSLayoutFormatOptions(), "v0", optionLetterLabel));
            optionView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0(25)]", new NSLayoutFormatOptions(), "v0", optionLabel));

            optionCheckImageView = new UIImageView();
            optionCheckImageView.Image = UIImage.FromBundle("OptionCheck");
            optionCheckImageView.TranslatesAutoresizingMaskIntoConstraints = false;
            optionCheckImageView.Hidden = true;

            optionEmptyCircle = new UIImageView();
            optionEmptyCircle.Image = UIImage.FromBundle("EmptyCircleImage");
            optionEmptyCircle.TranslatesAutoresizingMaskIntoConstraints = false;
            optionEmptyCircle.Hidden = false;

            AddSubview(optionImageView);
            AddSubview(optionView);
            AddSubview(optionCheckImageView);
            AddSubview(optionEmptyCircle);

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", optionImageView));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", optionView));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[v0(40)]-4-|", new NSLayoutFormatOptions(), "v0", optionCheckImageView));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[v0(40)]-4-|", new NSLayoutFormatOptions(), "v0", optionEmptyCircle));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0]|", new NSLayoutFormatOptions(), "v0", optionImageView));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-20-[v0(25)]", new NSLayoutFormatOptions(), "v0", optionView));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0(40)]-26-|", new NSLayoutFormatOptions(), "v0", optionCheckImageView));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0(40)]-26-|", new NSLayoutFormatOptions(), "v0", optionEmptyCircle));
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
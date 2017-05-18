using Foundation;
using System;
using System.Collections.Generic;
using UIKit;
using CoreGraphics;
using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Business;
using CoreFoundation;
using Askker.App.PortableLibrary.Util;
using Askker.App.PortableLibrary.Enums;
using ObjCRuntime;

namespace Askker.App.iOS
{
    public partial class FeedController : UICollectionViewController
    {
        public static List<SurveyModel> surveys { get; set; }
        public static NSCache imageCache = new NSCache();
        public static VoteManager voteManager = new VoteManager();
        public bool filterMine { get; set; }
        public bool filterForMe { get; set; }
        public bool filterFinished { get; set; }
        public UIActivityIndicatorView indicator;
        public UIRefreshControl refreshControl;
        public static NSString feedCellId = new NSString("feedCell");
        SurveyModel survey;
        FeedCollectionViewCell surveyCell;
        public MenuViewController menuViewController { get; set; }

        public FeedController (IntPtr handle) : base (handle)
        {
            indicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
            indicator.Frame = new CoreGraphics.CGRect(0.0, 0.0, 80.0, 80.0);
            indicator.Center = this.View.Center;
            Add(indicator);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            imageCache.RemoveAllObjects();
                        
            feedCollectionView.RegisterClassForCell(typeof(FeedCollectionViewCell), feedCellId);
            feedCollectionView.BackgroundColor = UIColor.FromWhiteAlpha(nfloat.Parse("0.95"), 1);
            feedCollectionView.Delegate = new FeedCollectionViewDelegate();
            feedCollectionView.AlwaysBounceVertical = true;

            surveys = new List<SurveyModel>();

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

            MenuViewController.feedMenu.EditButton.TouchUpInside += EditButton_TouchUpInside;
            MenuViewController.feedMenu.CleanButton.TouchUpInside += CleanButton_TouchUpInside;
            MenuViewController.feedMenu.FinishButton.TouchUpInside += FinishButton_TouchUpInside;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);


            MenuViewController.feedMenu.EditButton.TouchUpInside -= EditButton_TouchUpInside;
            MenuViewController.feedMenu.CleanButton.TouchUpInside -= CleanButton_TouchUpInside;
            MenuViewController.feedMenu.FinishButton.TouchUpInside -= FinishButton_TouchUpInside;
        }

        public override void ViewWillAppear(bool animated)
        {
            indicator.StartAnimating();
            fetchSurveys(filterMine, filterForMe, filterFinished);
        }

        private async void FinishButton_TouchUpInside(object sender, EventArgs e)
        {
            nint button = await Utils.ShowAlert("Close Survey", "The survey will be closed. Continue?", "Ok", "Cancel");

            try
            {
                if (button == 0)
                {
                    survey.finishDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    await new FeedManager().UpdateSurvey(survey, LoginController.tokenModel.access_token);
                    surveys.Remove(survey);
                    this.feedCollectionView.ReloadData();
                }

                MenuViewController.feedMenu.Hidden = true;
                MenuViewController.sidebarController.View.Alpha = 1f;
            }
            catch (Exception ex)
            {
                Utils.HandleException(ex);
            }
        }

        private async void CleanButton_TouchUpInside(object sender, EventArgs e)
        {
            nint button = await Utils.ShowAlert("Clean Votes", "All survey votes will be deleted. Continue?", "Ok", "Cancel");

            try
            {
                if (button == 0)
                {
                    await new FeedManager().CleanVotes(survey.userId + survey.creationDate, LoginController.tokenModel.access_token);

                    survey.optionSelected = null;
                    if (survey.type == SurveyType.Text.ToString())
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
            }
            catch (Exception ex)
            {
                Utils.HandleException(ex);
            }
        }

        private async void EditButton_TouchUpInside(object sender, EventArgs e)
        {
            if (survey.totalVotes > 0)
            {
                nint optionButton = await Utils.ShowAlert("Edit", "The survey have votes. Please clean the votes to edit the survey.", "OK");
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
                        this.PresentViewController(rootController, true, null);
                    }
                    //this.PresentViewController(CreateSurveyController.NavigationController, true, null);
                }
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
            }
            catch (Exception ex)
            {
                Utils.HandleException(ex);
            }

            indicator.StopAnimating();
            refreshControl.EndRefreshing();
            feedCollectionView.ReloadData();
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return surveys.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var feedCell = collectionView.DequeueReusableCell(feedCellId, indexPath) as FeedCollectionViewCell;

            if (surveys[indexPath.Row].profilePicture != null)
            {
                var url = new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + surveys[indexPath.Row].profilePicture);

                var imageFromCache = (UIImage)imageCache.ObjectForKey(NSString.FromObject(url.AbsoluteString));
                if (imageFromCache != null)
                {
                    feedCell.profileImageView.Image = imageFromCache;
                }
                else
                {
                    var task = NSUrlSession.SharedSession.CreateDataTask(url, (data, response, error) =>
                    {
                        if (response == null)
                        {
                            feedCell.profileImageView.Image = UIImage.FromBundle("Profile");
                        }
                        else
                        {
                            try
                            {
                                DispatchQueue.MainQueue.DispatchAsync(() =>
                                {
                                    var imageToCache = UIImage.LoadFromData(data);

                                    feedCell.profileImageView.Image = imageToCache;

                                    if (imageToCache != null)
                                    {
                                        imageCache.SetObjectforKey(imageToCache, NSString.FromObject(url.AbsoluteString));
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                Utils.HandleException(ex);
                            }
                        }
                    });
                    task.Resume();
                }
            }

            var attributedText = new NSMutableAttributedString(surveys[indexPath.Row].userName, UIFont.BoldSystemFontOfSize(14));
            attributedText.Append(new NSAttributedString("\nto Public", UIFont.SystemFontOfSize(12), UIColor.FromRGBA(nfloat.Parse("0.60"), nfloat.Parse("0.63"), nfloat.Parse("0.67"), nfloat.Parse("1"))));

            var paragraphStyle = new NSMutableParagraphStyle();
            paragraphStyle.LineSpacing = 4;
            attributedText.AddAttribute(new NSString("ParagraphStyle"), paragraphStyle, new NSRange(0, attributedText.Length));

            feedCell.nameLabel.AttributedText = attributedText;

            DateTime outputDateTimeValue;
            bool finished = false;
            if (surveys[indexPath.Row].finishDate != null &&
                DateTime.TryParseExact(surveys[indexPath.Row].finishDate, "yyyy-MM-dd HH:mm:ss", 
                                       System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out outputDateTimeValue) &&
                outputDateTimeValue < DateTime.Now) {
                
                feedCell.finishedLabel.Text = "Finished";
                feedCell.moreButton.Hidden = true;
                feedCell.optionsTableView.AllowsSelection = false;
                feedCell.optionsCollectionView.AllowsSelection = false;
                finished = true;
            }
            else
            {
                feedCell.finishedLabel.Text = "";
                feedCell.moreButton.Hidden = false;
                feedCell.optionsTableView.AllowsSelection = true;
                feedCell.optionsCollectionView.AllowsSelection = true;
                finished = false;
            }

            if (!surveys[indexPath.Row].userId.Equals(LoginController.userModel.id))
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

            feedCell.questionText.Text = surveys[indexPath.Row].question.text;

            if (surveys[indexPath.Row].type == SurveyType.Text.ToString())
            {                
                feedCell.optionsTableView.ContentMode = UIViewContentMode.ScaleAspectFill;
                feedCell.optionsTableView.Layer.MasksToBounds = true;
                feedCell.optionsTableView.TranslatesAutoresizingMaskIntoConstraints = false;
                feedCell.optionsTableView.ContentInset = new UIEdgeInsets(0, -10, 0, 0);
                feedCell.optionsTableView.Tag = indexPath.Row;

                feedCell.optionsTableViewSource.options = surveys[indexPath.Row].options;
                feedCell.optionsTableView.Source = feedCell.optionsTableViewSource;
                feedCell.optionsTableView.ReloadData();

                feedCell.optionsCollectionView.RemoveFromSuperview();
                feedCell.AddSubview(feedCell.optionsTableView);

                feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", feedCell.optionsTableView));
                feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(44)]-4-[v1]-4-[v2(1)][v3(<=176)]-8-[v4(24)]-8-[v5(1)][v6(44)]|", new NSLayoutFormatOptions(), "v0", feedCell.profileImageView, "v1", feedCell.questionText, "v2", feedCell.dividerLineView, "v3", feedCell.optionsTableView, "v4", feedCell.totalVotesLabel, "v5", feedCell.dividerLineView2, "v6", feedCell.contentViewButtons));
            }
            else
            {
                feedCell.optionsCollectionView.TranslatesAutoresizingMaskIntoConstraints = false;
                feedCell.optionsCollectionView.Tag = indexPath.Row;

                feedCell.optionsCollectionViewSource.options = surveys[indexPath.Row].options;
                feedCell.optionsCollectionView.Source = feedCell.optionsCollectionViewSource;
                feedCell.optionsCollectionViewDelegate.optionsCollectionViewSource = feedCell.optionsCollectionViewSource;
                feedCell.optionsCollectionView.Delegate = feedCell.optionsCollectionViewDelegate;
                feedCell.optionsCollectionView.ReloadData();

                feedCell.optionsTableView.RemoveFromSuperview();
                feedCell.AddSubview(feedCell.optionsCollectionView);

                feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", feedCell.optionsCollectionView));
                feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(44)]-4-[v1]-4-[v2(1)][v3(<=176)]-8-[v4(24)]-8-[v5(1)][v6(44)]|", new NSLayoutFormatOptions(), "v0", feedCell.profileImageView, "v1", feedCell.questionText, "v2", feedCell.dividerLineView, "v3", feedCell.optionsCollectionView, "v4", feedCell.totalVotesLabel, "v5", feedCell.dividerLineView2, "v6", feedCell.contentViewButtons));
            }

            if (surveys[indexPath.Row].totalVotes == 1)
            {
                feedCell.totalVotesLabel.Text = "1 Vote";
            }
            else
            {
                feedCell.totalVotesLabel.Text = Common.FormatNumberAbbreviation(surveys[indexPath.Row].totalVotes) + " Votes";
            }

            if (surveys[indexPath.Row].totalComments == 1)
            {
                feedCell.commentsLabel.Text = "1 Comment";
            }
            else
            {
                feedCell.commentsLabel.Text = Common.FormatNumberAbbreviation(surveys[indexPath.Row].totalComments) + " Comments";
            }

            feedCell.commentButton.AddTarget(this, new ObjCRuntime.Selector("CommentSelector:"), UIControlEvent.TouchUpInside);
            List<Object> commentValues = new List<Object>();
            commentValues.Add(indexPath.Row);
            commentValues.Add((float)feedCell.Frame.Height + 64);
            feedCell.commentButton.Params = commentValues;

            feedCell.resultButton.AddTarget(this, new ObjCRuntime.Selector("ResultSelector:"), UIControlEvent.TouchUpInside);
            List<Object> resultValues = new List<Object>();
            resultValues.Add(indexPath);
            resultValues.Add((float)feedCell.Frame.Height + 64);
            feedCell.resultButton.Params = resultValues;

            feedCell.moreButton.AddTarget(this, new ObjCRuntime.Selector("MoreSelector:"), UIControlEvent.TouchUpInside);
            List<Object> moreValues = new List<Object>();
            moreValues.Add(indexPath.Row);
            moreValues.Add(feedCell);
            feedCell.moreButton.Params = moreValues;

            var feedTapGestureRecognizer = new UIFeedTapGestureRecognizer(this, new Selector("TapProfilePictureSelector:"));
            List<Object> tapProfilePictureValues = new List<Object>();
            tapProfilePictureValues.Add(surveys[indexPath.Row].userId);
            feedTapGestureRecognizer.Params = tapProfilePictureValues;
            feedCell.profileImageView.AddGestureRecognizer(feedTapGestureRecognizer);

            return feedCell;
        }

        [Export("CommentSelector:")]
        private void CommentSelector(UIFeedButton button)
        {
            var commentController = menuViewController.Storyboard.InstantiateViewController("CommentViewController") as CommentViewController;
            commentController.feedHead = feedCollectionView;
            commentController.headHeight = (float)button.Params.ToArray()[1];

            survey = surveys[(int)button.Params.ToArray()[0]];
            surveys.Clear();
            surveys.Add(survey);

            commentController.survey = survey;

            menuViewController.NavigationController.PushViewController(commentController, true);
        }

        [Export("ResultSelector:")]
        private void ResultSelector(UIFeedButton button)
        {
            var resultController = this.Storyboard.InstantiateViewController("ResultViewController") as ResultViewController;
            resultController.feedHead = feedCollectionView;
            resultController.headHeight = (float)button.Params.ToArray()[1];
            resultController.feedCellIndexPath = (NSIndexPath)button.Params.ToArray()[0];

            survey = surveys[((NSIndexPath)button.Params.ToArray()[0]).Row];
            surveys.Clear();
            surveys.Add(survey);

            this.NavigationController.PushViewController(resultController, true);
        }

        [Export("MoreSelector:")]
        private void MoreSelector(UIFeedButton button)
        {
            survey = surveys[(int)button.Params.ToArray()[0]];
            surveyCell = (FeedCollectionViewCell)button.Params.ToArray()[1];

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
            Utils.OpenUserProfile(menuViewController.NavigationController, (string)tapGesture.Params.ToArray()[0]);
        }

        public static async void saveVote(int surveyIndex, int optionId)
        {
            try
            {
                surveys[surveyIndex].optionSelected = optionId;

                SurveyVoteModel surveyVoteModel = new SurveyVoteModel();
                surveyVoteModel.surveyId = surveys[surveyIndex].userId + surveys[surveyIndex].creationDate;
                surveyVoteModel.optionId = optionId;
                surveyVoteModel.user = new User();
                surveyVoteModel.user.id = LoginController.userModel.id;
                surveyVoteModel.user.gender = LoginController.userModel.gender;
                surveyVoteModel.user.age = LoginController.userModel.age;
                surveyVoteModel.user.city = LoginController.userModel.city;
                surveyVoteModel.user.country = LoginController.userModel.country;

                await voteManager.Vote(surveyVoteModel, "");

                if (LoginController.userModel.id != surveys[surveyIndex].userId)
                {
                    UserNotificationModel userNotificationModel = new UserNotificationModel();
                    userNotificationModel.notificationDate = "";
                    userNotificationModel.userId = surveys[surveyIndex].userId;
                    userNotificationModel.notificationUser = new UserFriendModel(LoginController.userModel.id, LoginController.userModel.name, LoginController.userModel.profilePicturePath);
                    userNotificationModel.type = UserNotificationType.SurveyVote.ToString();

                    if (surveys[surveyIndex].question.text.Length > 25)
                    {
                        userNotificationModel.text = LoginController.userModel.name + " voted on \"" + surveys[surveyIndex].question.text.Substring(0, 25) + "...\"";
                    }
                    else
                    {
                        userNotificationModel.text = LoginController.userModel.name + " voted on \"" + surveys[surveyIndex].question.text + "\"";
                    }

                    userNotificationModel.link = surveys[surveyIndex].userId + surveys[surveyIndex].creationDate;
                    userNotificationModel.isDismissed = 0;

                    await new NotificationManager().SetUserNotification(userNotificationModel, LoginController.tokenModel.access_token);
                }
            }
            catch (Exception ex)
            {
                Utils.HandleException(ex);
            }
        }
    }

    public class FeedCollectionViewDelegate : UICollectionViewDelegateFlowLayout
    {
        public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
        {
            var question = FeedController.surveys[indexPath.Row].question.text;
            if (!question.Equals(""))
            {
                var rect = new NSString(question).GetBoundingRect(new CGSize(collectionView.Frame.Width, 1000), NSStringDrawingOptions.UsesFontLeading | NSStringDrawingOptions.UsesLineFragmentOrigin, new UIStringAttributes() { Font = UIFont.SystemFontOfSize(14) }, null);

                var optionsHeight = 176;

                if (FeedController.surveys[indexPath.Row].type == SurveyType.Text.ToString() && FeedController.surveys[indexPath.Row].options.Count < 4)
                {
                    optionsHeight = FeedController.surveys[indexPath.Row].options.Count * 44;
                }

                // Heights of the vertical components to format the cell dinamic height
                var knownHeight = 8 + 44 + 4 + 4 + optionsHeight + 8 + 24 + 8 + 44;

                return new CGSize(collectionView.Frame.Width, rect.Height + knownHeight + 25);
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
        public UITableView optionsTableView { get; set; }
        public OptionsTableViewSource optionsTableViewSource { get; set; }
        public UICollectionView optionsCollectionView { get; set; }
        public OptionsCollectionViewSource optionsCollectionViewSource { get; set; }
        public OptionsCollectionViewDelegate optionsCollectionViewDelegate { get; set; }
        public UILabel totalVotesLabel { get; set; }
        public UILabel commentsLabel { get; set; }
        public UIView dividerLineView2 { get; set; }
        public UIFeedButton commentButton { get; set; }
        public UIFeedButton resultButton { get; set; }
        public UIFeedButton moreButton { get; set; }
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
            finishedLabel.TextColor = UIColor.Red;
            finishedLabel.Font = UIFont.SystemFontOfSize(14);
            finishedLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            questionText = new UITextView();
            questionText.Font = UIFont.SystemFontOfSize(14);
            questionText.BackgroundColor = UIColor.Clear;
            questionText.ScrollEnabled = false;
            questionText.Editable = false;
            questionText.TranslatesAutoresizingMaskIntoConstraints = false;

            dividerLineView = new UIView();
            dividerLineView.BackgroundColor = UIColor.FromRGBA(nfloat.Parse("0.88"), nfloat.Parse("0.89"), nfloat.Parse("0.90"), nfloat.Parse("1"));
            dividerLineView.TranslatesAutoresizingMaskIntoConstraints = false;

            optionsTableView = new UITableView();
            optionsTableView.RegisterClassForCellReuse(typeof(OptionTableViewCell), optionCellId);

            optionsTableViewSource = new OptionsTableViewSource();

            optionsCollectionView = new UICollectionView(new CGRect(), new UICollectionViewFlowLayout()
            {
                ScrollDirection = UICollectionViewScrollDirection.Horizontal
            });
            optionsCollectionView.BackgroundColor = UIColor.FromWhiteAlpha(nfloat.Parse("0.95"), 1);
            optionsCollectionView.RegisterClassForCell(typeof(OptionCollectionViewCell), optionCellId);

            optionsCollectionViewSource = new OptionsCollectionViewSource();

            optionsCollectionViewDelegate = new OptionsCollectionViewDelegate();

            totalVotesLabel = new UILabel();
            totalVotesLabel.Font = UIFont.SystemFontOfSize(12);
            totalVotesLabel.TextAlignment = UITextAlignment.Left;
            totalVotesLabel.TextColor = UIColor.FromRGBA(nfloat.Parse("0.60"), nfloat.Parse("0.63"), nfloat.Parse("0.67"), nfloat.Parse("1"));
            totalVotesLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            commentsLabel = new UILabel();
            commentsLabel.Font = UIFont.SystemFontOfSize(12);
            commentsLabel.TextAlignment = UITextAlignment.Right;
            commentsLabel.TextColor = UIColor.FromRGBA(nfloat.Parse("0.60"), nfloat.Parse("0.63"), nfloat.Parse("0.67"), nfloat.Parse("1"));
            commentsLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            dividerLineView2 = new UIView();
            dividerLineView2.BackgroundColor = UIColor.FromRGBA(nfloat.Parse("0.88"), nfloat.Parse("0.89"), nfloat.Parse("0.90"), nfloat.Parse("1"));
            dividerLineView2.TranslatesAutoresizingMaskIntoConstraints = false;

            commentButton = buttonForTitle(title: "Comment", imageName: "comment");
            resultButton = buttonForTitle(title: "Result", imageName: "result");
            moreButton = buttonForTitle(title: "More", imageName: "more");

            contentViewButtons = new UIView();
            contentViewButtons.AddSubviews(commentButton, resultButton, moreButton);
            contentViewButtons.TranslatesAutoresizingMaskIntoConstraints = false;

            AddSubview(profileImageView);
            AddSubview(nameLabel);
            AddSubview(finishedLabel);
            AddSubview(questionText);
            AddSubview(dividerLineView);
            AddSubview(totalVotesLabel);
            AddSubview(commentsLabel);
            AddSubview(dividerLineView2);
            AddSubview(contentViewButtons);

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-8-[v0(44)]-8-[v1]-[v2]-12-|", new NSLayoutFormatOptions(), "v0", profileImageView, "v1", nameLabel, "v2", finishedLabel));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-4-[v0]-4-|", new NSLayoutFormatOptions(), "v0", questionText));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", dividerLineView));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-12-[v0(v1)]-[v1]-12-|", NSLayoutFormatOptions.AlignAllCenterY, "v0", totalVotesLabel, "v1", commentsLabel));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", dividerLineView2));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", contentViewButtons));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0(v2)][v1(v2)][v2]|", new NSLayoutFormatOptions(), "v0", commentButton, "v1", resultButton, "v2", moreButton));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-12-[v0]", new NSLayoutFormatOptions(), "v0", nameLabel));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-12-[v0]", new NSLayoutFormatOptions(), "v0", finishedLabel));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0(44)]|", new NSLayoutFormatOptions(), "v0", commentButton));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0(44)]|", new NSLayoutFormatOptions(), "v0", resultButton));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0(44)]|", new NSLayoutFormatOptions(), "v0", moreButton));
        }

        public UIFeedButton buttonForTitle(string title, string imageName)
        {
            var button = new UIFeedButton();
            button.SetTitle(title, UIControlState.Normal);
            button.SetTitleColor(UIColor.FromRGBA(nfloat.Parse("0.56"), nfloat.Parse("0.58"), nfloat.Parse("0.63"), nfloat.Parse("1")), UIControlState.Normal);
            button.TitleLabel.Font = UIFont.BoldSystemFontOfSize(14);
            //button.SetImage(new UIImage(imageName), UIControlState.Normal);
            button.TitleEdgeInsets = new UIEdgeInsets(0, 8, 0, 0);
            button.TranslatesAutoresizingMaskIntoConstraints = false;
            return button;
        }
    }

    public class OptionsTableViewSource : UITableViewSource
    {
        public List<Option> options { get; set; }

        public OptionsTableViewSource()
        {
            options = new List<Option>();
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return options.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(FeedCollectionViewCell.optionCellId, indexPath) as OptionTableViewCell;
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            cell.Tag = options[indexPath.Row].id;

            cell.optionLetterLabel.Text = FeedCollectionViewCell.alphabet[indexPath.Row];
            cell.optionLabel.Text = options[indexPath.Row].text;

            if (FeedController.surveys[(int)tableView.Tag].optionSelected == options[indexPath.Row].id)
            {
                var optionCheckImage = new UIImageView(UIImage.FromBundle("OptionCheck"));
                optionCheckImage.Frame = new CGRect(0, 0, 40, 40);
                cell.AccessoryView = optionCheckImage;
                tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);
            }
            else
            {
                cell.AccessoryView = null;
            }

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var optionCell = tableView.CellAt(indexPath);

            if (optionCell.AccessoryView == null)
            {
                var optionCheckImage = new UIImageView(UIImage.FromBundle("OptionCheck"));
                optionCheckImage.Frame = new CGRect(0, 0, 40, 40);
                optionCell.AccessoryView = optionCheckImage;

                FeedController.saveVote((int)tableView.Tag, (int)optionCell.Tag);
            }
        }

        public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
        {
            var optionCell = tableView.CellAt(indexPath);

            if (optionCell == null)
            {
                optionCell = this.GetCell(tableView, indexPath);
            }

            optionCell.AccessoryView = null;
        }
    }

    public partial class OptionTableViewCell : UITableViewCell
    {
        public UILabel optionLetterLabel { get; set; }
        public UILabel optionLabel { get; set; }

        protected OptionTableViewCell(IntPtr handle) : base(handle)
        {
            optionLetterLabel = new UILabel();
            optionLetterLabel.Font = UIFont.BoldSystemFontOfSize(16);
            optionLetterLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            optionLabel = new UILabel();
            optionLabel.Font = UIFont.SystemFontOfSize(14);
            optionLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            ContentView.Add(optionLetterLabel);
            ContentView.Add(optionLabel);

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-35-[v0(25)]-10-[v1]-8-|", new NSLayoutFormatOptions(), "v0", optionLetterLabel, "v1", optionLabel));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(25)]", new NSLayoutFormatOptions(), "v0", optionLetterLabel));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(25)]", new NSLayoutFormatOptions(), "v0", optionLabel));
        }
    }

    public class OptionsCollectionViewSource : UICollectionViewSource
    {
        public List<Option> options { get; set; }

        public OptionsCollectionViewSource()
        {
            options = new List<Option>();
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return options.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var optionCell = collectionView.DequeueReusableCell(FeedCollectionViewCell.optionCellId, indexPath) as OptionCollectionViewCell;
            optionCell.BackgroundColor = UIColor.White;
            optionCell.Tag = options[indexPath.Row].id;

            if (options[indexPath.Row].image != null)
            {
                var url = new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + options[indexPath.Row].image);

                var imageFromCache = (UIImage)FeedController.imageCache.ObjectForKey(NSString.FromObject(url.AbsoluteString));
                if (imageFromCache != null)
                {
                    optionCell.optionImageView.Image = imageFromCache;
                }
                else
                {
                    var task = NSUrlSession.SharedSession.CreateDataTask(url, (data, response, error) =>
                    {
                        if (response == null)
                        {
                            optionCell.optionImageView.Image = null;
                        }
                        else
                        {
                            try
                            {
                                DispatchQueue.MainQueue.DispatchAsync(() => {
                                    var imageToCache = UIImage.LoadFromData(data);

                                    optionCell.optionImageView.Image = imageToCache;

                                    if (imageToCache != null)
                                    {
                                        FeedController.imageCache.SetObjectforKey(imageToCache, NSString.FromObject(url.AbsoluteString));
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                Utils.HandleException(ex);
                            }
                        }
                    });
                    task.Resume();
                }
            }

            optionCell.optionLetterLabel.Text = "  " + FeedCollectionViewCell.alphabet[indexPath.Row] + "  ";
            optionCell.optionLabel.Text = options[indexPath.Row].text;

            if (FeedController.surveys[(int)collectionView.Tag].optionSelected == options[indexPath.Row].id)
            {
                optionCell.optionCheckImageView.Hidden = false;
                collectionView.SelectItem(indexPath, false, UICollectionViewScrollPosition.None);
            }
            else
            {
                optionCell.optionCheckImageView.Hidden = true;
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

        public OptionsCollectionViewDelegate()
        {
            optionsCollectionViewSource = new OptionsCollectionViewSource();
        }

        public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
        {
            return new CGSize(270, 220);
        }

        public override nfloat GetMinimumLineSpacingForSection(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
        {
            return 0;
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var optionCell = collectionView.CellForItem(indexPath) as OptionCollectionViewCell;

            if (optionCell.optionCheckImageView.Hidden)
            {
                optionCell.optionCheckImageView.Hidden = false;
                FeedController.saveVote((int)collectionView.Tag, (int)optionCell.Tag);
            }
        }

        public override void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var optionCell = collectionView.CellForItem(indexPath) as OptionCollectionViewCell;

            if (optionCell == null)
            {
                optionCell = optionsCollectionViewSource.GetCustomCell(collectionView, indexPath);
            }

            optionCell.optionCheckImageView.Hidden = true;
        }
    }

    public class OptionCollectionViewCell : UICollectionViewCell
    {
        public UIImageView optionImageView { get; set; }
        public UILabel optionLetterLabel { get; set; }
        public UILabel optionLabel { get; set; }
        public UIImageView optionCheckImageView { get; set; }

        [Export("initWithFrame:")]
        public OptionCollectionViewCell(CGRect frame) : base(frame)
        {
            optionImageView = new UIImageView();
            optionImageView.Image = null;
            optionImageView.ContentMode = UIViewContentMode.ScaleToFill;
            optionImageView.Layer.MasksToBounds = true;
            optionImageView.TranslatesAutoresizingMaskIntoConstraints = false;

            optionLetterLabel = new UILabel();
            optionLetterLabel.Font = UIFont.BoldSystemFontOfSize(14);
            optionLetterLabel.TextColor = UIColor.White;
            optionLetterLabel.BackgroundColor = UIColor.DarkGray;
            optionLetterLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            optionLabel = new UILabel();
            optionLabel.Font = UIFont.SystemFontOfSize(12);
            optionLabel.TextColor = UIColor.White;
            optionLabel.BackgroundColor = UIColor.DarkGray;
            optionLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            optionCheckImageView = new UIImageView();
            optionCheckImageView.Image = UIImage.FromBundle("OptionCheck");
            optionCheckImageView.Frame = new CGRect(0, 0, 50, 50);
            optionCheckImageView.TranslatesAutoresizingMaskIntoConstraints = false;
            optionCheckImageView.Hidden = true;

            AddSubview(optionImageView);
            AddSubview(optionLetterLabel);
            AddSubview(optionLabel);
            AddSubview(optionCheckImageView);

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", optionImageView));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-8-[v0(25)][v1]-8-|", new NSLayoutFormatOptions(), "v0", optionLetterLabel, "v1", optionLabel));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[v0]-(<=1)-[v1]", NSLayoutFormatOptions.AlignAllCenterY, "v0", this, "v1", optionCheckImageView));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0]|", new NSLayoutFormatOptions(), "v0", optionImageView));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-25-[v0(25)]", new NSLayoutFormatOptions(), "v0", optionLetterLabel));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-25-[v0(25)]", new NSLayoutFormatOptions(), "v0", optionLabel));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0]-(<=1)-[v1]", NSLayoutFormatOptions.AlignAllCenterX, "v0", this, "v1", optionCheckImageView));
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
}
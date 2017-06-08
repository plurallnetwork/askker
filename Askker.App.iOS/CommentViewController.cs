using Askker.App.iOS.Resources;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Util;
using BigTed;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace Askker.App.iOS
{
    public partial class CommentViewController : UIViewController
    {
        public UICollectionView feed { get; set; }
        public static List<SurveyCommentModel> comments { get; set; }
        public SurveyModel survey { get; set; }
        NSLayoutConstraint toolbarHeightConstraint;
        NSLayoutConstraint toolbarBottomConstraint;
        CommentAreaView commentArea = CommentAreaView.Create();

        public FeedCollectionViewCell feedCell { get; set; }
        public FeedController feedController { get; set; }
        public string userId { get; set; }
        public string creationDate { get; set; }
        public int indexPathRow { get; set; }
        public string commentDate { get; set; }

        public static NSString feedHeadId = new NSString("feedHeadId");
        public static NSString commentCellId = new NSString("commentCellId");

        //Variables used when the keyboard appears
        private UIView activeview;             // Controller that activated the keyboard
        private float scroll_amount = 0.0f;    // amount to scroll 
        private float bottom = 0.0f;           // bottom point
        private float offset = 10.0f;          // extra offset
        private bool moveViewUp = false;       // which direction are we moving

        //Variables used to resize the comment textView
        private UIView activeviewarea;         // CommentArea

        public CommentViewController (IntPtr handle) : base (handle)
        {            
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            comments = new List<SurveyCommentModel>();

            commentArea.BackgroundColor = UIColor.FromWhiteAlpha(nfloat.Parse("0.95"), 1);
            commentArea.Frame = new CGRect(0, View.Frame.Height - 46, 375, 46);
            commentArea.TranslatesAutoresizingMaskIntoConstraints = false;

            commentArea.CommentText.Started += CommentText_Started;
            commentArea.CommentText.Changed += CommentText_Changed;
            commentArea.CommentButton.TouchUpInside += CommentButton_TouchUpInside;

            feed = new UICollectionView(new CGRect(), new UICollectionViewFlowLayout());
            feed.BackgroundColor = UIColor.White;
            //feed.RegisterClassForCell(typeof(UICollectionViewCell), commentCellId);
            feed.RegisterNibForCell(UINib.FromName("CommentCell", NSBundle.MainBundle), commentCellId);
            feed.RegisterClassForSupplementaryView(typeof(UICollectionReusableView), UICollectionElementKindSection.Header, feedHeadId);
            feed.AlwaysBounceVertical = true;
            feed.TranslatesAutoresizingMaskIntoConstraints = false;

            View.AddSubview(feed);

            var pinLeft = NSLayoutConstraint.Create(feed, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View, NSLayoutAttribute.Leading, 1f, 0f);
            View.AddConstraint(pinLeft);

            var pinRight = NSLayoutConstraint.Create(feed, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, View, NSLayoutAttribute.Trailing, 1f, 0f);
            View.AddConstraint(pinRight);

            var pinTop = NSLayoutConstraint.Create(feed, NSLayoutAttribute.Top, NSLayoutRelation.Equal, TopLayoutGuide, NSLayoutAttribute.Bottom, 1f, -64f);
            View.AddConstraint(pinTop);

            var pinBottom = NSLayoutConstraint.Create(feed, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, -46f);
            View.AddConstraint(pinBottom);

            View.AddSubview(commentArea);

            //View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", feed));
            //View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", commentArea));
            //View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0][v1(46)]|", new NSLayoutFormatOptions(), "v0", feed, "v1", commentArea));

            var pinLeftCommentArea = NSLayoutConstraint.Create(commentArea, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View, NSLayoutAttribute.Leading, 1f, 0f);
            View.AddConstraint(pinLeftCommentArea);

            var pinRightCommentArea = NSLayoutConstraint.Create(commentArea, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, View, NSLayoutAttribute.Trailing, 1f, 0f);
            View.AddConstraint(pinRightCommentArea);

            toolbarBottomConstraint = NSLayoutConstraint.Create(View, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, commentArea, NSLayoutAttribute.Bottom, 1f, 0f);
            View.AddConstraint(toolbarBottomConstraint);

            toolbarHeightConstraint = NSLayoutConstraint.Create(commentArea, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 0f, 46f);
            View.AddConstraint(toolbarHeightConstraint);

            // Keyboard popup
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification, KeyBoardUpNotification);

            // Keyboard Down
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, KeyBoardDownNotification);

            // Keyboard dispose when clicking outside the comment box
            var g = new UITapGestureRecognizer { CancelsTouchesInView = false };
            g.AddTarget(() => View.EndEditing(true));
            g.ShouldReceiveTouch += (recognizer, touch) => !(touch.View is UIControl);                                        
            View.AddGestureRecognizer(g);
        }

        private void CommentButton_TouchUpInside(object sender, EventArgs e)
        {
            SurveyCommentModel surveyCommentModel = new SurveyCommentModel();
            surveyCommentModel.text = commentArea.CommentText.Text;
            surveyCommentModel.surveyId = survey.userId + survey.creationDate;
            surveyCommentModel.profilePicture = LoginController.userModel.profilePicturePath;
            surveyCommentModel.userId = LoginController.userModel.id;
            surveyCommentModel.userName = LoginController.userModel.name;

            comments.Add(surveyCommentModel);
            var indexes = new NSIndexPath[] { NSIndexPath.FromItemSection(comments.IndexOf(comments.Last()), 0) };
            createComment(surveyCommentModel, indexes);

            feed.InsertItems(indexes);
            feed.ScrollToItem(indexes.First(), UICollectionViewScrollPosition.Bottom, true);

            commentArea.CommentText.Text = null;
            commentArea.CommentButton.Enabled = false;
            View.EndEditing(true);
            ScrollTheView(false);
        }

        private async void createComment(SurveyCommentModel surveyCommentModel, NSIndexPath[] indexes)
        {
            try
            {
                survey.totalComments += 1;
                feedCell.updateTotalComments(survey.totalComments);

                surveyCommentModel.commentDate = (await new CommentManager().CreateSurveyComment(surveyCommentModel, LoginController.tokenModel.access_token)).commentDate;

                try
                {
                    if (LoginController.userModel.id != survey.userId)
                    {
                        UserNotificationModel userNotificationModel = new UserNotificationModel();
                        userNotificationModel.notificationDate = "";
                        userNotificationModel.userId = survey.userId;
                        userNotificationModel.notificationUser = new UserFriendModel(LoginController.userModel.id, LoginController.userModel.name, LoginController.userModel.profilePicturePath);
                        userNotificationModel.type = UserNotificationType.SurveyComment.ToString();

                        if (survey.question.text.Length > 25)
                        {
                            userNotificationModel.text = "commented on \"" + survey.question.text.Substring(0, 25) + "...\"";
                        }
                        else
                        {
                            userNotificationModel.text = "commented on \"" + survey.question.text + "\"";
                        }

                        userNotificationModel.link = surveyCommentModel.surveyId + ";" + surveyCommentModel.commentDate;
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
                survey.totalComments -= 1;
                feedCell.updateTotalComments(survey.totalComments);

                comments.RemoveAt(indexes.First().Row);
                feed.DeleteItems(indexes);

                Utils.HandleException(ex);
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            fetchSurveyComments(false);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            AddObservers();
        }

        public async void fetchSurveyComments(bool scrollToLastItem)
        {
            try
            {
                if (survey == null)
                {
                    survey = await new FeedManager().GetSurvey(this.userId, this.creationDate, LoginController.tokenModel.access_token);
                }
                else
                {
                    this.userId = survey.userId;
                    this.creationDate = survey.creationDate;
                }

                comments = await new CommentManager().GetSurveyComments(this.userId + this.creationDate, LoginController.tokenModel.access_token);

                var feedCellHeight = FeedController.getHeightForFeedCell(survey, View.Frame.Width);
                feedCell = new FeedCollectionViewCell(new CGRect(0, 0, View.Frame.Width, feedCellHeight));
                feedController.BindFeedCell(feedCell, survey, indexPathRow);

                feedCell.commentButton.RemoveTarget(null, null, UIControlEvent.AllEvents);
                feedCell.commentButton.AddTarget(this, new ObjCRuntime.Selector("CommentSelector:"), UIControlEvent.TouchUpInside);

                if (feedController.NavigationController == null)
                {
                    feedCell.resultButton.Params[2] = this.NavigationController;
                    feedCell.moreButton.Params[2] = this;
                    (feedCell.profileImageView.GestureRecognizers[0] as UIFeedTapGestureRecognizer).Params[0] = this.NavigationController;
                }

                BTProgressHUD.Dismiss();

                feed.Source = new CommentsCollectionViewSource(comments, feedCell, this.NavigationController);
                feed.Delegate = new CommentsCollectionViewDelegate((float) feedCell.Frame.Height);
                feed.ReloadData();

                if (scrollToLastItem)
                {
                    var section = (int)feed.NumberOfSections() - 1;
                    var item = (int)feed.NumberOfItemsInSection(section) - 1;
                    NSIndexPath index = NSIndexPath.FromRowSection(item, section);
                    feed.ScrollToItem(index, UICollectionViewScrollPosition.Bottom, true);
                }
                else if (!string.IsNullOrEmpty(commentDate))
                {
                    var index = comments.FindIndex(q => q.surveyId == userId + creationDate && q.commentDate == commentDate);
                    if (index != -1)
                    {
                        feed.ScrollToItem(NSIndexPath.FromItemSection(index, 0), UICollectionViewScrollPosition.CenteredVertically, true);
                    }
                }
            }
            catch (Exception ex)
            {
                BTProgressHUD.Dismiss();
                Utils.HandleException(ex);
            }
        }

        [Export("CommentSelector:")]
        private void CommentSelector(UIFeedButton button)
        {
            commentArea.CommentText.BecomeFirstResponder();
        }

        private void KeyBoardUpNotification(NSNotification notification)
        {
            // get the keyboard size
            CGRect r = UIKeyboard.BoundsFromNotification(notification);

            // Find what opened the keyboard
            foreach (UIView view in this.View.Subviews)
            {
                if(view.GetType() == typeof (CommentAreaView)){
                    foreach (UIView view2 in view.Subviews)
                    {
                        if (view2.IsFirstResponder)
                            activeview = view2;
                    }
                }
            }

            // Bottom of the controller = initial position + height + offset (relative to the screen)     
            UIView relativePositionView = UIApplication.SharedApplication.KeyWindow;
            CGRect relativeFrame = activeview.Superview.ConvertRectToView(activeview.Frame, relativePositionView);

            bottom = (float)(relativeFrame.Y + relativeFrame.Height + offset);

            // Calculate how far we need to scroll
            scroll_amount = (float)(r.Height - (View.Frame.Size.Height - bottom));

            // Perform the scrolling
            if (scroll_amount > 0)
            {
                moveViewUp = true;
                ScrollTheView(moveViewUp);
            }
            else
            {
                moveViewUp = false;
            }
        }

        private void KeyBoardDownNotification(NSNotification notification)
        {
            if (moveViewUp) { ScrollTheView(false); }
        }

        private void ScrollTheView(bool move)
        {

            // scroll the view up or down
            UIView.BeginAnimations(string.Empty, System.IntPtr.Zero);
            UIView.SetAnimationDuration(0.1);

            CGRect frame = View.Frame;

            if (move)
            {
                frame.Y -= scroll_amount;
            }
            else
            {
                frame.Y += scroll_amount;
                scroll_amount = 0;
            }

            View.Frame = frame;
            UIView.CommitAnimations();
        }

        void AddObservers()
        {
            commentArea.CommentText.AddObserver(this, "contentSize", NSKeyValueObservingOptions.OldNew, IntPtr.Zero);
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (keyPath == "contentSize")
                OnSizeChanged(new NSObservedChange(change));
            else
                base.ObserveValue(keyPath, ofObject, change, context);
        }

        void OnSizeChanged(NSObservedChange change)
        {
            CGSize oldValue = ((NSValue)change.OldValue).CGSizeValue;
            CGSize newValue = ((NSValue)change.NewValue).CGSizeValue;

            var dy = newValue.Height - oldValue.Height;
            AdjustInputToolbarOnTextViewSizeChanged(dy);
        }

        void AdjustInputToolbarOnTextViewSizeChanged(nfloat dy)
        {
            bool isIncreasing = dy > 0;

            // Find commentArea view
            foreach (UIView view in this.View.Subviews)
            {
                if (view.GetType() == typeof(CommentAreaView))
                {
                    activeviewarea = view;                    
                }
            }

            // Bottom of the controller = initial position + height + offset (relative to the screen)     
            UIView relativePositionView = UIApplication.SharedApplication.KeyWindow;
            CGRect relativeFrame = activeviewarea.Superview.ConvertRectToView(activeviewarea.Frame, relativePositionView);
            
            if ((relativeFrame.GetMinY() == TopLayoutGuide.Length) && isIncreasing)
            {
                return;
            }

            nfloat oldY = relativeFrame.GetMinY();
            nfloat newY = oldY - dy;
            if (newY < TopLayoutGuide.Length)
                dy = oldY - TopLayoutGuide.Length;

            AdjustInputToolbar(dy);
        }

        void AdjustInputToolbar(nfloat change)
        {
            toolbarHeightConstraint.Constant += change;

            var topCorrect = commentArea.CommentText.ContentSize.Height - commentArea.CommentText.Bounds.Size.Height;
            commentArea.CommentText.ContentOffset = new CGPoint(0, topCorrect);

            if (toolbarHeightConstraint.Constant < 46f)
                toolbarHeightConstraint.Constant = 46f;

            View.SetNeedsUpdateConstraints();
            View.LayoutIfNeeded();
        }

        private void CommentText_Changed(object sender, EventArgs e)
        {
            UpdateButtonState();
        }

        private void CommentText_Started(object sender, EventArgs e)
        {
            ScrollToBottom(true);
        }

        void UpdateButtonState()
        {
            commentArea.CommentButton.Enabled = !string.IsNullOrWhiteSpace(commentArea.CommentText.Text);
        }

        void ScrollToBottom(bool animated)
        {
            if (feed.NumberOfSections() == 0)
                return;

            var items = (int)feed.NumberOfItemsInSection(0);
            if (items == 0)
                return;

            var finalRow = (int)NMath.Max(0, feed.NumberOfItemsInSection(0) - 1);
            NSIndexPath finalIndexPath = NSIndexPath.FromRowSection(finalRow, 0);
            feed.ScrollToItem(finalIndexPath, UICollectionViewScrollPosition.Top, animated);
        }
    }

    public class CommentsCollectionViewSource : UICollectionViewSource
    {
        public List<SurveyCommentModel> comments { get; set; }
        public FeedCollectionViewCell feedCell { get; set; }
        public static NSCache imageCache = new NSCache();
        public UINavigationController navigationController { get; set; }

        public CommentsCollectionViewSource(List<SurveyCommentModel> comments, FeedCollectionViewCell feedCell, UINavigationController navigationController)
        {
            this.comments = comments;
            this.feedCell = feedCell;
            this.navigationController = navigationController;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return comments.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var commentCell = collectionView.DequeueReusableCell(CommentViewController.commentCellId, indexPath) as CommentCell;

            var imageView = commentCell.GetImageView();
            imageView.Image = UIImage.FromBundle("Profile");

            if (!string.IsNullOrEmpty(comments[indexPath.Row].profilePicture))
            {
                Utils.SetImageFromNSUrlSession(comments[indexPath.Row].profilePicture, imageView, this);
            }

            commentCell.UpdateCell(comments[indexPath.Row].userName, comments[indexPath.Row].text, this.navigationController, comments[indexPath.Row].userId);

            return commentCell;
        }

        public override UICollectionReusableView GetViewForSupplementaryElement(UICollectionView collectionView, NSString elementKind, NSIndexPath indexPath)
        {
            var headerView = collectionView.DequeueReusableSupplementaryView(elementKind, CommentViewController.feedHeadId, indexPath);

            headerView.AddSubview(feedCell);

            return headerView;
        }
    }

    public class CommentsCollectionViewDelegate : UICollectionViewDelegateFlowLayout
    {
        public float heightHeaderCell { get; set; }

        public CommentsCollectionViewDelegate(float heightHeaderCell)
        {
            this.heightHeaderCell = heightHeaderCell;
        }

        public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
        {
            var comment = CommentViewController.comments[indexPath.Row].text;
            if (!comment.Equals(""))
            {
                var rect = new NSString(comment).GetBoundingRect(new CGSize(collectionView.Frame.Width, 1000), NSStringDrawingOptions.UsesFontLeading | NSStringDrawingOptions.UsesLineFragmentOrigin, new UIStringAttributes() { Font = UIFont.SystemFontOfSize(14) }, null);
                
                // Heights of the vertical components to format the cell dinamic height
                // top padding = 8
                // profileImage = 40
                // bootom padding 8
                var knownHeight = 56;

                return new CGSize(collectionView.Frame.Width, rect.Height + knownHeight);
            }

            return new CGSize(collectionView.Frame.Width, 68);
        }

        public override nfloat GetMinimumLineSpacingForSection(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
        {
            return 0;
        }

        public override CGSize GetReferenceSizeForHeader(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
        {
            return new CGSize(collectionView.Frame.Width, heightHeaderCell);
        }
    }
}
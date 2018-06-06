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
using ObjCRuntime;
using Cirrious.FluentLayouts.Touch;

namespace Askker.App.iOS
{
    public partial class CommentViewController : CustomUIViewController
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
        private UIView activeview;              // Controller that activated the keyboard
        private float scroll_amount = 0.0f;     // amount to scroll 
        private float bottom = 0.0f;            // bottom point
        private float offset = 8.0f;            // extra offset
        private bool moveViewUp = false;        // which direction are we moving
        private float lastCommentBottom = 0.0f; // Bottom of the last comment

        //Variables used to resize the comment textView
        private UIView activeviewarea;          // CommentArea

        public static CommentMenuView commentMenu = CommentMenuView.Create();
        public SurveyCommentModel comment { get; set; }

        public CommentViewController (IntPtr handle) : base (handle)
        {
            
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);
            AutomaticallyAdjustsScrollViewInsets = false;

            UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = UIColor.FromRGB(90, 89, 89)
            };

            comments = new List<SurveyCommentModel>();

            commentArea.BackgroundColor = UIColor.FromWhiteAlpha(nfloat.Parse("0.95"), 1);
            commentArea.Frame = new CGRect(0, View.Frame.Height - 46, 375, 46);
            commentArea.TranslatesAutoresizingMaskIntoConstraints = false;

            commentArea.CommentText.Started += CommentText_Started;
            commentArea.CommentText.Changed += CommentText_Changed;
            commentArea.CommentButton.TouchUpInside += CommentButton_TouchUpInside;

            feed = new UICollectionView(new CGRect(), new UICollectionViewFlowLayout());
            feed.BackgroundColor = UIColor.White;
            feed.RegisterClassForCell(typeof(CommentCell), commentCellId);
            //feed.RegisterNibForCell(UINib.FromName("CommentCell", NSBundle.MainBundle), commentCellId);
            feed.RegisterClassForSupplementaryView(typeof(UICollectionReusableView), UICollectionElementKindSection.Header, feedHeadId);
            feed.AlwaysBounceVertical = true;
            feed.TranslatesAutoresizingMaskIntoConstraints = false;

            View.AddSubview(feed);
            View.AddSubview(commentArea);

            var pinLeft = NSLayoutConstraint.Create(feed, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View, NSLayoutAttribute.Leading, 1f, 0f);
            View.AddConstraint(pinLeft);

            var pinRight = NSLayoutConstraint.Create(feed, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, View, NSLayoutAttribute.Trailing, 1f, 0f);
            View.AddConstraint(pinRight);

            var pinTop = NSLayoutConstraint.Create(feed, NSLayoutAttribute.Top, NSLayoutRelation.Equal, TopLayoutGuide, NSLayoutAttribute.Bottom, 1f, 0f);
            View.AddConstraint(pinTop);

            var pinBottom = NSLayoutConstraint.Create(feed, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, commentArea, NSLayoutAttribute.Top, 1f, 0f);
            View.AddConstraint(pinBottom);

            //View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", feed));
            //View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", commentArea));
            //View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0][v1(46)]|", new NSLayoutFormatOptions(), "v0", feed, "v1", commentArea));

            var pinLeftCommentArea = NSLayoutConstraint.Create(commentArea, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View, NSLayoutAttribute.Leading, 1f, 0f);
            View.AddConstraint(pinLeftCommentArea);

            var pinRightCommentArea = NSLayoutConstraint.Create(commentArea, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, View, NSLayoutAttribute.Trailing, 1f, 0f);
            View.AddConstraint(pinRightCommentArea);

            toolbarBottomConstraint = NSLayoutConstraint.Create(View, NSLayoutAttribute.BottomMargin, NSLayoutRelation.Equal, commentArea, NSLayoutAttribute.Bottom, 1f, 0f);
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

            commentMenu.commentView = this.View;
            commentMenu.Hidden = true;
            commentMenu.CancelButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                commentMenu.Layer.AddAnimation(new CoreAnimation.CATransition
                {
                    Duration = 0.2,
                    Type = CoreAnimation.CAAnimation.TransitionPush,
                    Subtype = CoreAnimation.CAAnimation.TransitionFromBottom
                }, "hideMenu");

                commentMenu.Hidden = true;
                commentMenu.commentView.Alpha = 1f;
            };

            var rootViewController = UIApplication.SharedApplication.KeyWindow?.RootViewController;
            while (rootViewController?.PresentedViewController != null)
            {
                rootViewController = rootViewController.PresentedViewController;
            }
            if (rootViewController == null)
                rootViewController = this;

            commentMenu.Frame = rootViewController.View.Frame;
            rootViewController.View.AddSubview(commentMenu);
        }

        private void CommentButton_TouchUpInside(object sender, EventArgs e)
        {
            SurveyCommentModel surveyCommentModel = new SurveyCommentModel();
            surveyCommentModel.text = commentArea.CommentText.Text;
            surveyCommentModel.surveyId = survey.userId + survey.creationDate;
            surveyCommentModel.profilePicture = LoginController.userModel.profilePicturePath;
            surveyCommentModel.userId = LoginController.userModel.id;
            surveyCommentModel.userName = LoginController.userModel.name;
            surveyCommentModel.commentDate = EnvironmentConstants.getServerDateTime().ToString("yyyyMMddTHHmmssfff");



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
            base.ViewWillAppear(animated);
            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            fetchSurveyComments(true);
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

                comments = await new CommentManager().GetSurveyComments(this.userId + this.creationDate, LoginController.userModel.id ,LoginController.tokenModel.access_token);

                var feedCellHeight = Utils.getHeightForFeedCell(survey, View.Frame.Width);
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

                feed.Source = new CommentsCollectionViewSource(comments, feedCell, this);
                feed.Delegate = new CommentsCollectionViewDelegate((float) feedCell.Frame.Height);                
                feed.ReloadData();

                var section = (int)feed.NumberOfSections() - 1;
                var item = (int)feed.NumberOfItemsInSection(section) - 1;
                if (item != -1)
                {
                    if (scrollToLastItem)
                    {
                        NSIndexPath indexPath = NSIndexPath.FromRowSection(item, section);
                        feed.ScrollToItem(indexPath, UICollectionViewScrollPosition.Bottom, true);
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

                BTProgressHUD.Dismiss();
                commentArea.CommentText.BecomeFirstResponder();
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
            //ScrollToBottom(true);
            commentArea.CommentText.BecomeFirstResponder();
        }

        private void KeyBoardUpNotification(NSNotification notification)
        {
            if (!moveViewUp)
            {
                // get the keyboard size
                CGRect r = UIKeyboard.BoundsFromNotification(notification);

                // Find what opened the keyboard
                foreach (UIView view in this.View.Subviews)
                {
                    if (view.GetType() == typeof(CommentAreaView))
                    {
                        foreach (UIView view2 in view.Subviews)
                        {
                            if (view2.IsFirstResponder)
                                activeview = view2;
                        }
                    }
                }

                // Bottom of the controller = initial position + height - View Y position + offset (relative to the screen)     
                UIView relativePositionView = UIApplication.SharedApplication.KeyWindow;
                CGRect relativeFrame = activeview.Superview.ConvertRectToView(activeview.Frame, relativePositionView);

                bottom = (float)((relativeFrame.Y) + relativeFrame.Height - View.Frame.Y + offset);

                // Calculate how far we need to scroll
                scroll_amount = (float)(r.Height - (View.Frame.Size.Height - bottom));

                // Check the necessity of scroll
                var section = (int)feed.NumberOfSections() - 1;
                var item = (int)feed.NumberOfItemsInSection(section) - 1;
                if (item != -1)
                {
                    NSIndexPath indexPath = NSIndexPath.FromRowSection(item, section);
                    lastCommentBottom = (float)feed.GetLayoutAttributesForItem(indexPath).Frame.Y + (float)feed.GetLayoutAttributesForItem(indexPath).Frame.Height + offset;
                }
                else
                {
                    lastCommentBottom = 0f;
                }

                // Perform the scrolling
                moveViewUp = true;
                ScrollTheView(moveViewUp);
            }
        }

        private void KeyBoardDownNotification(NSNotification notification)
        {
            moveViewUp = false;
            ScrollTheView(moveViewUp);
        }

        private void ScrollTheView(bool move)
        {
            // scroll the view up or down
            UIView.BeginAnimations(string.Empty, System.IntPtr.Zero);
            UIView.SetAnimationDuration(0.1);

            if (move)
            {
                commentArea.Frame = new CGRect(commentArea.Frame.X, commentArea.Frame.Y - scroll_amount, commentArea.Frame.Width, commentArea.Frame.Height);
                if ((View.Frame.Height - scroll_amount) < lastCommentBottom)
                {
                    if (View.Frame.Height > lastCommentBottom)
                    {
                        feed.Frame = new CGRect(feed.Frame.X, feed.Frame.Y - (scroll_amount - (feed.Frame.Height - lastCommentBottom) - offset), feed.Frame.Width, feed.Frame.Height);
                    }
                    else
                    {
                        feed.Frame = new CGRect(feed.Frame.X, feed.Frame.Y - scroll_amount, feed.Frame.Width, feed.Frame.Height);
                    }
                }
            }
            else
            {
                commentArea.Frame = new CGRect(commentArea.Frame.X, commentArea.Frame.Y + scroll_amount, commentArea.Frame.Width, commentArea.Frame.Height);
                if ((View.Frame.Height - scroll_amount) < lastCommentBottom)
                {
                    if (View.Frame.Height > lastCommentBottom)
                    {
                        feed.Frame = new CGRect(feed.Frame.X, feed.Frame.Y + (scroll_amount - (feed.Frame.Height - lastCommentBottom) - offset), feed.Frame.Width, feed.Frame.Height);
                    }
                    else
                    {
                        feed.Frame = new CGRect(feed.Frame.X, feed.Frame.Y + scroll_amount, feed.Frame.Width, feed.Frame.Height);
                    }
                }
                scroll_amount = 0;
            }

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
            var feedFrame = feed.Frame;
            var commentAreaFrame = commentArea.Frame;

            toolbarHeightConstraint.Constant += change;

            var topCorrect = commentArea.CommentText.ContentSize.Height - commentArea.CommentText.Bounds.Size.Height;
            commentArea.CommentText.ContentOffset = new CGPoint(0, topCorrect);

            if (toolbarHeightConstraint.Constant < 46f)
                toolbarHeightConstraint.Constant = 46f;
            
            View.SetNeedsUpdateConstraints();
            View.LayoutIfNeeded();

            feed.Frame = new CGRect(feed.Frame.X, (feedFrame.Y - change), feed.Frame.Width, feedFrame.Height);
            commentArea.Frame = new CGRect(commentArea.Frame.X, (commentAreaFrame.Y - change), commentArea.Frame.Width, commentArea.Frame.Height);
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

        [Export("HandleLongPressSelector:")]
        private void HandleLongPressSelector(UICommentLongPressGestureRecognizer gestureRecognizer)
        {
            if (gestureRecognizer.State == UIGestureRecognizerState.Began)
            {
                View.EndEditing(true);
                this.comment = (SurveyCommentModel)gestureRecognizer.Params.ToArray()[0];

                commentMenu.Layer.AddAnimation(new CoreAnimation.CATransition
                {
                    Duration = 0.2,
                    Type = CoreAnimation.CAAnimation.TransitionPush,
                    Subtype = CoreAnimation.CAAnimation.TransitionFromTop
                }, "showMenu");

                commentMenu.Hidden = false;
                commentMenu.commentView.Alpha = 0.5f;
            }
        }

        [Export("DeleteCommentSelector:")]
        private async void DeleteCommentSelector(UIFeedButton but)
        {
            nint button = await Utils.ShowAlert("Delete Comment", "The comment will be deleted. Continue?", "Ok", "Cancel");

            try
            {
                if (button == 0)
                {
                    BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);

                    await new CommentManager().DeleteSurveyComment(comment, LoginController.tokenModel.access_token);

                    comments.Remove(comment);

                    if (survey.totalComments > 0)
                    {
                        survey.totalComments--;
                    }

                    feedCell.updateTotalComments(survey.totalComments);

                    feed.Source = new CommentsCollectionViewSource(comments, feedCell, this);
                    feed.Delegate = new CommentsCollectionViewDelegate((float)feedCell.Frame.Height);
                    feed.ReloadData();
                }

                commentMenu.Hidden = true;
                commentMenu.commentView.Alpha = 1f;
                BTProgressHUD.Dismiss();
            }
            catch (Exception ex)
            {
                BTProgressHUD.Dismiss();
                Utils.HandleException(ex);
            }
        }
    }

    public class CommentsCollectionViewSource : UICollectionViewSource
    {
        public List<SurveyCommentModel> comments { get; set; }
        public FeedCollectionViewCell feedCell { get; set; }
        public static NSCache imageCache = new NSCache();
        public CommentViewController commentViewController { get; set; }

        public CommentsCollectionViewSource(List<SurveyCommentModel> comments, FeedCollectionViewCell feedCell, CommentViewController commentViewController)
        {
            this.comments = comments;
            this.feedCell = feedCell;
            this.commentViewController = commentViewController;
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
                Utils.SetImageFromNSUrlSession(comments[indexPath.Row].profilePicture, imageView, this, PictureType.Profile);
            }

            commentCell.surveyCommentModel = comments[indexPath.Row];

            commentCell.UpdateCell(comments[indexPath.Row].userName, comments[indexPath.Row].text, comments[indexPath.Row].commentDate, commentViewController.NavigationController, comments[indexPath.Row].userId);

            if (comments[indexPath.Row].userId == LoginController.userModel.id)
            {
                var longPress = new UICommentLongPressGestureRecognizer(commentViewController, new Selector("HandleLongPressSelector:"));
                longPress.Params = new List<object>() { comments[indexPath.Row] };
                commentCell.AddGestureRecognizer(longPress);

                if (CommentViewController.commentMenu.DeleteButton.AllTargets.Count <= 0)
                {
                    CommentViewController.commentMenu.DeleteButton.AddTarget(commentViewController, new Selector("DeleteCommentSelector:"), UIControlEvent.TouchUpInside);
                }
                else if (!CommentViewController.commentMenu.DeleteButton.AllTargets.IsEqual(this))
                {
                    CommentViewController.commentMenu.DeleteButton.RemoveTarget(null, null, UIControlEvent.AllEvents);
                    CommentViewController.commentMenu.DeleteButton.AddTarget(commentViewController, new Selector("DeleteCommentSelector:"), UIControlEvent.TouchUpInside);
                }
            }

            if (comments[indexPath.Row].userLiked == true)
            {
                commentCell.updateLikeButton(false);
            }
            else if (comments[indexPath.Row].userLiked == null || comments[indexPath.Row].userLiked == false)
            {
                commentCell.updateLikeButton(true);
            }

            commentCell.updateLikeCount(comments[indexPath.Row].totalLikes);

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
                // 80 is a padding number to decrease from width, because the comment view dowsn't have the same width of screen
                var rect = new NSString(comment).GetBoundingRect(new CGSize(collectionView.Frame.Width - 80, 1000), NSStringDrawingOptions.UsesFontLeading | NSStringDrawingOptions.UsesLineFragmentOrigin, new UIStringAttributes() { Font = UIFont.SystemFontOfSize(14) }, null);

                // Heights of the vertical components to format the cell dinamic height
                // top padding = 8
                // profileImage = 40
                // date label = 18
                // bootom padding 8
                var knownHeight = 68;

                return new CGSize(collectionView.Frame.Width, rect.Height + knownHeight);
            }

            return new CGSize(collectionView.Frame.Width, 88);
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

    public class UICommentLongPressGestureRecognizer : UILongPressGestureRecognizer
    {
        public List<Object> Params { get; set; }

        public UICommentLongPressGestureRecognizer(NSObject target, Selector action) : base(target, action)
        {
        }
    }

    class CommentCell : UICollectionViewCell
    {
        public CommentLikeManager commentLikeManager = new CommentLikeManager();
        public UITextView commentText { get; set; }
        public UIImageView imageView { get; set; }
        public UIFeedButton likeButton { get; set; }
        public UIView lineSeparator { get; set; }
        public UILabel nameLabel { get; set; }
        public UILabel dateLabel { get; set; }
        public SurveyCommentModel surveyCommentModel { get; set; }

        [Export("initWithFrame:")]
        public CommentCell(CGRect frame) : base(frame)
        {
            imageView = new UIImageView();
            imageView.Layer.CornerRadius = 22;
            imageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            imageView.Layer.MasksToBounds = true;
            imageView.UserInteractionEnabled = true;
            imageView.TranslatesAutoresizingMaskIntoConstraints = false;

            commentText = new UITextView();
            commentText.BackgroundColor = UIColor.White;
            commentText.TextColor = UIColor.FromRGB(90, 89, 89);
            commentText.Selectable = false;
            commentText.Font = UIFont.SystemFontOfSize(14);
            commentText.TranslatesAutoresizingMaskIntoConstraints = false;
            commentText.Editable = false;
            commentText.ScrollEnabled = false;

            likeButton = new UIFeedButton();
            likeButton.SetTitle("Like", UIControlState.Normal);
            likeButton.AddTarget(Self, new ObjCRuntime.Selector("LikeCommentClick:"), UIControlEvent.TouchUpInside);

            nameLabel = new UILabel();
            nameLabel.TextColor = UIColor.FromRGB(90, 89, 89);
            nameLabel.Font = UIFont.BoldSystemFontOfSize(14);

            dateLabel = new UILabel();
            dateLabel.TextColor = UIColor.FromRGB(90, 89, 89);
            dateLabel.Font = UIFont.SystemFontOfSize(12);

            lineSeparator = new UIView();
            lineSeparator.BackgroundColor = UIColor.FromRGBA(nfloat.Parse("0.88"), nfloat.Parse("0.89"), nfloat.Parse("0.90"), nfloat.Parse("1"));

            ContentView.Add(imageView);
            ContentView.Add(likeButton);
            ContentView.Add(nameLabel);
            ContentView.Add(commentText);
            ContentView.Add(dateLabel);
            ContentView.Add(lineSeparator);

            ContentView.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            ContentView.AddConstraints(
                imageView.AtLeftOf(ContentView, 8),
                imageView.Width().EqualTo(40),
                imageView.Height().EqualTo(40),
                imageView.AtTopOf(ContentView, 8),

                likeButton.AtRightOf(ContentView, 8),
                likeButton.Width().EqualTo(60),
                likeButton.Height().EqualTo(24),
                likeButton.AtTopOf(ContentView, 4),

                nameLabel.Right().EqualTo().LeftOf(likeButton).Plus(8),
                nameLabel.AtTopOf(ContentView, 8),
                nameLabel.Left().EqualTo().RightOf(imageView).Plus(8),
                nameLabel.Height().EqualTo(20),

                commentText.AtTopOf(ContentView, 28),
                commentText.AtRightOf(ContentView, 8),
                commentText.Left().EqualTo().RightOf(imageView).Plus(3),
                commentText.AtBottomOf(ContentView, 30),

                dateLabel.Below(commentText, 5),
                dateLabel.Height().EqualTo(20),
                dateLabel.AtRightOf(ContentView, 8),
                dateLabel.AtLeftOf(ContentView, 56),                
                dateLabel.AtBottomOf(ContentView, 5),

                lineSeparator.AtBottomOf(ContentView, 1),
                lineSeparator.AtRightOf(ContentView, 8),
                lineSeparator.AtLeftOf(ContentView, 8),
                lineSeparator.Height().EqualTo(1)

            );
        }

        public void UpdateCell(string name, string text, string commentDate, UINavigationController navigationController, string id)
        {
            nameLabel.Text = name;
            commentText.Text = text;

            dateLabel.Text = Utils.TimeAgoDisplay(Utils.ConvertToLocalTime(DateTime.ParseExact(commentDate, "yyyyMMddTHHmmssfff", null)));

            dateLabel.Text += " • 0 Like";

            var feedTapGestureRecognizer = new UIFeedTapGestureRecognizer(this, new Selector("TapProfilePictureSelector:"));
            List<Object> tapProfilePictureValues = new List<Object>();
            tapProfilePictureValues.Add(navigationController);
            tapProfilePictureValues.Add(id);
            feedTapGestureRecognizer.Params = tapProfilePictureValues;
            imageView.AddGestureRecognizer(feedTapGestureRecognizer);

            LayoutSubviews();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            imageView.Layer.CornerRadius = 22;
            imageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            imageView.Layer.MasksToBounds = true;
            imageView.UserInteractionEnabled = true;
            imageView.TranslatesAutoresizingMaskIntoConstraints = false;

            likeButton.BackgroundColor = UIColor.White;
            likeButton.SetTitleColor(UIColor.FromRGB(117, 227, 213), UIControlState.Normal);
            likeButton.Font = UIFont.BoldSystemFontOfSize(14);            
            likeButton.Layer.BorderColor = UIColor.FromRGB(117, 227, 213).CGColor;
            likeButton.Layer.BorderWidth = 1;
            likeButton.Layer.CornerRadius = 4;
            likeButton.TranslatesAutoresizingMaskIntoConstraints = false;

            commentText.BackgroundColor = UIColor.White;
            commentText.TextColor = UIColor.FromRGB(90, 89, 89);
            commentText.Selectable = false;
            commentText.Font = UIFont.SystemFontOfSize(14);

            nameLabel.TextColor = UIColor.FromRGB(90, 89, 89);
            nameLabel.Font = UIFont.BoldSystemFontOfSize(14);

            dateLabel.TextColor = UIColor.FromRGB(90, 89, 89);
            dateLabel.Font = UIFont.SystemFontOfSize(12);

        }

        [Export("LikeCommentClick:")]
        private async void LikeCommentClick(UIFeedButton button)
        {
            button.LoadingIndicatorButton(true);

            if ("Like".Equals(button.CurrentTitle))
            {
                button.SetTitle("Unlike", UIControlState.Normal);

                updateLikeCount(++surveyCommentModel.totalLikes);

                SurveyCommentLikeModel surveyCommentLikeModel = new SurveyCommentLikeModel();
                surveyCommentLikeModel.commentId = surveyCommentModel.surveyId + surveyCommentModel.commentDate;
                surveyCommentLikeModel.user = new User();
                surveyCommentLikeModel.user.id = LoginController.userModel.id;

                surveyCommentModel.userLiked = true;

                await commentLikeManager.LikeComment(surveyCommentLikeModel, LoginController.tokenModel.access_token);
            }
            else
            {
                button.SetTitle("Like", UIControlState.Normal);
                updateLikeCount(--surveyCommentModel.totalLikes);

                surveyCommentModel.userLiked = null;

                await commentLikeManager.UnlikeComment(surveyCommentModel.surveyId + surveyCommentModel.commentDate, LoginController.userModel.id, LoginController.tokenModel.access_token);
            }

            LayoutSubviews();

            button.LoadingIndicatorButton(false);
        }

        private string getLikeText(int likeCount)
        {
            if(likeCount > 1)
            {
                return " " + likeCount + " Likes";
            }
            else
            {
                return " " + likeCount + " Like";
            }
        }

        public void updateLikeCount(int totalLikes)
        {
            dateLabel.Text = dateLabel.Text.Remove(dateLabel.Text.IndexOf("•") + 1);
            dateLabel.Text += getLikeText(totalLikes);

            LayoutSubviews();
        }

        public void updateLikeButton(bool fromLike)
        {
            if (fromLike)
            {
                likeButton.SetTitle("Like", UIControlState.Normal);                
            }
            else
            {
                likeButton.SetTitle("Unlike", UIControlState.Normal);                
            }

            LayoutSubviews();
        }

        public UIImageView GetImageView()
        {
            return this.imageView;
        }

        [Export("TapProfilePictureSelector:")]
        public void TapProfilePictureSelector(UIFeedTapGestureRecognizer tapGesture)
        {
            Utils.OpenUserProfile((UINavigationController)tapGesture.Params.ToArray()[0], (string)tapGesture.Params.ToArray()[1]);
        }
    }
}
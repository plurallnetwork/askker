using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace Askker.App.iOS
{
    public partial class CommentViewController : UIViewController
    {
        public UICollectionView feed { get; set; }
        public UICollectionView feedHead { get; set; }
        public List<SurveyCommentModel> comments { get; set; }
        public float headHeight { get; set; }

        public static NSString feedHeadId = new NSString("feedHeadId");
        public static NSString commentCellId = new NSString("commentCellId");

        public CommentViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            AutomaticallyAdjustsScrollViewInsets = false;

            comments = new List<SurveyCommentModel>();

            var commentArea = new UIView();
            commentArea.BackgroundColor = UIColor.Blue;
            commentArea.TranslatesAutoresizingMaskIntoConstraints = false;

            feed = new UICollectionView(new CGRect(), new UICollectionViewFlowLayout() {
                HeaderReferenceSize = new System.Drawing.SizeF((float)View.Frame.Width, headHeight)
            });
            feed.BackgroundColor = UIColor.FromWhiteAlpha(nfloat.Parse("0.95"), 1);
            feed.RegisterClassForCell(typeof(UICollectionViewCell), commentCellId);
            feed.RegisterClassForSupplementaryView(typeof(UICollectionReusableView), UICollectionElementKindSection.Header, feedHeadId);
            feed.AlwaysBounceVertical = true;
            feed.TranslatesAutoresizingMaskIntoConstraints = false;

            feedHead.AlwaysBounceVertical = false;
            feedHead.ScrollEnabled = false;
            feedHead.TranslatesAutoresizingMaskIntoConstraints = false;

            View.AddSubview(feed);
            View.AddSubview(commentArea);

            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", feed));
            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", commentArea));
            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0][v1(50)]|", new NSLayoutFormatOptions(), "v0", feed, "v1", commentArea));
        }

        public override void ViewWillAppear(bool animated)
        {
            fetchSurveyComments();
        }

        public async void fetchSurveyComments()
        {
            feedHead.ReloadData();
            comments = await new CommentManager().GetSurveyComments("75e4441c-4414-4fb2-8966-62c53d8ef85420170201T120657", LoginController.tokenModel.access_token);
            feed.Source = new CommentsCollectionViewSource(comments, feedHead);
            feed.Delegate = new CommentsCollectionViewDelegate();
            feed.ReloadData();
        }
    }

    public class CommentsCollectionViewSource : UICollectionViewSource
    {
        public List<SurveyCommentModel> comments { get; set; }
        public UICollectionView feedHead { get; set; }

        public CommentsCollectionViewSource(List<SurveyCommentModel> comments, UICollectionView feedHead)
        {
            this.comments = comments;
            this.feedHead = feedHead;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return comments.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var commentCell = collectionView.DequeueReusableCell(CommentViewController.commentCellId, indexPath) as UICollectionViewCell;
            commentCell.BackgroundColor = UIColor.Green;

            var label = new UILabel();
            label.Text = comments[indexPath.Row].text;
            label.TranslatesAutoresizingMaskIntoConstraints = false;

            commentCell.AddSubview(label);

            commentCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-12-[v0]-12-|", new NSLayoutFormatOptions(), "v0", label));
            commentCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(24)]-8-|", new NSLayoutFormatOptions(), "v0", label));

            return commentCell;
        }

        public override UICollectionReusableView GetViewForSupplementaryElement(UICollectionView collectionView, NSString elementKind, NSIndexPath indexPath)
        {
            var headerView = collectionView.DequeueReusableSupplementaryView(elementKind, CommentViewController.feedHeadId, indexPath);

            headerView.AddSubview(feedHead);

            headerView.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", feedHead));
            headerView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0]|", new NSLayoutFormatOptions(), "v0", feedHead));

            return headerView;
        }
    }

    public class CommentsCollectionViewDelegate : UICollectionViewDelegateFlowLayout
    {
        public CommentsCollectionViewDelegate()
        {
        }

        public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
        {
            return new CGSize(collectionView.Frame.Width, 50);
        }

        public override nfloat GetMinimumLineSpacingForSection(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
        {
            return 0;
        }
    }
}
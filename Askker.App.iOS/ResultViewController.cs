using Askker.App.PortableLibrary.Enums;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace Askker.App.iOS
{
    public partial class ResultViewController : UIViewController
    {
        public UICollectionView feed { get; set; }
        public UICollectionView feedHead { get; set; }
        public float headHeight { get; set; }
        public NSIndexPath feedCellIndexPath { get; set; }

        public List<ReportType> reports { get; set; }

        public static NSString feedHeadId = new NSString("feedHeadId");
        public static NSString resultCellId = new NSString("resultCellId");

        public ResultViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            AutomaticallyAdjustsScrollViewInsets = false;

            reports = new List<ReportType>();
            reports.Add(ReportType.Overall);
            reports.Add(ReportType.Gender);
            reports.Add(ReportType.Age);

            feed = new UICollectionView(new CGRect(), new UICollectionViewFlowLayout()
            {
                HeaderReferenceSize = new System.Drawing.SizeF((float)View.Frame.Width, headHeight)
            });
            feed.BackgroundColor = UIColor.FromWhiteAlpha(nfloat.Parse("0.95"), 1);
            feed.RegisterClassForCell(typeof(ResultCollectionViewCell), resultCellId);
            feed.RegisterClassForSupplementaryView(typeof(UICollectionReusableView), UICollectionElementKindSection.Header, feedHeadId);
            feed.AlwaysBounceVertical = true;
            feed.TranslatesAutoresizingMaskIntoConstraints = false;

            feedHead.AlwaysBounceVertical = false;
            feedHead.ScrollEnabled = false;
            feedHead.TranslatesAutoresizingMaskIntoConstraints = false;

            View.AddSubview(feed);

            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", feed));
            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0]|", new NSLayoutFormatOptions(), "v0", feed));
        }

        public override void ViewWillAppear(bool animated)
        {
            fetchSurveyDetail();
        }

        public async void fetchSurveyDetail()
        {
            feedHead.ReloadData();
            
            feed.Source = new ResultsCollectionViewSource(feedHead, feedCellIndexPath, reports);
            feed.Delegate = new ResultsCollectionViewDelegate();
            feed.ReloadData();
        }
    }

    public class ResultsCollectionViewSource : UICollectionViewSource
    {
        public UICollectionView feedHead { get; set; }
        public NSIndexPath feedCellIndexPath { get; set; }
        public List<ReportType> reports { get; set; }

        public ResultsCollectionViewSource(UICollectionView feedHead, NSIndexPath feedCellIndexPath, List<ReportType> reports)
        {
            this.feedHead = feedHead;
            this.feedCellIndexPath = feedCellIndexPath;
            this.reports = reports;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return reports.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var resultCell = collectionView.DequeueReusableCell(ResultViewController.resultCellId, indexPath) as ResultCollectionViewCell;
            resultCell.BackgroundColor = UIColor.White;

            resultCell.sectionLabel.Text = reports[indexPath.Row].ToString();

            return resultCell;
        }

        public override UICollectionReusableView GetViewForSupplementaryElement(UICollectionView collectionView, NSString elementKind, NSIndexPath indexPath)
        {
            var headerView = collectionView.DequeueReusableSupplementaryView(elementKind, ResultViewController.feedHeadId, indexPath);

            if (feedCellIndexPath != null)
            {
                var feedCell = feedHead.CellForItem(feedCellIndexPath) as FeedCollectionViewCell;

                if (feedCell != null)
                {
                    feedCell.resultButton.Enabled = false;
                }
            }

            headerView.AddSubview(feedHead);

            headerView.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", feedHead));
            headerView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0]|", new NSLayoutFormatOptions(), "v0", feedHead));

            return headerView;
        }
    }

    public class ResultsCollectionViewDelegate : UICollectionViewDelegateFlowLayout
    {
        public ResultsCollectionViewDelegate()
        {
        }

        public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
        {
            return new CGSize(collectionView.Frame.Width, 200);
        }

        public override nfloat GetMinimumLineSpacingForSection(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
        {
            return 0;
        }
    }

    public class ResultCollectionViewCell : UICollectionViewCell
    {
        public UILabel sectionLabel { get; set; }
        public UIView chartArea { get; set; }

        [Export("initWithFrame:")]
        public ResultCollectionViewCell(CGRect frame) : base(frame)
        {
            sectionLabel = new UILabel();
            sectionLabel.Font = UIFont.BoldSystemFontOfSize(12);
            sectionLabel.TextColor = UIColor.Gray;
            sectionLabel.TextAlignment = UITextAlignment.Center;
            sectionLabel.BackgroundColor = UIColor.FromWhiteAlpha(nfloat.Parse("0.95"), 1);
            sectionLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            chartArea = new UIView();
            chartArea.BackgroundColor = UIColor.Green;
            chartArea.TranslatesAutoresizingMaskIntoConstraints = false;

            AddSubview(sectionLabel);
            AddSubview(chartArea);

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", sectionLabel));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", chartArea));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0(24)][v1]|", new NSLayoutFormatOptions(), "v0", sectionLabel, "v1", chartArea));
        }
    }
}
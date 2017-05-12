using Askker.App.PortableLibrary.Enums;
using CoreGraphics;
using Foundation;
using iOSCharts;
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

            string[] months = new string[] { "Janeiro", "Fevereiro", "Março", "Abril", "Maio" };

            double[] values = new double[] { 50, 25, 30, 40, 41 };

            double[] unitsSold = new double[] { 20.0, 4.0, 6.0, 3.0, 12.0 };
            double[] unitsBought = new double[] { 10.0, 14.0, 60.0, 13.0, 2.0 };

            if (ReportType.Overall.Equals(reports[indexPath.Row]))
            {
                resultCell.sectionLabel.Text = reports[indexPath.Row].ToString();

                var dataEntries = new List<PieChartDataEntry>();
                for (int i = 0; i < values.Length; i++)
                {
                    dataEntries.Add(new PieChartDataEntry(values[i], months[i]));
                }

                var dataSet = new PieChartDataSet(dataEntries.ToArray(), "");
                dataSet.SliceSpace = 2;

                dataSet.Colors = ChartColorTemplates.Joyful;
                dataSet.ValueTextColor = UIColor.Black;
                dataSet.XValuePosition = PieChartValuePosition.OutsideSlice;
                dataSet.YValuePosition = PieChartValuePosition.OutsideSlice;

                resultCell.pieChartView.Data = new PieChartData(new PieChartDataSet[] { dataSet });
                resultCell.pieChartView.AnimateWithXAxisDuration(1.4, ChartEasingOption.EaseOutBack);
                resultCell.pieChartView.DescriptionText = "Total 196 votes";
                resultCell.pieChartView.Legend.Enabled = false;

                resultCell.AddSubview(resultCell.pieChartView);

                resultCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", resultCell.pieChartView));
                resultCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0(24)][v1]|", new NSLayoutFormatOptions(), "v0", resultCell.sectionLabel, "v1", resultCell.pieChartView));
            }
            else
            {
                resultCell.sectionLabel.Text = "By " + reports[indexPath.Row].ToString();

                resultCell.barChartView.Legend.Enabled = true;
                resultCell.barChartView.Legend.HorizontalAlignment = ChartLegendHorizontalAlignment.Right;
                resultCell.barChartView.Legend.VerticalAlignment = ChartLegendVerticalAlignment.Top;
                resultCell.barChartView.Legend.Orientation = ChartLegendOrientation.Vertical;
                resultCell.barChartView.Legend.DrawInside = true;
                resultCell.barChartView.Legend.YOffset = 10.0f;
                resultCell.barChartView.Legend.XOffset = 10.0f;
                resultCell.barChartView.Legend.YEntrySpace = 0.0f;

                resultCell.barChartView.XAxis.DrawGridLinesEnabled = true;
                resultCell.barChartView.XAxis.LabelPosition = XAxisLabelPosition.Bottom;
                resultCell.barChartView.XAxis.CenterAxisLabelsEnabled = true;
                resultCell.barChartView.XAxis.ValueFormatter = new ChartIndexAxisValueFormatter(months);
                resultCell.barChartView.XAxis.Granularity = 1;

                resultCell.barChartView.LeftAxis.SpaceTop = 0.35f;
                resultCell.barChartView.LeftAxis.AxisMinimum = 0;
                resultCell.barChartView.LeftAxis.DrawGridLinesEnabled = false;

                resultCell.barChartView.RightAxis.Enabled = false;

                var dataEntries = new List<BarChartDataEntry>();
                var dataEntries1 = new List<BarChartDataEntry>();

                for (int i = 0; i < months.Length; i++)
                {
                    dataEntries.Add(new BarChartDataEntry(i, unitsSold[i]));
                    dataEntries1.Add(new BarChartDataEntry(i, unitsBought[i]));
                }

                var chartDataSet = new BarChartDataSet(dataEntries.ToArray(), "Unit sold");
                var chartDataSet1 = new BarChartDataSet(dataEntries1.ToArray(), "Unit Bought");

                var dataSets = new BarChartDataSet[] { chartDataSet, chartDataSet1 };
                chartDataSet.SetColor(UIColor.FromRGBA(nfloat.Parse("0.60"), nfloat.Parse("0.63"), nfloat.Parse("0.67"), nfloat.Parse("1")));

                var chartData = new BarChartData(dataSets);

                var groupSpace = 0.3;
                var barSpace = 0.05;
                var barWidth = 0.3;
                // (0.3 + 0.05) * 2 + 0.3 = 1.00 -> interval per "group" 

                var groupCount = months.Length;
                var startYear = 0;

                chartData.BarWidth = barWidth;
                resultCell.barChartView.XAxis.AxisMinimum = startYear;

                resultCell.barChartView.XAxis.AxisMaximum = startYear + chartData.GroupWidthWithGroupSpace(groupSpace, barSpace) * groupCount;

                chartData.GroupBarsFromX(startYear, groupSpace, barSpace);

                resultCell.barChartView.Data = chartData;
                resultCell.barChartView.AnimateWithXAxisDuration(1.4, ChartEasingOption.Linear);
                resultCell.barChartView.DescriptionText = "";

                resultCell.AddSubview(resultCell.barChartView);

                resultCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", resultCell.barChartView));
                resultCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0(24)][v1]|", new NSLayoutFormatOptions(), "v0", resultCell.sectionLabel, "v1", resultCell.barChartView));
            }

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
            return new CGSize(collectionView.Frame.Width, 300);
        }

        public override nfloat GetMinimumLineSpacingForSection(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
        {
            return 0;
        }
    }

    public class ResultCollectionViewCell : UICollectionViewCell
    {
        public UILabel sectionLabel { get; set; }
        public PieChartView pieChartView { get; set; }
        public BarChartView barChartView { get; set; }

        [Export("initWithFrame:")]
        public ResultCollectionViewCell(CGRect frame) : base(frame)
        {
            sectionLabel = new UILabel();
            sectionLabel.Font = UIFont.BoldSystemFontOfSize(12);
            sectionLabel.TextColor = UIColor.Gray;
            sectionLabel.TextAlignment = UITextAlignment.Center;
            sectionLabel.BackgroundColor = UIColor.FromWhiteAlpha(nfloat.Parse("0.95"), 1);
            sectionLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            pieChartView = new PieChartView();
            pieChartView.TranslatesAutoresizingMaskIntoConstraints = false;

            barChartView = new BarChartView();
            barChartView.TranslatesAutoresizingMaskIntoConstraints = false;

            AddSubview(sectionLabel);

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", sectionLabel));
        }
    }
}
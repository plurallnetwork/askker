using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
using CoreGraphics;
using Foundation;
using iOSCharts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

            ReportManager reportManager = new ReportManager();
            List<ReportModel> reportsDatasets = new List<ReportModel>();
            reportsDatasets.Add(await reportManager.GetOverallResults("75e4441c-4414-4fb2-8966-62c53d8ef854", "20170303T120657", LoginController.tokenModel.access_token));
            reportsDatasets.Add(await reportManager.GetResultsByGender("75e4441c-4414-4fb2-8966-62c53d8ef854", "20170303T120657", LoginController.tokenModel.access_token));
            reportsDatasets.Add(await reportManager.GetResultsByAge("75e4441c-4414-4fb2-8966-62c53d8ef854", "20170303T120657", LoginController.tokenModel.access_token));

            feed.Source = new ResultsCollectionViewSource(feedHead, feedCellIndexPath, reports, reportsDatasets);
            feed.Delegate = new ResultsCollectionViewDelegate();
            feed.ReloadData();
        }
    }

    public class ResultsCollectionViewSource : UICollectionViewSource
    {
        public UICollectionView feedHead { get; set; }
        public NSIndexPath feedCellIndexPath { get; set; }
        public List<ReportType> reports { get; set; }
        public List<ReportModel> reportsDatasets { get; set; }
        public List<UIColor> chartColors { get; set; }

        public ResultsCollectionViewSource(UICollectionView feedHead, NSIndexPath feedCellIndexPath, List<ReportType> reports, List<ReportModel> reportsDatasets)
        {
            this.feedHead = feedHead;
            this.feedCellIndexPath = feedCellIndexPath;
            this.reports = reports;

            this.reportsDatasets = reportsDatasets; //GetReports();

            this.chartColors = new List<UIColor>();
            this.chartColors.Add(UIColor.FromRGBA(nfloat.Parse("0.97"), nfloat.Parse("0.65"), nfloat.Parse("0.14"), nfloat.Parse("1")));
            this.chartColors.Add(UIColor.FromRGBA(nfloat.Parse("0.98"), nfloat.Parse("0.74"), nfloat.Parse("0.36"), nfloat.Parse("1")));
            this.chartColors.Add(UIColor.FromRGBA(nfloat.Parse("0.99"), nfloat.Parse("0.96"), nfloat.Parse("0.58"), nfloat.Parse("1")));
            this.chartColors.Add(UIColor.FromRGBA(nfloat.Parse("0.76"), nfloat.Parse("0.72"), nfloat.Parse("0.32"), nfloat.Parse("1")));
            this.chartColors.Add(UIColor.FromRGBA(nfloat.Parse("0.57"), nfloat.Parse("0.49"), nfloat.Parse("0.36"), nfloat.Parse("1")));
            this.chartColors.Add(UIColor.FromRGBA(nfloat.Parse("0.72"), nfloat.Parse("0.0"), nfloat.Parse("0.48"), nfloat.Parse("1"))); 
        }

        //public void GetReports()
        //{
        //    try
        //    {
        //        ReportManager reportManager = new ReportManager();

        //        this.reportsDatasets.Add(await reportManager.GetOverallResults("75e4441c-4414-4fb2-8966-62c53d8ef854", "20170303T120657", LoginController.tokenModel.access_token));
        //        this.reportsDatasets.Add(await reportManager.GetResultsByGender("75e4441c-4414-4fb2-8966-62c53d8ef854", "20170303T120657", LoginController.tokenModel.access_token));
        //        this.reportsDatasets.Add(await reportManager.GetResultsByAge("75e4441c-4414-4fb2-8966-62c53d8ef854", "20170303T120657", LoginController.tokenModel.access_token));
        //    }
        //    catch (Exception ex)
        //    {
        //        Utils.HandleException(ex);
        //    }
        //}

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return reports.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var resultCell = collectionView.DequeueReusableCell(ResultViewController.resultCellId, indexPath) as ResultCollectionViewCell;
            resultCell.BackgroundColor = UIColor.White;

            //string[] months = new string[] { "Janeiro", "Fevereiro", "Março", "Abril", "Maio" };

            //double[] values = new double[] { 50, 25, 30, 40, 41 };

            //double[] unitsSold = new double[] { 20.0, 4.0, 6.0, 3.0, 12.0 };
            //double[] unitsBought = new double[] { 10.0, 14.0, 60.0, 13.0, 2.0 };

            var reportDataSet = this.reportsDatasets[indexPath.Row];

            if (ReportType.Overall.Equals(reports[indexPath.Row]))
            {
                resultCell.sectionLabel.Text = reports[indexPath.Row].ToString();

                var dataEntries = new List<PieChartDataEntry>();
                for (int i = 0; i < reportDataSet.dataSets[0].Count; i++)
                {
                    if (Convert.ToInt32(reportDataSet.dataSets[0][i]) > 0)
                    {
                        dataEntries.Add(new PieChartDataEntry(reportDataSet.dataSets[0][i], reportDataSet.labels[i]));
                    }
                }

                var dataSet = new PieChartDataSet(dataEntries.ToArray(), "");
                dataSet.SliceSpace = 2;

                dataSet.Colors = ChartColorTemplates.Joyful;
                dataSet.ValueTextColor = UIColor.Black;
                dataSet.XValuePosition = PieChartValuePosition.OutsideSlice;
                dataSet.YValuePosition = PieChartValuePosition.OutsideSlice;

                resultCell.pieChartView.Data = new PieChartData(new PieChartDataSet[] { dataSet });
                resultCell.pieChartView.AnimateWithXAxisDuration(1.4, ChartEasingOption.EaseOutBack);
                resultCell.pieChartView.DescriptionText = string.Format("Total {0} votes", reportDataSet.totalVotes);
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
                resultCell.barChartView.XAxis.ValueFormatter = new ChartIndexAxisValueFormatter(reportDataSet.groups.ToArray());
                resultCell.barChartView.XAxis.Granularity = 1;

                resultCell.barChartView.LeftAxis.SpaceTop = 0.35f;
                resultCell.barChartView.LeftAxis.AxisMinimum = 0;
                resultCell.barChartView.LeftAxis.DrawGridLinesEnabled = false;

                resultCell.barChartView.RightAxis.Enabled = false;

                var groupCount = reportDataSet.groups.Count;
                var optionsCount = reportDataSet.labels.Count;

                var dataEntriesList = new List<List<BarChartDataEntry>>();

                for (int i = 0; i < optionsCount; i++)
                {
                    var dataEntries = new List<BarChartDataEntry>();

                    for (int j = 0; j < groupCount; j++)
                    {
                        dataEntries.Add(new BarChartDataEntry(i, reportDataSet.dataSets[i][j]));
                    }

                    dataEntriesList.Add(dataEntries);
                }

                var chartDataSetList = new List<BarChartDataSet>();

                for (int i = 0; i < dataEntriesList.Count; i++)
                {
                    var barChartDataSet = new BarChartDataSet(dataEntriesList[i].ToArray(), reportDataSet.labels[i]);
                    barChartDataSet.SetColor(this.chartColors[i]);

                    chartDataSetList.Add(barChartDataSet);
                }

                var dataSets = chartDataSetList.ToArray();

                var chartData = new BarChartData(dataSets);

                var initialXValue = 0;

                var groupSpace = 0.3;
                var barSpace = 0.05;
                var barWidth = (0.7 - (0.05 * optionsCount)) / optionsCount; // (barWidth + 0.05) * optionsCount + 0.3 = 1.00 -> interval per "group"

                chartData.BarWidth = barWidth;
                resultCell.barChartView.XAxis.AxisMinimum = initialXValue;
                resultCell.barChartView.XAxis.AxisMaximum = initialXValue + chartData.GroupWidthWithGroupSpace(groupSpace, barSpace) * groupCount;

                chartData.GroupBarsFromX(initialXValue, groupSpace, barSpace);

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
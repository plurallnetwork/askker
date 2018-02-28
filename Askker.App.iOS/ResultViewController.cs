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
using ObjCRuntime;
using BigTed;

namespace Askker.App.iOS
{
    public partial class ResultViewController : CustomUIViewController
    {
        public UICollectionView feed { get; set; }
        public FeedCollectionViewCell feedCell { get; set; }
        public FeedController feedController { get; set; }
        
        public List<ReportType> reports { get; set; }

        public SurveyModel survey { get; set; }
        public string userId { get; set; }
        public string creationDate { get; set; }
        public int indexPathRow { get; set; }

        public static NSString feedHeadId = new NSString("feedHeadId");
        public static NSString resultCellId = new NSString("resultCellId");

        public ResultViewController (IntPtr handle) : base (handle)
        {            
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);

            UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = UIColor.FromRGB(90, 89, 89)
            };

            reports = new List<ReportType>();
            reports.Add(ReportType.Overall);
            reports.Add(ReportType.Gender);
            reports.Add(ReportType.Age);

            feed = new UICollectionView(new CGRect(), new UICollectionViewFlowLayout());
            feed.BackgroundColor = UIColor.FromWhiteAlpha(nfloat.Parse("0.95"), 1);
            feed.RegisterClassForCell(typeof(ResultCollectionViewCell), resultCellId);
            feed.RegisterClassForSupplementaryView(typeof(UICollectionReusableView), UICollectionElementKindSection.Header, feedHeadId);
            feed.AlwaysBounceVertical = true;
            feed.TranslatesAutoresizingMaskIntoConstraints = false;

            View.AddSubview(feed);

            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", feed));
            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0]|", new NSLayoutFormatOptions(), "v0", feed));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            fetchSurveyDetail();
        }

        public async void fetchSurveyDetail()
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

                ReportManager reportManager = new ReportManager();
                List<ReportModel> reportsDatasets = new List<ReportModel>();
                reportsDatasets.Add(await reportManager.GetOverallResults(this.userId, this.creationDate, LoginController.tokenModel.access_token));
                reportsDatasets.Add(await reportManager.GetResultsByGender(this.userId, this.creationDate, LoginController.tokenModel.access_token));
                reportsDatasets.Add(await reportManager.GetResultsByAge(this.userId, this.creationDate, LoginController.tokenModel.access_token));

                var feedCellHeight = Utils.getHeightForFeedCell(survey, View.Frame.Width);
                feedCell = new FeedCollectionViewCell(new CGRect(0, 0, View.Frame.Width, feedCellHeight));
                feedController.BindFeedCell(feedCell, survey, indexPathRow);

                feedCell.totalVotesLabel.RemoveTarget(null, null, UIControlEvent.AllEvents);

                if (feedController.NavigationController == null)
                {
                    feedCell.commentButton.Params[2] = this.NavigationController;
                    feedCell.moreButton.Params[2] = this;
                    (feedCell.profileImageView.GestureRecognizers[0] as UIFeedTapGestureRecognizer).Params[0] = this.NavigationController;
                }
                
                feed.Source = new ResultsCollectionViewSource(reports, reportsDatasets, feedCell);
                feed.Delegate = new ResultsCollectionViewDelegate((float) feedCell.Frame.Height);
                feed.ReloadData();

                BTProgressHUD.Dismiss();
            }
            catch (Exception ex)
            {
                BTProgressHUD.Dismiss();
                Utils.HandleException(ex);
            }
        }
    }

    public class ResultsCollectionViewSource : UICollectionViewSource
    {
        public List<ReportType> reports { get; set; }
        public List<ReportModel> reportsDatasets { get; set; }
        public List<UIColor> chartColors { get; set; }
        public FeedCollectionViewCell feedCell { get; set; }

        public ResultsCollectionViewSource(List<ReportType> reports, List<ReportModel> reportsDatasets, FeedCollectionViewCell feedCell)
        {
            this.reports = reports;
            this.feedCell = feedCell;

            this.reportsDatasets = reportsDatasets;

            this.chartColors = new List<UIColor>();
            this.chartColors.Add(UIColor.FromRGBA(nfloat.Parse("0.97"), nfloat.Parse("0.65"), nfloat.Parse("0.14"), nfloat.Parse("1")));
            this.chartColors.Add(UIColor.FromRGBA(nfloat.Parse("0.98"), nfloat.Parse("0.74"), nfloat.Parse("0.36"), nfloat.Parse("1")));
            this.chartColors.Add(UIColor.FromRGBA(nfloat.Parse("0.99"), nfloat.Parse("0.96"), nfloat.Parse("0.58"), nfloat.Parse("1")));
            this.chartColors.Add(UIColor.FromRGBA(nfloat.Parse("0.76"), nfloat.Parse("0.72"), nfloat.Parse("0.32"), nfloat.Parse("1")));
            this.chartColors.Add(UIColor.FromRGBA(nfloat.Parse("0.57"), nfloat.Parse("0.49"), nfloat.Parse("0.36"), nfloat.Parse("1")));
            this.chartColors.Add(UIColor.FromRGBA(nfloat.Parse("0.72"), nfloat.Parse("0.0"), nfloat.Parse("0.48"), nfloat.Parse("1"))); 
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return reports.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var resultCell = collectionView.DequeueReusableCell(ResultViewController.resultCellId, indexPath) as ResultCollectionViewCell;
            resultCell.BackgroundColor = UIColor.White;

            var reportDataSet = this.reportsDatasets[indexPath.Row];

            if (ReportType.Overall.Equals(reports[indexPath.Row]))
            {
                resultCell.sectionLabel.Text = reports[indexPath.Row].ToString();

                if (reportDataSet.totalVotes > 0)
                {
                    var dataEntries = new List<PieChartDataEntry>();
                    for (int i = 0; i < reportDataSet.dataSets[0].Count; i++)
                    {
                        if (Convert.ToInt32(reportDataSet.dataSets[0][i]) > 0)
                        {
                            dataEntries.Add(new PieChartDataEntry(reportDataSet.dataSets[0][i], reportDataSet.labels[i]));
                        }
                    }

                    var dataSet = new PieChartDataSet(dataEntries.ToArray(), "");

                    dataSet.ValueFormatter = new ChartDefaultValueFormatter(new NSNumberFormatter() { MinimumFractionDigits = 0 });
                    dataSet.SliceSpace = 2;
                    dataSet.Colors = ChartColorTemplates.Joyful;
                    dataSet.ValueTextColor = UIColor.FromRGB(90, 89, 89);
                    dataSet.ValueLineColor = UIColor.FromRGB(90, 89, 89);
                    dataSet.EntryLabelColor = UIColor.FromRGB(90, 89, 89);
                    dataSet.XValuePosition = PieChartValuePosition.OutsideSlice;
                    dataSet.YValuePosition = PieChartValuePosition.OutsideSlice;

                    resultCell.pieChartView.Data = new PieChartData(new PieChartDataSet[] { dataSet });
                }

                resultCell.pieChartView.AnimateWithXAxisDuration(1.4, ChartEasingOption.EaseOutBack);
                resultCell.pieChartView.DescriptionText = "";
                var colorAttributes = new UIStringAttributes
                {
                    ForegroundColor = UIColor.FromRGB(90, 89, 89)                    
                };
                resultCell.pieChartView.CenterAttributedText = new NSAttributedString(string.Format("Total {0} votes", reportDataSet.totalVotes, colorAttributes));                
                resultCell.pieChartView.Legend.Enabled = false;
                resultCell.pieChartView.NoDataText = "No results to show";
                
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
                resultCell.barChartView.Legend.TextColor = UIColor.FromRGB(90, 89, 89);

                resultCell.barChartView.XAxis.DrawGridLinesEnabled = true;
                resultCell.barChartView.XAxis.LabelPosition = XAxisLabelPosition.Bottom;
                resultCell.barChartView.XAxis.CenterAxisLabelsEnabled = true;
                resultCell.barChartView.XAxis.ValueFormatter = new ChartIndexAxisValueFormatter(TranslateReportGroups(reportDataSet.groups.ToArray()));
                resultCell.barChartView.XAxis.Granularity = 1;
                resultCell.barChartView.XAxis.LabelTextColor = UIColor.FromRGB(90, 89, 89);
                resultCell.barChartView.XAxis.GridColor = UIColor.FromRGB(90, 89, 89);
                resultCell.barChartView.XAxis.AxisLineColor = UIColor.FromRGB(90, 89, 89);

                resultCell.barChartView.LeftAxis.SpaceTop = 0.35f;
                resultCell.barChartView.LeftAxis.AxisMinimum = 0;
                resultCell.barChartView.LeftAxis.DrawGridLinesEnabled = false;
                resultCell.barChartView.LeftAxis.Granularity = 1;
                resultCell.barChartView.LeftAxis.ValueFormatter = new ChartDefaultAxisValueFormatter(new NSNumberFormatter() { MinimumFractionDigits = 0 });
                resultCell.barChartView.LeftAxis.LabelTextColor = UIColor.FromRGB(90, 89, 89);
                resultCell.barChartView.LeftAxis.GridColor = UIColor.FromRGB(90, 89, 89);
                resultCell.barChartView.LeftAxis.AxisLineColor = UIColor.FromRGB(90, 89, 89);
                resultCell.barChartView.LeftAxis.ZeroLineColor = UIColor.FromRGB(90, 89, 89);

                resultCell.barChartView.RightAxis.Enabled = false;

                if (reportDataSet.totalVotes > 0)
                {
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
                        barChartDataSet.ValueFormatter = new ChartDefaultValueFormatter(new NSNumberFormatter() { MinimumFractionDigits = 0, ZeroSymbol = "" });
                        barChartDataSet.ValueTextColor = UIColor.FromRGB(90, 89, 89);
                        barChartDataSet.BarBorderColor = UIColor.FromRGB(90, 89, 89);                        

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
                }
                
                resultCell.barChartView.AnimateWithXAxisDuration(1.4, ChartEasingOption.Linear);
                resultCell.barChartView.DescriptionText = "";
                resultCell.barChartView.NoDataText = "No results to show";

                resultCell.AddSubview(resultCell.barChartView);

                resultCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", resultCell.barChartView));
                resultCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0(24)][v1]|", new NSLayoutFormatOptions(), "v0", resultCell.sectionLabel, "v1", resultCell.barChartView));
            }

            return resultCell;
        }

        public override UICollectionReusableView GetViewForSupplementaryElement(UICollectionView collectionView, NSString elementKind, NSIndexPath indexPath)
        {
            var headerView = collectionView.DequeueReusableSupplementaryView(elementKind, ResultViewController.feedHeadId, indexPath);

            headerView.AddSubview(feedCell);

            return headerView;
        }

        public string[] TranslateReportGroups(string[] groups)
        {
            for (int i = 0; i < groups.Length; i++)
            {
                if (groups[i].ToString().Equals("Masculino"))
                {
                    groups[i] = "Male";
                }
                else if (groups[i].ToString().Equals("Feminino"))
                {
                    groups[i] = "Female";
                }
                else if (groups[i].ToString().Contains("18"))
                {
                    groups[i] = "0 - 18";
                }
                else if (groups[i].ToString().Contains("19"))
                {
                    groups[i] = "19 - 30";
                }
                else if (groups[i].ToString().Contains("31"))
                {
                    groups[i] = "31 - 40";
                }
                else if (groups[i].ToString().Contains("41"))
                {
                    groups[i] = "41+";
                }
            }

            return groups;
        }
    }

    public class ResultsCollectionViewDelegate : UICollectionViewDelegateFlowLayout
    {
        public float heightHeaderCell { get; set; }

        public ResultsCollectionViewDelegate(float heightHeaderCell)
        {
            this.heightHeaderCell = heightHeaderCell;
        }

        public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
        {
            return new CGSize(collectionView.Frame.Width, 300);
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
            sectionLabel.TextColor = UIColor.FromRGB(90, 89, 89); 
            sectionLabel.TextAlignment = UITextAlignment.Center;
            sectionLabel.BackgroundColor = UIColor.FromWhiteAlpha(nfloat.Parse("0.95"), 1);
            sectionLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            pieChartView = new PieChartView();
            pieChartView.TranslatesAutoresizingMaskIntoConstraints = false;
            pieChartView.DescriptionTextColor = UIColor.FromRGB(90, 89, 89);
            pieChartView.EntryLabelColor = UIColor.FromRGB(90, 89, 89);
            pieChartView.TintColor = UIColor.FromRGB(90, 89, 89);
            pieChartView.NoDataTextColor = UIColor.FromRGB(90, 89, 89);

            barChartView = new BarChartView();
            barChartView.TranslatesAutoresizingMaskIntoConstraints = false;
            barChartView.DescriptionTextColor = UIColor.FromRGB(90, 89, 89);
            barChartView.BorderColor = UIColor.FromRGB(90, 89, 89);
            barChartView.TintColor = UIColor.FromRGB(90, 89, 89);
            barChartView.NoDataTextColor = UIColor.FromRGB(90, 89, 89);

            AddSubview(sectionLabel);

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", sectionLabel));
        }
    }
}

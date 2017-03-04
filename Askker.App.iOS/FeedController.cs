using Foundation;
using System;
using System.Collections.Generic;
using UIKit;
using CoreGraphics;
using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Business;
using CoreFoundation;
using Askker.App.PortableLibrary.Util;

namespace Askker.App.iOS
{
    public partial class FeedController : UICollectionViewController
    {
        public static List<SurveyModel> surveys { get; set; }
        public static NSCache imageCache = new NSCache();

        public FeedController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            imageCache.RemoveAllObjects();

            feedCollectionView.BackgroundColor = UIColor.FromWhiteAlpha(nfloat.Parse("0.95"), 1);
            feedCollectionView.Delegate = new FeedCollectionViewDelegate();
            feedCollectionView.AlwaysBounceVertical = true;

            surveys = new List<SurveyModel>();
            fetchSurveys();
        }

        public async void fetchSurveys()
        {
            surveys = await new FeedManager().GetSurveys(LoginController.tokenModel.Id, LoginController.tokenModel.Access_Token);

            foreach (var survey in surveys)
            {
                survey.Options = Common.Randomize(survey.Options);
            }
            
            feedCollectionView.ReloadData();
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return surveys.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var feedCell = collectionView.DequeueReusableCell("feedCell", indexPath) as UICollectionViewCell;

            feedCell.BackgroundColor = UIColor.White;

            var profileImageView = new UIImageView();
            profileImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            profileImageView.Image = UIImage.FromBundle("Profile");
            profileImageView.Layer.CornerRadius = 22;
            profileImageView.Layer.MasksToBounds = true;
            profileImageView.TranslatesAutoresizingMaskIntoConstraints = false;

            if (surveys[indexPath.Row].profilePicture != null)
            {
                var url = new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + surveys[indexPath.Row].UserId + "/" + surveys[indexPath.Row].profilePicture);

                var imageFromCache = (UIImage)imageCache.ObjectForKey(NSString.FromObject(url.AbsoluteString));
                if (imageFromCache != null)
                {
                    profileImageView.Image = imageFromCache;
                }
                else
                {
                    var task = NSUrlSession.SharedSession.CreateDataTask(url, (data, response, error) =>
                    {
                        if (response == null)
                        {
                            profileImageView.Image = UIImage.FromBundle("Profile");
                        }
                        else
                        {
                            try
                            {
                                DispatchQueue.MainQueue.DispatchAsync(() => {
                                    var imageToCache = UIImage.LoadFromData(data);

                                    profileImageView.Image = imageToCache;

                                    if (imageToCache != null)
                                    {
                                        imageCache.SetObjectforKey(imageToCache, NSString.FromObject(url.AbsoluteString));
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message);
                            }
                        }
                    });
                    task.Resume();
                }
            }

            var nameLabel = new UILabel();
            nameLabel.Lines = 2;
            var attributedText = new NSMutableAttributedString(surveys[indexPath.Row].UserName, UIFont.BoldSystemFontOfSize(14));
            attributedText.Append(new NSAttributedString("\nto Public", UIFont.SystemFontOfSize(12), UIColor.FromRGBA(nfloat.Parse("0.60"), nfloat.Parse("0.63"), nfloat.Parse("0.67"), nfloat.Parse("1"))));

            var paragraphStyle = new NSMutableParagraphStyle();
            paragraphStyle.LineSpacing = 4;
            attributedText.AddAttribute(new NSString("ParagraphStyle"), paragraphStyle, new NSRange(0, attributedText.Length));

            nameLabel.AttributedText = attributedText;
            nameLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            var questionText = new UITextView();
            questionText.Font = UIFont.SystemFontOfSize(14);
            questionText.Text = surveys[indexPath.Row].Question.Text;
            questionText.BackgroundColor = UIColor.Clear;
            questionText.ScrollEnabled = false;
            questionText.TranslatesAutoresizingMaskIntoConstraints = false;

            var dividerLineView = new UIView();
            dividerLineView.BackgroundColor = UIColor.FromRGBA(nfloat.Parse("0.88"), nfloat.Parse("0.89"), nfloat.Parse("0.90"), nfloat.Parse("1"));
            dividerLineView.TranslatesAutoresizingMaskIntoConstraints = false;

            var optionsTableView = new UITableView();
            
            var optionsCollectionView = new UICollectionView(new CGRect(), new UICollectionViewFlowLayout()
            {
                ScrollDirection = UICollectionViewScrollDirection.Horizontal
            });

            if (surveys[indexPath.Row].Type.Equals("Text"))
            {
                optionsTableView.ContentMode = UIViewContentMode.ScaleAspectFill;
                optionsTableView.Layer.MasksToBounds = true;
                optionsTableView.TranslatesAutoresizingMaskIntoConstraints = false;
                optionsTableView.ContentInset = new UIEdgeInsets(0, -10, 0, 0);

                new OptionsTableViewController(optionsTableView, surveys[indexPath.Row].Options);
            }
            else
            {
                optionsCollectionView.TranslatesAutoresizingMaskIntoConstraints = false;

                new OptionsCollectionViewController(optionsCollectionView, surveys[indexPath.Row].Options);
            }

            var commentButton = buttonForTitle(title: "Comment", imageName: "comment");
            var resultButton = buttonForTitle(title: "Result", imageName: "result");
            var moreButton = buttonForTitle(title: "More", imageName: "more");

            var contentViewButtons = new UIView();
            contentViewButtons.AddSubviews(commentButton, resultButton, moreButton);
            contentViewButtons.TranslatesAutoresizingMaskIntoConstraints = false;

            feedCell.AddSubview(profileImageView);
            feedCell.AddSubview(nameLabel);
            feedCell.AddSubview(questionText);
            feedCell.AddSubview(dividerLineView);

            if (surveys[indexPath.Row].Type.Equals("Text"))
            {
                feedCell.AddSubview(optionsTableView);
            }
            else
            {
                feedCell.AddSubview(optionsCollectionView);
            }

            feedCell.AddSubview(contentViewButtons);

            feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-8-[v0(44)]-8-[v1]|", new NSLayoutFormatOptions(), "v0", profileImageView, "v1", nameLabel));
            feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-4-[v0]-4-|", new NSLayoutFormatOptions(), "v0", questionText));
            feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", dividerLineView));

            if (surveys[indexPath.Row].Type.Equals("Text"))
            {
                feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", optionsTableView));
            }
            else
            {
                feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", optionsCollectionView));
            }

            feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", contentViewButtons));
            feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0(v2)][v1(v2)][v2]|", new NSLayoutFormatOptions(), "v0", commentButton, "v1", resultButton, "v2", moreButton));

            feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-12-[v0]", new NSLayoutFormatOptions(), "v0", nameLabel));

            if (surveys[indexPath.Row].Type.Equals("Text"))
            {
                feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(44)]-4-[v1]-4-[v2(1)]-4-[v3(<=220)][v4(44)]|", new NSLayoutFormatOptions(), "v0", profileImageView, "v1", questionText, "v2", dividerLineView, "v3", optionsTableView, "v4", contentViewButtons));
            }
            else
            {
                feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(44)]-4-[v1]-4-[v2(1)][v3(<=176)][v4(44)]|", new NSLayoutFormatOptions(), "v0", profileImageView, "v1", questionText, "v2", dividerLineView, "v3", optionsCollectionView, "v4", contentViewButtons));
            }

            feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0(44)]|", new NSLayoutFormatOptions(), "v0", commentButton));
            feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0(44)]|", new NSLayoutFormatOptions(), "v0", resultButton));
            feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0(44)]|", new NSLayoutFormatOptions(), "v0", moreButton));

            return feedCell;
        }

        public UIButton buttonForTitle(string title, string imageName) {
            var button = new UIButton();
            button.SetTitle(title, UIControlState.Normal);
            button.SetTitleColor(UIColor.FromRGBA(nfloat.Parse("0.56"), nfloat.Parse("0.58"), nfloat.Parse("0.63"), nfloat.Parse("1")), UIControlState.Normal);
            button.TitleLabel.Font = UIFont.BoldSystemFontOfSize(14);
            //button.SetImage(new UIImage(imageName), UIControlState.Normal);
            button.TitleEdgeInsets = new UIEdgeInsets(0, 8, 0, 0);
            button.TranslatesAutoresizingMaskIntoConstraints = false;
            return button;
        }

        class FeedCollectionViewDelegate : UICollectionViewDelegateFlowLayout
        {
            public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
            {
                var question = FeedController.surveys[indexPath.Row].Question.Text;
                if (!question.Equals(""))
                {
                    var rect = new NSString(question).GetBoundingRect(new CGSize(collectionView.Frame.Width, 1000), NSStringDrawingOptions.UsesFontLeading | NSStringDrawingOptions.UsesLineFragmentOrigin, new UIStringAttributes() { Font = UIFont.SystemFontOfSize(14) }, null);

                    var optionsHeight = 220;

                    if (FeedController.surveys[indexPath.Row].Options.Count < 5)
                    {
                        optionsHeight = FeedController.surveys[indexPath.Row].Options.Count * 44;
                    }

                    // Heights of the vertical components to format the cell dinamic height
                    var knownHeight = 8 + 44 + 4 + 4 + optionsHeight + 8 + 44;

                    return new CGSize(collectionView.Frame.Width, rect.Height + knownHeight + 25);
                }

                return new CGSize(collectionView.Frame.Width, 380);
            }
        }
    }

    public class OptionsTableViewController : UITableViewController
    {
        static NSString optionCellId = new NSString("OptionCell");
        public static string[] alphabet = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };

        public OptionsTableViewController(UITableView optionsTableView, List<Option> options)
        {
            optionsTableView.RegisterClassForCellReuse(typeof(UITableViewCell), optionCellId);
            optionsTableView.Source = new OptionsTableViewDataSource(options);
        }

        class OptionsTableViewDataSource : UITableViewSource
        {
            List<Option> options;

            public OptionsTableViewDataSource(List<Option> options)
            {
                this.options = options;
            }

            public override nint RowsInSection(UITableView tableView, nint section)
            {
                return options.Count;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(OptionsTableViewController.optionCellId);
                cell.Tag = options[indexPath.Row].Id;

                var optionLetterLabel = new UILabel();
                optionLetterLabel.Text = OptionsTableViewController.alphabet[indexPath.Row];
                optionLetterLabel.Font = UIFont.BoldSystemFontOfSize(16);
                optionLetterLabel.TranslatesAutoresizingMaskIntoConstraints = false;

                var optionLabel = new UILabel();
                optionLabel.Text = options[indexPath.Row].Text;
                optionLabel.Font = UIFont.SystemFontOfSize(14);
                optionLabel.TranslatesAutoresizingMaskIntoConstraints = false;

                cell.AddSubview(optionLetterLabel);
                cell.AddSubview(optionLabel);

                cell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-35-[v0(25)]-10-[v1]-8-|", new NSLayoutFormatOptions(), "v0", optionLetterLabel, "v1", optionLabel));

                cell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(25)]", new NSLayoutFormatOptions(), "v0", optionLetterLabel));
                cell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(25)]", new NSLayoutFormatOptions(), "v0", optionLabel));

                return cell;
            }
        }
    }

    public class OptionsCollectionViewController : UICollectionViewController
    {
        static NSString optionCellId = new NSString("optionCell");

        public OptionsCollectionViewController(UICollectionView optionsCollectionView, List<Option> options)
        {
            optionsCollectionView.BackgroundColor = UIColor.FromWhiteAlpha(nfloat.Parse("0.95"), 1);
            optionsCollectionView.RegisterClassForCell(typeof(UICollectionViewCell), optionCellId);
            optionsCollectionView.Delegate = new OptionsCollectionViewDelegate();
            optionsCollectionView.DataSource = new OptionsCollectionViewDataSource(options);
        }

        class OptionsCollectionViewDataSource : UICollectionViewSource
        {
            List<Option> options;

            public OptionsCollectionViewDataSource(List<Option> options) {
                this.options = options;
            }

            public override nint GetItemsCount(UICollectionView collectionView, nint section)
            {
                return options.Count;
            }

            public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
            {
                var optionCell = collectionView.DequeueReusableCell(optionCellId, indexPath) as UICollectionViewCell;
                optionCell.BackgroundColor = UIColor.White;
                optionCell.Tag = options[indexPath.Row].Id;

                var optionImageView = new UIImageView();
                optionImageView.Image = null;
                optionImageView.ContentMode = UIViewContentMode.ScaleToFill;
                optionImageView.Layer.MasksToBounds = true;
                optionImageView.TranslatesAutoresizingMaskIntoConstraints = false;

                if (options[indexPath.Row].Image != null)
                {
                    var url = new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + options[indexPath.Row].Image);

                    var imageFromCache = (UIImage)FeedController.imageCache.ObjectForKey(NSString.FromObject(url.AbsoluteString));
                    if (imageFromCache != null)
                    {
                        optionImageView.Image = imageFromCache;
                    }
                    else
                    {
                        var task = NSUrlSession.SharedSession.CreateDataTask(url, (data, response, error) =>
                    {
                        if (response == null)
                        {
                            optionImageView.Image = null;
                        }
                        else
                        {
                            try
                            {
                                DispatchQueue.MainQueue.DispatchAsync(() => {
                                    var imageToCache = UIImage.LoadFromData(data);

                                    optionImageView.Image = imageToCache;

                                    if (imageToCache != null)
                                    {
                                        FeedController.imageCache.SetObjectforKey(imageToCache, NSString.FromObject(url.AbsoluteString));
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message);
                            }
                        }
                    });
                    task.Resume();
                    }
                }

                var optionLetterLabel = new UILabel();
                optionLetterLabel.Text = "  " + OptionsTableViewController.alphabet[indexPath.Row] + "  ";
                optionLetterLabel.Font = UIFont.BoldSystemFontOfSize(14);
                optionLetterLabel.TextColor = UIColor.White;
                optionLetterLabel.BackgroundColor = UIColor.DarkGray;
                optionLetterLabel.TranslatesAutoresizingMaskIntoConstraints = false;

                var optionLabel = new UILabel();
                optionLabel.Text = options[indexPath.Row].Text;
                optionLabel.Font = UIFont.SystemFontOfSize(12);
                optionLabel.TextColor = UIColor.White;
                optionLabel.BackgroundColor = UIColor.DarkGray;
                optionLabel.TranslatesAutoresizingMaskIntoConstraints = false;

                optionCell.AddSubview(optionImageView);
                optionCell.AddSubview(optionLetterLabel);
                optionCell.AddSubview(optionLabel);

                optionCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", optionImageView));
                optionCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-8-[v0(25)][v1]-8-|", new NSLayoutFormatOptions(), "v0", optionLetterLabel, "v1", optionLabel));

                optionCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[v0]|", new NSLayoutFormatOptions(), "v0", optionImageView));
                optionCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-25-[v0(25)]", new NSLayoutFormatOptions(), "v0", optionLetterLabel));
                optionCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-25-[v0(25)]", new NSLayoutFormatOptions(), "v0", optionLabel));

                return optionCell;
            }
        }

        class OptionsCollectionViewDelegate : UICollectionViewDelegateFlowLayout
        {
            public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
            {
                return new CGSize(270, 220);
            }

            public override nfloat GetMinimumLineSpacingForSection(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
            {
                return 0;
            }
        }
    }
}
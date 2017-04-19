﻿using Foundation;
using System;
using System.Collections.Generic;
using UIKit;
using CoreGraphics;
using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Business;
using CoreFoundation;
using Askker.App.PortableLibrary.Util;
using Askker.App.PortableLibrary.Enums;

namespace Askker.App.iOS
{
    public partial class FeedController : UICollectionViewController
    {
        public static List<SurveyModel> surveys { get; set; }
        public static NSCache imageCache = new NSCache();
        public static VoteManager voteManager = new VoteManager();
        public string filter { get; set; }
        public UIActivityIndicatorView indicator;
        static NSString feedCellId = new NSString("feedCell");

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

            CollectionView.RegisterClassForCell(typeof(FeedCollectionViewCellCell), feedCellId);
            feedCollectionView.BackgroundColor = UIColor.FromWhiteAlpha(nfloat.Parse("0.95"), 1);
            feedCollectionView.Delegate = new FeedCollectionViewDelegate();
            feedCollectionView.AlwaysBounceVertical = true;

            surveys = new List<SurveyModel>();

            if (filter == null)
            {
                filter = "nofilter";
            }

            indicator.StartAnimating();
            fetchSurveys(filter);
        }

        public async void fetchSurveys(string filter)
        {
            surveys = await new FeedManager().GetFeed(LoginController.userModel.id, filter, LoginController.tokenModel.access_token);

            foreach (var survey in surveys)
            {
                survey.options = Common.Randomize(survey.options);
            }

            indicator.StopAnimating();
            feedCollectionView.ReloadData();
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return surveys.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var feedCell = collectionView.DequeueReusableCell(feedCellId, indexPath) as FeedCollectionViewCellCell;

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
                                throw new Exception(ex.Message);
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

            feedCell.questionText.Text = surveys[indexPath.Row].question.text;

            if (surveys[indexPath.Row].type == SurveyType.Text.ToString())
            {
                feedCell.optionsTableView.ContentMode = UIViewContentMode.ScaleAspectFill;
                feedCell.optionsTableView.Layer.MasksToBounds = true;
                feedCell.optionsTableView.TranslatesAutoresizingMaskIntoConstraints = false;
                feedCell.optionsTableView.ContentInset = new UIEdgeInsets(0, -10, 0, 0);
                feedCell.optionsTableView.Tag = indexPath.Row;

                new OptionsTableViewController(feedCell.optionsTableView, surveys[indexPath.Row].options);
            }
            else
            {
                feedCell.optionsCollectionView.TranslatesAutoresizingMaskIntoConstraints = false;
                feedCell.optionsCollectionView.Tag = indexPath.Row;

                new OptionsCollectionViewController(feedCell.optionsCollectionView, surveys[indexPath.Row].options);
            }

            if (surveys[indexPath.Row].type == SurveyType.Text.ToString())
            {
                feedCell.optionsCollectionView.RemoveFromSuperview();
                feedCell.AddSubview(feedCell.optionsTableView);
            }
            else
            {
                feedCell.optionsTableView.RemoveFromSuperview();
                feedCell.AddSubview(feedCell.optionsCollectionView);
            }

            if (surveys[indexPath.Row].type == SurveyType.Text.ToString())
            {
                feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", feedCell.optionsTableView));
            }
            else
            {
                feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", feedCell.optionsCollectionView));
            }

            if (surveys[indexPath.Row].type == SurveyType.Text.ToString())
            {
                feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(44)]-4-[v1]-4-[v2(1)]-4-[v3(<=176)][v4(44)]|", new NSLayoutFormatOptions(), "v0", feedCell.profileImageView, "v1", feedCell.questionText, "v2", feedCell.dividerLineView, "v3", feedCell.optionsTableView, "v4", feedCell.contentViewButtons));
            }
            else
            {
                feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(44)]-4-[v1]-4-[v2(1)][v3(<=176)][v4(44)]|", new NSLayoutFormatOptions(), "v0", feedCell.profileImageView, "v1", feedCell.questionText, "v2", feedCell.dividerLineView, "v3", feedCell.optionsCollectionView, "v4", feedCell.contentViewButtons));
            }

            return feedCell;
        }

        public class FeedCollectionViewCellCell : UICollectionViewCell
        {
            public UIImageView profileImageView { get; set; }
            public UILabel nameLabel { get; set; }
            public UITextView questionText { get; set; }
            public UIView dividerLineView { get; set; }
            public UITableView optionsTableView { get; set; }
            public UICollectionView optionsCollectionView { get; set; }
            public UIView contentViewButtons { get; set; }

            [Export("initWithFrame:")]
            public FeedCollectionViewCellCell(CGRect frame) : base(frame)
            {
                ContentView.BackgroundColor = UIColor.White;

                profileImageView = new UIImageView();
                profileImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
                profileImageView.Image = UIImage.FromBundle("Profile");
                profileImageView.Layer.CornerRadius = 22;
                profileImageView.Layer.MasksToBounds = true;
                profileImageView.TranslatesAutoresizingMaskIntoConstraints = false;

                nameLabel = new UILabel();
                nameLabel.Lines = 2;
                nameLabel.TranslatesAutoresizingMaskIntoConstraints = false;

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

                optionsCollectionView = new UICollectionView(new CGRect(), new UICollectionViewFlowLayout()
                {
                    ScrollDirection = UICollectionViewScrollDirection.Horizontal
                });

                var commentButton = buttonForTitle(title: "Comment", imageName: "comment");
                var resultButton = buttonForTitle(title: "Result", imageName: "result");
                var moreButton = buttonForTitle(title: "More", imageName: "more");

                contentViewButtons = new UIView();
                contentViewButtons.AddSubviews(commentButton, resultButton, moreButton);
                contentViewButtons.TranslatesAutoresizingMaskIntoConstraints = false;

                AddSubview(profileImageView);
                AddSubview(nameLabel);
                AddSubview(questionText);
                AddSubview(dividerLineView);

                AddSubview(contentViewButtons);

                AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-8-[v0(44)]-8-[v1]|", new NSLayoutFormatOptions(), "v0", profileImageView, "v1", nameLabel));
                AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-4-[v0]-4-|", new NSLayoutFormatOptions(), "v0", questionText));
                AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", dividerLineView));

                AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", contentViewButtons));
                AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0(v2)][v1(v2)][v2]|", new NSLayoutFormatOptions(), "v0", commentButton, "v1", resultButton, "v2", moreButton));

                AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-12-[v0]", new NSLayoutFormatOptions(), "v0", nameLabel));

                AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0(44)]|", new NSLayoutFormatOptions(), "v0", commentButton));
                AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0(44)]|", new NSLayoutFormatOptions(), "v0", resultButton));
                AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[v0(44)]|", new NSLayoutFormatOptions(), "v0", moreButton));
            }

            public UIButton buttonForTitle(string title, string imageName)
            {
                var button = new UIButton();
                button.SetTitle(title, UIControlState.Normal);
                button.SetTitleColor(UIColor.FromRGBA(nfloat.Parse("0.56"), nfloat.Parse("0.58"), nfloat.Parse("0.63"), nfloat.Parse("1")), UIControlState.Normal);
                button.TitleLabel.Font = UIFont.BoldSystemFontOfSize(14);
                //button.SetImage(new UIImage(imageName), UIControlState.Normal);
                button.TitleEdgeInsets = new UIEdgeInsets(0, 8, 0, 0);
                button.TranslatesAutoresizingMaskIntoConstraints = false;
                return button;
            }
        }

        public static async void saveVote(int surveyIndex, int optionId)
        {
            surveys[surveyIndex].optionSelected = optionId;

            VoteModel voteModel = new VoteModel();
            voteModel.surveyId = surveys[surveyIndex].userId + surveys[surveyIndex].creationDate;
            voteModel.optionId = optionId;
            voteModel.user = new User();
            voteModel.user.id = LoginController.userModel.id;
            voteModel.user.gender = LoginController.userModel.gender;
            voteModel.user.city = LoginController.userModel.city;
            voteModel.user.country = LoginController.userModel.country;

            await voteManager.Vote(voteModel, "");
        }

        class FeedCollectionViewDelegate : UICollectionViewDelegateFlowLayout
        {
            public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
            {
                var question = FeedController.surveys[indexPath.Row].question.text;
                if (!question.Equals(""))
                {
                    var rect = new NSString(question).GetBoundingRect(new CGSize(collectionView.Frame.Width, 1000), NSStringDrawingOptions.UsesFontLeading | NSStringDrawingOptions.UsesLineFragmentOrigin, new UIStringAttributes() { Font = UIFont.SystemFontOfSize(14) }, null);

                    var optionsHeight = 176;

                    if (surveys[indexPath.Row].type == SurveyType.Text.ToString() && FeedController.surveys[indexPath.Row].options.Count < 4)
                    {
                        optionsHeight = FeedController.surveys[indexPath.Row].options.Count * 44;
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
            optionsTableView.RegisterClassForCellReuse(typeof(OptionTableViewCell), optionCellId);
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
                var cell = tableView.DequeueReusableCell(OptionsTableViewController.optionCellId, indexPath) as OptionTableViewCell;
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                cell.Tag = options[indexPath.Row].id;

                cell.optionLetterLabel.Text = OptionsTableViewController.alphabet[indexPath.Row];
                cell.optionLabel.Text = options[indexPath.Row].text;

                if (FeedController.surveys[(int)tableView.Tag].optionSelected == options[indexPath.Row].id)
                {
                    var optionCheckImage = new UIImageView(UIImage.FromBundle("OptionCheck"));
                    optionCheckImage.Frame = new CGRect(0, 0, 40, 40);
                    cell.AccessoryView = optionCheckImage;
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

                //else
                //{
                //    optionCell.AccessoryView = null;
                //    optionCell.SetSelected(false, false);
                //}
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

    public class OptionsCollectionViewController : UICollectionViewController
    {
        static NSString optionCellId = new NSString("optionCell");

        public OptionsCollectionViewController(UICollectionView optionsCollectionView, List<Option> options)
        {

            var optionsCollectionViewSource = new OptionsCollectionViewSource(options);
            var optionsCollectionViewDelegate = new OptionsCollectionViewDelegate(optionsCollectionViewSource);

            CollectionView = optionsCollectionView;

            CollectionView.BackgroundColor = UIColor.FromWhiteAlpha(nfloat.Parse("0.95"), 1);
            CollectionView.RegisterClassForCell(typeof(OptionCollectionViewCell), optionCellId);
            CollectionView.Source = optionsCollectionViewSource;
            CollectionView.Delegate = optionsCollectionViewDelegate;
        }

        class OptionsCollectionViewSource : UICollectionViewSource
        {
            List<Option> options;

            public OptionsCollectionViewSource(List<Option> options) {
                this.options = options;
            }

            public override nint GetItemsCount(UICollectionView collectionView, nint section)
            {
                return options.Count;
            }

            public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
            {
                var optionCell = collectionView.DequeueReusableCell(optionCellId, indexPath) as OptionCollectionViewCell;
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
                                throw new Exception(ex.Message);
                            }
                        }
                    });
                    task.Resume();
                    }
                }

                optionCell.optionLetterLabel.Text = "  " + OptionsTableViewController.alphabet[indexPath.Row] + "  ";
                optionCell.optionLabel.Text = options[indexPath.Row].text;

                if (FeedController.surveys[(int)collectionView.Tag].optionSelected == options[indexPath.Row].id)
                {
                    optionCell.optionCheckImageView.Hidden = false;
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

        class OptionsCollectionViewDelegate : UICollectionViewDelegateFlowLayout
        {
            OptionsCollectionViewSource optionsCollectionViewSource;

            public OptionsCollectionViewDelegate(OptionsCollectionViewSource optionsCollectionViewSource)
            {
                this.optionsCollectionViewSource = optionsCollectionViewSource;
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
}
using Askker.App.iOS.HorizontalSwipe;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Askker.App.iOS
{
    public partial class CreateSurveySecondStep : CustomUIViewController, IMultiStepProcessStep
    {
        public int StepIndex { get; set; }
        public FeedCollectionViewCell feedCell { get; set; }

        public event EventHandler<MultiStepProcessStepEventArgs> StepActivated;
        public event EventHandler<MultiStepProcessStepEventArgs> StepDeactivated;

        public override void LoadView()
        {
            View = new UIView();
            View.BackgroundColor = UIColor.White;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);

            var feedCellHeight = Utils.getHeightForFeedCell(CreateSurveyController.SurveyModel, View.Frame.Width);
            feedCell = new FeedCollectionViewCell(new CGRect(0, 0, View.Frame.Width, feedCellHeight));

            Utils.BindFeedCell(feedCell, CreateSurveyController.SurveyModel, 0, this, true);

            UIScrollView scrollView = new UIScrollView();
            scrollView.Frame = new CGRect(0, 70, View.Frame.Width, View.Frame.Height - 70);
            scrollView.ContentSize = new CGSize(View.Frame.Width, feedCellHeight);
            scrollView.LayoutIfNeeded();
            scrollView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            scrollView.AddSubview(feedCell);

            View.AddSubview(scrollView);

            //View.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            //View.AddConstraints(

            //    scrollView.WithSameCenterX(View),
            //    scrollView.AtTopOf(View, 70),
            //    scrollView.AtLeftOf(View),
            //    scrollView.AtRightOf(View),
            //    scrollView.Height().EqualTo(feedCellHeight),

            //    feedCell.WithSameCenterX(scrollView),
            //    feedCell.AtTopOf(scrollView),
            //    feedCell.AtLeftOf(scrollView),
            //    feedCell.AtRightOf(scrollView),
            //    feedCell.Height().EqualTo(feedCellHeight)
            //);

            StepActivated?.Invoke(this, new MultiStepProcessStepEventArgs { Index = StepIndex });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            foreach (UIView subview in View.Subviews)
            {
                if (subview.GetType() == typeof(FeedCollectionViewCell))
                {
                    subview.RemoveFromSuperview();
                }
            }

            StepDeactivated?.Invoke(this, new MultiStepProcessStepEventArgs { Index = StepIndex });
        }
    }
}

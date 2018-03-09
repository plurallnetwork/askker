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
            
            View.Add(feedCell);

            View.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            View.AddConstraints(

                feedCell.WithSameCenterX(View),
                feedCell.AtTopOf(View, 70),
                feedCell.AtLeftOf(View),
                feedCell.AtRightOf(View),
                feedCell.Height().EqualTo(feedCellHeight)
            );

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

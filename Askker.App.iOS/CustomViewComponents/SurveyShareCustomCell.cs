using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Askker.App.iOS.CustomViewComponents
{
    public class SurveyShareCustomCell : UITableViewCell
    {
        UILabel textLabel;
        UIImageView customImageView;

        public SurveyShareCustomCell(NSString cellId) : base (UITableViewCellStyle.Default, cellId)
        {
            customImageView = new UIImageView();
            textLabel = new UILabel();

            ContentView.AddSubviews(new UIView[] { customImageView, textLabel });

        }

        public void UpdateCell(string caption)
        {
            textLabel.Text = caption;
        }

        public void UpdateCell(string caption, UIImage image)
        {
            textLabel.Text = caption;
            customImageView.Image = image;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            customImageView.Frame = new CGRect(8, 8, 30, 30);
            customImageView.Layer.CornerRadius = 15;
            customImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            customImageView.Layer.MasksToBounds = true;
            customImageView.TranslatesAutoresizingMaskIntoConstraints = false;
            customImageView.ClipsToBounds = true;
            this.LayoutIfNeeded();

            textLabel.Frame = new CGRect(50, 10, ContentView.Bounds.Width - 50, 25);
            textLabel.TextColor = UIColor.FromRGB(90, 89, 89);
        }

        public UIImageView GetCustomImageView()
        {
            return this.customImageView;
        }
    }
}

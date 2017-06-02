using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Askker.App.iOS.CustomViewComponents
{
    public class SearchAllCustomCell : UITableViewCell
    {
        UILabel textLabel;
        UIImageView imageView;

        public SearchAllCustomCell(NSString cellId) : base (UITableViewCellStyle.Default, cellId)
        {
            imageView = new UIImageView();
            textLabel = new UILabel();
            
            ContentView.AddSubviews(new UIView[] { imageView, textLabel });

        }

        public void UpdateCell(string caption)
        {
            textLabel.Text = caption;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            imageView.Frame = new CGRect(4, 4, 34, 34);
            imageView.Layer.CornerRadius = 17;
            imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            imageView.Layer.MasksToBounds = true;
            imageView.TranslatesAutoresizingMaskIntoConstraints = false;

            textLabel.Frame = new CGRect(50, 10, ContentView.Bounds.Width - 50, 25);
        }

        public UIImageView GetImageView()
        {
            return this.imageView;
        }
    }
}

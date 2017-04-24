using Askker.App.PortableLibrary.Enums;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Askker.App.iOS.CustomViewComponents
{
    public class SurveyOptionCustomCell : UITableViewCell
    {
        public UILabel textLabel;
        public UIImageView imageView;

        public SurveyOptionCustomCell(NSString cellId) : base(UITableViewCellStyle.Default, cellId)
        {
            imageView = new UIImageView();
            textLabel = new UILabel();

            ContentView.AddSubviews(new UIView[] { imageView, textLabel });

        }

        public void UpdateCell(string caption)
        {
            imageView.Image = null;
            textLabel.Text = caption;
        }

        public void UpdateCell(string caption, UIImage image)
        {
            imageView.Image = image;
            textLabel.Text = caption;
        }       

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            imageView.Frame = new CGRect(8, 4, 34, 34);
            imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            imageView.Layer.MasksToBounds = true;
            imageView.TranslatesAutoresizingMaskIntoConstraints = false;

            if (CreateSurveyController.SurveyModel.type == SurveyType.Text.ToString() || "<- Add new option".Equals(textLabel.Text))
            {
                textLabel.Frame = new CGRect(8, 10, ContentView.Bounds.Width - 8, 25);
            }
            else
            {
                textLabel.Frame = new CGRect(50, 10, ContentView.Bounds.Width - 50, 25);
            }            
        }
    }
}

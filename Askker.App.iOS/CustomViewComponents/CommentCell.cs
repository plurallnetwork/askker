using System;

using Foundation;
using UIKit;

namespace Askker.App.iOS.Resources
{
    public partial class CommentCell : UICollectionViewCell
    {
        public static readonly NSString Key = new NSString("CommentCell");
        public static readonly UINib Nib;

        static CommentCell()
        {
            Nib = UINib.FromName("CommentCell", NSBundle.MainBundle);
        }

        protected CommentCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public void UpdateCell(string name, string text, UIImage profilePicture)
        {
            nameLabel.Text = name;
            commentText.Text = text;
            imageView.Image = profilePicture;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            imageView.Layer.CornerRadius = 22;
            imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            imageView.Layer.MasksToBounds = true;
            imageView.TranslatesAutoresizingMaskIntoConstraints = false;

            commentText.BackgroundColor = UIColor.White;
        }
    }
}
using System;
using ObjCRuntime;
using Foundation;
using UIKit;
using System.Collections.Generic;

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

        public void UpdateCell(string name, string text, UIImage profilePicture, UINavigationController navigationController, string id)
        {
            nameLabel.Text = name;
            commentText.Text = text;
            imageView.Image = profilePicture;

            var feedTapGestureRecognizer = new UIFeedTapGestureRecognizer(this, new Selector("TapProfilePictureSelector:"));
            List<Object> tapProfilePictureValues = new List<Object>();
            tapProfilePictureValues.Add(navigationController);
            tapProfilePictureValues.Add(id);
            feedTapGestureRecognizer.Params = tapProfilePictureValues;
            imageView.AddGestureRecognizer(feedTapGestureRecognizer);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            imageView.Layer.CornerRadius = 22;
            imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            imageView.Layer.MasksToBounds = true;
            imageView.UserInteractionEnabled = true;
            imageView.TranslatesAutoresizingMaskIntoConstraints = false;

            commentText.BackgroundColor = UIColor.White;
        }

        [Export("TapProfilePictureSelector:")]
        public void TapProfilePictureSelector(UIFeedTapGestureRecognizer tapGesture)
        {
            Utils.OpenUserProfile((UINavigationController)tapGesture.Params.ToArray()[0], (string)tapGesture.Params.ToArray()[1]);
        }
    }
}
using Foundation;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class CommentAreaView : UIView
    {
        static readonly UIColor ButtonTextColorNormal = UIColor.FromRGB(1, 122, 255);
        static readonly UIColor ButtonTextColorDisabled = UIColor.FromRGB(142, 142, 147);
        static readonly UIFont ButtonFont = UIFont.BoldSystemFontOfSize(17f);

        static readonly UIColor InputBackgroundColor = UIColor.FromWhiteAlpha(250, 1);
        static readonly UIColor InputBorderColor = UIColor.FromRGB(200, 200, 205);
        const float BorderWidth = 0.5f;
        const float CornerRadius = 5;
        public static readonly float ToolbarMinHeight = 46f;

        public CommentAreaView (IntPtr handle) : base (handle)
        {
        }

        public UITextView CommentText
        {
            get { return commentText; }
            set { commentText = value; }
        }

        public UIButton CommentButton
        {
            get { return commentButton; }
            set { commentButton = value; }
        }

        public static CommentAreaView Create()
        {

            var arr = NSBundle.MainBundle.LoadNib("CommentAreaView", null, null);
            var v = ObjCRuntime.Runtime.GetNSObject<CommentAreaView>(arr.ValueAt(0));

            return v;
        }

        public override void LayoutSubviews()
        {
            commentText.TranslatesAutoresizingMaskIntoConstraints = false;
            commentText.BackgroundColor = InputBackgroundColor;
            commentText.ScrollIndicatorInsets = new UIEdgeInsets(CornerRadius, 0f, CornerRadius, 0f);
            commentText.TextContainerInset = new UIEdgeInsets(4f, 2f, 4f, 2f);
            commentText.ContentInset = new UIEdgeInsets(1f, 0f, 1f, 0f);
            commentText.ScrollEnabled = true;
            commentText.ScrollsToTop = false;
            commentText.UserInteractionEnabled = true;
            commentText.Font = UIFont.SystemFontOfSize(16f);
            commentText.TextAlignment = UITextAlignment.Natural;
            commentText.ContentMode = UIViewContentMode.Redraw;
            commentText.Layer.BorderColor = InputBorderColor.CGColor;
            commentText.Layer.BorderWidth = BorderWidth;
            commentText.Layer.CornerRadius = CornerRadius;

            commentButton.TranslatesAutoresizingMaskIntoConstraints = false;

            var c1 = NSLayoutConstraint.FromVisualFormat("H:|-[input]-[button]-|",
                0,
                "input", commentText,
                "button", commentButton
            );
            var top = NSLayoutConstraint.Create(commentText, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 8f);
            var bot = NSLayoutConstraint.Create(commentText, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1f, -8f);
            AddConstraint(top);
            AddConstraint(bot);
            // We want Send button was centered when Toolbar has MinHeight (pin button in this state)
            var c2 = NSLayoutConstraint.Create(commentButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1f, -ToolbarMinHeight / 2);
            AddConstraints(c1);
            AddConstraint(c2);
        }
    }
}
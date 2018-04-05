using Foundation;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class CommentMenuView : UIView
    {
        public CommentMenuView (IntPtr handle) : base (handle)
        {
        }

        public static CommentMenuView Create()
        {

            var arr = NSBundle.MainBundle.LoadNib("CommentMenuView", null, null);
            var v = ObjCRuntime.Runtime.GetNSObject<CommentMenuView>(arr.ValueAt(0));

            return v;
        }

        public UIButton CancelButton
        {
            get { return cancelBtn; }
            set { cancelBtn = value; }
        }

        public UIButton DeleteButton
        {
            get { return deleteBtn; }
            set { deleteBtn = value; }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            cancelBtn.SetTitleColor(UIColor.FromRGB(90, 89, 89), UIControlState.Normal);
            deleteBtn.SetTitleColor(UIColor.FromRGB(90, 89, 89), UIControlState.Normal);
        }

        public UIView commentView { get; set; }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            UITouch touch = touches.AnyObject as UITouch;
            if (touch != null && touch.View.GetType() == typeof(CommentMenuView))
            {
                this.Hidden = true;
                this.commentView.Alpha = 1f;
            }
        }
    }
}
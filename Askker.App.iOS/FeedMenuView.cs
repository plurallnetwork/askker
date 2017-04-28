using CoreGraphics;
using Foundation;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class FeedMenuView : UIView
    {
        public FeedMenuView (IntPtr handle) : base (handle)
        {
        }

        public static FeedMenuView Create()
        {

            var arr = NSBundle.MainBundle.LoadNib("FeedMenuView", null, null);
            var v = ObjCRuntime.Runtime.GetNSObject<FeedMenuView>(arr.ValueAt(0));
            
            return v;
        }

        public UIButton CancelButton
        {
            get { return cancelBtn; }
            set { cancelBtn = value; }
        }

        public UIButton EditButton
        {
            get { return editBtn; }
            set { editBtn = value; }
        }

        public UIButton CleanButton
        {
            get { return cleanBtn; }
            set { cleanBtn = value; }
        }

        public UIButton FinishButton
        {
            get { return finishBtn; }
            set { finishBtn = value; }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            //editBtn.ImageView.Frame = new CGRect(editBtn.ImageView.Frame.X, 5, 35, 35);
            editBtn.ContentMode = UIViewContentMode.ScaleToFill;
        }


        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            UITouch touch = touches.AnyObject as UITouch;
            if(touch != null && touch.View.GetType() == typeof (FeedMenuView))
            {
                this.Hidden = true;
                MenuViewController.sidebarController.View.Alpha = 1f;
            }
        }
    }
}
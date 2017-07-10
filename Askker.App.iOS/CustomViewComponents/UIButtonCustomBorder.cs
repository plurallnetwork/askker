using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UIKit;

namespace Askker.App.iOS.CustomViewComponents
{
    [Register("UIButtonCustomBorder")]
    [DesignTimeVisible(true)]
    public class UIButtonCustomBorder : UIButton
    {
        /* If set, overrides individual widths */
        public nfloat BorderWidthAll { get; set; }
        /* If set, overrides individual colors */
        public UIColor BorderColorAll { get; set; }

        /* For specifying individual widths */
        public UIEdgeInsets BorderWidth { get; set; }
        public UIColor BorderColorTop { get; set; }
        public UIColor BorderColorBottom { get; set; }
        public UIColor BorderColorLeft { get; set; }
        public UIColor BorderColorRight { get; set; }

        public UIButtonCustomBorder() : base() { }
        public UIButtonCustomBorder(UIButtonType type) : base(type) { }
        public UIButtonCustomBorder(NSCoder coder) : base(coder) { }
        public UIButtonCustomBorder(CGRect frame) : base(frame) { }
        protected UIButtonCustomBorder(NSObjectFlag t) : base(t) { }
        protected UIButtonCustomBorder(IntPtr handle) : base(handle){ }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            if (BorderWidthAll > 0)
            {
                BorderWidth = new UIEdgeInsets(BorderWidthAll, BorderWidthAll, BorderWidthAll, BorderWidthAll);
            }

            if (BorderColorAll != null)
            {
                BorderColorTop = BorderColorBottom = BorderColorLeft = BorderColorRight = BorderColorAll;
            }

            var xMin = rect.GetMinX();
            var xMax = rect.GetMaxX();

            var yMin = rect.GetMinY();
            var yMax = rect.GetMaxY();

            var fWidth = this.Frame.Size.Width;
            var fHeight = this.Frame.Size.Height;

            var context = UIGraphics.GetCurrentContext();

            DrawBorders(context, xMin, xMax, yMin, yMax, fWidth, fHeight);
        }

        void DrawBorders(CGContext context, nfloat xMin, nfloat xMax, nfloat yMin, nfloat yMax, nfloat fWidth, nfloat fHeight)
        {
            if (BorderColorTop != null)
            {
                context.SetFillColor(BorderColorTop.CGColor);
                context.FillRect(new CGRect(xMin, yMin, fWidth, BorderWidth.Top));
            }

            if (BorderColorLeft != null)
            {
                context.SetFillColor(BorderColorLeft.CGColor);
                context.FillRect(new CGRect(xMin, yMin, BorderWidth.Left, fHeight));
            }

            if (BorderColorRight != null)
            {
                context.SetFillColor(BorderColorRight.CGColor);
                context.FillRect(new CGRect(xMax - BorderWidth.Right, yMin, BorderWidth.Right, fHeight));
            }

            if (BorderColorBottom != null)
            {
                context.SetFillColor(BorderColorBottom.CGColor);
                context.FillRect(new CGRect(xMin, yMax - BorderWidth.Bottom, fWidth, BorderWidth.Bottom));
            }
        }

        public void SetLeftBorder(UIColor color, nfloat width)
        {
            BorderColorLeft = UIColor.LightGray;
            BorderWidthAll = width;
        }
    }
}

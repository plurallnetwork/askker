using System;
using System.Linq;
using CoreGraphics;
using UIKit;

namespace Askker.App.iOS.HorizontalSwipe
{
    public class HorizontalSwipePageControl : UIPageControl
    {

        public HorizontalSwipePageControl()
        {
            ActiveImagePurple = UIImage.FromBundle("images/purple-pip-full.png");
            ActiveImageWhite = UIImage.FromBundle("images/white-pip-full.png");
            InactiveImagePurple = UIImage.FromBundle("images/purple-pip-circle.png");
            InactiveImageWhite = UIImage.FromBundle("images/white-pip-circle.png");
        }

        public UIImage ActiveImagePurple { get; set; }
        public UIImage InactiveImagePurple { get; set; }
        public UIImage ActiveImageWhite { get; set; }
        public UIImage InactiveImageWhite { get; set; }

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

        private void UpdateDots()
        {
            for (int index = 0; index < Subviews.Length; index++)
            {
                var view = Subviews[index];
                var dot = view.Subviews.OfType<UIImageView>().Select(subview => subview).FirstOrDefault();

                if (dot == null)
                {
                    dot = new UIImageView(new CGRect(0, 0, view.Frame.Width, view.Frame.Height));
                    view.AddSubview(dot);
                }

                dot.Image = index == CurrentPage
                    ? CurrentPage == 0 ? ActiveImagePurple : ActiveImageWhite
                    : CurrentPage == 0 ? InactiveImagePurple : InactiveImageWhite;

            }
        }

        public override nint CurrentPage
        {
            get { return base.CurrentPage; }
            set { base.CurrentPage = value; UpdateDots(); }
        }

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
    }
}

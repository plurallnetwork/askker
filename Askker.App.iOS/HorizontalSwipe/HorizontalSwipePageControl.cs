using System;
using System.Linq;
using CoreGraphics;
using UIKit;
using System.Drawing;
using Cirrious.FluentLayouts.Touch;

namespace Askker.App.iOS.HorizontalSwipe
{
    public class HorizontalSwipePageControl : UIPageControl
    {

        public HorizontalSwipePageControl()
        {
            Dot1Yellow = UIImage.FromBundle("Step1");
            Dot2Gray = UIImage.FromBundle("Step2Gray");
            Dot2Yellow = UIImage.FromBundle("Step2");
            Dot3Gray = UIImage.FromBundle("Step3Gray");
            Dot3Yellow = UIImage.FromBundle("Step3");
            DotCheck = UIImage.FromBundle("StepCheck");            
        }

        public UIImage Dot1Yellow { get; set; }
        public UIImage Dot2Gray { get; set; }
        public UIImage Dot2Yellow { get; set; }
        public UIImage Dot3Gray { get; set; }
        public UIImage Dot3Yellow { get; set; }
        public UIImage DotCheck { get; set; }

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

                var w1 = DotCheck.Size.Width;
                var h1 = DotCheck.Size.Height;

                if (dot == null)
                {
                    var x = 0f;
                    var y = -17;

                    if (index == 0)
                    {
                        x = -10 * ((float)UIScreen.MainScreen.Scale - 1);
                    }
                    else if (index == 1)
                    {
                        x = -4 * ((float)UIScreen.MainScreen.Scale - 1);
                    }
                    else if (index == 2)
                    {
                        x = 2 * ((float)UIScreen.MainScreen.Scale - 1);
                    }

                    dot = new UIImageView(new CGRect(x, y, DotCheck.Size.Width, DotCheck.Size.Height));
                    view.AddSubview(dot);
                    
                }

                if (CurrentPage == index)
                {
                    if(CurrentPage == 0) //Step1
                    {
                        dot.Image = Dot1Yellow;
                    }
                    else if (CurrentPage == 1) //Step2
                    {
                        dot.Image = Dot2Yellow;
                    }
                    else if (CurrentPage == 2) //Step3
                    {
                        dot.Image = Dot3Yellow;
                    }
                }
                else
                {
                    if (index == 0) //Step1
                    {
                        dot.Image = DotCheck;
                    }
                    else if (index == 1) //Step2
                    {
                        if(CurrentPage > index)
                        {
                            dot.Image = DotCheck;
                        }
                        else
                        {
                            dot.Image = Dot2Gray;
                        }
                    }
                    else if (index == 2) //Step3
                    {
                        if (CurrentPage > index)
                        {
                            dot.Image = DotCheck;
                        }
                        else
                        {
                            dot.Image = Dot3Gray;
                        }
                    }
                }
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

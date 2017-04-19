using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Askker.App.iOS
{
    class Utils
    {
        public static Task<nint> ShowAlert(string title, string message, params string[] buttons)
        {
            var tcs = new TaskCompletionSource<nint>();
            var alert = new UIAlertView
            {
                Title = title,
                Message = message
            };
            foreach (var button in buttons)
                alert.AddButton(button);
            alert.Clicked += (s, e) => tcs.TrySetResult(e.ButtonIndex);
            alert.Show();
            return tcs.Task;
        }

        public static NSData CompressImage(UIImage sourceImage)
        {
            var actualHeight = sourceImage.Size.Height;
            var actualWidth = sourceImage.Size.Width;
            var maxHeight = 1280f;
            var maxWidth = 1280f;
            var imgRatio = actualWidth / actualHeight;
            var maxRatio = maxWidth / maxHeight;
            var compressionQuality = 0.5f;

            if (actualHeight > maxHeight || actualWidth > maxWidth)
            {
                if (imgRatio < maxRatio)
                {
                    //adjust width according to maxHeight
                    imgRatio = maxHeight / actualHeight;
                    actualWidth = imgRatio * actualWidth;
                    actualHeight = maxHeight;
                }
                else if (imgRatio > maxRatio)
                {
                    //adjust height according to maxWidth
                    imgRatio = maxWidth / actualWidth;
                    actualHeight = imgRatio * actualHeight;
                    actualWidth = maxWidth;
                }
                else
                {
                    actualHeight = maxHeight;
                    actualWidth = maxWidth;
                }
            }

            var rect = new CGRect(0.0, 0.0, actualWidth, actualHeight);
            UIGraphics.BeginImageContext(rect.Size);
            sourceImage.Draw(rect);
            var imgContext = UIGraphics.GetImageFromCurrentImageContext();
            var imageData = imgContext.AsJPEG(compressionQuality);
            UIGraphics.EndImageContext();
            return imageData;
        }
    }
}

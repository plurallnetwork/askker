﻿using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Askker.App.iOS
{
    public class Utils
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

        public static void HandleException(Exception ex)
        {
            if (ex.Message.Equals("Unauthorized"))
            {
                if (CredentialsService.DoCredentialsExist())
                {
                    CredentialsService.DeleteCredentials();
                }

                if (UIApplication.SharedApplication.KeyWindow.RootViewController.GetType() != typeof(LoginController))
                {
                    UIStoryboard Storyboard = UIStoryboard.FromName("Main", null);

                    var initialController = Storyboard.InstantiateInitialViewController() as UIViewController;

                    if (initialController != null)
                    {
                        UIApplication.SharedApplication.KeyWindow.RootViewController = initialController;
                    }
                }

                var alert = new UIAlertView
                {
                    Title = "Session expired",
                    Message = "Your session has expired. Please login again."
                };

                alert.AddButton("OK");
                alert.Show();
            }
            else
            {
                var alert = new UIAlertView
                {
                    Title = "Something went wrong",
                    Message = ex.Message
                };

                alert.AddButton("OK");
                alert.Show();
            }
        }
    }
}

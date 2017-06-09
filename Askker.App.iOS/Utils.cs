using BigTed;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using SDWebImage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Askker.App.iOS
{
    public class Utils
    {
        static UIActivityIndicatorView activityIndicator;

        public static void ShowAlertOk(string title, string message)
        {
            var alert = new UIAlertView
            {
                Title = title,
                Message = message
            };
            alert.AddButton("OK");
            alert.Show();
        }

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

                //var alert = new UIAlertView
                //{
                //    Title = "Session expired",
                //    Message = "Your session has expired. Please login again."
                //};

                //alert.AddButton("OK");
                //alert.Show();

                ShowToast("Your session has expired. Please login again.", 3000);                
            }
            else
            {
                //var alert = new UIAlertView
                //{
                //    Title = "Something went wrong",
                //    Message = ex.Message
                //};

                //alert.AddButton("OK");
                //alert.Show();
                ShowToast(ex.Message, 3000);                
            }
        }

        public static void ShowToast(string msg, double timeout)
        {
            BTProgressHUD.ShowToast(msg, showToastCentered: false, timeoutMs: timeout);            
        }

        public static void OpenUserProfile(UINavigationController navigationController, string userId)
        {
            UIStoryboard Storyboard = UIStoryboard.FromName("Main", null);

            if (userId.Equals(LoginController.userModel.id))
            {
                var profileController = Storyboard.InstantiateViewController("ProfileController") as ProfileController;
                if (profileController != null)
                {
                    navigationController.PushViewController(profileController, true);
                }
            }
            else
            {
                var profileOtherController = Storyboard.InstantiateViewController("ProfileOtherController") as ProfileOtherController;
                if (profileOtherController != null)
                {
                    profileOtherController.friendUserId = userId;
                    navigationController.PushViewController(profileOtherController, true);
                }
            }
        }

        //public static void SetImageFromNSUrlSession(string imagePath, UIImageView imageView, NSCache imageCache = null)
        //{
        //    UIImage imageFromCache = null;
        //    var url = new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + imagePath);
        //    var noCacheStr = "?nocache=" + String.Format("{0:yyyyMMddHHmmssffff}", DateTime.Now);
        //    var fetchUrl = new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + imagePath + noCacheStr);

        //    if (imageCache != null)
        //    {
        //        imageFromCache = (UIImage)imageCache.ObjectForKey(NSString.FromObject(url.AbsoluteString));
        //    }

        //    if (imageFromCache != null)
        //    {
        //        imageView.Image = imageFromCache;
        //    }
        //    else
        //    {
        //        var task = NSUrlSession.SharedSession.CreateDataTask(fetchUrl, (data, response, error) =>
        //        {
        //            try
        //            {
        //                DispatchQueue.MainQueue.DispatchAsync(() =>
        //                {
        //                    if (response != null && ((NSHttpUrlResponse)response).StatusCode != 403 && error == null)
        //                    {
        //                        if (imageCache != null)
        //                        {
        //                            var imageToCache = UIImage.LoadFromData(data);

        //                            imageView.Image = imageToCache;

        //                            if (imageToCache != null)
        //                            {
        //                                imageCache.SetObjectforKey(imageToCache, NSString.FromObject(url.AbsoluteString));
        //                            }
        //                        }
        //                        else
        //                        {
        //                            imageView.Image = UIImage.LoadFromData(data);
        //                        }
        //                    }
        //                });
        //            }
        //            catch (Exception ex)
        //            {
        //                Utils.HandleException(ex);
        //            }
        //        });
        //        task.Resume();
        //    }
        //}

        public static void SetImageFromNSUrlSession(string imagePath, UIImageView imageView, NSObject controller)
        {
            UIImage placeholder = UIImage.FromBundle("ImagePlaceholder");
            try
            {
                if(imagePath != null && imagePath.Contains("profile-picture"))
                {
                    placeholder = UIImage.FromBundle("Profile");
                }

                imageView.SetImage(new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + imagePath), placeholder, SDWebImageOptions.ProgressiveDownload, //HighPriority
                    progressBlock: (receivedSize, completedSize) =>
                    {
                        if (activityIndicator == null)
                        {
                            controller.InvokeOnMainThread(() =>
                            {
                                activityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
                                imageView.AddSubview(activityIndicator);
                                activityIndicator.Center = imageView.Center;
                                activityIndicator.StartAnimating();
                            });
                        }
                    },
                    completedBlock: (image, error, cacheType, finished) =>
                    {
                        Console.WriteLine("Image = " + image);
                        Console.WriteLine("Error = " + error);
                        Console.WriteLine("Cache Type = " + cacheType);
                        Console.WriteLine("Finished = " + finished);                        

                        if (activityIndicator != null)
                        {
                            controller.InvokeOnMainThread(() =>
                            {
                                activityIndicator.RemoveFromSuperview();
                                activityIndicator = null;
                            });
                        }

                        if (error != null && error.ToString().Contains("1100"))
                        {
                            imageView.SetImage(new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + imagePath), placeholder, SDWebImageOptions.RetryFailed);
                        }

                        if (image != null)
                        {
                            imageView.Image = image;
                        }
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static UIImage GetImageFromNSUrl(string imagePath)
        {
            UIImage result = null;
            SDWebImageManager.SharedManager.Download(new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + imagePath), SDWebImageOptions.ProgressiveDownload, //HighPriority
                progressBlock: (receivedSize, completedSize) =>
                {
                    //do nothing
                },
                completedBlock: (image, error, cacheType, finished, imageUrl) =>
                {
                    Console.WriteLine("Image = " + image);
                    Console.WriteLine("Error = " + error);
                    Console.WriteLine("Cache Type = " + cacheType);
                    Console.WriteLine("Finished = " + finished);

                    if(error != null && error.ToString().Contains("1100"))
                    {
                        SDWebImageManager.SharedManager.Download(new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + imagePath), SDWebImageOptions.RetryFailed,
                            progressBlock: (receivedSize, completedSize) =>
                            {
                                //do nothing
                            },
                            completedBlock: (image1, error1, cacheType1, finished1, imageUrl1) =>
                            {
                                Console.WriteLine("Image1 = " + image);
                                Console.WriteLine("Error1 = " + error);
                                Console.WriteLine("Cache Type1 = " + cacheType);
                                Console.WriteLine("Finished1 = " + finished);

                                if (image1 != null)
                                {
                                    result = image1;
                                }
                            });
                    }
                    
                    if (image != null && result == null)
                    {
                        result = image;
                    }
                }
            );

            return result;
        }

        public static void RemoveImageFromCache(string imagePath)
        {
            SDWebImageManager.SharedManager.ImageCache.RemoveImage("https://s3-us-west-2.amazonaws.com/askker-desenv/" + imagePath, true ,completion: () =>
            {
                Console.WriteLine("https://s3-us-west-2.amazonaws.com/askker-desenv/" + imagePath + " removed from cache");
            });
        }
        }
}

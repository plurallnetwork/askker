﻿using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
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
    public static class Utils
    {
        static UIActivityIndicatorView activityIndicator;
        static NSTimer timer;

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

                if (UIApplication.SharedApplication.KeyWindow != null && UIApplication.SharedApplication.KeyWindow.RootViewController.GetType() != typeof(LoginController))
                {
                    UIStoryboard Storyboard = UIStoryboard.FromName("Main", null);

                    var initialController = Storyboard.InstantiateInitialViewController() as UIViewController;

                    if (initialController != null)
                    {
                        UIApplication.SharedApplication.KeyWindow.RootViewController = initialController;
                    }

                    ShowToast("Your session has expired. Please login again.", 3000);
                }
                else
                {
                    var alert = new UIAlertView
                    {
                        Title = "Session expired",
                        Message = "Your session has expired. Please login again."
                    };

                    alert.AddButton("OK");
                    alert.Show();
                }
            }
            else
            {
                if (UIApplication.SharedApplication.KeyWindow != null)
                {
                    ShowToast(ex.Message, 3000);
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

        public static void ShowToast(string msg, double timeout)
        {
            BTProgressHUD.ShowToast(msg, showToastCentered: false, timeoutMs: timeout);
            KillAfter((float)timeout / 1000);       
        }

        public static void KillAfter(float timeout = 1)
        {
            if (timer != null)
            {
                timer.Invalidate();
            }
            timer = NSTimer.CreateRepeatingTimer(timeout, delegate
            {
                BTProgressHUD.Dismiss();
            });
            NSRunLoop.Current.AddTimer(timer, NSRunLoopMode.Common);
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

        public static void OpenGroupProfile(UINavigationController navigationController, string groupId)
        {
            UIStoryboard Storyboard = UIStoryboard.FromName("Main", null);
            
            var groupProfileController = Storyboard.InstantiateViewController("GroupProfileController") as GroupProfileController;
            if (groupProfileController != null)
            {
                groupProfileController.groupId = groupId;
                navigationController.PushViewController(groupProfileController, true);
            }            
        }

        public static void OpenGroupMembers(UINavigationController navigationController, string userId, string groupId, string groupProfilePicture)
        {
            UIStoryboard Storyboard = UIStoryboard.FromName("Main", null);

            if (userId.Equals(LoginController.userModel.id))
            {
                var userGroupMembersAdminController = Storyboard.InstantiateViewController("UserGroupMembersAdminController") as UserGroupMembersAdminController;
                if (userGroupMembersAdminController != null)
                {
                    userGroupMembersAdminController.groupId = groupId;
                    //userGroupMembersAdminController.groupProfilePicture = groupProfilePicture;
                    navigationController.PushViewController(userGroupMembersAdminController, true);
                }
            }
            else
            {
                var userGroupMembersOtherController = Storyboard.InstantiateViewController("UserGroupMembersOtherController") as UserGroupMembersOtherController;
                if (userGroupMembersOtherController != null)
                {
                    userGroupMembersOtherController.groupId = groupId;
                    //userGroupMembersOtherController.groupProfilePicture = groupProfilePicture;
                    navigationController.PushViewController(userGroupMembersOtherController, true);
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

        public static void SetImageFromNSUrlSession(string imagePath, UIImageView imageView, NSObject controller, PictureType picType)
        {
            UIImage placeholder = UIImage.FromBundle("ImagePlaceholder");
            try
            {
                if(picType.Equals(PictureType.Profile))
                {
                    placeholder = UIImage.FromBundle("Profile");
                }

                if (picType.Equals(PictureType.Group))
                {
                    placeholder = UIImage.FromBundle("Group");
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

        public static UIImageView GetSystemWarningImage(string bundleName)
        {
            UIImageView system = new UIImageView(UIImage.FromBundle(bundleName));
            system.ContentMode = UIViewContentMode.Center;

            return system;
        }

        public static nfloat getHeightForFeedCell(SurveyModel survey, nfloat width)
        {
            var rect = new NSString(survey.question.text).GetBoundingRect(new CGSize(width, 1000), NSStringDrawingOptions.UsesFontLeading | NSStringDrawingOptions.UsesLineFragmentOrigin, new UIStringAttributes() { Font = UIFont.BoldSystemFontOfSize(16) }, null);

            var optionsHeight = 176;

            if (survey.type == SurveyType.Text.ToString())
            {
                optionsHeight = survey.options.Count * 44;
            }

            // Heights of the vertical components to format the cell dinamic height
            var knownHeight = 0;
            if (string.IsNullOrEmpty(survey.finishDate))
            {
                knownHeight = 8 + 44 + 4 + 4 + optionsHeight + 8 + 24 + 8 + 44;
            }
            else
            {
                knownHeight = 8 + 44 + 8 + 32 + 4 + 4 + optionsHeight + 8 + 24 + 8 + 44;
            }

            return rect.Height + knownHeight + 25;
        }

        public static void BindFeedCell(FeedCollectionViewCell feedCell, SurveyModel survey, int indexPathRow, NSObject controller, bool isPreview = false)
        {
            if (survey.profilePicture != null)
            {
                Utils.SetImageFromNSUrlSession(survey.profilePicture, feedCell.profileImageView, controller, PictureType.Profile);
            }
            else
            {
                feedCell.profileImageView.Image = UIImage.FromBundle("Profile");
            }

            var attributedText = new NSMutableAttributedString(survey.userName, UIFont.BoldSystemFontOfSize(14));
            if (!isPreview)
            {
                if ("Groups".Equals(survey.targetAudience))
                {
                    var x = 0;
                    foreach (string group in survey.targetAudienceGroups.names)
                    {
                        if (x == 0)
                        {
                            attributedText.Append(new NSAttributedString("\n" + group, UIFont.SystemFontOfSize(12), UIColor.FromRGBA(nfloat.Parse("0.60"), nfloat.Parse("0.63"), nfloat.Parse("0.67"), nfloat.Parse("1"))));
                        }
                        else
                        {
                            attributedText.Append(new NSAttributedString(", " + group, UIFont.SystemFontOfSize(12), UIColor.FromRGBA(nfloat.Parse("0.60"), nfloat.Parse("0.63"), nfloat.Parse("0.67"), nfloat.Parse("1"))));
                        }
                        x++;
                    }

                }
                else
                {
                    attributedText.Append(new NSAttributedString("\n" + survey.targetAudience, UIFont.SystemFontOfSize(12), UIColor.FromRGBA(nfloat.Parse("0.60"), nfloat.Parse("0.63"), nfloat.Parse("0.67"), nfloat.Parse("1"))));
                }

                attributedText.Append(new NSAttributedString(" • " + TimeAgoDisplay(DateTime.ParseExact(survey.creationDate, "yyyyMMddTHHmmss", null)), UIFont.SystemFontOfSize(12), UIColor.FromRGBA(nfloat.Parse("0.60"), nfloat.Parse("0.63"), nfloat.Parse("0.67"), nfloat.Parse("1"))));
            }

            var paragraphStyle = new NSMutableParagraphStyle();
            paragraphStyle.LineSpacing = 4;
            attributedText.AddAttribute(new NSString("ParagraphStyle"), paragraphStyle, new NSRange(0, attributedText.Length));

            feedCell.nameLabel.AttributedText = attributedText;


            bool finished = false;
            if (string.IsNullOrEmpty(survey.finishDate))
            {
                feedCell.finishedLabel.Text = "";
                feedCell.moreButton.Hidden = false;
                feedCell.optionsTableView.AllowsSelection = true;
                feedCell.optionsCollectionView.AllowsSelection = true;
                finished = false;
            }
            else
            {
                feedCell.finishedLabel.Text = "Finished";
                feedCell.moreButton.Hidden = true;
                feedCell.optionsTableView.AllowsSelection = false;
                feedCell.optionsCollectionView.AllowsSelection = false;
                finished = true;

                feedCell.AddSubview(feedCell.finishedLabel);
                feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", feedCell.finishedLabel));
            }

            if (isPreview)
            {
                finished = false;
            }

            if (!survey.userId.Equals(LoginController.userModel.id))
            {
                feedCell.moreButton.Hidden = true;
            }
            else
            {
                if (finished)
                {
                    feedCell.moreButton.Hidden = true;
                }
                else
                {
                    feedCell.moreButton.Hidden = false;
                }
            }

            feedCell.questionText.Text = survey.question.text;

            if (survey.type == SurveyType.Text.ToString())
            {
                feedCell.optionsTableView.ContentMode = UIViewContentMode.ScaleAspectFill;
                feedCell.optionsTableView.Layer.MasksToBounds = true;
                feedCell.optionsTableView.TranslatesAutoresizingMaskIntoConstraints = false;
                feedCell.optionsTableView.ContentInset = new UIEdgeInsets(0, -10, 0, 0);
                feedCell.optionsTableView.Tag = indexPathRow;
                feedCell.optionsTableView.FeedCell = feedCell;

                feedCell.optionsTableViewSource.survey = survey;
                feedCell.optionsTableView.Source = feedCell.optionsTableViewSource;
                feedCell.optionsTableView.ReloadData();

                feedCell.optionsCollectionView.RemoveFromSuperview();
                feedCell.AddSubview(feedCell.optionsTableView);

                feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", feedCell.optionsTableView));

                if (finished)
                {
                    feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(44)]-8-[v1(32)]-4-[v2]-4-[v3(1)][v4]-8-[v5(24)]-8-[v6(1)][v7(44)]|", new NSLayoutFormatOptions(), "v0", feedCell.profileImageView, "v1", feedCell.finishedLabel, "v2", feedCell.questionText, "v3", feedCell.dividerLineView, "v4", feedCell.optionsTableView, "v5", feedCell.totalVotesLabel, "v6", feedCell.dividerLineView2, "v7", feedCell.contentViewButtons));
                }
                else
                {
                    feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(44)]-4-[v1]-4-[v2(1)][v3]-8-[v4(24)]-8-[v5(1)][v6(44)]|", new NSLayoutFormatOptions(), "v0", feedCell.profileImageView, "v1", feedCell.questionText, "v2", feedCell.dividerLineView, "v3", feedCell.optionsTableView, "v4", feedCell.totalVotesLabel, "v5", feedCell.dividerLineView2, "v6", feedCell.contentViewButtons));
                }

                if (isPreview)
                {
                    feedCell.optionsTableView.ScrollEnabled = false;
                    feedCell.optionsTableView.AllowsSelection = false;
                }
            }
            else
            {
                feedCell.optionsCollectionView.TranslatesAutoresizingMaskIntoConstraints = false;
                feedCell.optionsCollectionView.Tag = indexPathRow;
                feedCell.optionsCollectionView.FeedCell = feedCell;

                feedCell.optionsCollectionViewSource.survey = survey;
                feedCell.optionsCollectionViewSource.isPreview = isPreview;
                feedCell.optionsCollectionView.Source = feedCell.optionsCollectionViewSource;
                feedCell.optionsCollectionViewDelegate.optionsCollectionViewSource = feedCell.optionsCollectionViewSource;
                feedCell.optionsCollectionViewDelegate.survey = survey;
                feedCell.optionsCollectionView.Delegate = feedCell.optionsCollectionViewDelegate;
                feedCell.optionsCollectionView.ReloadData();

                feedCell.optionsTableView.RemoveFromSuperview();
                feedCell.AddSubview(feedCell.optionsCollectionView);

                feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[v0]|", new NSLayoutFormatOptions(), "v0", feedCell.optionsCollectionView));

                if (finished)
                {
                    feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(44)]-8-[v1(32)]-4-[v2]-4-[v3(1)][v4(<=176)]-8-[v5(24)]-8-[v6(1)][v7(44)]|", new NSLayoutFormatOptions(), "v0", feedCell.profileImageView, "v1", feedCell.finishedLabel, "v2", feedCell.questionText, "v3", feedCell.dividerLineView, "v4", feedCell.optionsCollectionView, "v5", feedCell.totalVotesLabel, "v6", feedCell.dividerLineView2, "v7", feedCell.contentViewButtons));
                }
                else
                {
                    feedCell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(44)]-4-[v1]-4-[v2(1)][v3(<=176)]-8-[v4(24)]-8-[v5(1)][v6(44)]|", new NSLayoutFormatOptions(), "v0", feedCell.profileImageView, "v1", feedCell.questionText, "v2", feedCell.dividerLineView, "v3", feedCell.optionsCollectionView, "v4", feedCell.totalVotesLabel, "v5", feedCell.dividerLineView2, "v6", feedCell.contentViewButtons));
                }

                if (isPreview)
                {
                    feedCell.optionsCollectionView.AllowsSelection = false;
                }
            }

            feedCell.updateTotalVotes(survey.totalVotes);
            feedCell.updateTotalComments(survey.totalComments);
        }

        public static string TimeAgoDisplay(DateTime date)
        {
            var secondsAgo = (int)DateTime.Now.Subtract(date).TotalSeconds;
            var minute = 60;
            var hour = 60 * minute;
            var day = 24 * hour;

            if (secondsAgo < minute) {
                return secondsAgo + " seconds ago";
            }
            else if (secondsAgo < hour) {
                return (secondsAgo / minute) + " minutes ago";
            }
            else if (secondsAgo < day) {
                return (secondsAgo / hour) + " hours ago";
            }

            return date.ToString("dd MMMM");
        }

        public static void LoadingIndicatorButton (this UIButton button, bool show)
        {
            var tag = 808404;
            if (show)
            {
                button.Enabled = false;
                button.Alpha = 0.5f;
                var indicator = new UIActivityIndicatorView();
                indicator.Center = new CGPoint(button.Bounds.Size.Width / 2, button.Bounds.Size.Height / 2);
                indicator.Tag = tag;
                button.AddSubview(indicator);
                indicator.StartAnimating();
            }
            else
            {
                button.Enabled = true;
                button.Alpha = 1.0f;
                var indicator = (UIActivityIndicatorView)button.ViewWithTag(tag);
                if (indicator != null) {
                    indicator.StopAnimating();
                    indicator.RemoveFromSuperview();
                }
            }
        } 
    }
}

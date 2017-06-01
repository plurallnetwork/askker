using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using AssetsLibrary;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using UIKit;

namespace Askker.App.iOS
{
    public partial class ProfileController : UIViewController
    {
        UIImagePickerController imagePicker = new UIImagePickerController();
        string fileName;
        public static NSCache imageCache = new NSCache();

        public ProfileController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            imageCache.RemoveAllObjects();

            profileImageView.ClipsToBounds = true;

            uploadButton.TouchUpInside += btnUpload_TouchUpInside;
            nameButton.TouchUpInside += btnName_TouchUpInside;
            emailButton.TouchUpInside += btnEmail_TouchUpInside;
            ageButton.TouchUpInside += btnAge_TouchUpInside;
            manButton.TouchUpInside += btnMan_TouchUpInside;
            womanButton.TouchUpInside += btnWoman_TouchUpInside;
            imagePicker.FinishedPickingMedia += Handle_FinishedPickingMedia;
            imagePicker.Canceled += Handle_Canceled;

            emailText.EditingDidEnd += EmailText_EditingDidEnd;

            ageText.KeyboardType = UIKeyboardType.NumberPad;
            ageText.ShouldChangeCharacters = (textField, range, replacement) =>
            {
                int number;
                return replacement.Length == 0 || int.TryParse(replacement, out number);
            };

            uploadButton.Layer.BorderColor = UIColor.Black.CGColor;
            uploadButton.Layer.BorderWidth = 1f;

            nameText.Text = LoginController.userModel.name;
            emailText.Text = LoginController.userModel.userName;
            ageText.Text = LoginController.userModel.age.ToString();
            if ("male".Equals(LoginController.userModel.gender) || "female".Equals(LoginController.userModel.gender))
            {
                genderText.Text = LoginController.userModel.gender;
            }

            if (LoginController.userModel.profilePicturePath != null)
            {
                fileName = LoginController.userModel.profilePicturePath;
                var url = new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + LoginController.userModel.profilePicturePath);

                var imageFromCache = (UIImage)imageCache.ObjectForKey(NSString.FromObject(url.AbsoluteString));
                if (imageFromCache != null)
                {
                    profileImageView.Image = imageFromCache;
                }
                else
                {
                    var task = NSUrlSession.SharedSession.CreateDataTask(url, (data, response, error) =>
                    {
                        if (response == null)
                        {
                            profileImageView.Image = UIImage.FromBundle("Profile");
                        }
                        else
                        {
                            try
                            {
                                DispatchQueue.MainQueue.DispatchAsync(() =>
                                {
                                    var imageToCache = UIImage.LoadFromData(data);

                                    profileImageView.Image = imageToCache;

                                    if (imageToCache != null)
                                    {
                                        imageCache.SetObjectforKey(imageToCache, NSString.FromObject(url.AbsoluteString));
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message);
                            }
                        }
                    });
                    task.Resume();
                }
            }
        }


        private void btnUpload_TouchUpInside(object sender, EventArgs e)
        {
            imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            imagePicker.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);
            NavigationController.PresentModalViewController(imagePicker, true);
        }

        private void disableButtons(string sender)
        {
            if ("ageButton".Equals(sender))
            {
                nameButton.Enabled = false;
                emailButton.Enabled = false;
                manButton.Enabled = false;
                womanButton.Enabled = false;
                uploadButton.Enabled = false;
            }
            else if ("nameButton".Equals(sender))
            {
                ageButton.Enabled = false;
                emailButton.Enabled = false;
                manButton.Enabled = false;
                womanButton.Enabled = false;
                uploadButton.Enabled = false;
            }
            else if ("emailButton".Equals(sender))
            {
                nameButton.Enabled = false;
                ageButton.Enabled = false;
                manButton.Enabled = false;
                womanButton.Enabled = false;
                uploadButton.Enabled = false;
            }
        }

        private void enableButtons(string sender)
        {
            if ("ageButton".Equals(sender))
            {
                nameButton.Enabled = true;
                emailButton.Enabled = true;
                manButton.Enabled = true;
                womanButton.Enabled = true;
                uploadButton.Enabled = true;
            }
            else if ("nameButton".Equals(sender))
            {
                ageButton.Enabled = true;
                emailButton.Enabled = true;
                manButton.Enabled = true;
                womanButton.Enabled = true;
                uploadButton.Enabled = true;
            }
            else if ("emailButton".Equals(sender))
            {
                nameButton.Enabled = true;
                ageButton.Enabled = true;
                manButton.Enabled = true;
                womanButton.Enabled = true;
                uploadButton.Enabled = true;
            }
        }

        private async void Update()
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    await new LoginManager().Update(LoginController.tokenModel.access_token, LoginController.userModel, null, null);
                }
                else
                {
                    LoginController.userModel.profilePicturePath = LoginController.userModel.id + "/profile-picture.jpg";
                    using (NSData imageData = Utils.CompressImage(profileImageView.Image))
                    {
                        byte[] myByteArray = new byte[imageData.Length];
                        System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, myByteArray, 0, Convert.ToInt32(imageData.Length));
                        await new LoginManager().Update(LoginController.tokenModel.access_token, LoginController.userModel, myByteArray, "profile-picture.jpg");
                    }
                }

                await new LoginManager().UpdateUserInformation(LoginController.userModel, LoginController.tokenModel.access_token);
            }
            catch (Exception ex)
            {
                Utils.HandleException(ex);
            }
        }

        private void btnName_TouchUpInside(object sender, EventArgs e)
        {
            if (nameText.Enabled)
            {
                LoginController.userModel.name = nameText.Text;
                nameButton.SetImage(UIImage.FromBundle("images/icons/editar.png"), UIControlState.Normal);
                enableButtons("nameButton");
                nameText.Enabled = false;
                Update();
            }
            else
            {
                nameButton.SetImage(UIImage.FromBundle("images/icons/check.png"), UIControlState.Normal);
                disableButtons("nameButton");
                nameText.Enabled = true;
                nameText.BecomeFirstResponder();
            }
                
        }

        private void btnMan_TouchUpInside(object sender, EventArgs e)
        {
            genderText.Text = "male";
            LoginController.userModel.gender = genderText.Text;
            Update();
        }

        private void btnWoman_TouchUpInside(object sender, EventArgs e)
        {
            genderText.Text = "female";
            LoginController.userModel.gender = genderText.Text;
            Update();
        }

        private void btnEmail_TouchUpInside(object sender, EventArgs e)
        {
            if (emailText.Enabled)
            {
                if (Regex.Match(emailText.Text.Trim(), @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$").Success || string.IsNullOrEmpty(emailText.Text))
                {
                    LoginController.userModel.userName = emailText.Text;
                    emailButton.SetImage(UIImage.FromBundle("images/icons/editar.png"), UIControlState.Normal);
                    enableButtons("emailButton");
                    emailText.Enabled = false;
                    Update();
                }
            }
            else
            {
                emailButton.SetImage(UIImage.FromBundle("images/icons/check.png"), UIControlState.Normal);
                disableButtons("emailButton");
                emailText.Enabled = true;
                emailText.BecomeFirstResponder();
            }

        }

        private void btnAge_TouchUpInside(object sender, EventArgs e)
        {
            if (ageText.Enabled)
            {
                int value;
                if(int.TryParse(ageText.Text, out value))
                {
                    LoginController.userModel.age = value;
                }                
                ageButton.SetImage(UIImage.FromBundle("images/icons/editar.png"), UIControlState.Normal);
                enableButtons("ageButton");
                ageText.Enabled = false;
                Update();
            }
            else
            {
                ageButton.SetImage(UIImage.FromBundle("images/icons/check.png"), UIControlState.Normal);
                disableButtons("ageButton");
                ageText.Enabled = true;
                ageText.BecomeFirstResponder();
            }

        }

        private void EmailText_EditingDidEnd(object sender, EventArgs e)
        {
            if (Regex.Match(emailText.Text.Trim(), @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$").Success)
            {
                btnEmail_TouchUpInside(Self, null);
            }
            else
            {
                new UIAlertView("E-mail", "Please write a valid e-mail", null, "OK", null).Show();

                if (ageText.Enabled) btnAge_TouchUpInside(Self, null);
                if (nameText.Enabled) btnName_TouchUpInside(Self, null);

                emailText.BecomeFirstResponder();

                return;
            }
        }

        protected void Handle_FinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs e)
        {
            bool isImage = false;
            switch (e.Info[UIImagePickerController.MediaType].ToString())
            {
                case "public.image":
                    isImage = true;
                    break;
                case "public.video":
                    break;
            }

            // get common info (shared between images and video)
            NSUrl referenceURL = e.Info[new NSString("UIImagePickerControllerReferenceURL")] as NSUrl;

            fileName = null;

            ALAssetsLibrary assetsLibrary = new ALAssetsLibrary();
            assetsLibrary.AssetForUrl(referenceURL, delegate (ALAsset asset)
            {
                ALAssetRepresentation representation = asset.DefaultRepresentation;
                if (representation == null)
                {
                    return;
                }
                else
                {
                    fileName = representation.Filename.ToLower();
                }
            }, delegate (NSError error) {});
            
            if (isImage)
            {
                UIImage originalImage = e.Info[UIImagePickerController.OriginalImage] as UIImage;
                if (originalImage != null)
                {
                    profileImageView.Image = originalImage;
                    fileName = "1.jpg";
                    Update();
                }
            }
            else
            {
                NSUrl mediaURL = e.Info[UIImagePickerController.MediaURL] as NSUrl;
                if (mediaURL != null)
                {
                    Console.WriteLine(mediaURL.ToString());
                }
            }
            imagePicker.DismissModalViewController(true);
        }

        void Handle_Canceled(object sender, EventArgs e)
        {
            imagePicker.DismissModalViewController(true);
        }

        private static object CGRect(double v1, double v2, nfloat actualWidth, nfloat actualHeight)
        {
            throw new NotImplementedException();
        }
    }
}
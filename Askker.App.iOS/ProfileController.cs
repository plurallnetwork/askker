using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using AssetsLibrary;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using SDWebImage;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using UIKit;
using System.Globalization;
using BigTed;
using Askker.App.PortableLibrary.Enums;

namespace Askker.App.iOS
{
    public partial class ProfileController : CustomUIViewController
    {
        UIImagePickerController imagePicker = new UIImagePickerController();
        string fileName;
        public static NSCache imageCache = new NSCache();

        public ProfileController (IntPtr handle) : base (handle)
        {                      
        }

        public override void ViewDidLoad()
        {
            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);
            base.ViewDidLoad();

            this.EdgesForExtendedLayout = UIRectEdge.None;
            this.ExtendedLayoutIncludesOpaqueBars = false;
            this.AutomaticallyAdjustsScrollViewInsets = false;
            this.NavigationController.NavigationBar.Translucent = false;

            UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = UIColor.FromRGB(90, 89, 89)
            };

            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);

            imageCache.RemoveAllObjects();

            profileImageView.Layer.MasksToBounds = true;
            profileImageView.Image = null;

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

            uploadButton.Layer.BorderColor = UIColor.FromRGB(90, 89, 89).CGColor;
            uploadButton.Layer.BorderWidth = 1f;

            uploadButton.TintColor = UIColor.FromRGB(90, 89, 89);
            uploadButton.SetTitleColor( UIColor.FromRGB(90, 89, 89), UIControlState.Normal);
            
            nameText.TextColor = UIColor.FromRGB(90, 89, 89);
            emailText.TextColor = UIColor.FromRGB(90, 89, 89);
            ageText.TextColor = UIColor.FromRGB(90, 89, 89);
            genderText.TextColor = UIColor.FromRGB(90, 89, 89);

            nameText.Text = LoginController.userModel.name;
            emailText.Text = LoginController.userModel.userName;
            ageText.Text = LoginController.userModel.age.ToString();

            if ("male".Equals(LoginController.userModel.gender) || "female".Equals(LoginController.userModel.gender))
            {
                genderText.Text = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(LoginController.userModel.gender);
                UpdateGenderButtonStatus(LoginController.userModel.gender);
            }
            else
            {
                manButton.Alpha = 0.3f;
                womanButton.Alpha = 0.3f;
            }

            if (LoginController.userModel.profilePicturePath != null)
            {
                fileName = LoginController.userModel.profilePicturePath;
                Utils.SetImageFromNSUrlSession(fileName, profileImageView, this, PictureType.Profile);                
            }
            else
            {
                profileImageView.Image = UIImage.FromBundle("Profile");
            }
            BTProgressHUD.Dismiss();
        }

        private void UpdateGenderButtonStatus(string genderPressioned)
        {
            //TODO: Setar imagem habilitada/desabilitada para o male/female
            if ("male".Equals(genderPressioned))
            {
                manButton.Alpha = 1;
                womanButton.Alpha = 0.3f;
            }
            else if ("female".Equals(genderPressioned))
            {
                manButton.Alpha = 0.3f;
                womanButton.Alpha = 1;
            }
            else
            {
                manButton.Alpha = 0.3f;
                womanButton.Alpha = 0.3f;
            }
        }

        private void btnUpload_TouchUpInside(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(LoginController.userModel.profilePicturePath))
            {
                Utils.RemoveImageFromCache(LoginController.userModel.profilePicturePath);
            }
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

            UpdateGenderButtonStatus(LoginController.userModel.gender);
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

                        //SDWebImageManager.SharedManager.ImageCache.StoreImage(profileImageView.Image, "https://s3-us-west-2.amazonaws.com/askker-prod/" + LoginController.userModel.profilePicturePath);
                        
                        NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("UpdateProfilePicture"), null);
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
                nameButton.SetImage(UIImage.FromBundle("EditProfile"), UIControlState.Normal);
                enableButtons("nameButton");
                nameText.Enabled = false;
                NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("UpdateUserName"), null);
                Update();
            }
            else
            {
                nameButton.SetImage(UIImage.FromBundle("CheckProfile"), UIControlState.Normal);
                disableButtons("nameButton");
                nameText.Enabled = true;
                nameText.BecomeFirstResponder();
            }
                
        }

        private void btnMan_TouchUpInside(object sender, EventArgs e)
        {
            genderText.Text = "Male";
            LoginController.userModel.gender = genderText.Text.ToLower();

            UpdateGenderButtonStatus(LoginController.userModel.gender);

            Update();
        }

        private void btnWoman_TouchUpInside(object sender, EventArgs e)
        {
            genderText.Text = "Female";
            LoginController.userModel.gender = genderText.Text.ToLower();

            UpdateGenderButtonStatus(LoginController.userModel.gender);

            Update();
        }

        private void btnEmail_TouchUpInside(object sender, EventArgs e)
        {
            if (emailText.Enabled)
            {
                if (Regex.Match(emailText.Text.Trim(), @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$").Success || string.IsNullOrEmpty(emailText.Text))
                {
                    LoginController.userModel.userName = emailText.Text;
                    emailButton.SetImage(UIImage.FromBundle("EditProfile"), UIControlState.Normal);
                    enableButtons("emailButton");
                    emailText.Enabled = false;
                    Update();
                }
            }
            else
            {
                emailButton.SetImage(UIImage.FromBundle("CheckProfile"), UIControlState.Normal);
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
                ageButton.SetImage(UIImage.FromBundle("EditProfile"), UIControlState.Normal);
                enableButtons("ageButton");
                ageText.Enabled = false;
                Update();
            }
            else
            {
                ageButton.SetImage(UIImage.FromBundle("CheckProfile"), UIControlState.Normal);
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
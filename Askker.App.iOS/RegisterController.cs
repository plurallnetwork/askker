using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using BigTed;
using Foundation;
using SafariServices;
using System;
using System.Linq;
using UIKit;

namespace Askker.App.iOS
{
    public partial class RegisterController : CustomUIViewController
    {
        public RegisterController (IntPtr handle) : base (handle)
        {
            
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);

            txtName.TextColor = UIColor.FromRGB(90, 89, 89);
            txtEmail.TextColor = UIColor.FromRGB(90, 89, 89);
            txtPassword.TextColor = UIColor.FromRGB(90, 89, 89);
            txtConfirmPassword.TextColor = UIColor.FromRGB(90, 89, 89);

            NSMutableAttributedString terms = new NSMutableAttributedString("I agree with Askker terms and conditions and privacy policy");
            var linkRange = terms.MutableString.LocalizedStandardRangeOfString(new NSString("Askker terms"));
            terms.AddAttribute(UIStringAttributeKey.ForegroundColor, UIColor.Blue, linkRange);

            termsText.AttributedText = terms;

            var gAgree = new UITapGestureRecognizer();
            gAgree.AddTarget(() => PresentViewControllerAsync(new SFSafariViewController(new NSUrl("https://askker.io/privacy-policy.html")), true));
            termsText.AddGestureRecognizer(gAgree);
            termsText.UserInteractionEnabled = true;

            UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = UIColor.FromRGB(90, 89, 89)
            };


            // Keyboard dispose when clicking outside the comment box
            var g = new UITapGestureRecognizer { CancelsTouchesInView = false };
            g.AddTarget(() => View.EndEditing(true));
            View.AddGestureRecognizer(g);

            btnRegister.TouchUpInside += async (sender, e) =>
            {
                if ("".Equals(txtName.Text))
                {
                    var alert = UIAlertController.Create("Name", "Please fill in the Name", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                else if ("".Equals(txtEmail.Text))
                {
                    var alert = UIAlertController.Create("E-Mail", "Please fill in the E-Mail", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                else if ("".Equals(txtPassword.Text))
                {
                    var alert = UIAlertController.Create("Password", "Please fill in the Password", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                else if ("".Equals(txtConfirmPassword.Text))
                {
                    var alert = UIAlertController.Create("Confirm Password", "Please confirm the Password", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                else if (!txtConfirmPassword.Text.Any(c => char.IsUpper(c)))
                {
                    var alert = UIAlertController.Create("Confirm Password", "Password must have at least one uppercase", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                else if (!txtConfirmPassword.Text.Any(c => char.IsDigit(c)))
                {
                    var alert = UIAlertController.Create("Confirm Password", "Password must have at least one number", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                else if (!txtPassword.Text.Equals(txtConfirmPassword.Text))
                {
                    var alert = UIAlertController.Create("Confirm Password", "Password and Confirm Password must match", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                else if (swAgree.On == false)
                {
                    var alert = UIAlertController.Create("Policy Agreement", "Please accept the policy", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                else
                {
                    try
                    {
                        BTProgressHUD.Show("Registering...", -1, ProgressHUD.MaskType.Clear);

                        RegisterManager registerManager = new RegisterManager();
                        UserRegisterModel userRegisterModel = new UserRegisterModel(txtName.Text, txtEmail.Text, txtPassword.Text, txtConfirmPassword.Text, swAgree.On);

                        await registerManager.RegisterUser(userRegisterModel);

                        LoginManager loginManager = new LoginManager();
                        UserLoginModel userLoginModel = new UserLoginModel(txtEmail.Text, txtPassword.Text);

                        LoginController.tokenModel = await loginManager.GetAuthorizationToken(userLoginModel);
                        LoginController.userModel = await loginManager.GetUserById(LoginController.tokenModel.access_token);

                        CredentialsService.SaveCredentials(LoginController.tokenModel, LoginController.userModel);

                        var menuController = this.Storyboard.InstantiateViewController("MenuNavController");
                        if (menuController != null)
                        {
                            this.PresentViewController(menuController, true, null);
                        }

                        BTProgressHUD.Dismiss();
                    }
                    catch (Exception ex)
                    {
                        BTProgressHUD.Dismiss();

                        if (ex.Message.Equals("902"))
                        {
                            var alert = UIAlertController.Create("Register", "E-mail already registered", UIAlertControllerStyle.Alert);
                            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                            PresentViewController(alert, true, null);
                        }
                        else
                        {
                            Utils.HandleException(ex);
                        }
                    }
                }
            };
        }
    }
}
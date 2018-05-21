using Foundation;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using System;
using UIKit;
using BigTed;
using SafariServices;

namespace Askker.App.iOS
{
    public partial class LoginController : CustomUIViewController
    {
        public static TokenModel tokenModel;
        public static UserModel userModel;
        public SFSafariViewController sfViewController;
        private NSObject finishAuthenticationObserver;

        public LoginManager loginManager;

        public LoginController (IntPtr handle) : base (handle)
        {
            loginManager = new LoginManager();
        }

        public override void ViewDidAppear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(true, false);
            base.ViewDidAppear(animated);
            
            //btnLoginFacebook.TouchUpInside += btnLoginFacebook_TouchUpInside;
            //btnLoginGoogle.TouchUpInside += btnLoginGoogle_TouchUpInside;
            btnSignUp.TouchUpInside += BtnSignUp_TouchUpInside;
            btnForgotPassword.TouchUpInside += BtnForgotPassword_TouchUpInside;

            txtUsername.TextColor = UIColor.FromRGB(90, 89, 89);
            txtPassword.TextColor = UIColor.FromRGB(90, 89, 89);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            //btnLoginFacebook.TouchUpInside -= btnLoginFacebook_TouchUpInside;
            //btnLoginGoogle.TouchUpInside -= btnLoginGoogle_TouchUpInside;
            btnSignUp.TouchUpInside -= BtnSignUp_TouchUpInside;
            btnForgotPassword.TouchUpInside -= BtnForgotPassword_TouchUpInside;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = UIColor.FromRGB(90, 89, 89)
            };

            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);

            finishAuthenticationObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("FinishAuthentication"), FinishAuthentication);

            // Keyboard dispose when clicking outside the comment box
            var g = new UITapGestureRecognizer { CancelsTouchesInView = false };
            g.AddTarget(() => View.EndEditing(true));
            View.AddGestureRecognizer(g);
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            if (finishAuthenticationObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(finishAuthenticationObserver);
            }
        }

        private void BtnSignUp_TouchUpInside(object sender, EventArgs e)
        {
            var registerController = this.Storyboard.InstantiateViewController("RegisterController");
            if (registerController != null)
            {
                this.NavigationController.PushViewController(registerController, true);
                NavigationController.SetNavigationBarHidden(false, true);
            }
        }

        private void BtnForgotPassword_TouchUpInside(object sender, EventArgs e)
        {
            var forgotPasswordController = this.Storyboard.InstantiateViewController("ForgotPasswordController");
            if (forgotPasswordController != null)
            {
                this.NavigationController.PushViewController(forgotPasswordController, true);
                NavigationController.SetNavigationBarHidden(false, true);
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            var registerContoller = segue.DestinationViewController as RegisterController;
        }

        public void Login()
        {
            //In iOS 6 and later, the  ViewDidUnload method is never called. So the observer is removed here, because it was causing an error on social login after logging out from e-mail authentication.
            if (finishAuthenticationObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(finishAuthenticationObserver);
            }

            var menuController = this.Storyboard.InstantiateViewController("MenuNavController");
            if (menuController != null)
            {
                this.PresentViewController(menuController, true, null);
            }

            BTProgressHUD.Dismiss();
        }

        async partial void btnEnter_TouchUpInside(UIKit.UIButton sender)
        {
            BTProgressHUD.Show("Checking your credentials...",-1,ProgressHUD.MaskType.Clear);

            if (string.Empty.Equals(txtUsername.Text))
            {
                BTProgressHUD.Dismiss();
                UIAlertView alert = new UIAlertView()
                {
                    Title = "E-mail",
                    Message = "Please fill in the E-mail"
                };
                alert.AddButton("OK");
                alert.Show();
            }
            else if (string.Empty.Equals(txtPassword.Text))
            {
                BTProgressHUD.Dismiss();
                var alert = UIAlertController.Create("Password", "Please fill in the Password", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
            else
            {
                try
                {
                    UserLoginModel userLoginModel = new UserLoginModel(txtUsername.Text, txtPassword.Text);

                    tokenModel = await loginManager.GetAuthorizationToken(userLoginModel);
                    userModel = await loginManager.GetUserById(tokenModel.access_token);

                    CredentialsService.SaveCredentials(tokenModel, userModel);

                    Login();
                }
                catch (Exception ex)
                {
                    if (ex.Message.Equals("901"))
                    {
                        var alert = UIAlertController.Create("Login", "The user name or password is incorrect.", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                        PresentViewController(alert, true, null);
                    }
                    else if (ex.Message.Equals("903"))
                    {
                        var alert = UIAlertController.Create("Login", "An error occurred while sending the e-mail confirmation.", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                        PresentViewController(alert, true, null);
                    }
                    else if (ex.Message.Equals("905"))
                    {
                        var alert = UIAlertController.Create("Login", "The user e-mail is not confirmed. A new e-mail confirmation has been sent to user.", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                        PresentViewController(alert, true, null);
                    }
                    else
                    {
                        Utils.HandleException(ex);
                    }

                    BTProgressHUD.Dismiss();
                }
            }
        }

        private void btnLoginFacebook_TouchUpInside(object sender, EventArgs e)
        {
            SocialLogin("Facebook");
        }

        private void btnLoginGoogle_TouchUpInside(object sender, EventArgs e)
        {
            SocialLogin("Google");
        }

        public void SocialLogin(string provider)
        {            
            string returnUrlLogin = "fb919804201485283:%2F%2Fauthorize";
            string externalProviderUrl = EnvironmentConstants.getServerUrl() + "api/Account/ExternalLogin?provider=" + provider + "&response_type=token&client_id=self&redirect_uri=" + returnUrlLogin + "&isAdmin=1";
            
            sfViewController = new SFSafariViewController(new NSUrl(externalProviderUrl));
            PresentViewControllerAsync(sfViewController, true);
        }

        private async void FinishAuthentication(NSNotification notification)
        {
            sfViewController.DismissViewController(true, null);

            try
            {
                string url = notification.Object.ToString();

                if (url.Contains("access_token"))
                {
                    BTProgressHUD.Show("Checking your credentials...", -1, ProgressHUD.MaskType.Clear);

                    string accessToken = url.Split('#')[1].Split('=')[1].Split('&')[0];
                    string tokenType = url.Split('=')[2].Split('&')[0];
                    string expiresIn = url.Split('=')[3];

                    Console.WriteLine("Access Token = " + accessToken);

                    userModel = await loginManager.GetUserById(accessToken);

                    if (tokenModel == null)
                    {
                        tokenModel = new TokenModel();
                        tokenModel.access_token = accessToken;
                        tokenModel.userName = userModel.userName;
                        tokenModel.expires_in = Int32.Parse(expiresIn);
                        tokenModel.token_type = tokenType;
                        tokenModel.expires = DateTime.Now.AddSeconds(tokenModel.expires_in);
                    }

                    CredentialsService.SaveCredentials(tokenModel, userModel);

                    Console.WriteLine("User = " + userModel.userName);

                    Login();

                    Console.WriteLine("User Logged!");
                }

                if (url.Contains("#error=access_denied"))
                {
                    BTProgressHUD.Dismiss();
                }
            }
            catch (Exception ex)
            {
                BTProgressHUD.Dismiss();
                Utils.HandleException(ex);
            }
        }
    }
}
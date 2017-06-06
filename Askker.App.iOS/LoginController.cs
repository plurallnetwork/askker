using Foundation;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using System;
using UIKit;
using WebKit;

namespace Askker.App.iOS
{
    public partial class LoginController : UIViewController, IWKNavigationDelegate
    {
        public static TokenModel tokenModel;
        public static UserModel userModel;
        public UIActivityIndicatorView indicator;
        WKWebView wkwebview;

        public LoginManager loginManager;

        public LoginController (IntPtr handle) : base (handle)
        {
            loginManager = new LoginManager();

            indicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
            indicator.Frame = new CoreGraphics.CGRect(0.0, 0.0, 80.0, 80.0);
            indicator.Center = this.View.Center;
            Add(indicator);
        }

        public override void ViewDidAppear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(true, false);
            base.ViewDidAppear(animated);            

            btnLoginFacebook.TouchUpInside += btnLoginFacebook_TouchUpInside;
            btnLoginGoogle.TouchUpInside += btnLoginGoogle_TouchUpInside;
            btnSignUp.TouchUpInside += BtnSignUp_TouchUpInside;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            btnLoginFacebook.TouchUpInside -= btnLoginFacebook_TouchUpInside;
            btnLoginGoogle.TouchUpInside -= btnLoginGoogle_TouchUpInside;
            btnSignUp.TouchUpInside -= BtnSignUp_TouchUpInside;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            // Keyboard dispose when clicking outside the comment box
            var g = new UITapGestureRecognizer { CancelsTouchesInView = false };
            g.AddTarget(() => View.EndEditing(true));
            View.AddGestureRecognizer(g);
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

        //public override async void ViewDidAppear(bool animated)
        //{
        //    //TODO: Remove this if the AppDelegate code works as expected
        //    if (CredentialsService.DoCredentialsExist())
        //    {
        //        try
        //        {
        //            tokenModel = CredentialsService.GetTokenModel();
        //            userModel = await loginManager.GetUserById(CredentialsService.access_token);

        //            Login();
        //        }
        //        catch (Exception ex)
        //        {
        //            Utils.HandleException(ex);
        //        }
        //    }
        //}

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            // set the View Controller that’s powering the screen we’re
            // transitioning to

            var registerContoller = segue.DestinationViewController as RegisterController;
        }

        public void Login()
        {
            var menuController = this.Storyboard.InstantiateViewController("MenuNavController");
            if (menuController != null)
            {
                this.PresentViewController(menuController, true, null);
            }
        }

        //public async System.Threading.Tasks.Task<UserModel> GetUser(string accessToken)
        //{
        //    return await loginManager.GetUserById(accessToken);
        //}

        async partial void btnEnter_TouchUpInside(UIKit.UIButton sender)
        {
            if (string.Empty.Equals(txtUsername.Text))
            {
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
            #region Vote Sample
            /*
            VoteManager voteManager = new VoteManager();
            SurveyVoteModel surveyVoteModel = new SurveyVoteModel();
            surveyVoteModel.surveyId = "ea462cb6-ca13-45bf-b680-e689e314c9d920170301T120549";
            surveyVoteModel.optionId = 3;
            surveyVoteModel.user = new User();
            surveyVoteModel.user.id = "ea462cb6-ca13-45bf-b680-e689e314c9d9";
            surveyVoteModel.user.gender = "male";
            surveyVoteModel.user.city = "SP";
            surveyVoteModel.user.country = "BR";

            await voteManager.Vote(surveyVoteModel, "");
            */
            #endregion

            NavigationController.SetNavigationBarHidden(false, true);

            //string returnUrlLogin = "http://www.facebook.com/connect/login_success.html";
            string returnUrlLogin = "https:%2F%2Fblinq-development.com%2Fvote%2FexternalLogin";
            string externalProviderUrl = "https://blinq-development.com:44322/api/Account/ExternalLogin?provider=" + provider + "&response_type=token&client_id=self&redirect_uri=" + returnUrlLogin + "&isAdmin=1";

            var rect = new CoreGraphics.CGRect(UIScreen.MainScreen.Bounds.X, UIScreen.MainScreen.Bounds.Y + this.NavigationController.NavigationBar.Bounds.Height + 20 /*time/battery bar height*/, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height);
            wkwebview = new WKWebView(rect, new WKWebViewConfiguration());
            
            wkwebview.NavigationDelegate = this;

            Add(wkwebview);


            wkwebview.LoadRequest(new NSUrlRequest(new Uri(externalProviderUrl)));

            this.NavigationItem.SetLeftBarButtonItem(
                new UIBarButtonItem(UIBarButtonSystemItem.Cancel, (sender, args) => {
                    this.NavigationItem.SetLeftBarButtonItem(null, true);
                    NavigationController.SetNavigationBarHidden(true, true);
                    wkwebview.RemoveFromSuperview();
                    indicator.StopAnimating();
                })
            , true);

            #region UIWebView Implementation
            /*
            var webView = new UIWebView(View.Bounds);

            webView.LoadFinished += async (object wvSender, EventArgs wvE) => {
                try
                {
                    string url = webView.Request.Url.ToString();

                    if (url.Contains("access_token"))
                    {
                        string accessToken = url.Split('#')[1].Split('=')[1].Split('&')[0];

                        //var alert = UIAlertController.Create("Social Login", "Access Token = " + accessToken, UIAlertControllerStyle.Alert);
                        //alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                        //PresentViewController(alert, true, null);

                        webView.RemoveFromSuperview();
                        //string loadingPageFileName = "Resources/LoadingPage.html";
                        //string localHtmlUrl = System.IO.Path.Combine(NSBundle.MainBundle.BundlePath, loadingPageFileName);
                        //webView.LoadRequest(new NSUrlRequest(new NSUrl(localHtmlUrl, false)));

                        tokenModel = await GetUser(accessToken);

                        Login();
                    }
                    else
                    {
                        var alert = UIAlertController.Create("Social Login", "Load Finished Without Access Token", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                        PresentViewController(alert, true, null);
                    }
                }
                catch (Exception ex)
                {
                    var alert = UIAlertController.Create("Social Login", ex.Message, UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
            };

            webView.LoadError += (object wvSender, UIWebErrorArgs wvE) =>
            {
                var alert = UIAlertController.Create("Social Login", "Load Error - " + webView.Request.Url, UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            };

            View.AddSubview(webView);

            webView.LoadRequest(new NSUrlRequest(new NSUrl(externalProviderUrl)));*/
            #endregion
        }

        [Export("webView:didFinishNavigation:")]
        public async void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            try
            {
                string url = webView.Url.ToString();

                if (url.Contains("access_token"))
                {
                    webView.LoadRequest(new NSUrlRequest(new Uri("about:blank")));

                    indicator.StartAnimating();

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
                    this.NavigationItem.SetLeftBarButtonItem(null, true);
                    NavigationController.SetNavigationBarHidden(true, true);
                    wkwebview.RemoveFromSuperview();
                    indicator.StopAnimating();
                }
            }
            catch (Exception ex)
            {
                Utils.HandleException(ex);

                indicator.StopAnimating();
            }
        }
    }
}
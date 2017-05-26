﻿using Foundation;
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

        public LoginManager loginManager;

        public LoginController (IntPtr handle) : base (handle)
        {
            loginManager = new LoginManager();

            indicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
            indicator.Frame = new CoreGraphics.CGRect(0.0, 0.0, 80.0, 80.0);
            indicator.Center = this.View.Center;
            Add(indicator);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            btnLoginFacebook.TouchUpInside += btnLoginFacebook_TouchUpInside;
            btnLoginGoogle.TouchUpInside += btnLoginGoogle_TouchUpInside;
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

            //string returnUrlLogin = "http://www.facebook.com/connect/login_success.html";
            string returnUrlLogin = "https:%2F%2Fblinq-development.com%2Flogin";
            string externalProviderUrl = "https://blinq-development.com:44322/api/Account/ExternalLogin?provider=" + provider + "&response_type=token&client_id=self&redirect_uri=" + returnUrlLogin + "&isAdmin=1";

            var wkwebview = new WKWebView(UIScreen.MainScreen.Bounds, new WKWebViewConfiguration());
            wkwebview.NavigationDelegate = this;

            Add(wkwebview);


            wkwebview.LoadRequest(new NSUrlRequest(new Uri(externalProviderUrl)));

            this.NavigationItem.SetLeftBarButtonItem(
                new UIBarButtonItem(UIBarButtonSystemItem.Cancel, (sender, args) => {
                    this.NavigationItem.SetLeftBarButtonItem(null, true);
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

                    Console.WriteLine("Access Token = " + accessToken);

                    userModel = await loginManager.GetUserById(accessToken);

                    if (tokenModel == null)
                    {
                        tokenModel = new TokenModel();
                    }
                    
                    tokenModel.access_token = accessToken;
                    tokenModel.userName = userModel.userName;
                    

                    CredentialsService.SaveCredentials(tokenModel, userModel);

                    Console.WriteLine("User = " + userModel.userName);

                    Login();

                    Console.WriteLine("User Logged!");
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
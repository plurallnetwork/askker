using BigTed;
using Foundation;
using SDWebImage;
using System;
using System.Net;
using UIKit;

namespace Askker.App.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations
        public static string AppName { get { return "askker"; } }

        public override UIWindow Window
        {
            get;
            set;
        }

        public UIInterfaceOrientationMask ScreenOrientation
        {
            get;
            set;
        }

        [Export("application:supportedInterfaceOrientationsForWindow:")]
        public UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, IntPtr forWindow)
        {
            return this.ScreenOrientation;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method

            string userAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 8_2 like Mac OS X) AppleWebKit/600.1.4 (KHTML, like Gecko) Mobile/12D508 [FBAN/FBIOS;FBAV/27.0.0.10.12;FBBV/8291884;FBDV/iPhone7,1;FBMD/iPhone;FBSN/iPhone OS;FBSV/8.2;FBSS/3; FBCR/vodafoneIE;FBID/phone;FBLC/en_US;FBOP/5]";

            // set default useragent
            NSDictionary dictionary = NSDictionary.FromObjectAndKey(NSObject.FromObject(userAgent), NSObject.FromObject("UserAgent"));
            NSUserDefaults.StandardUserDefaults.RegisterDefaults(dictionary);

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            SDWebImageManager.SharedManager.ImageCache.ClearMemory();
            SDWebImageManager.SharedManager.ImageCache.ClearDisk();

            if (CredentialsService.DoCredentialsExist())
            {
                if (!CredentialsService.GetTokenModel().isStillValid(System.DateTime.Now))
                {
                    CredentialsService.DeleteCredentials();
                }
                else
                {
                    Login();
                }
            }

            //Loading View config
            ProgressHUD.Shared.HudBackgroundColour = UIColor.FromWhiteAlpha(0.0f, 0.8f);
            ProgressHUD.Shared.HudForegroundColor = UIColor.White;
            ProgressHUD.Shared.Ring.BackgroundColor = UIColor.FromWhiteAlpha(0.0f, 0.8f);
            ProgressHUD.Shared.Ring.Color = UIColor.White;
            
            return true;
        }

        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
            if (CredentialsService.DoCredentialsExist() && !CredentialsService.GetTokenModel().isStillValid(System.DateTime.Now))
            {
                ResetCredentials();
            }
        }

        private void ResetCredentials()
        {
            CredentialsService.DeleteCredentials();

            var loginController = this.Window.RootViewController.Storyboard.InstantiateViewController("LoginNavController") as LoginController;

            if (loginController != null)
            {
                this.Window.RootViewController = loginController;
            }
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }

        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            if (sourceApplication == "com.apple.SafariViewService")
            {
                NSNotificationCenter.DefaultCenter.PostNotificationName("FinishAuthentication", url);
            }

            return true;
        }

        public void Login()
        {
            //TODO: Try to update this attribute with a broadcast message
            try
            {
                LoginController.tokenModel = CredentialsService.GetTokenModel();
                LoginController.userModel = new PortableLibrary.Business.LoginManager().GetUserByTokenSync(CredentialsService.access_token);

                if (LoginController.userModel != null)
                {
                    var menuController = this.Window.RootViewController.Storyboard.InstantiateViewController("MenuNavController");

                    if (menuController != null)
                    {
                        this.Window.RootViewController = menuController;
                    }
                }
                else
                {
                    ResetCredentials();
                }
            }
            catch (System.Exception ex)
            {
                Utils.HandleException(ex);
            }
        }
    }
}
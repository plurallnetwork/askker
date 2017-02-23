using Foundation;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using System;

using UIKit;
using System.Collections.Generic;

namespace Askker.App.iOS
{
    public partial class LoginController : UIViewController
    {
        public LoginController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
        }

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

        async partial void btnEnter_TouchUpInside(UIKit.UIButton sender)
        {
            if (string.Empty.Equals(txtUsername.Text))
            {
                var alert = UIAlertController.Create("E-mail", "Please fill in the E-mail", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
            else if (string.Empty.Equals(txtPassword.Text))
            {
                var alert = UIAlertController.Create("Password", "Please fill in the Password", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
            else
            {
                LoginManager loginManager = new LoginManager();
                UserLoginModel userLoginModel = new UserLoginModel(txtUsername.Text, txtPassword.Text);

                TokenModel tokenModel = await loginManager.GetAuthorizationToken(userLoginModel);

                if (tokenModel.access_token != null)
                {
                    FeedManager feedManager = new FeedManager();

                    List<SurveyModel> surveys = await feedManager.GetSurveys("4e046d7e-cb49-4bfc-9e2d-3ec8c6f74047", "WVQ-8xwEp4uAbVxjj-_6wF9K9x-N0nDA1-ghQvVXwQV4xyXL-oCHNUrUAO3hYZPIsrp4XmWiOZpt3TeTsRp746N8h_NK6-b8C3PxSnm4mlmxOXwagloCQstmAGvCTaxGOJ5LRvYfaObOPB2X0yDSKpfKFWN-Qj_Fa8cXLCsqGjnOz47hCznItaSs9lhuL0udLZp0Jt9s78y2O0qmWxZpx3yf4X3lG87tFLKpQhTFaIxAMGVlvH6S0TOQ1AfxwS6ti31HoK1hyBOmzIgriSsalMCkE_625VQvePAi-p-Dl0U3B_I0amLlGx6LnbfYaf22744JYoF0loRiviW2JBwqIwF8tc1iRlp-TLz8iyFr9b2X-W9ftO7b_Ey0WRyaqVIyB1j2v7C3P-55jiwukqpQdJTlgIaChlnNrDN8ALjCpxhmNhXYtqjLzUmSzK1H-GI0mbkGFHEBWzZvZm4B8ewIM62jixF0j-BvROIWtdHvHdXyf_NSGvtH3TBGBguAyqFPpeh0y7GEh5AubeG4BoLoqA97_nwQUbpDqb2W9Ak58M4-_SKWCKeYaG15a6hYjxXZ");

                    var alert = UIAlertController.Create("Surveys", "Total: " + surveys.Count.ToString(), UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                else
                {
                    var alert = UIAlertController.Create("Token", tokenModel.error + " - " + tokenModel.error_description, UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
            }
        }
    }
}
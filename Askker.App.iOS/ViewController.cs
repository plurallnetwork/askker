using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using System;

using UIKit;

namespace Askker.App.iOS
{
    public partial class ViewController : UIViewController
    {
        public ViewController(IntPtr handle) : base(handle)
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

        async partial void btnEnter_TouchUpInside(UIButton sender)
        {
            LoginManager loginManager = new LoginManager();
            UserLoginModel userLoginModel = new UserLoginModel("leandrolg21@hotmail.com", "Teste@123");

            TokenModel tokenModel = await loginManager.GetAuthorizationToken(userLoginModel);

            if (tokenModel.access_token != null)
            {
                var alert = UIAlertController.Create("Token", tokenModel.access_token, UIAlertControllerStyle.Alert);
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
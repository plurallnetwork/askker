using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using Foundation;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class RegisterController : UIViewController
    {
        public RegisterController (IntPtr handle) : base (handle)
        {
            
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RegisterButton.TouchUpInside += async (sender, e) =>
            {
                if ("".Equals(NameText.Text))
                {
                    var alert = UIAlertController.Create("Name", "Please fill in the Name", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                else if ("".Equals(EmailText.Text))
                {
                    var alert = UIAlertController.Create("E-Mail", "Please fill in the E-Mail", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                else if ("".Equals(PasswordText.Text))
                {
                    var alert = UIAlertController.Create("Password", "Please fill in the Password", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                else if ("".Equals(ConfirmPasswordText.Text))
                {
                    var alert = UIAlertController.Create("Confirm Password", "Please confirm the Password", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }                
                else if (!PasswordText.Text.Equals(ConfirmPasswordText.Text))
                {
                    var alert = UIAlertController.Create("Confirm Password", "Password and Confirm Password must match", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                else if (AgreeSwitch.On == false)
                {
                    var alert = UIAlertController.Create("Policy Agreement", "Please accept the policy", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                else
                {
                    RegisterManager registerManager = new RegisterManager();
                    UserRegisterModel userRegisterModel = new UserRegisterModel(NameText.Text, EmailText.Text, PasswordText.Text, ConfirmPasswordText.Text, AgreeSwitch.On);

                    var result = await registerManager.RegisterUser(userRegisterModel);

                    if (result.Equals(""))
                    {
                        LoginManager loginManager = new LoginManager();
                        UserLoginModel userLoginModel = new UserLoginModel(EmailText.Text, PasswordText.Text);

                        TokenModel tokenModel = await loginManager.GetAuthorizationToken(userLoginModel);

                        if (tokenModel.access_token != null)
                        {
                            var alert = UIAlertController.Create("Register", "Registered and logged user successfully", UIAlertControllerStyle.Alert);
                            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                            PresentViewController(alert, true, null);
                        }
                        else
                        {
                            var alert = UIAlertController.Create("Error", tokenModel.error + " - " + tokenModel.error_description, UIAlertControllerStyle.Alert);
                            alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                            PresentViewController(alert, true, null);
                        }
                    }
                    else if (result.Equals("902"))
                    {
                        var alert = UIAlertController.Create("Register", "E-mail already registered", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                        PresentViewController(alert, true, null);
                    }
                    else
                    {
                        var alert = UIAlertController.Create("Register", result, UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                        PresentViewController(alert, true, null);
                    }
                }
            };
        }
    }
}
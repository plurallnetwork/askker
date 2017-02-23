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
                    RegisterManager registerManager = new RegisterManager();
                    UserRegisterModel userRegisterModel = new UserRegisterModel(txtName.Text, txtEmail.Text, txtPassword.Text, txtConfirmPassword.Text, swAgree.On);

                    var result = await registerManager.RegisterUser(userRegisterModel);

                    if (result.Equals(""))
                    {
                        LoginManager loginManager = new LoginManager();
                        UserLoginModel userLoginModel = new UserLoginModel(txtEmail.Text, txtPassword.Text);

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
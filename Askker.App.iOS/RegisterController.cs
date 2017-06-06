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
                    }
                    catch (Exception ex)
                    {
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
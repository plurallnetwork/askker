using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using BigTed;
using Foundation;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class ForgotPasswordController : CustomUIViewController
    {
        Random rdm = new Random();

        public ForgotPasswordController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);
                        
            // Keyboard dispose when clicking outside the comment box
            var g = new UITapGestureRecognizer { CancelsTouchesInView = false };
            g.AddTarget(() => View.EndEditing(true));
            View.AddGestureRecognizer(g);

            txtEmail.TextColor = UIColor.FromRGB(90, 89, 89);

            txtEmail.ShouldReturn = delegate (UITextField textField)
            {
                btnSend.SendActionForControlEvents(UIControlEvent.TouchUpInside);
                return true;
            };
        }

        async partial void BtnSend_TouchUpInside(UIButton sender)
        {
            if (string.Empty.Equals(txtEmail.Text))
            {
                Utils.ShowToast("Please fill in the E-mail", 3000);
            }
            else
            {
                try
                {
                    BTProgressHUD.Show("Checking...", -1, ProgressHUD.MaskType.Clear);

                    UserModel user = await new LoginManager().GetUserByEmail(txtEmail.Text);

                    if (user == null)
                    {
                        Utils.ShowToast("E-Mail not found", 3000);
                        Utils.KillAfter(3);
                    }
                    else if (!string.IsNullOrEmpty(user.provider))
                    {
                        Utils.ShowToast("Your acconut is binded with " + user.provider + ".\nPlease log in using " + user.provider, 3000);
                        Utils.KillAfter(3);
                        var loginController = this.Storyboard.InstantiateViewController("LoginController");
                        if (loginController != null)
                        {
                            this.NavigationController.PushViewController(loginController, true);
                        }
                    }
                    else
                    {
                        BTProgressHUD.Show("Sending the E-mail...", -1, ProgressHUD.MaskType.Clear);

                        NSUserDefaults.StandardUserDefaults.RemoveObject("AskkerPwdCode");
                        NSUserDefaults.StandardUserDefaults.RemoveObject("AskkerPwdTimeStamp");

                        int code = rdm.Next(10000);

                        NSUserDefaults.StandardUserDefaults.SetString(code.ToString("0000"), "AskkerPwdCode");
                        NSUserDefaults.StandardUserDefaults.SetString(DateTime.Now.ToString("yyyyMMddHHmmss"), "AskkerPwdTimeStamp");

                        ResetPasswordModel model = new ResetPasswordModel(txtEmail.Text, code.ToString("0000"));

                        await new LoginManager().SendEmailResetPasswordFromApp(model);


                        BTProgressHUD.ShowSuccessWithStatus("E-mail Sent");

                        var confirmCodeController = this.Storyboard.InstantiateViewController("ConfirmCodeController") as ConfirmCodeController;
                        if (confirmCodeController != null)
                        {
                            confirmCodeController.Email = txtEmail.Text;

                            this.NavigationController.PushViewController(confirmCodeController, true);
                            NavigationController.SetNavigationBarHidden(false, true);
                        }

                        txtEmail.Text = "";                        
                    }
                }
                catch (Exception ex)
                {
                    BTProgressHUD.Dismiss();

                    NSUserDefaults.StandardUserDefaults.RemoveObject("AskkerPwdCode");
                    NSUserDefaults.StandardUserDefaults.RemoveObject("AskkerPwdTimeStamp");

                    if (ex.Message.Equals("906"))
                    {
                        Utils.ShowAlertOk("Forgot Password", "E-mail not registered");
                    }
                    else
                    {
                        Utils.ShowAlertOk("Something went wrong", ex.Message);
                    }
                }
            }
        }
    }
}
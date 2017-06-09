using Askker.App.PortableLibrary.Business;
using BigTed;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class ForgotPasswordController : UIViewController
    {
        public ForgotPasswordController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Keyboard dispose when clicking outside the comment box
            var g = new UITapGestureRecognizer { CancelsTouchesInView = false };
            g.AddTarget(() => View.EndEditing(true));
            View.AddGestureRecognizer(g);
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
                    BTProgressHUD.Show("Sending the E-mail...", -1, ProgressHUD.MaskType.Clear);

                    await new LoginManager().SendEmailResetPassword(txtEmail.Text);

                    txtEmail.Text = "";
                    BTProgressHUD.ShowSuccessWithStatus("E-mail Sent");
                }
                catch (Exception ex)
                {
                    BTProgressHUD.Dismiss();

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
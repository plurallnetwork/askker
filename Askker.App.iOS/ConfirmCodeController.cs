using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using BigTed;
using CoreGraphics;
using Foundation;
using System;
using System.Globalization;
using UIKit;

namespace Askker.App.iOS
{
    public partial class ConfirmCodeController : CustomUIViewController
    {
        private UIView activeview;             // Controller that activated the keyboard
        private float scroll_amount = 0.0f;    // amount to scroll 
        private float bottom = 0.0f;           // bottom point
        private float offset = 8.0f;           // extra offset
        private bool moveViewUp = false;       // which direction are we moving

        public string Email { get; set; }

        public ConfirmCodeController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);

            UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = UIColor.FromRGB(90, 89, 89)
            };

            // Keyboard dispose when clicking outside the comment box
            var g = new UITapGestureRecognizer { CancelsTouchesInView = false };
            g.AddTarget(() => View.EndEditing(true));
            View.AddGestureRecognizer(g);

            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification, KeyBoardUpNotification);

            // Keyboard Down
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, KeyBoardDownNotification);

            txtPassword.ShouldReturn = delegate (UITextField textField)
            {
                View.EndEditing(true);
                txtConfirmPassword.BecomeFirstResponder();
                return true;
            };

            txtConfirmPassword.ShouldReturn = delegate (UITextField textField)
            {
                btnSend.SendActionForControlEvents(UIControlEvent.TouchUpInside);
                return true;
            };
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            btnSend.TouchUpInside += BtnSend_TouchUpInside;
        }
        
        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            btnSend.TouchUpInside -= BtnSend_TouchUpInside;
        }

        private void KeyBoardUpNotification(NSNotification notification)
        {
            if (!moveViewUp)
            {
                // get the keyboard size
                CGRect r = UIKeyboard.BoundsFromNotification(notification);

                // Find what opened the keyboard
                foreach (UIView view in this.View.Subviews)
                {
                    if (view.IsFirstResponder)
                        activeview = view;
                }

                if (activeview != null)
                {
                    // Bottom of the controller = initial position + height - View Y position + offset (relative to the screen)     
                    UIView relativePositionView = UIApplication.SharedApplication.KeyWindow;
                    CGRect relativeFrame = activeview.Superview.ConvertRectToView(activeview.Frame, relativePositionView);

                    bottom = (float)((relativeFrame.Y) + relativeFrame.Height - View.Frame.Y + offset);

                    // Calculate how far we need to scroll
                    scroll_amount = (float)(r.Height - (View.Frame.Size.Height - bottom));
                }
                else
                {
                    scroll_amount = 0;
                }

                // Perform the scrolling
                moveViewUp = true;
                ScrollTheView(moveViewUp);
            }
        }

        private void KeyBoardDownNotification(NSNotification notification)
        {
            moveViewUp = false;
            ScrollTheView(moveViewUp);
        }

        private void ScrollTheView(bool move)
        {
            // scroll the view up or down
            UIView.BeginAnimations(string.Empty, System.IntPtr.Zero);
            UIView.SetAnimationDuration(0.3);

            CGRect frame = View.Frame;

            if (move)
            {
                if (scroll_amount > 0)
                {
                    frame.Y -= scroll_amount;
                }
            }
            else
            {
                //frame.Y += scroll_amount + 44;
                frame.Y = 0;
                scroll_amount = 0;
            }

            View.Frame = frame;
            UIView.CommitAnimations();
        }

        private async void BtnSend_TouchUpInside(object sender, EventArgs e)
        {
            if (string.Empty.Equals(txtCode.Text))
            {
                Utils.ShowToast("Please fill in the code", 3000);
            }
            else if (txtCode.Text.Length != 4)
            {
                Utils.ShowToast("Please fill in the 4-digit code", 3000);
            }
            else if (string.Empty.Equals(txtPassword.Text))
            {
                Utils.ShowToast("Please fill in the new password", 3000);
            }
            else if (string.Empty.Equals(txtConfirmPassword.Text))
            {
                Utils.ShowToast("Please fill in the password confirmation", 3000);
            }
            else if (!txtPassword.Text.Equals(txtConfirmPassword.Text))
            {
                Utils.ShowToast("Passwords must match", 3000);
            }
            else
            {
                try
                {
                    BTProgressHUD.Show("Validating...", -1, ProgressHUD.MaskType.Clear);

                    DateTime dt = DateTime.ParseExact(NSUserDefaults.StandardUserDefaults.StringForKey("AskkerPwdTimeStamp"), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                    DateTime now = DateTime.Now;

                    TimeSpan span = now.Subtract(dt);

                    if (!txtCode.Text.Equals(NSUserDefaults.StandardUserDefaults.StringForKey("AskkerPwdCode")))
                    {
                        BTProgressHUD.ShowErrorWithStatus("Invalid code", 3000);
                    }
                    else if (span.Seconds > 300)
                    {
                        BTProgressHUD.ShowErrorWithStatus("Code expired, please request another code.", 3000);
                    }

                    SetPasswordModel model = new SetPasswordModel(Email, txtPassword.Text, txtConfirmPassword.Text);

                    await new LoginManager().SetPasswordFromApp(model);

                    BTProgressHUD.ShowSuccessWithStatus("Everything OK!\nPlease login");

                    NSUserDefaults.StandardUserDefaults.RemoveObject("AskkerPwdCode");
                    NSUserDefaults.StandardUserDefaults.RemoveObject("AskkerPwdTimeStamp");

                    var loginController = this.Storyboard.InstantiateViewController("LoginController");
                    if (loginController != null)
                    {
                        this.NavigationController.PushViewController(loginController, true);
                    }
                }
                catch (Exception ex)
                {
                    BTProgressHUD.Dismiss();

                    Utils.ShowAlertOk("Something went wrong", ex.Message);
                }
            }
        }
    }
}
// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace Askker.App.iOS
{
    [Register ("RegisterController")]
    partial class RegisterController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISwitch AgreeSwitch { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField ConfirmPasswordText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField EmailText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField NameText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField PasswordText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton RegisterButton { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (AgreeSwitch != null) {
                AgreeSwitch.Dispose ();
                AgreeSwitch = null;
            }

            if (ConfirmPasswordText != null) {
                ConfirmPasswordText.Dispose ();
                ConfirmPasswordText = null;
            }

            if (EmailText != null) {
                EmailText.Dispose ();
                EmailText = null;
            }

            if (NameText != null) {
                NameText.Dispose ();
                NameText = null;
            }

            if (PasswordText != null) {
                PasswordText.Dispose ();
                PasswordText = null;
            }

            if (RegisterButton != null) {
                RegisterButton.Dispose ();
                RegisterButton = null;
            }
        }
    }
}
// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace Askker.App.iOS
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnEnter { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnLoginFacebook { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnLoginGoogle { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblNeedHelp { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton RegisterButton { get; set; }

        [Action ("btnEnter_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void btnEnter_TouchUpInside ();

        void ReleaseDesignerOutlets ()
        {
            if (btnEnter != null) {
                btnEnter.Dispose ();
                btnEnter = null;
            }

            if (btnLoginFacebook != null) {
                btnLoginFacebook.Dispose ();
                btnLoginFacebook = null;
            }

            if (btnLoginGoogle != null) {
                btnLoginGoogle.Dispose ();
                btnLoginGoogle = null;
            }

            if (lblNeedHelp != null) {
                lblNeedHelp.Dispose ();
                lblNeedHelp = null;
            }

            if (RegisterButton != null) {
                RegisterButton.Dispose ();
                RegisterButton = null;
            }
        }
    }
}
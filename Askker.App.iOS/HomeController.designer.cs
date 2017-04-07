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
    [Register ("HomeController")]
    partial class HomeController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnMenu { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton profileOtherButton { get; set; }

        [Action ("BtnMenu_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void BtnMenu_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (btnMenu != null) {
                btnMenu.Dispose ();
                btnMenu = null;
            }

            if (profileOtherButton != null) {
                profileOtherButton.Dispose ();
                profileOtherButton = null;
            }
        }
    }
}
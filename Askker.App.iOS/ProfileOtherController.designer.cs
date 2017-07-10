// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace Askker.App.iOS
{
    [Register ("ProfileOtherController")]
    partial class ProfileOtherController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField ageText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnRelationship { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField emailText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField genderText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField nameText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView profileImageView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (ageText != null) {
                ageText.Dispose ();
                ageText = null;
            }

            if (btnRelationship != null) {
                btnRelationship.Dispose ();
                btnRelationship = null;
            }

            if (emailText != null) {
                emailText.Dispose ();
                emailText = null;
            }

            if (genderText != null) {
                genderText.Dispose ();
                genderText = null;
            }

            if (nameText != null) {
                nameText.Dispose ();
                nameText = null;
            }

            if (profileImageView != null) {
                profileImageView.Dispose ();
                profileImageView = null;
            }
        }
    }
}
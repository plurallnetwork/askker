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
    [Register ("GroupProfileController")]
    partial class GroupProfileController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnRelationship { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblName { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView profileImageView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView textDescription { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnRelationship != null) {
                btnRelationship.Dispose ();
                btnRelationship = null;
            }

            if (lblName != null) {
                lblName.Dispose ();
                lblName = null;
            }

            if (profileImageView != null) {
                profileImageView.Dispose ();
                profileImageView = null;
            }

            if (textDescription != null) {
                textDescription.Dispose ();
                textDescription = null;
            }
        }
    }
}
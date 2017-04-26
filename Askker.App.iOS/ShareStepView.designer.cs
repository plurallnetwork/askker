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
    [Register ("ShareStepView")]
    partial class ShareStepView
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnFriends { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnPrivate { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnPublic { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView questionText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel shareMessageLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView shareTable { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnFriends != null) {
                btnFriends.Dispose ();
                btnFriends = null;
            }

            if (btnPrivate != null) {
                btnPrivate.Dispose ();
                btnPrivate = null;
            }

            if (btnPublic != null) {
                btnPublic.Dispose ();
                btnPublic = null;
            }

            if (questionText != null) {
                questionText.Dispose ();
                questionText = null;
            }

            if (shareMessageLabel != null) {
                shareMessageLabel.Dispose ();
                shareMessageLabel = null;
            }

            if (shareTable != null) {
                shareTable.Dispose ();
                shareTable = null;
            }
        }
    }
}
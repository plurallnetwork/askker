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
        UIKit.UIButton doneButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton friendsButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton groupsButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView questionText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView shareTable { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (doneButton != null) {
                doneButton.Dispose ();
                doneButton = null;
            }

            if (friendsButton != null) {
                friendsButton.Dispose ();
                friendsButton = null;
            }

            if (groupsButton != null) {
                groupsButton.Dispose ();
                groupsButton = null;
            }

            if (questionText != null) {
                questionText.Dispose ();
                questionText = null;
            }

            if (shareTable != null) {
                shareTable.Dispose ();
                shareTable = null;
            }
        }
    }
}
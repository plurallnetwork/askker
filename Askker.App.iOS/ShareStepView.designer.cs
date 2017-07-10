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
    [Register ("ShareStepView")]
    partial class ShareStepView
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView questionText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView shareOptions { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView shareTable { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (questionText != null) {
                questionText.Dispose ();
                questionText = null;
            }

            if (shareOptions != null) {
                shareOptions.Dispose ();
                shareOptions = null;
            }

            if (shareTable != null) {
                shareTable.Dispose ();
                shareTable = null;
            }
        }
    }
}
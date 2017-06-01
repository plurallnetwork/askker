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
    [Register ("OptionsStepView")]
    partial class OptionsStepView
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton doneButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton imageButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView optionsTable { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView questionText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton textButton { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (doneButton != null) {
                doneButton.Dispose ();
                doneButton = null;
            }

            if (imageButton != null) {
                imageButton.Dispose ();
                imageButton = null;
            }

            if (optionsTable != null) {
                optionsTable.Dispose ();
                optionsTable = null;
            }

            if (questionText != null) {
                questionText.Dispose ();
                questionText = null;
            }

            if (textButton != null) {
                textButton.Dispose ();
                textButton = null;
            }
        }
    }
}
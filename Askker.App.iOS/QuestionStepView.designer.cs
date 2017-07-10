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
    [Register ("QuestionStepView")]
    partial class QuestionStepView
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField questionText { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (questionText != null) {
                questionText.Dispose ();
                questionText = null;
            }
        }
    }
}
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
    [Register ("FeedMenuView")]
    partial class FeedMenuView
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton cancelBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton cleanBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton deleteBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton editBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton finishBtn { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (cancelBtn != null) {
                cancelBtn.Dispose ();
                cancelBtn = null;
            }

            if (cleanBtn != null) {
                cleanBtn.Dispose ();
                cleanBtn = null;
            }

            if (deleteBtn != null) {
                deleteBtn.Dispose ();
                deleteBtn = null;
            }

            if (editBtn != null) {
                editBtn.Dispose ();
                editBtn = null;
            }

            if (finishBtn != null) {
                finishBtn.Dispose ();
                finishBtn = null;
            }
        }
    }
}
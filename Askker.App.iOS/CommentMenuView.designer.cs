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
    [Register ("CommentMenuView")]
    partial class CommentMenuView
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton cancelBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton deleteBtn { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (cancelBtn != null) {
                cancelBtn.Dispose ();
                cancelBtn = null;
            }

            if (deleteBtn != null) {
                deleteBtn.Dispose ();
                deleteBtn = null;
            }
        }
    }
}
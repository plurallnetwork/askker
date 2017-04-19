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
        UIKit.UIView friendsView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView publicView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView questionText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView shareTable { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView shareView { get; set; }

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

            if (friendsView != null) {
                friendsView.Dispose ();
                friendsView = null;
            }

            if (publicView != null) {
                publicView.Dispose ();
                publicView = null;
            }

            if (questionText != null) {
                questionText.Dispose ();
                questionText = null;
            }

            if (shareTable != null) {
                shareTable.Dispose ();
                shareTable = null;
            }

            if (shareView != null) {
                shareView.Dispose ();
                shareView = null;
            }
        }
    }
}
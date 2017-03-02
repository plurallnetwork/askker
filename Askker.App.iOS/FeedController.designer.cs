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
    [Register ("FeedController")]
    partial class FeedController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UICollectionView feedCollectionView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (feedCollectionView != null) {
                feedCollectionView.Dispose ();
                feedCollectionView = null;
            }
        }
    }
}
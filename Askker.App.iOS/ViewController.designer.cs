﻿// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace Askker.App.iOS
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnEnter { get; set; }

        [Action ("btnEnter_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void btnEnter_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (btnEnter != null) {
                btnEnter.Dispose ();
                btnEnter = null;
            }
        }
    }
}
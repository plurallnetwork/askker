using Foundation;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class ShareStepView : UIView
    {
        public ShareStepView (IntPtr handle) : base (handle)
        {
        }

        public UITextView QuestionText
        {
            get { return questionText; }
            set { questionText = value; }
        }

        public UIButton GroupsButton
        {
            get { return groupsButton; }
            set { groupsButton = value; }
        }

        public UIButton FriendsButton
        {
            get { return friendsButton; }
            set { friendsButton = value; }
        }

        public UIButton DoneButton
        {
            get { return doneButton; }
            set { doneButton = value; }
        }

        public UITableView ShareTable
        {
            get { return shareTable; }
            set { shareTable = value; }
        }

        public static ShareStepView Create()
        {

            var arr = NSBundle.MainBundle.LoadNib("ShareStepView", null, null);
            var v = ObjCRuntime.Runtime.GetNSObject<ShareStepView>(arr.ValueAt(0));

            return v;
        }

        public override void AwakeFromNib()
        {
            doneButton.Frame = new CoreGraphics.CGRect(0f, 587f, 375f, 80f);
        }
    }
}
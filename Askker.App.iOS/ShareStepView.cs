using Foundation;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class ShareStepView : UIView
    {
        public ShareStepView(IntPtr handle) : base(handle)
        {
            
        }

        public UITextView QuestionText
        {
            get { return questionText; }
            set { questionText = value; }
        }

        public UIButton PrivateButton
        {
            get { return btnPrivate; }
            set { btnPrivate = value; }
        }

        public UIButton FriendsButton
        {
            get { return btnFriends; }
            set { btnFriends = value; }
        }

        public UIButton PublicButton
        {
            get { return btnPublic; }
            set { btnPublic = value; }
        }

        public UITableView ShareTable
        {
            get { return shareTable; }
            set { shareTable = value; }
        }

        public UILabel ShareMessageLabel
        {
            get { return shareMessageLabel; }
            set { shareMessageLabel = value; }
        }

        public static ShareStepView Create()
        {

            var arr = NSBundle.MainBundle.LoadNib("ShareStepView", null, null);
            var v = ObjCRuntime.Runtime.GetNSObject<ShareStepView>(arr.ValueAt(0));

            return v;
        }
    }
}
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

        public UITableView ShareOptions
        {
            get { return shareOptions; }
            set { shareOptions = value; }
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
    }
}
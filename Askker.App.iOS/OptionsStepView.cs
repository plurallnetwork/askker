using Cirrious.FluentLayouts.Touch;
using Foundation;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class OptionsStepView : UIView
    {
        public UITextView QuestionText
        {
            get { return questionText; }
            set { questionText = value; }
        }

        public OptionsStepView(IntPtr handle) : base(handle)
        {
        }

        public UIButton TextButton
        {
            get { return textButton; }
            set { textButton = value; }
        }

        public UIButton ImageButton
        {
            get { return imageButton; }
            set { imageButton = value; }
        }

        public UIButton DoneButton
        {
            get { return doneButton; }
            set { doneButton = value; }
        }

        public UITableView OptionsTable
        {
            get { return optionsTable; }
            set { optionsTable = value; }
        }

        public static OptionsStepView Create()
        {

            var arr = NSBundle.MainBundle.LoadNib("OptionsStepView", null, null);
            var v = ObjCRuntime.Runtime.GetNSObject<OptionsStepView>(arr.ValueAt(0));
            
            return v;
        }

        public override void AwakeFromNib()
        {
            doneButton.Frame = new CoreGraphics.CGRect(0f, 587f, 375f, 80f);                                    
        }
    }
}
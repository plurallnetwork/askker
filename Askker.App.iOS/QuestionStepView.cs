using Askker.App.iOS.CustomViewComponents;
using Askker.App.PortableLibrary.Models;
using Foundation;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class QuestionStepView : UIView
    {
        public UITextView QuestionText
        {
            get { return questionText; }
            set {questionText = value; }
        }
        
        public QuestionStepView(IntPtr handle) : base(handle)
        {
        }

        public static QuestionStepView Create()
        {

            var arr = NSBundle.MainBundle.LoadNib("QuestionStepView", null, null);
            var v = ObjCRuntime.Runtime.GetNSObject<QuestionStepView>(arr.ValueAt(0));

            return v;
        }

        //public override void AwakeFromNib()
        //{
        //    questionText.Placeholder = "Write your question here...";
        //    questionText.PlaceholderColor = UIColor.LightGray;
        //    questionText.PlaceholderFont = UIFont.SystemFontOfSize(20);
        //}
    }
}
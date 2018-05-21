using Foundation;
using QuickLook;
using System;
using System.Collections.Generic;
using System.Text;

namespace Askker.App.iOS.PDFComponents
{
    public class QLPreviewItemFileSystem : QLPreviewItem
    {
        string _fileName, _filePath;

        public QLPreviewItemFileSystem(string fileName, string filePath)
        {
            _fileName = fileName;
            _filePath = filePath;
        }

        public override string ItemTitle
        {
            get
            {
                return _fileName;
            }
        }
        public override NSUrl ItemUrl
        {
            get
            {
                return NSUrl.FromFilename(_filePath);
            }
        }
    }

    public class QLPreviewItemBundle : QLPreviewItem
    {
        string _fileName;
        NSUrl _filePath;
        public QLPreviewItemBundle(string fileName, NSUrl filePath)
        {
            _fileName = fileName;
            _filePath = filePath;
        }

        public override string ItemTitle
        {
            get
            {
                return _fileName;
            }
        }
        public override NSUrl ItemUrl
        {
            get
            {
                //var documents = NSBundle.MainBundle.BundlePath;
                //var lib = Path.Combine(documents, _filePath);
                //var url = NSUrl.FromFilename(lib);
                //return url;

                return _filePath;
            }
        }
    }
}

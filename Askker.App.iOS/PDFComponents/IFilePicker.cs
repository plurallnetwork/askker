using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Askker.App.iOS.PDFComponents
{
    public interface IFilePicker
    {
        Task<FileData> PickFile();

        Task<FileData> PickFile(UIViewController viewController);

        Task<bool> SaveFile(FileData fileToSave);

        void OpenFile(string fileToOpen);

        void OpenFile(FileData fileToOpen);
    }
}

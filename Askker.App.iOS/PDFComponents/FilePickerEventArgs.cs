﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Askker.App.iOS.PDFComponents
{
    public class FilePickerEventArgs : EventArgs
    {
        public byte[] FileByte { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public FilePickerEventArgs()
        {

        }

        public FilePickerEventArgs(byte[] fileByte)
        {
            FileByte = fileByte;
        }

        public FilePickerEventArgs(byte[] fileByte, string fileName)
            : this(fileByte)
        {
            FileName = fileName;
        }

        public FilePickerEventArgs(byte[] fileByte, string fileName, string filePath)
            : this(fileByte, fileName)
        {
            FilePath = filePath;
        }
    }
}
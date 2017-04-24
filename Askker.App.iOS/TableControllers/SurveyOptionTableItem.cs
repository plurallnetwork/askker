using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Askker.App.iOS.TableControllers
{
    public class SurveyOptionTableItem
    {
        public string Text { get; set; }

        public string ImageExtension { get; set; }

        public byte[] Image { get; set; }

        public UITableViewCellStyle CellStyle
        {
            get { return cellStyle; }
            set { cellStyle = value; }
        }
        protected UITableViewCellStyle cellStyle = UITableViewCellStyle.Default;

        public UITableViewCellAccessory CellAccessory
        {
            get { return cellAccessory; }
            set { cellAccessory = value; }
        }
        protected UITableViewCellAccessory cellAccessory = UITableViewCellAccessory.None;

        public SurveyOptionTableItem() { }

        public SurveyOptionTableItem(string text)
        { Text = text; }

        public SurveyOptionTableItem(string text, byte[] image)
        {
            Text = text;
            Image = image;
        }

        public SurveyOptionTableItem(string text, string imageExtension, byte[] image)
        {
            Text = text;
            Image = image;
            ImageExtension = imageExtension;
        }
    }
}

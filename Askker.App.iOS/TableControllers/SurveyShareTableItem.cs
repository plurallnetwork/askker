using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Askker.App.iOS.TableControllers
{
    public class SurveyShareTableItem
    {
        public string Title { get; set; }

        public string ImageName { get; set; }

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

        public SurveyShareTableItem() { }

        public SurveyShareTableItem(string title)
        { Title = title; }

        public SurveyShareTableItem(string title, string imageName)
        {
            Title = title;
            ImageName = imageName;
        }
    }
}

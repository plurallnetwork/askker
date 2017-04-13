using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Askker.App.iOS.TableControllers
{
    public class SurveyOptionTableItem
    {
        public string Heading { get; set; }

        public string SubHeading { get; set; }

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

        public SurveyOptionTableItem(string heading)
        { Heading = heading; }

        public SurveyOptionTableItem(string heading, byte[] image)
        {
            Heading = heading;
            Image = image;
        }

        public SurveyOptionTableItem(string heading, string subheading, byte[] image)
        {
            Heading = heading;
            Image = image;
            SubHeading = subheading;
        }
    }
}

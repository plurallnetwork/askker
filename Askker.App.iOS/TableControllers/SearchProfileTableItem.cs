using UIKit;

namespace Askker.App.iOS.TableControllers
{
    public class SearchProfileTableItem
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ProfilePicture { get; set; }

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

        public SearchProfileTableItem() { }

        public SearchProfileTableItem(string name)
        {
            Name = name;
        }

        public SearchProfileTableItem(string name, string profilePicture)
        {
            Name = name;
            ProfilePicture = profilePicture;
        }

        public SearchProfileTableItem(string id, string name, string profilePicture)
        {
            Id = id;
            Name = name;
            ProfilePicture = profilePicture;
        }
    }
}

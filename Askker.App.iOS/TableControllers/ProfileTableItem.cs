using UIKit;

namespace Askker.App.iOS.TableControllers
{
    public class ProfileTableItem
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

        public ProfileTableItem() { }

        public ProfileTableItem(string name)
        {
            Name = name;
        }

        public ProfileTableItem(string name, string profilePicture)
        {
            Name = name;
            ProfilePicture = profilePicture;
        }

        public ProfileTableItem(string id, string name, string profilePicture)
        {
            Id = id;
            Name = name;
            ProfilePicture = profilePicture;
        }
    }
}

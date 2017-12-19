using UIKit;

namespace Askker.App.iOS.TableControllers
{
    public class SearchUserGroupsTableItem
    {
        public string Id { get; set; }

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

        public SearchUserGroupsTableItem() { }

        public SearchUserGroupsTableItem(string title)
        { Title = title; }

        public SearchUserGroupsTableItem(string title, string imageName)
        {
            Title = title;
            ImageName = imageName;
        }

        public SearchUserGroupsTableItem(string id, string title, string imageName)
        {
            Id = id;
            Title = title;
            ImageName = imageName;
        }
    }
}

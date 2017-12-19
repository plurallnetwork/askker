using Askker.App.PortableLibrary.Enums;
using System;
using System.Collections.Generic;

namespace Askker.App.iOS.Models
{
    public class MenuModel
    {
        public MenuItem MenuItem { get; set; }

        public String Title { get; set; }

        public String ImageName { get; set; }
    }

    public class MenuPagesModel : MenuModel
    {
        public List<MenuModel> MenuItems { get; }

        public MenuPagesModel()
        {
            MenuItems = new List<MenuModel>();
            //MenuItems.Add(new MenuModel() { MenuItem = MenuItem.Feed, Title = "Feed", ImageName = "pagesFeed" });
            MenuItems.Add(new MenuModel() { MenuItem = MenuItem.MyFriends, Title = "My Friends", ImageName = "MyFriends" });
            MenuItems.Add(new MenuModel() { MenuItem = MenuItem.SearchFriends, Title = "Find Friends", ImageName = "AddFriends" });
            MenuItems.Add(new MenuModel() { MenuItem = MenuItem.SearchGroups, Title = "Find Groups", ImageName = "AddFriends" });
            //MenuItems.Add(new MenuModel() { MenuItem = MenuItem.Settings, Title = "Settings", ImageName = "pagesSettings" });
            //MenuItems.Add(new MenuModel() { MenuItem = MenuItem.Help, Title = "Help", ImageName = "pagesHelp" });
            //MenuItems.Add(new MenuModel() { MenuItem = MenuItem.Logout, Title = "Log out", ImageName = "pagesLogOut" });
        }
    }

    public class MenuFilterModel : MenuModel
    {
        public List<MenuModel> MenuItems { get; }

        public MenuFilterModel()
        {
            MenuItems = new List<MenuModel>();
            MenuItems.Add(new MenuModel() { MenuItem = MenuItem.Mine, Title = "My surveys", ImageName = "MySurveys" });
            MenuItems.Add(new MenuModel() { MenuItem = MenuItem.ForMe, Title = "For me", ImageName = "ForMe" });
            MenuItems.Add(new MenuModel() { MenuItem = MenuItem.Finished, Title = "Finished", ImageName = "Finished" });
        }
    }

    public class MenuLogoutModel : MenuModel
    {
        public MenuLogoutModel()
        {
            this.MenuItem = MenuItem.Logout;
            this.Title = "Log out";
            this.ImageName = "Logout";
        }
    }
}
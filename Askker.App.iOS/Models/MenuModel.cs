using System;
using System.Collections.Generic;

namespace Askker.App.iOS.Models
{
    public class MenuModel
    {
        public String Title
        {
            get;
            set;
        }

        public String ImageName
        {
            get;
            set;
        }
    }

    public class MenuPagesModel : MenuModel
    {
        public List<MenuModel> MenuItems
        {
            get;
        }

        public MenuPagesModel()
        {
            MenuItems = new List<MenuModel>();
            MenuItems.Add(new MenuModel() { Title = "Feed", ImageName = "pagesFeed" });
            MenuItems.Add(new MenuModel() { Title = "Friends", ImageName = "pagesFriends" });
            MenuItems.Add(new MenuModel() { Title = "Settings", ImageName = "pagesSettings" });
            MenuItems.Add(new MenuModel() { Title = "Help", ImageName = "pagesHelp" });
            MenuItems.Add(new MenuModel() { Title = "Log out", ImageName = "pagesLogOut" });
        }
    }

    public class MenuFilterModel : MenuModel
    {
        public List<MenuModel> MenuItems
        {
            get;
        }

        public MenuFilterModel()
        {
            MenuItems = new List<MenuModel>();
            MenuItems.Add(new MenuModel() { Title = "Mine", ImageName = "filterMine" });
            MenuItems.Add(new MenuModel() { Title = "To You", ImageName = "filterToYou" });
            MenuItems.Add(new MenuModel() { Title = "Public", ImageName = "filterPublic" });
        }
    }
}
using Askker.App.iOS.Models;
using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Util;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using SidebarNavigation;
using System;
using System.Collections.Generic;
using System.Drawing;
using UIKit;

namespace Askker.App.iOS
{
    public partial class SideMenuController : UIViewController
    {
        MenuViewController menuViewController;
        
        public SideMenuController(MenuViewController menuViewController)
        {
            this.menuViewController = menuViewController;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.FromRGB(.9f, .9f, .9f);

            var scrollView = new UIScrollView(new RectangleF(0, 0, (float)View.Frame.Width, (float)View.Frame.Height));

            var profileImageView = new UIImageView(new RectangleF(85, 80, 90, 90));
            profileImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            profileImageView.Image = UIImage.FromBundle("Profile");
            profileImageView.Layer.CornerRadius = (profileImageView.Frame.Width / 2);
            profileImageView.Layer.MasksToBounds = true;

            var tapGestureRecognizer = new UITapGestureRecognizer(this, new ObjCRuntime.Selector("TapProfilePictureSelector:"));
            profileImageView.AddGestureRecognizer(tapGestureRecognizer);
            profileImageView.UserInteractionEnabled = true;

            if (LoginController.userModel.profilePicturePath != null)
            {
                Utils.SetImageFromNSUrlSession(LoginController.userModel.profilePicturePath, profileImageView);
            }

            var name = new UILabel(new RectangleF(20, 180, 220, 20));
            name.Font = UIFont.SystemFontOfSize(14.0f);
            name.TextAlignment = UITextAlignment.Center;
            name.TextColor = UIColor.Black;
            name.Text = LoginController.userModel.name;

            var dividerLineView = new UIView(new RectangleF(20, 220, 220, 0.5f));
            dividerLineView.BackgroundColor = UIColor.Black;

            var pagesLabel = new UILabel(new RectangleF(20, 225, 220, 20));
            pagesLabel.Font = UIFont.BoldSystemFontOfSize(12.0f);
            pagesLabel.TextAlignment = UITextAlignment.Left;
            pagesLabel.TextColor = UIColor.Black;
            pagesLabel.Text = "Pages";

            var pagesItems = new MenuPagesModel().MenuItems;
            var pagesTableView = new UITableView(new RectangleF(20, 250, 220, pagesItems.Count * 44));
            pagesTableView.ContentInset = new UIEdgeInsets(0, 20, 0, 0);
            pagesTableView.BackgroundColor = UIColor.Clear;
            pagesTableView.ScrollEnabled = false;
            new MenuTableViewController(pagesTableView, pagesItems, menuViewController);

            var dividerLineView2 = new UIView(new RectangleF(20, (float)pagesTableView.Frame.Y + (float)pagesTableView.Frame.Height + 25, 220, 0.5f));
            dividerLineView2.BackgroundColor = UIColor.Black;

            var filterLabel = new UILabel(new RectangleF(20, (float)dividerLineView2.Frame.Y + 5, 220, 20));
            filterLabel.Font = UIFont.BoldSystemFontOfSize(12.0f);
            filterLabel.TextAlignment = UITextAlignment.Left;
            filterLabel.TextColor = UIColor.Black;
            filterLabel.Text = "Filter";

            var filterItems = new MenuFilterModel().MenuItems;
            var filterTableView = new UITableView(new RectangleF(20, (float)filterLabel.Frame.Y + 25, 220, filterItems.Count * 44));
            filterTableView.ContentInset = new UIEdgeInsets(0, 20, 0, 0);
            filterTableView.BackgroundColor = UIColor.Clear;
            filterTableView.ScrollEnabled = false;
            new MenuTableViewController(filterTableView, filterItems, menuViewController);

            var dividerLineView3 = new UIView(new RectangleF(20, (float)filterTableView.Frame.Y + (float)filterTableView.Frame.Height + 25, 220, 0.5f));
            dividerLineView3.BackgroundColor = UIColor.Black;

            var hashtagLabel = new UILabel(new RectangleF(20, (float)dividerLineView3.Frame.Y + 5, 220, 20));
            hashtagLabel.Font = UIFont.BoldSystemFontOfSize(12.0f);
            hashtagLabel.TextAlignment = UITextAlignment.Left;
            hashtagLabel.TextColor = UIColor.Black;
            hashtagLabel.Text = "Hashtag";

            var hashtagText = new UITextField(new RectangleF(20, (float)hashtagLabel.Frame.Y + 30, 220, 40));
            hashtagText.BorderStyle = UITextBorderStyle.Bezel;
            hashtagText.Placeholder = "inserts tags to filter here";

            scrollView.Add(profileImageView);
            scrollView.Add(name);
            scrollView.Add(dividerLineView);
            scrollView.Add(pagesLabel);
            scrollView.Add(pagesTableView);
            scrollView.Add(dividerLineView2);
            scrollView.Add(filterLabel);
            scrollView.Add(filterTableView);
            scrollView.Add(dividerLineView3);
            scrollView.Add(hashtagLabel);
            scrollView.Add(hashtagText);

            scrollView.ContentSize = new CGSize(View.Frame.Width, 1000);

            View.AddSubview(scrollView);
        }

        [Export("TapProfilePictureSelector:")]
        public void TapProfilePictureSelector(UITapGestureRecognizer tapGesture)
        {
            Utils.OpenUserProfile(menuViewController.NavigationController, LoginController.userModel.id);
            NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("CloseSideMenu"), null);
        }
    }

    public class MenuTableViewController : UITableViewController
    {
        static NSString menuCellId = new NSString("MenuCell");

        public MenuTableViewController(UITableView menuTableView, List<MenuModel> menuItems, MenuViewController menuViewController)
        {
            menuTableView.RegisterClassForCellReuse(typeof(MenuTableViewCell), menuCellId);
            menuTableView.Source = new MenuTableViewDataSource(menuItems, menuViewController);
        }

        class MenuTableViewDataSource : UITableViewSource
        {
            List<MenuModel> menuItems;
            MenuViewController menuViewController;
            bool filterMine = false;
            bool filterForMe = false;
            bool filterFinished = false;

            public MenuTableViewDataSource(List<MenuModel> menuItems, MenuViewController menuViewController)
            {
                this.menuItems = menuItems;
                this.menuViewController = menuViewController;
            }

            public override nint RowsInSection(UITableView tableView, nint section)
            {
                return menuItems.Count;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(MenuTableViewController.menuCellId) as MenuTableViewCell;
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                cell.BackgroundColor = UIColor.Clear;
                cell.ContentView.BackgroundColor = UIColor.Clear;
                cell.SeparatorInset = new UIEdgeInsets(0.0f, -20, 0.0f, cell.Bounds.Size.Width);

                cell.menuImageView.Image = UIImage.FromBundle("assets/img/" + menuItems[indexPath.Row].ImageName);

                cell.menuTitleLabel.Text = menuItems[indexPath.Row].Title;

                if (menuItems[indexPath.Row].MenuItem == MenuItem.Feed)
                {
                    cell.menuTitleLabel.TextColor = UIColor.FromRGB(88, 185, 185);
                    tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);
                }

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath) as MenuTableViewCell;

                if (menuItems[indexPath.Row].MenuItem == MenuItem.Feed)
                {
                    cell.menuTitleLabel.TextColor = UIColor.FromRGB(88, 185, 185);
                    var menuController = menuViewController.Storyboard.InstantiateViewController("MenuNavController");
                    menuViewController.PresentViewController(menuController, true, null);
                }
                else if (menuItems[indexPath.Row].MenuItem == MenuItem.MyFriends)
                {
                    var friendsController = menuViewController.Storyboard.InstantiateViewController("FriendsController");
                    menuViewController.NavigationController.PushViewController(friendsController, true);
                    NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("CloseSideMenu"), null);
                }
                else if (menuItems[indexPath.Row].MenuItem == MenuItem.SearchFriends)
                {
                    var searchAllController = menuViewController.Storyboard.InstantiateViewController("SearchAllController");
                    menuViewController.NavigationController.PushViewController(searchAllController, true);
                    NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("CloseSideMenu"), null);
                }
                else if (menuItems[indexPath.Row].MenuItem == MenuItem.Logout)
                {
                    CredentialsService.DeleteCredentials();

                    var loginController = menuViewController.Storyboard.InstantiateViewController("LoginNavController");
                    if (loginController != null)
                    {
                        menuViewController.PresentViewController(loginController, true, null);
                    }
                }
                else if (menuItems[indexPath.Row].MenuItem == MenuItem.Mine)
                {
                    if (filterMine)
                    {
                        cell.menuTitleLabel.TextColor = UIColor.Black;
                        filterMine = false;
                    }
                    else
                    {
                        cell.menuTitleLabel.TextColor = UIColor.FromRGB(88, 185, 185);
                        filterMine = true;
                    }

                    var feedController = menuViewController.Storyboard.InstantiateViewController("FeedController") as FeedController;
                    feedController.filterMine = filterMine;
                    feedController.filterForMe = filterForMe;
                    feedController.filterFinished = filterFinished;
                    menuViewController.changeContentView(feedController);

                }
                else if (menuItems[indexPath.Row].MenuItem == MenuItem.ForMe)
                {
                    if (filterForMe)
                    {
                        cell.menuTitleLabel.TextColor = UIColor.Black;
                        filterForMe = false;
                    }
                    else
                    {
                        cell.menuTitleLabel.TextColor = UIColor.FromRGB(88, 185, 185);
                        filterForMe = true;
                    }

                    var feedController = menuViewController.Storyboard.InstantiateViewController("FeedController") as FeedController;
                    feedController.filterMine = filterMine;
                    feedController.filterForMe = filterForMe;
                    feedController.filterFinished = filterFinished;
                    menuViewController.changeContentView(feedController);
                }
                else if (menuItems[indexPath.Row].MenuItem == MenuItem.Finished)
                {
                    if (filterFinished)
                    {
                        cell.menuTitleLabel.TextColor = UIColor.Black;
                        filterFinished = false;
                    }
                    else
                    {
                        cell.menuTitleLabel.TextColor = UIColor.FromRGB(88, 185, 185);
                        filterFinished = true;
                    }

                    var feedController = menuViewController.Storyboard.InstantiateViewController("FeedController") as FeedController;
                    feedController.filterMine = filterMine;
                    feedController.filterForMe = filterForMe;
                    feedController.filterFinished = filterFinished;
                    menuViewController.changeContentView(feedController);
                }
            }

            //public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            //{
            //    var menuItem = menuItems[indexPath.Row].MenuItem;
            //    if (menuItem == MenuItem.Mine || menuItem == MenuItem.ToYou || menuItem == MenuItem.Public)
            //    {
            //        var cell = tableView.CellAt(indexPath) as MenuTableViewCell;

            //        if (cell == null)
            //        {
            //            cell = this.GetCell(tableView, indexPath) as MenuTableViewCell;
            //        }

            //        cell.menuTitleLabel.TextColor = UIColor.Black;
            //    }
            //}
        }
    }

    public partial class MenuTableViewCell : UITableViewCell
    {
        public UIImageView menuImageView { get; set; }
        public UILabel menuTitleLabel { get; set; }

        protected MenuTableViewCell(IntPtr handle) : base(handle)
        {
            menuImageView = new UIImageView();
            menuImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            menuImageView.Layer.MasksToBounds = true;
            menuImageView.TranslatesAutoresizingMaskIntoConstraints = false;

            menuTitleLabel = new UILabel();
            menuTitleLabel.Font = UIFont.SystemFontOfSize(14);
            menuTitleLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            ContentView.Add(menuImageView);
            ContentView.Add(menuTitleLabel);

            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-8-[v0(28)]-20-[v1]-8-|", new NSLayoutFormatOptions(), "v0", menuImageView, "v1", menuTitleLabel));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(28)]", new NSLayoutFormatOptions(), "v0", menuImageView));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-10-[v0(24)]", new NSLayoutFormatOptions(), "v0", menuTitleLabel));
        }
    }
}
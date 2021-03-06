﻿using Askker.App.iOS.Models;
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
        public MenuViewController menuViewController { get; set; }
        private NSObject updateProfilePictureObserver;
        private NSObject updateUserNameObserver;
        UIImageView profileImageView;
        UILabel userName;

        public SideMenuController(MenuViewController menuViewController)
        {
            this.menuViewController = menuViewController;
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            if (updateProfilePictureObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(updateProfilePictureObserver);
            }

            if (updateUserNameObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(updateUserNameObserver);
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            updateProfilePictureObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UpdateProfilePicture"), UpdateProfilePicture);
            updateUserNameObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UpdateUserName"), UpdateUserName);

            View.BackgroundColor = UIColor.FromRGB(33, 33, 33);

            var scrollView = new UIScrollView(new RectangleF(0, 0, (float)View.Frame.Width, (float)View.Frame.Height));

            profileImageView = new UIImageView(new RectangleF(85, 20, 90, 90));
            profileImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            profileImageView.Image = UIImage.FromBundle("Profile");
            profileImageView.Layer.CornerRadius = (profileImageView.Frame.Width / 2);
            profileImageView.Layer.MasksToBounds = true;

            var tapGestureRecognizer = new UITapGestureRecognizer(this, new ObjCRuntime.Selector("TapProfilePictureSelector:"));
            profileImageView.AddGestureRecognizer(tapGestureRecognizer);
            profileImageView.UserInteractionEnabled = true;

            if (LoginController.userModel.profilePicturePath != null)
            {
                Utils.SetImageFromNSUrlSession(LoginController.userModel.profilePicturePath, profileImageView, this, PictureType.Profile);
            }

            var editProfileButtonImageView = new UIImageView(new RectangleF(220, 20, 24, 24));
            editProfileButtonImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            editProfileButtonImageView.Image = UIImage.FromBundle("EditMenu");
            editProfileButtonImageView.Layer.MasksToBounds = true;
            var tapGestureRecognizerEdit = new UITapGestureRecognizer(this, new ObjCRuntime.Selector("TapProfilePictureSelector:"));
            editProfileButtonImageView.AddGestureRecognizer(tapGestureRecognizerEdit);
            editProfileButtonImageView.UserInteractionEnabled = true;

            userName = new UILabel(new RectangleF(20, 120, 220, 20));
            userName.Font = UIFont.SystemFontOfSize(14.0f);
            userName.TextAlignment = UITextAlignment.Center;
            userName.TextColor = UIColor.White;
            userName.Text = LoginController.userModel.name;

            var dividerLineView = new UIView(new RectangleF(20, 145, 220, 0.5f));
            dividerLineView.BackgroundColor = UIColor.FromRGB(80, 80, 80);

            var pagesItems = new MenuPagesModel().MenuItems;
            var pagesTableView = new UITableView(new RectangleF(20, 150, 220, (pagesItems.Count * 40) - 10));
            pagesTableView.ContentInset = new UIEdgeInsets(0, 20, 0, 0);
            pagesTableView.BackgroundColor = UIColor.Clear;
            pagesTableView.ScrollEnabled = false;
            new MenuTableViewController(pagesTableView, pagesItems, menuViewController);

            var dividerLineView2 = new UIView(new RectangleF(20, (float)pagesTableView.Frame.Y + (float)pagesTableView.Frame.Height + 15, 220, 0.5f));
            dividerLineView2.BackgroundColor = UIColor.FromRGB(80, 80, 80);

            var filterLabel = new UILabel(new RectangleF(20, (float)dividerLineView2.Frame.Y + 5, 220, 20));
            filterLabel.Font = UIFont.BoldSystemFontOfSize(12.0f);
            filterLabel.TextAlignment = UITextAlignment.Left;
            filterLabel.TextColor = UIColor.FromRGB(80, 80, 80);
            filterLabel.Text = "Filter";

            var filterTipLabel = new UILabel(new RectangleF(20, (float)dividerLineView2.Frame.Y + 5, 220, 20));
            filterTipLabel.Font = UIFont.BoldSystemFontOfSize(9.0f);
            filterTipLabel.TextAlignment = UITextAlignment.Right;
            filterTipLabel.TextColor = UIColor.FromRGB(196, 155, 9);
            filterTipLabel.Text = "You may select more than one";

            var filterItems = new MenuFilterModel().MenuItems;
            var filterTableView = new UITableView(new RectangleF(26, (float)filterLabel.Frame.Y + 15, 214, filterItems.Count * 40));
            filterTableView.ContentInset = new UIEdgeInsets(0, 14, 0, 0);
            filterTableView.SeparatorColor = UIColor.FromRGB(80, 80, 80);
            filterTableView.BackgroundColor = UIColor.Clear;
            filterTableView.ScrollEnabled = false;
            new MenuTableViewController(filterTableView, filterItems, menuViewController);

            var dividerLineView3 = new UIView(new RectangleF(20, (float)filterTableView.Frame.Y + (float)filterTableView.Frame.Height + 15, 220, 0.5f));
            dividerLineView3.BackgroundColor = UIColor.FromRGB(80, 80, 80);

            #region Hashtag Menu
            //var dividerLineView3 = new UIView(new RectangleF(20, (float)filterTableView.Frame.Y + (float)filterTableView.Frame.Height + 25, 220, 0.5f));
            //dividerLineView3.BackgroundColor = UIColor.Black;

            //var hashtagLabel = new UILabel(new RectangleF(20, (float)dividerLineView3.Frame.Y + 5, 220, 20));
            //hashtagLabel.Font = UIFont.BoldSystemFontOfSize(12.0f);
            //hashtagLabel.TextAlignment = UITextAlignment.Left;
            //hashtagLabel.TextColor = UIColor.Black;
            //hashtagLabel.Text = "Hashtag";

            //var hashtagText = new UITextField(new RectangleF(20, (float)hashtagLabel.Frame.Y + 30, 220, 40));
            //hashtagText.BorderStyle = UITextBorderStyle.Bezel;
            //hashtagText.Placeholder = "inserts tags to filter here";
            #endregion

            #region Logout Button
            var logoutModel = new MenuLogoutModel();

            var logoutButton = new UIButton(new RectangleF(0, (float)dividerLineView3.Frame.Y + 5, (float)View.Frame.Width, 40));
            //logoutButton.BackgroundColor = UIColor.FromRGB(50, 50, 50);
            logoutButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                CredentialsService.DeleteCredentials();
                LoginController.tokenModel = null;
                LoginController.userModel = null;

                var loginController = menuViewController.Storyboard.InstantiateViewController("LoginNavController");
                if (loginController != null)
                {
                    menuViewController.PresentViewController(loginController, true, null);
                }
            };
            

            var logoutIcoImageView = new UIImageView();
            logoutIcoImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            logoutIcoImageView.Layer.MasksToBounds = true;
            logoutIcoImageView.TranslatesAutoresizingMaskIntoConstraints = false;
            logoutIcoImageView.Image = UIImage.FromBundle(logoutModel.ImageName);

            var logoutTitleLabel = new UILabel();
            logoutTitleLabel.Font = UIFont.SystemFontOfSize(14);
            logoutTitleLabel.Text = logoutModel.Title;
            logoutTitleLabel.TextColor = UIColor.White;
            logoutTitleLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            logoutButton.Add(logoutIcoImageView);
            logoutButton.Add(logoutTitleLabel);

            logoutButton.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-48-[v0(24)]-20-[v1]-8-|", new NSLayoutFormatOptions(), "v0", logoutIcoImageView, "v1", logoutTitleLabel));
            logoutButton.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(24)]", new NSLayoutFormatOptions(), "v0", logoutIcoImageView));
            logoutButton.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-10-[v0(24)]", new NSLayoutFormatOptions(), "v0", logoutTitleLabel));
            #endregion

            scrollView.AddSubview(profileImageView);
            scrollView.AddSubview(userName);
            scrollView.AddSubview(editProfileButtonImageView);
            scrollView.AddSubview(dividerLineView);
            scrollView.AddSubview(pagesTableView);
            scrollView.AddSubview(dividerLineView2);
            scrollView.AddSubview(filterLabel);
            scrollView.AddSubview(filterTipLabel);
            scrollView.AddSubview(filterTableView);
            scrollView.AddSubview(dividerLineView3);
            #region Hashtag Menu
            //scrollView.Add(dividerLineView3);
            //scrollView.Add(hashtagLabel);
            //scrollView.Add(hashtagText);
            #endregion
            scrollView.AddSubview(logoutButton);

            var scrollHeight = 20 + profileImageView.Frame.Height + 10 + userName.Frame.Height + 25 + dividerLineView.Frame.Height + 5 + pagesTableView.Frame.Height + 15 +
                               dividerLineView2.Frame.Height + 5 + filterLabel.Frame.Height + 10 + filterTableView.Frame.Height + 5 + dividerLineView3.Frame.Height + 5 +
                               logoutButton.Frame.Height + 40;
            
            if (scrollHeight > View.Frame.Height)
            {
                scrollView.ContentSize = new CGSize(View.Frame.Width, scrollHeight);
            }
            else
            {
                scrollView.ContentSize = new CGSize(View.Frame.Width, View.Frame.Height);
            }

            View.AddSubview(scrollView);
        }

        [Export("TapProfilePictureSelector:")]
        public void TapProfilePictureSelector(UITapGestureRecognizer tapGesture)
        {
            Utils.OpenUserProfile(menuViewController.NavigationController, LoginController.userModel.id);
            NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("CloseSideMenu"), null);
        }

        private void UpdateProfilePicture(NSNotification notification)
        {
            if (LoginController.userModel.profilePicturePath != null)
            {
                Utils.SetImageFromNSUrlSession(LoginController.userModel.profilePicturePath, profileImageView, this, PictureType.Profile);
            }
        }

        private void UpdateUserName(NSNotification notification)
        {
            if (LoginController.userModel.name != null)
            {
                userName.Text = LoginController.userModel.name;
            }
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

                cell.menuImageView.Image = UIImage.FromBundle(menuItems[indexPath.Row].ImageName);
                
                cell.menuTitleLabel.Text = menuItems[indexPath.Row].Title;
                cell.menuTitleLabel.TextColor = UIColor.White;

                if (menuItems[indexPath.Row].MenuItem == MenuItem.Mine || menuItems[indexPath.Row].MenuItem == MenuItem.ForMe)
                {
                    cell.SeparatorInset = new UIEdgeInsets(0, 8, 0, 0);
                    cell.menuCheckImageView.Image = UIImage.FromBundle("EmptyCircleMenu");
                }
                else if (menuItems[indexPath.Row].MenuItem == MenuItem.Finished)
                {
                    cell.menuCheckImageView.Image = UIImage.FromBundle("EmptyCircleMenu");
                }

                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return 40;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath) as MenuTableViewCell;

                if (menuItems[indexPath.Row].MenuItem == MenuItem.MyFriends)
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
                else if (menuItems[indexPath.Row].MenuItem == MenuItem.MyGroups)
                {
                    var groupsController = menuViewController.Storyboard.InstantiateViewController("GroupsController");
                    menuViewController.NavigationController.PushViewController(groupsController, true);
                    NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("CloseSideMenu"), null);
                }
                else if (menuItems[indexPath.Row].MenuItem == MenuItem.SearchGroups)
                {
                    var searchUserGroupsController = menuViewController.Storyboard.InstantiateViewController("SearchUserGroupsController");
                    menuViewController.NavigationController.PushViewController(searchUserGroupsController, true);
                    NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("CloseSideMenu"), null);
                }
                else if (menuItems[indexPath.Row].MenuItem == MenuItem.Mine)
                {
                    if (filterMine)
                    {
                        cell.menuCheckImageView.Image = UIImage.FromBundle("EmptyCircleMenu");
                        filterMine = false;
                    }
                    else
                    {
                        cell.menuCheckImageView.Image = UIImage.FromBundle("OptionCheck");
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
                        cell.menuCheckImageView.Image = UIImage.FromBundle("EmptyCircleMenu");
                        filterForMe = false;
                    }
                    else
                    {
                        cell.menuCheckImageView.Image = UIImage.FromBundle("OptionCheck");
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
                        cell.menuCheckImageView.Image = UIImage.FromBundle("EmptyCircleMenu");
                        filterFinished = false;
                    }
                    else
                    {
                        cell.menuCheckImageView.Image = UIImage.FromBundle("OptionCheck");
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
        public UIImageView menuCheckImageView { get; set; }

        protected MenuTableViewCell(IntPtr handle) : base(handle)
        {
            menuImageView = new UIImageView();
            menuImageView.Frame = new CGRect(0, 0, 24, 24);
            menuImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            menuImageView.Layer.MasksToBounds = true;
            menuImageView.TranslatesAutoresizingMaskIntoConstraints = false;

            menuTitleLabel = new UILabel();
            menuTitleLabel.Font = UIFont.SystemFontOfSize(14);
            menuTitleLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            menuCheckImageView = new UIImageView(); ;
            menuCheckImageView.Frame = new CGRect(0, 0, 24, 24);
            this.AccessoryView = menuCheckImageView;

            ContentView.Add(menuImageView);
            ContentView.Add(menuTitleLabel);
            ContentView.Add(menuCheckImageView);
            
            AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-8-[v0(24)]-20-[v1]-8-|", new NSLayoutFormatOptions(), "v0", menuImageView, "v1", menuTitleLabel));
            //AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[v0]-0-|", new NSLayoutFormatOptions(), "v0", menuCheckImageView));

            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(24)]", new NSLayoutFormatOptions(), "v0", menuImageView));
            AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-10-[v0(24)]", new NSLayoutFormatOptions(), "v0", menuTitleLabel));
            //AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-8-[v0(28)]", new NSLayoutFormatOptions(), "v0", menuCheckImageView));
        }
    }
}
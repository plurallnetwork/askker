using Askker.App.iOS.Models;
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
            profileImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            profileImageView.Image = UIImage.FromBundle("Profile");
            profileImageView.Layer.CornerRadius = (profileImageView.Frame.Width / 2);
            profileImageView.Layer.MasksToBounds = true;

            if (LoginController.userModel.profilePicturePath != null)
            {
                var url = new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + LoginController.userModel.profilePicturePath);

                var task = NSUrlSession.SharedSession.CreateDataTask(url, (data, response, error) =>
                {
                    if (response == null)
                    {
                        profileImageView.Image = UIImage.FromBundle("Profile");
                    }
                    else
                    {
                        try
                        {
                            DispatchQueue.MainQueue.DispatchAsync(() => {
                                profileImageView.Image = UIImage.LoadFromData(data);
                            });
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                });
                task.Resume();
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
    }

    public class MenuTableViewController : UITableViewController
    {
        static NSString menuCellId = new NSString("MenuCell");

        public MenuTableViewController(UITableView menuTableView, List<MenuModel> menuItems, MenuViewController menuViewController)
        {
            menuTableView.RegisterClassForCellReuse(typeof(UITableViewCell), menuCellId);
            menuTableView.Source = new MenuTableViewDataSource(menuItems, menuViewController);
        }

        class MenuTableViewDataSource : UITableViewSource
        {
            List<MenuModel> menuItems;
            MenuViewController menuViewController;

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
                var cell = tableView.DequeueReusableCell(MenuTableViewController.menuCellId);
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                cell.BackgroundColor = UIColor.Clear;
                cell.ContentView.BackgroundColor = UIColor.Clear;
                cell.SeparatorInset = new UIEdgeInsets(0.0f, -20, 0.0f, cell.Bounds.Size.Width);

                cell.ImageView.Image = Common.ResizeImage(UIImage.FromBundle("assets/img/" + menuItems[indexPath.Row].ImageName), 30, 23);

                cell.TextLabel.Text = menuItems[indexPath.Row].Title;
                cell.TextLabel.Font = UIFont.SystemFontOfSize(14);

                if (menuItems[indexPath.Row].Title.Equals("Feed") || menuItems[indexPath.Row].Title.Equals("Public"))
                {
                    cell.TextLabel.TextColor = UIColor.FromRGB(88, 185, 185);
                    tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);
                }

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);

                cell.TextLabel.TextColor = UIColor.FromRGB(88, 185, 185);

                if (menuItems[indexPath.Row].Title.Equals("Feed"))
                {
                    var homeNavController = menuViewController.Storyboard.InstantiateViewController("HomeNavController");
                    menuViewController.changeContentView(homeNavController);
                }
                else if (menuItems[indexPath.Row].Title.Equals("Log out"))
                {
                    CredentialsService.DeleteCredentials();

                    var loginController = menuViewController.Storyboard.InstantiateViewController("LoginController");
                    if (loginController != null)
                    {
                        menuViewController.PresentViewController(loginController, true, null);
                    }
                }
                else if (menuItems[indexPath.Row].Title.Equals("Public"))
                {
                    var feedController = menuViewController.Storyboard.InstantiateViewController("FeedController") as FeedController;
                    feedController.filter = "nofilter";
                    menuViewController.changeContentView(feedController);
                }
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);

                if (cell == null)
                {
                    cell = this.GetCell(tableView, indexPath);
                }

                cell.TextLabel.TextColor = UIColor.Black;
            }
        }
    }
}
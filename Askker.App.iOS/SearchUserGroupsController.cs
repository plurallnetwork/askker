using Askker.App.iOS.TableControllers;
using Askker.App.PortableLibrary.Enums;
using CoreGraphics;
using System;
using System.Collections.Generic;
using UIKit;

namespace Askker.App.iOS
{
    public partial class SearchUserGroupsController : CustomUIViewController
    {
        public SearchUserGroupsController(IntPtr handle) : base(handle)
        {            
        }

        public SearchProfileTableViewController tableController { get; set; }
        public UISearchBar searchBar { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);
            // Perform any additional setup after loading the view, typically from a nib.

            //Declare the search bar and add it to the header of the table
            searchBar = new UISearchBar();
            searchBar.SizeToFit();
            searchBar.AutocorrectionType = UITextAutocorrectionType.No;
            searchBar.AutocapitalizationType = UITextAutocapitalizationType.None;
            searchBar.Placeholder = "Type at least 3 characters";
            searchBar.OnEditingStarted += (sender, e) =>
            {
                searchBar.ShowsCancelButton = true;
            };
            searchBar.OnEditingStopped += (sender, e) =>
            {
                searchBar.ShowsCancelButton = false;
            };
            searchBar.CancelButtonClicked += (sender, e) =>
            {
                cleanTable();
                searchBar.ShowsCancelButton = false;
                searchBar.Text = "";
                searchBar.ResignFirstResponder();
            };
            searchBar.TextChanged += (sender, e) =>
            {
                //this is the method that is called when the user searches
                searchTable();
            };

            foreach (UIView subView in searchBar.Subviews)
            {
                foreach (UIView secondLevelSubview in subView.Subviews)
                {
                    if (secondLevelSubview is UITextField)
                    {
                        UITextField searchBarTextField = (UITextField)secondLevelSubview;

                        //set font color here
                        searchBarTextField.TextColor = UIColor.FromRGB(90, 89, 89);
                        break;
                    }
                }
            }

            tableController = new SearchProfileTableViewController(SearchProfileType.Groups, NavigationController);
            tableController.TableView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height - 20);
            tableController.TableView.TableHeaderView = searchBar;
            Add(tableController.TableView);

            tableController.TableView.ReloadData();
        }

        private void cleanTable()
        {
            tableController.tableItems = new List<SearchProfileTableItem>();
            tableController.TableView.ReloadData();
        }

        private void searchTable()
        {
            //perform the search, and refresh the table with the results
            if (searchBar.Text.Length >= 3)
            {
                tableController.PerformSearch(searchBar.Text);
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
using Askker.App.iOS.TableControllers;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace Askker.App.iOS
{
    public partial class SearchAllController : CustomUIViewController
    {
        public SearchAllController (IntPtr handle) : base (handle)
        {
        }

        public static UITableView table { get; set; }
        SearchAllTableSource tableSource;
        List<SearchAllTableItem> tableItems;
        UISearchBar searchBar;

        public override async void ViewDidLoad()
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

            table = new UITableView(new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height - 20));
            //table.AutoresizingMask = UIViewAutoresizing.All;
            tableItems = new List<SearchAllTableItem>();
            
            tableSource = new SearchAllTableSource(tableItems, this.NavigationController);
            table.Source = tableSource;
            table.TableHeaderView = searchBar;
            Add(table);
        }

        private void cleanTable()
        {
            tableItems = new List<SearchAllTableItem>();

            table.Source = new SearchAllTableSource(tableItems, this.NavigationController);
            table.ReloadData();
        }

        private void searchTable()
        {
            //perform the search, and refresh the table with the results
            if(searchBar.Text.Length >= 3)
            {
                tableSource.PerformSearch(searchBar.Text);
                table.ReloadData();
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
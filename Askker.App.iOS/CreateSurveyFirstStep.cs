using Askker.App.iOS.HorizontalSwipe;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
using AssetsLibrary;
using BigTed;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UIKit;

namespace Askker.App.iOS
{
    public partial class CreateSurveyFirstStep : CustomUIViewController, IMultiStepProcessStep, IUITextFieldDelegate
    {
        public int StepIndex { get; set; }

        public event EventHandler<MultiStepProcessStepEventArgs> StepActivated;
        public event EventHandler<MultiStepProcessStepEventArgs> StepDeactivated;
        private NSObject addNewRowObserver;
        private NSObject removeRowObserver;
        private NSObject updateRowObserver;
        private NSObject updateCellObserver;
        private NSObject updateTextCellObserver;
        private NSObject deleteCellObserver;

        static NSString imageCellId = new NSString("ImageCellId");

        private UINavigationController navigationController;


        static List<TextOptionTableItem> tableItems;
        static List<ImageOptionTableItem> collectionViewItems;

        UITextField questionText;
        UIButton checkButton;
        UIView questionSeparator;
        UILabel typeLabel;
        UIButton textBtn;
        UIButton imageBtn;
        UITableView textTableView;
        UICollectionView imageCollectionView;

        TextOptionSource tableSource;
        ImageOptionSource collectionViewSource;

        PBCollectionViewDelegateWaterfallLayout collectionViewDelegate;
        PBCollectionViewWaterfallLayout collectionViewLayout;

        private List<float> cellHeights;
        private float cellWidth;

        //Variables used when the keyboard appears
        private UIView activeview;              // Controller that activated the keyboard
        private float scroll_amount = 0.0f;     // amount to scroll 
        private float bottom = 0.0f;            // bottom point
        private float offset = 8.0f;            // extra offset
        private bool moveViewUp = false;        // which direction are we moving
                
        public CreateSurveyFirstStep(UINavigationController navigationController) : base()
        {
            this.navigationController = navigationController;
        }

        public static List<TextOptionTableItem> GetTextItems()
        {
            return tableItems;
        }

        public static List<ImageOptionTableItem> GetImageItems()
        {
            return collectionViewItems;
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            if (addNewRowObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(addNewRowObserver);
            }

            if (removeRowObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(removeRowObserver);
            }

            if (updateRowObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(updateRowObserver);
            }

            if (updateCellObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(updateCellObserver);
            }

            if (deleteCellObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(deleteCellObserver);
            }

            if (updateTextCellObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(updateTextCellObserver);
            }
        }

        public override void LoadView()
        {
            View = new UIView();
            View.BackgroundColor = UIColor.White;
            
            questionText = new UITextField();
            questionText.Placeholder = "Type your question here";
            questionText.TextColor = UIColor.FromRGB(90, 89, 89);
            questionText.Delegate = this;

            checkButton = new UIButton();
            checkButton.AddTarget(Self, new ObjCRuntime.Selector("CheckButtonClick:"), UIControlEvent.TouchUpInside);
            checkButton.SetImage(UIImage.FromBundle("CheckProfile"), UIControlState.Normal);
            checkButton.Hidden = true;

            questionSeparator = new UIView();
            questionSeparator.BackgroundColor = UIColor.FromRGB(90, 89, 89);

            typeLabel = new UILabel();
            typeLabel.Text = "Choose the type:";
            typeLabel.TextColor = UIColor.FromRGB(90, 89, 89);

            textBtn = new UIButton();
            textBtn.SetImage(UIImage.FromBundle("TextSurveyActive"), UIControlState.Normal);
            textBtn.AddTarget(Self, new ObjCRuntime.Selector("TextButtonBtn:"), UIControlEvent.TouchUpInside);

            imageBtn = new UIButton();
            imageBtn.SetImage(UIImage.FromBundle("ImageSurveyInactive"), UIControlState.Normal);
            imageBtn.AddTarget(Self, new ObjCRuntime.Selector("ImageButtonBtn:"), UIControlEvent.TouchUpInside);

            textTableView = new UITableView();

            imageCollectionView = new UICollectionView(new CGRect(), new PBCollectionViewWaterfallLayout());

            View.Add(questionText);
            View.Add(checkButton);
            View.Add(questionSeparator);
            View.Add(typeLabel);
            View.Add(textBtn);
            View.Add(imageBtn);
            View.Add(textTableView);
            View.Add(imageCollectionView);

            View.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            View.AddConstraints(

                questionText.WithSameCenterX(View),
                questionText.AtTopOf(View, 70),
                questionText.AtLeftOf(View, Constants.padding),
                questionText.AtRightOf(View, 40),
                questionText.Height().EqualTo(30),

                checkButton.AtTopOf(View, 70),
                checkButton.Width().EqualTo(30),
                checkButton.WithSameHeight(questionText),
                checkButton.Left().EqualTo().RightOf(questionText),

                questionSeparator.WithSameCenterX(View),
                questionSeparator.Below(questionText, Constants.padding /2),
                questionSeparator.AtLeftOf(View, Constants.padding),
                questionSeparator.AtRightOf(View, Constants.padding),
                questionSeparator.Height().EqualTo(1),

                typeLabel.Below(questionSeparator, Constants.padding),
                typeLabel.AtLeftOf(View, Constants.padding),

                textBtn.Below(typeLabel, Constants.padding),
                textBtn.AtLeftOf(View, Constants.padding),
                textBtn.Width().EqualTo(Constants.cellWidth),
                textBtn.Height().EqualTo(40),

                imageBtn.Below(typeLabel, Constants.padding),
                imageBtn.WithSameWidth(textBtn),
                imageBtn.Left().EqualTo().RightOf(textBtn).Plus(Constants.padding),
                imageBtn.WithSameHeight(textBtn),

                textTableView.Below(textBtn, Constants.padding),
                textTableView.WithSameWidth(View),
                textTableView.AtBottomOf(View),
                textTableView.AtLeftOf(View),
                textTableView.AtRightOf(View),

                imageCollectionView.Below(textBtn, Constants.padding),
                imageCollectionView.WithSameWidth(View),
                imageCollectionView.AtBottomOf(View),
                imageCollectionView.AtLeftOf(View),
                imageCollectionView.AtRightOf(View)
            );
        }

        [Export("textFieldDidBeginEditing:")]
        public void EditingStarted(UITextField textField)
        {
            checkButton.Hidden = false;
        }

        [Export("CheckButtonClick:")]
        private void CheckButtonClick(UIButton button)
        {
            checkButton.Hidden = true;
            View.EndEditing(true);
        }

        [Export("TextButtonBtn:")]
        private async void TextButtonBtn(UIButton button)
        {
            if (CreateSurveyController.SurveyModel.type.Equals(SurveyType.Image.ToString()))
            {
                if (collectionViewItems.Where(x => x.Type.Equals(OptionType.Option) && !string.IsNullOrEmpty(x.Text.Trim())).ToList().Count() >= 2)
                {
                    nint alertBtn = await Utils.ShowAlert("Options", "All image options will be deleted. Continue?", "Ok", "Cancel");

                    if (alertBtn == 1)
                    {
                        return;
                    }
                }

                CreateSurveyController.SurveyModel.type = SurveyType.Text.ToString();

                textTableView.Hidden = false;
                imageCollectionView.Hidden = true;

                tableItems = new List<TextOptionTableItem>();
                tableItems.Add(new TextOptionTableItem("", OptionType.Option));
                tableItems.Add(new TextOptionTableItem("", OptionType.Option));
                tableItems.Add(new TextOptionTableItem("Add new option -->", OptionType.Insert));
                tableSource = new TextOptionSource(tableItems, this);
                textTableView.Source = tableSource;
                textTableView.ReloadData();

                textBtn.SetImage(UIImage.FromBundle("TextSurveyActive"), UIControlState.Normal);
                imageBtn.SetImage(UIImage.FromBundle("ImageSurveyInactive"), UIControlState.Normal);

                CreateSurveyController._nextButton.SetTitleColor(UIColor.FromRGB(220, 220, 220), UIControlState.Normal);
                CreateSurveyController._nextButton.BackgroundColor = UIColor.White;
            }
        }

        [Export("ImageButtonBtn:")]
        private async void ImageButtonBtn(UIButton button)
        {
            if (CreateSurveyController.SurveyModel.type.Equals(SurveyType.Text.ToString()))
            {
                if (tableItems.Where(x => x.Type.Equals(OptionType.Option) && !string.IsNullOrEmpty(x.Text.Trim())).ToList().Count() >= 2)
                {
                    nint alertBtn = await Utils.ShowAlert("Options", "All text options will be deleted. Continue?", "Ok", "Cancel");

                    if (alertBtn == 1)
                    {
                        return;
                    }
                }

                CreateSurveyController.SurveyModel.type = SurveyType.Image.ToString();

                textTableView.Hidden = true;
                imageCollectionView.Hidden = false;

                collectionViewItems = new List<ImageOptionTableItem>();
                collectionViewItems.Add(new ImageOptionTableItem(UIImage.FromBundle("AddImage"), null, "", OptionType.Insert));
                collectionViewSource = new ImageOptionSource(collectionViewItems, this.navigationController);
                imageCollectionView.Source = collectionViewSource;
                imageCollectionView.ReloadData();

                imageCollectionView.BackgroundColor = UIColor.White;
                imageCollectionView.RegisterClassForCell(typeof(ImageOptionCustomCell), imageCellId);
                SetupLayout();
                imageCollectionView.CollectionViewLayout = collectionViewLayout;

                cellWidth = Constants.cellWidth;
                UpdateLayout();

                textBtn.SetImage(UIImage.FromBundle("TextSurveyInactive"), UIControlState.Normal);
                imageBtn.SetImage(UIImage.FromBundle("ImageSurveyActive"), UIControlState.Normal);

                CreateSurveyController._nextButton.SetTitleColor(UIColor.FromRGB(220, 220, 220), UIControlState.Normal);
                CreateSurveyController._nextButton.BackgroundColor = UIColor.White;
            }
        }

        private void AddNewRow(NSNotification notification)
        {
            tableItems.Insert(tableItems.Count - 1, new TextOptionTableItem("", OptionType.Option));
            tableSource = new TextOptionSource(tableItems, this);
            textTableView.Source = tableSource;
            textTableView.ReloadData();

            UpdateNextButton();
        }

        private void RemoveRow(NSNotification notification)
        {
            var index = Int32.Parse(notification.Object.ToString());
            tableItems.RemoveAt(index);
            tableSource = new TextOptionSource(tableItems, this);
            textTableView.Source = tableSource;
            textTableView.ReloadData();

            UpdateNextButton();
        }

        private void UpdateRow(NSNotification notification)
        {
            var dict = notification.Object as NSDictionary;
            var index = Int32.Parse(dict[new NSString("index")].ToString());
            var text = dict[new NSString("value")].ToString();
            //tableItems.RemoveAt(index);
            //tableItems.Insert(index, new TextOptionTableItem(text, OptionType.Option));
            tableItems.ElementAt(index).Text = text;
            tableItems.ElementAt(index).Type = OptionType.Option;
            tableSource = new TextOptionSource(tableItems, this);
            textTableView.Source = tableSource;
            textTableView.ReloadData();

            UpdateNextButton();
        }

        private void UpdateCell(NSNotification notification)
        {
            var dict = notification.Object as NSDictionary;
            var index = Int32.Parse(dict[new NSString("index")].ToString());
            var image = dict[new NSString("image")] as NSData;
            byte[] myByteArray = new byte[image.Length];
            System.Runtime.InteropServices.Marshal.Copy(image.Bytes, myByteArray, 0, Convert.ToInt32(image.Length));

            //collectionViewItems.RemoveAt(index);
            //collectionViewItems.Insert(index, new ImageOptionTableItem(UIImage.LoadFromData(NSData.FromArray(myByteArray)), myByteArray, "", OptionType.Option));
            collectionViewItems.ElementAt(index).Image = UIImage.LoadFromData(NSData.FromArray(myByteArray));
            collectionViewItems.ElementAt(index).ImageArray = myByteArray;
            collectionViewItems.ElementAt(index).Text = "";
            collectionViewItems.ElementAt(index).Type = OptionType.Option;
            if (collectionViewItems.Where(x => x.Type.Equals(OptionType.Insert)).ToList().Count() <= 0)
            {
                collectionViewItems.Add(new ImageOptionTableItem(UIImage.FromBundle("AddImage"), null, "", OptionType.Insert));
            }
            collectionViewSource = new ImageOptionSource(collectionViewItems, this.navigationController);
            imageCollectionView.Source = collectionViewSource;
            imageCollectionView.ReloadData();

            CalculateCellHeights();
            var layout = imageCollectionView.CollectionViewLayout as PBCollectionViewWaterfallLayout;
            var vdelegate = layout.Delegate as ImageOptionDelegate;
            vdelegate.cellHeights = cellHeights;
            layout.UpdateLayout();

            UpdateNextButton();
        }

        private void UpdateTextCell(NSNotification notification)
        {
            var dict = notification.Object as NSDictionary;
            var index = Int32.Parse(dict[new NSString("index")].ToString());
            var text = dict[new NSString("value")].ToString();

            collectionViewItems.ElementAt(index).Text = text;
            collectionViewSource = new ImageOptionSource(collectionViewItems, this.navigationController);
            imageCollectionView.Source = collectionViewSource;
            imageCollectionView.ReloadData();

            CalculateCellHeights();
            var layout = imageCollectionView.CollectionViewLayout as PBCollectionViewWaterfallLayout;
            var vdelegate = layout.Delegate as ImageOptionDelegate;
            vdelegate.cellHeights = cellHeights;
            layout.UpdateLayout();

            UpdateNextButton();
        }

        private void DeleteCell(NSNotification notification)
        {
            var index = Int32.Parse(notification.Object.ToString());
            collectionViewItems.RemoveAt(index);
            collectionViewSource = new ImageOptionSource(collectionViewItems, this.navigationController);
            imageCollectionView.Source = collectionViewSource;
            imageCollectionView.ReloadData();

            CalculateCellHeights();
            var layout = imageCollectionView.CollectionViewLayout as PBCollectionViewWaterfallLayout;
            var vdelegate = layout.Delegate as ImageOptionDelegate;
            vdelegate.cellHeights = cellHeights;
            layout.UpdateLayout();

            UpdateNextButton();
        }

        private void UpdateNextButton()
        {
            if (CreateSurveyController._nextButton != null)
            {
                if (!string.IsNullOrWhiteSpace(questionText.Text.Trim()))
                {
                    if (CreateSurveyController.SurveyModel.type.Equals(SurveyType.Text.ToString()))
                    {
                        if (tableItems.Where(x => x.Type.Equals(OptionType.Option) && !string.IsNullOrEmpty(x.Text.Trim())).ToList().Count() >= 2)
                        {
                            CreateSurveyController._nextButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                            CreateSurveyController._nextButton.BackgroundColor = UIColor.FromRGB(70, 230, 130);
                        }
                        else
                        {
                            CreateSurveyController._nextButton.SetTitleColor(UIColor.FromRGB(220, 220, 220), UIControlState.Normal);
                            CreateSurveyController._nextButton.BackgroundColor = UIColor.White;
                        }
                    }
                    else
                    {
                        if (collectionViewItems.Where(x => x.Type.Equals(OptionType.Option) && !string.IsNullOrEmpty(x.Text.Trim())).ToList().Count() >= 2)
                        {
                            CreateSurveyController._nextButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                            CreateSurveyController._nextButton.BackgroundColor = UIColor.FromRGB(70, 230, 130);
                        }
                        else
                        {
                            CreateSurveyController._nextButton.SetTitleColor(UIColor.FromRGB(220, 220, 220), UIControlState.Normal);
                            CreateSurveyController._nextButton.BackgroundColor = UIColor.White;
                        }
                    }
                }
                else
                {
                    CreateSurveyController._nextButton.SetTitleColor(UIColor.FromRGB(220, 220, 220), UIControlState.Normal);
                    CreateSurveyController._nextButton.BackgroundColor = UIColor.White;
                }
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);

            NSNotificationCenter.DefaultCenter.AddObserver(UITextField.TextFieldTextDidChangeNotification, TextChangedEvent);
            addNewRowObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("AddNewRow"), AddNewRow);
            removeRowObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("RemoveRow"), RemoveRow);
            updateRowObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UpdateRow"), UpdateRow);
            updateCellObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UpdateCell"), UpdateCell);
            deleteCellObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("DeleteCell"), DeleteCell);
            updateTextCellObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UpdateTextCell"), UpdateTextCell);

            tableItems = new List<TextOptionTableItem>();
            collectionViewItems = new List<ImageOptionTableItem>();

            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);

            if (CreateSurveyController.ScreenState == ScreenState.Create.ToString())
            {
                textTableView.Hidden = false;
                imageCollectionView.Hidden = true;

                tableItems = new List<TextOptionTableItem>();
                tableItems.Add(new TextOptionTableItem("", OptionType.Option));
                tableItems.Add(new TextOptionTableItem("", OptionType.Option));
                tableItems.Add(new TextOptionTableItem("Add new option -->", OptionType.Insert));
                tableSource = new TextOptionSource(tableItems, this);
                textTableView.Source = tableSource;
                textTableView.ReloadData();

                if (CreateSurveyController.SurveyModel == null)
                {
                    CreateSurveyController.SurveyModel = new SurveyModel();
                }

                if (CreateSurveyController.SurveyModel.question == null)
                {
                    CreateSurveyController.SurveyModel.question = new Question();
                }

                CreateSurveyController.SurveyModel.type = SurveyType.Text.ToString();

                textBtn.SetImage(UIImage.FromBundle("TextSurveyActive"), UIControlState.Normal);
                imageBtn.SetImage(UIImage.FromBundle("ImageSurveyInactive"), UIControlState.Normal);
            }
            else
            {
                try
                {
                    if (CreateSurveyController.SurveyModel.type == SurveyType.Image.ToString())
                    {
                        foreach (var option in CreateSurveyController.SurveyModel.options)
                        {
                            UIImage image = Utils.GetImageFromNSUrl(option.image);
                            if (image != null)
                            {
                                image.Dispose();
                            }
                        }
                    }
                    questionText.Text = CreateSurveyController.SurveyModel.question.text;

                    //questionText.BecomeFirstResponder();
                }
                catch (Exception ex)
                {
                    BTProgressHUD.Dismiss();
                    Utils.HandleException(ex);
                }

                foreach (var option in CreateSurveyController.SurveyModel.options)
                {
                    if (CreateSurveyController.SurveyModel.type == SurveyType.Text.ToString())
                    {
                        tableItems.Add(new TextOptionTableItem(option.text, OptionType.Option));
                    }
                    else
                    {
                        //UIImageView imageView = new UIImageView();
                        //Utils.SetImageFromNSUrlSession(option.image, imageView, this, PictureType.OptionImage);
                        UIImage image = Utils.GetImageFromNSUrl(option.image);



                        try
                        {
                            using (NSData imageData = image.AsJPEG())
                            {
                                byte[] myByteArray = new byte[imageData.Length];
                                System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, myByteArray, 0, Convert.ToInt32(imageData.Length));
                                collectionViewItems.Add(new ImageOptionTableItem(image, myByteArray, option.text, OptionType.Option));
                            }

                        }
                        catch (Exception ex)
                        {
                            BTProgressHUD.Dismiss();
                            Utils.HandleException(ex);
                        }
                    }
                }

                if (CreateSurveyController.SurveyModel.type == SurveyType.Text.ToString())
                {
                    textTableView.Hidden = false;
                    imageCollectionView.Hidden = true;

                    tableItems.Add(new TextOptionTableItem("Add new option -->", OptionType.Insert));
                    tableSource = new TextOptionSource(tableItems, this);
                    textTableView.Source = tableSource;
                    textTableView.ReloadData();

                    textBtn.SetImage(UIImage.FromBundle("TextSurveyActive"), UIControlState.Normal);
                    imageBtn.SetImage(UIImage.FromBundle("ImageSurveyInactive"), UIControlState.Normal);
                }
                else
                {
                    textTableView.Hidden = true;
                    imageCollectionView.Hidden = false;

                    if (collectionViewItems.Where(x => x.Type.Equals(OptionType.Insert)).ToList().Count() <= 0)
                    {
                        collectionViewItems.Add(new ImageOptionTableItem(UIImage.FromBundle("AddImage"), null, "", OptionType.Insert));
                    }
                    collectionViewSource = new ImageOptionSource(collectionViewItems, this.navigationController);
                    imageCollectionView.Source = collectionViewSource;
                    imageCollectionView.ReloadData();

                    imageCollectionView.BackgroundColor = UIColor.White;
                    imageCollectionView.RegisterClassForCell(typeof(ImageOptionCustomCell), imageCellId);
                    SetupLayout();
                    imageCollectionView.CollectionViewLayout = collectionViewLayout;

                    cellWidth = Constants.cellWidth;
                    UpdateLayout();

                    CalculateCellHeights();
                    var layout = imageCollectionView.CollectionViewLayout as PBCollectionViewWaterfallLayout;
                    var vdelegate = layout.Delegate as ImageOptionDelegate;
                    vdelegate.cellHeights = cellHeights;
                    layout.UpdateLayout();

                    textBtn.SetImage(UIImage.FromBundle("TextSurveyInactive"), UIControlState.Normal);
                    imageBtn.SetImage(UIImage.FromBundle("ImageSurveyActive"), UIControlState.Normal);
                }

                UpdateNextButton();
            }

            // Keyboard popup
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification, KeyBoardUpNotification);

            // Keyboard Down
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, KeyBoardDownNotification);
            
            BTProgressHUD.Dismiss();
        }

        private void KeyBoardUpNotification(NSNotification notification)
        {
            CreateSurveyController._nextButton.Hidden = true;

            if (!moveViewUp)
            {
                // get the keyboard size
                CGRect r = UIKeyboard.BoundsFromNotification(notification);

                // Find what opened the keyboard
                activeview = Utils.findFirstResponder(this.View);

                UIView relativePositionView = null;
                if (CreateSurveyController.SurveyModel.type == SurveyType.Image.ToString())
                {
                    // Bottom of the controller = initial position + height - View Y position + offset (relative to the screen)     
                    relativePositionView = imageCollectionView;
                    CGRect relativeFrame = activeview.Superview.ConvertRectToView(activeview.Frame, relativePositionView);

                    bottom = (float)((relativeFrame.Y) + relativeFrame.Height - View.Frame.Y + offset);

                    // Calculate how far we need to scroll
                    scroll_amount = (float)(r.Height - (imageCollectionView.Frame.Size.Height - bottom));

                    moveViewUp = true;
                }
                else 
                {
                    // Bottom of the controller = initial position + height - View Y position + offset (relative to the screen)     
                    relativePositionView = textTableView;
                    CGRect relativeFrame = activeview.Superview.ConvertRectToView(activeview.Frame, relativePositionView);

                    bottom = (float)((relativeFrame.Y) + relativeFrame.Height - View.Frame.Y + offset);

                    // Calculate how far we need to scroll
                    scroll_amount = (float)(r.Height - (textTableView.Frame.Size.Height - bottom));

                    var pageControlHeight = 50;
                    var screenHeight = (float)UIScreen.MainScreen.Bounds.Height;

                    var diffActiveScreen = pageControlHeight + relativePositionView.Frame.Y + bottom;
                    var diffKeyboardScreen = screenHeight - r.Height;

                    if (diffActiveScreen < diffKeyboardScreen)
                    {
                        moveViewUp = false;
                    }
                    else
                    {
                        moveViewUp = true;
                    }
                }

                // Perform the scrolling
                ScrollTheView(moveViewUp);

                activeview = null;
            }
        }

        private void KeyBoardDownNotification(NSNotification notification)
        {
            CreateSurveyController._nextButton.Hidden = false;
            moveViewUp = false;
            ScrollTheView(moveViewUp);
        }

        private void ScrollTheView(bool move)
        {
            // scroll the view up or down
            UIView.BeginAnimations(string.Empty, System.IntPtr.Zero);
            UIView.SetAnimationDuration(0.1);

            if (move)
            {
                if (CreateSurveyController.SurveyModel.type == SurveyType.Image.ToString())
                {
                    imageCollectionView.SetContentOffset(new CGPoint(0, scroll_amount), true);
                }else
                {
                    textTableView.SetContentOffset(new CGPoint(0, scroll_amount), true);
                }
                
            }
            else
            {
                if (CreateSurveyController.SurveyModel.type == SurveyType.Image.ToString())
                {
                    imageCollectionView.SetContentOffset(new CGPoint(0, 0), true);
                }
                //else
                //{
                //    textTableView.SetContentOffset(new CGPoint(0, 0), true);
                //}

                scroll_amount = 0;
            }

            UIView.CommitAnimations();
        }

        private void SetupLayout()
        {
            CalculateCellHeights();

            collectionViewDelegate = new ImageOptionDelegate(cellHeights);

            collectionViewLayout = new PBCollectionViewWaterfallLayout()
            {
                ColumnCount = 2,
                ItemWidth = Constants.cellWidth,
                Delegate = collectionViewDelegate,
                SectionInset = new UIEdgeInsets(Constants.padding, 0, Constants.padding, Constants.padding)
            };
        }

        private void CalculateCellHeights()
        {
            cellHeights = new List<float>();

            foreach (var item in collectionViewItems)
            {
                var height = Constants.cellHeight + 30;// tag.Image.Size.Height;
                cellHeights.Add((float)height);
            }
        }

        private void UpdateLayout()
        {
            var layout = (PBCollectionViewWaterfallLayout)imageCollectionView.CollectionViewLayout;
            layout.ColumnCount = 2;// (int)(UIScreen.MainScreen.Bounds.Width / cellWidth);
            layout.ItemWidth = Constants.cellWidth;
        }

        private void TextChangedEvent(NSNotification notification)
        {
            UITextField field = (UITextField)notification.Object;

            if (field == questionText)
            {
                if (CreateSurveyController.SurveyModel == null)
                {
                    CreateSurveyController.SurveyModel = new SurveyModel();
                }

                if (CreateSurveyController.SurveyModel.question == null)
                {
                    CreateSurveyController.SurveyModel.question = new Question();
                }
                CreateSurveyController.SurveyModel.question.text = questionText.Text;
                CreateSurveyController.SurveyModel.question.image = "";                
            }

            UpdateNextButton();
        }
        
        public override void ViewWillUnload()
        {
            if (questionText.Text.Length <= 0)
            {
                new UIAlertView("question", "Please write a question", null, "OK", null).Show();

                return;
            }

        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            StepActivated?.Invoke(this, new MultiStepProcessStepEventArgs { Index = StepIndex });
        }
        

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (CreateSurveyController.ScreenState == ScreenState.Create.ToString())
            {
                Question q = new Question();
                q.text = questionText.Text;
                q.image = "";
                if (CreateSurveyController.SurveyModel == null)
                {
                    CreateSurveyController.SurveyModel = new SurveyModel();
                }
                CreateSurveyController.SurveyModel.userId = LoginController.userModel.id;
                CreateSurveyController.SurveyModel.userName = LoginController.userModel.name;
                CreateSurveyController.SurveyModel.profilePicture = LoginController.userModel.profilePicturePath;
                CreateSurveyController.SurveyModel.isArchived = 0;
                CreateSurveyController.SurveyModel.choiceType = "UniqueChoice";
                CreateSurveyController.SurveyModel.question = q;
                CreateSurveyController.SurveyModel.columnOptions = new List<ColumnOption>();
                CreateSurveyController.SurveyModel.finishDate = "";
                CreateSurveyController.SurveyModel.creationDate = "";
            }
            if (CreateSurveyController.ScreenState == ScreenState.Edit.ToString())
            {
                if (CreateSurveyController.SurveyModel != null) //When Cancel button is clicked the SurveyModel become null
                {
                    CreateSurveyController.SurveyModel.question.text = questionText.Text;
                }
            }
            StepDeactivated?.Invoke(this, new MultiStepProcessStepEventArgs { Index = StepIndex });
        }
    }

    #region -= tableview methods =-

    class TextOptionSource : UITableViewSource
    {
        List<TextOptionTableItem> tableItems;
        UIViewController viewController;
        protected NSString cellIdentifier = new NSString("TableCell");

        public TextOptionSource(List<TextOptionTableItem> items, UIViewController viewController)
        {
            tableItems = items;
            this.viewController = viewController;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return tableItems.Count;
        }

        public override NSIndexPath WillSelectRow(UITableView tableView, NSIndexPath indexPath)
        {
            TextOptionCustomCell cell = tableView.CellAt(indexPath) as TextOptionCustomCell;
            if(cell.SelectionStyle == UITableViewCellSelectionStyle.None)
            {
                return null;
            }
            return indexPath;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // request a recycled cell to save memory
            TextOptionCustomCell cell = tableView.DequeueReusableCell(cellIdentifier) as TextOptionCustomCell;
                        
            // if there are no cells to reuse, create a new one
            if (cell == null)
            {
                cell = new TextOptionCustomCell(cellIdentifier);
            }

            cell.UpdateCell(tableItems[indexPath.Row].Text, tableItems[indexPath.Row].Type, indexPath);

            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            return cell;
        }
    }

    public class TextOptionTableItem
    {
        public string Text { get; set; }
        public OptionType Type { get; set; }

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

        public TextOptionTableItem() { }

        public TextOptionTableItem(string text, OptionType type)
        {
            Text = text;
            Type = type;
        }                
    }

    class TextOptionCustomCell : UITableViewCell, IUITextFieldDelegate
    {
        public UITextField textField;
        public UIButton button;
        public NSIndexPath indexPath;
        public OptionType type;

        public TextOptionCustomCell(NSString cellId) : base(UITableViewCellStyle.Default, cellId)
        {
            button = new UIButton();
            button.AddTarget(Self, new ObjCRuntime.Selector("CellButtonBtn:"), UIControlEvent.TouchUpInside);                            

            textField = new UITextField();
            textField.Placeholder = "Type your option here";
            textField.TextColor = UIColor.FromRGB(90, 89, 89);
            textField.Delegate = this;
            
            ContentView.Add(textField);
            ContentView.Add(button);

            ContentView.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            ContentView.AddConstraints(
                textField.AtLeftOf(ContentView, 10),
                textField.Width().EqualTo((UIScreen.MainScreen.Bounds.Width - 3 * Constants.padding) - 34),
                textField.AtTopOf(ContentView, 10),

                button.Left().EqualTo().RightOf(textField).Plus(10),
                button.Width().EqualTo(34),
                button.AtTopOf(ContentView, 4)
            );
        }
        
        [Export("textFieldDidBeginEditing:")]
        public void EditingStarted(UITextField textField)
        {
            button.SetImage(UIImage.FromBundle("CheckProfile"), UIControlState.Normal);
        }

        [Export("textFieldDidEndEditing:")]
        public void EditingEnded(UITextField textField)
        {
            if (button.ImageView.Image.Equals(UIImage.FromBundle("CheckProfile")))
            {
                var keys = new[]
                {
                        new NSString("index"),
                        new NSString("value")
                    };

                var objects = new[]
                {
                        new NSString(indexPath.Row.ToString()),
                        new NSString(textField.Text)
                    };

                NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("UpdateRow"), new NSDictionary<NSString, NSObject>(keys, objects));
                button.SetImage(UIImage.FromBundle("DeleteOption"), UIControlState.Normal);
            }
        }

        [Export("CellButtonBtn:")]
        private void CellButtonBtn(UIButton button)
        {
            if (OptionType.Insert.Equals(type))
            {
                NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("AddNewRow"), null);
            }
            else
            {
                if (button.ImageView.Image.Equals(UIImage.FromBundle("CheckProfile")))
                {
                    var keys = new[]
                    {
                        new NSString("index"),
                        new NSString("value")
                    };

                    var objects = new[]
                    {
                        new NSString(indexPath.Row.ToString()),
                        new NSString(textField.Text)
                    };

                    NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("UpdateRow"), new NSDictionary<NSString, NSObject>(keys, objects));
                    button.SetImage(UIImage.FromBundle("DeleteOption"), UIControlState.Normal);
                }
                else
                {
                    NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("RemoveRow"), new NSString(indexPath.Row.ToString()));
                }
            }                
        }
                
        public void UpdateCell(string text, OptionType type, NSIndexPath indexPath)
        {
            textField.Text = text;
            this.indexPath = indexPath;
            this.type = type;

            if (OptionType.Insert.Equals(type))
            {
                textField.Enabled = false;
                button.SetImage(UIImage.FromBundle("AddLine"), UIControlState.Normal);
            }
            else
            {
                textField.Enabled = true;
                button.SetImage(UIImage.FromBundle("DeleteOption"), UIControlState.Normal);
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
        }
    }

    #endregion

    #region -= collectionview methods =-

    abstract class PBCollectionViewDelegateWaterfallLayout : UICollectionViewDelegate
    {
        public PBCollectionViewDelegateWaterfallLayout(List<float> cellHeights) { }

        public abstract float HeightForItem(UICollectionView collectionView, PBCollectionViewWaterfallLayout collectionViewLayout, NSIndexPath indexPath);
    }

    class PBCollectionViewWaterfallLayout : UICollectionViewLayout
    {
        // Property fields
        private int columnCount;
        private float itemWidth;
        private UIEdgeInsets sectionInset;

        // Class-related fields
        private int itemCount;
        private List<float> columnHeights;
        private float interItemSpacing;
        private List<UICollectionViewLayoutAttributes> itemAttributes;
        private static CGRect rectForLayoutAttributes;

        public PBCollectionViewWaterfallLayout()
        {
            // Default settings
            ColumnCount = 2;
            ItemWidth = (float)(UIScreen.MainScreen.Bounds.Width - 3 * Constants.padding) / 2;
            SectionInset = UIEdgeInsets.Zero;            

            columnHeights = new List<float>();
            itemAttributes = new List<UICollectionViewLayoutAttributes>();
        }

        public PBCollectionViewWaterfallLayout(int numberOfColumns, float cellWidth, UIEdgeInsets insetOfSection)
        {
            ColumnCount = numberOfColumns;
            ItemWidth = cellWidth;
            SectionInset = insetOfSection;

            columnHeights = new List<float>();
            itemAttributes = new List<UICollectionViewLayoutAttributes>();
        }

        public int ColumnCount
        {
            get
            {
                return columnCount;
            }
            set
            {
                if (value != columnCount)
                {
                    columnCount = value;
                    InvalidateLayout();
                }
            }
        }

        public float ItemWidth
        {
            get
            {
                return itemWidth;
            }
            set
            {
                if (value != itemWidth)
                {
                    itemWidth = value;
                    InvalidateLayout();
                }
            }
        }

        public UIEdgeInsets SectionInset
        {
            get
            {
                return sectionInset;
            }
            set
            {
                if (!UIEdgeInsets.Equals(sectionInset, value))
                {
                    sectionInset = value;
                    InvalidateLayout();
                }
            }
        }

        public PBCollectionViewDelegateWaterfallLayout Delegate { get; set; }

        public override CGPoint TargetContentOffset(CGPoint proposedContentOffset, CGPoint scrollingVelocity)
        {
            return base.TargetContentOffset(proposedContentOffset, scrollingVelocity);
        }

        public override CGSize CollectionViewContentSize
        {
            get
            {
                if (itemCount == 0)
                {
                    return new CGSize(0, 0);
                }

                var contentSize = CollectionView.Frame.Size;
                var columnIndex = LongestColumnIndex();
                var height = columnHeights[columnIndex];

                // Originally: contentSize.Height = height - interItemSpacing + sectionInset.Bottom;
                contentSize.Height = height + sectionInset.Bottom;

                return contentSize;
            }
        }

        public override void PrepareLayout()
        {
            base.PrepareLayout();

            itemCount = (int)CollectionView.NumberOfItemsInSection(0);

            if (ColumnCount <= 1)
            {
                throw new ApplicationException("You must have at least two columns to use UICollectionViewWaterfallLayout.");
            }
            var width = UIScreen.MainScreen.Bounds.Width - sectionInset.Left - sectionInset.Right;
            interItemSpacing = (float)Constants.padding; //(float)Math.Floor((width - columnCount * itemWidth) / (columnCount - 1));

            SetupSectionInsets();
            PlaceItem();
        }

        public void UpdateLayout()
        {
            base.PrepareLayout();

            ColumnCount = 2;
            ItemWidth = Constants.cellWidth;
            SectionInset = new UIEdgeInsets(Constants.padding, 0, Constants.padding, Constants.padding);

            columnHeights = new List<float>();
            itemAttributes = new List<UICollectionViewLayoutAttributes>();

            itemCount = (int)CollectionView.NumberOfItemsInSection(0);

            if (ColumnCount <= 1)
            {
                throw new ApplicationException("You must have at least two columns to use UICollectionViewWaterfallLayout.");
            }
            var width = UIScreen.MainScreen.Bounds.Width - sectionInset.Left - sectionInset.Right;
            interItemSpacing = (float)Constants.padding; //(float)Math.Floor((width - columnCount * itemWidth) / (columnCount - 1));

            SetupSectionInsets();
            PlaceItem();
        }

        public override UICollectionViewLayoutAttributes LayoutAttributesForItem(NSIndexPath indexPath)
        {
            return itemAttributes[indexPath.Row];
        }


        public override UICollectionViewLayoutAttributes[] LayoutAttributesForElementsInRect(CGRect rect)
        {
            rectForLayoutAttributes = rect;

            List<UICollectionViewLayoutAttributes> attributes = itemAttributes.FindAll(FindItemAttributes);

            return attributes.ToArray();
        }

        public override bool ShouldInvalidateLayoutForBoundsChange(CGRect newBounds)
        {
            return true;
        }

        private static bool FindItemAttributes(UICollectionViewLayoutAttributes attribute)
        {
            if (rectForLayoutAttributes.IntersectsWith(attribute.Frame))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SetupSectionInsets()
        {
            if (itemAttributes.Count <= 0)
            {
                for (int i = 0; i < columnCount; i++)
                {
                    columnHeights.Add((float)sectionInset.Top);
                }
            }
        }

        private void PlaceItem()
        {
            if (itemAttributes.Count <= 0)
            {
                for (int i = 0; i < itemCount; i++)
                {
                    var indexPath = NSIndexPath.FromItemSection(i, 0);
                    var itemHeight = Delegate.HeightForItem(CollectionView, this, indexPath);
                    var columnIndex = ShortestColumnIndex();

                    var xOffset = sectionInset.Left + (itemWidth + interItemSpacing) * columnIndex;
                    var yOffset = columnHeights[columnIndex];

                    var attributes = UICollectionViewLayoutAttributes.CreateForCell(indexPath);
                    attributes.Frame = new CGRect(xOffset, yOffset, itemWidth, itemHeight);
                    itemAttributes.Add(attributes);
                    columnHeights[columnIndex] = yOffset + itemHeight + interItemSpacing;
                }
            }
        }

        private int ShortestColumnIndex()
        {
            var shortestIndex = 0;
            var shortestHeight = float.MaxValue;

            int index = 0;
            foreach (var height in columnHeights)
            {
                if (height < shortestHeight)
                {
                    shortestHeight = height;
                    shortestIndex = index;
                }

                index++;
            }

            return shortestIndex;
        }

        private int LongestColumnIndex()
        {
            var largestIndex = 0;
            var largestHeight = float.MaxValue;

            int index = 0;
            foreach (var height in columnHeights)
            {
                if (height > largestHeight)
                {
                    largestHeight = height;
                    largestIndex = index;
                }

                index++;
            }

            return largestIndex;
        }
    }

    class ImageOptionDelegate : PBCollectionViewDelegateWaterfallLayout
    {
        public List<float> cellHeights { get; set; }

        public ImageOptionDelegate(List<float> cellHeights) : base (cellHeights)
		{
            this.cellHeights = cellHeights;
        }

        // Important: The only method that the delegate *has* to override. Just return the value the cell's height, which you have usually calculated
        // beforehand.
        public override float HeightForItem(UICollectionView collectionView, PBCollectionViewWaterfallLayout collectionViewLayout, NSIndexPath indexPath)
        {
            return cellHeights[indexPath.Row];
            // will always have same height
            //return cellHeights[0];
        }
    }

    class ImageOptionCustomCell : UICollectionViewCell, IUITextFieldDelegate
    {
        public UITextField ImageLabel { get; set; }
        public UIImageView Image { get; set; }
        public UIView TextSeparator { get; set; }
        public UIButton Button { get; set; }

        public NSIndexPath indexPath;

        [Export("initWithFrame:")]
        public ImageOptionCustomCell(CGRect frame) : base(frame)
        {
            ImageLabel = new UITextField()
            {
                BackgroundColor = UIColor.White,
                Placeholder = "Image Caption",
                TextColor = UIColor.FromRGB(90, 89, 89)
            };
            ImageLabel.Delegate = this;

            Image = new UIImageView()
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleHeight,
                Frame = new CGRect(0, ImageLabel.Bounds.Height, 0, 0),
                BackgroundColor = UIColor.FromRGB(90, 89, 89)
            };

            Button = new UIButton();
            Button.SetImage(UIImage.FromBundle("DeleteOption"), UIControlState.Normal);
            Button.AddTarget(Self, new ObjCRuntime.Selector("ButtonClick:"), UIControlEvent.TouchUpInside);

            TextSeparator = new UIView();
            TextSeparator.BackgroundColor = UIColor.FromRGB(90, 89, 89);

            ContentView.Add(Image);
            ContentView.Add(ImageLabel);
            ContentView.Add(Button);
            ContentView.Add(TextSeparator);

            ContentView.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            ContentView.AddConstraints(
                Image.AtLeftOf(ContentView, 10),
                Image.Width().EqualTo(Constants.cellWidth),
                Image.Height().EqualTo(Constants.cellWidth),
                Image.AtTopOf(ContentView),

                ImageLabel.Below(Image),
                ImageLabel.AtRightOf(ContentView, 10),
                ImageLabel.AtLeftOf(ContentView, 10),
                ImageLabel.Height().EqualTo(30),
                                                
                Button.Below(Image),
                Button.Width().EqualTo(20),
                Button.WithSameHeight(ImageLabel),
                Button.Left().EqualTo().RightOf(ImageLabel),

                TextSeparator.Below(ImageLabel),
                TextSeparator.WithSameWidth(Image),
                TextSeparator.AtLeftOf(ContentView, 10),
                TextSeparator.Height().EqualTo(1)
            );
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            Image.Layer.CornerRadius = (float)Constants.cellWidth * 0.05f;
            Image.Layer.BorderColor = UIColor.FromRGB(90, 89, 89).CGColor;
            Image.Layer.BorderWidth = 1;
        }

        [Export("ButtonClick:")]
        private void ButtonClick(UIButton button)
        {   
            if (button.ImageView.Image.Equals(UIImage.FromBundle("CheckProfile")))
            {
                var keys = new[]
                {
                    new NSString("index"),
                    new NSString("value")
                };

                var objects = new[]
                {
                    new NSString(indexPath.Row.ToString()),
                    new NSString(ImageLabel.Text)
                };

                NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("UpdateTextCell"), new NSDictionary<NSString, NSObject>(keys, objects));

                button.SetImage(UIImage.FromBundle("DeleteOption"), UIControlState.Normal);
            }
            else
            {
                NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("DeleteCell"), new NSString(indexPath.Row.ToString()));

            }
        }

        [Export("textFieldDidBeginEditing:")]
        public void EditingStarted(UITextField textField)
        {
            Button.SetImage(UIImage.FromBundle("CheckProfile"), UIControlState.Normal);
        }

        [Export("textFieldDidEndEditing:")]
        public void EditingEnded(UITextField textField)
        {
            if (Button.ImageView.Image.Equals(UIImage.FromBundle("CheckProfile")))
            {
                var keys = new[]
                {
                    new NSString("index"),
                    new NSString("value")
                };

                var objects = new[]
                {
                    new NSString(indexPath.Row.ToString()),
                    new NSString(ImageLabel.Text)
                };

                NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("UpdateTextCell"), new NSDictionary<NSString, NSObject>(keys, objects));

                Button.SetImage(UIImage.FromBundle("DeleteOption"), UIControlState.Normal);
            }
        }

        public void PopulateCell(string label, UIImage image, OptionType type, NSIndexPath indexPath)
        {
            ImageLabel.Text = label;
            Image.Image = image;
            this.indexPath = indexPath;

            if (type.Equals(OptionType.Insert))
            {
                Image.ContentMode = UIViewContentMode.Center;
                Image.BackgroundColor = UIColor.FromRGB(90, 89, 89);
                Button.Hidden = true;
                ImageLabel.Hidden = true;
                TextSeparator.Hidden = true;
            }
            else
            {
                Image.ContentMode = UIViewContentMode.ScaleAspectFit;
                Image.BackgroundColor = UIColor.White;
                Button.Hidden = false;
                ImageLabel.Hidden = false;
                TextSeparator.Hidden = false;
            }

            LayoutSubviews();
        }
    }

    public class ImageOptionTableItem
    {
        public string Text { get; set; }
        public UIImage Image { get; set; }
        public byte[] ImageArray { get; set; }
        public OptionType Type { get; set; }

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

        public ImageOptionTableItem() { }

        public ImageOptionTableItem(UIImage image, byte[] imageArray, string text, OptionType type)
        {
            Image = image;
            ImageArray = imageArray;
            Text = text;
            Type = type;
        }
    }

    class ImageOptionSource : UICollectionViewSource
    {
        List<ImageOptionTableItem> data;
        UIImagePickerController imagePicker;
        UINavigationController navigationController;
        NSIndexPath indexPath;

        static NSString imageCellId = new NSString("ImageCellId");

        public ImageOptionSource(List<ImageOptionTableItem> items, UINavigationController navigationController)
		{
            data = items;
            this.navigationController = navigationController;
            LoadImagePicker();
        }

        private void LoadImagePicker()
        {
            if(imagePicker == null)
            {
                imagePicker = new UIImagePickerController();
                imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;

                imagePicker.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);

                imagePicker.FinishedPickingMedia += Handle_FinishedPickingMedia;

                imagePicker.Canceled += Handle_Canceled;
            }
        }

        public override nint NumberOfSections(UICollectionView collectionView)
        {
            return 1;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return data.Count;
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            this.indexPath = indexPath;

            //var mainWindow = UIApplication.SharedApplication.KeyWindow;
            //var viewController = mainWindow?.RootViewController;
            //while (viewController?.PresentedViewController != null)
            //{
            //    viewController = viewController.PresentedViewController;
            //}
            //if (viewController == null)
            //    viewController = this.viewController;
            //imagePicker.View.Frame = viewController.View.Frame;
            //viewController.PresentModalViewController(imagePicker, true);

            this.navigationController.PresentModalViewController(imagePicker, true);
        }


        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = (ImageOptionCustomCell)collectionView.DequeueReusableCell(imageCellId, indexPath);
            var item = data[indexPath.Row];

            cell.PopulateCell(item.Text, item.Image, item.Type, indexPath);

            return cell;
        }

        private void Handle_Canceled(object sender, EventArgs e)
        {
            imagePicker.DismissModalViewController(true);
        }

        protected void Handle_FinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs e)
        {
            // determine what was selected, video or image
            bool isImage = false;
            string fileExtension = "";
            switch (e.Info[UIImagePickerController.MediaType].ToString())
            {
                case "public.image":
                    isImage = true;
                    break;

                case "public.video":
                    break;
            }

            // get common info (shared between images and video)
            NSUrl referenceURL = e.Info[new NSString("UIImagePickerControllerReferenceURL")] as NSUrl;

            Console.WriteLine("image path :" + referenceURL.Path.ToString());
            Console.WriteLine("image relative path :" + referenceURL.RelativePath.ToString());

            ALAssetsLibrary assetsLibrary = new ALAssetsLibrary();
            assetsLibrary.AssetForUrl(referenceURL, delegate (ALAsset asset)
            {
                ALAssetRepresentation representation = asset.DefaultRepresentation;
                if (representation == null)
                {
                    return;
                }
                else
                {
                    string fileName = representation.Filename;
                    fileExtension = Path.GetExtension(fileName).ToLower();
                    Console.WriteLine("image Filename :" + fileName);
                }
            }, delegate (NSError error) {
                Console.WriteLine("User denied access to photo Library... {0}", error);
            });


            // if it was an image, get the other image info
            if (isImage)
            {

                // get the original image
                UIImage originalImage = e.Info[UIImagePickerController.OriginalImage] as UIImage;
                if (originalImage != null)
                {
                    // do something with the image
                    Console.WriteLine("got the original image");
                    //imageView.image = originalImage;

                    using (NSData imageData = Utils.CompressImage(originalImage))
                    {
                        //byte[] myByteArray = new byte[imageData.Length];
                        //System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, myByteArray, 0, Convert.ToInt32(imageData.Length));                        
                        //tableItems.Insert(indexPath.Row, new SurveyOptionTableItem(alert.GetTextField(0).Text, ".jpg", myByteArray));

                        var keys = new[]
                        {
                            new NSString("index"),
                            new NSString("image")
                        };

                        var objects = new NSObject[]
                        {
                            new NSString(this.indexPath.Row.ToString()),
                            imageData
                        };

                        NSNotificationCenter.DefaultCenter.PostNotificationName(new NSString("UpdateCell"), new NSDictionary<NSString, NSObject>(keys, objects));
                    }
                    //---- insert a new row in the table
                    //tableView.InsertRows(new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
                }

                // get the edited image
                UIImage editedImage = e.Info[UIImagePickerController.EditedImage] as UIImage;
                if (editedImage != null)
                {
                    // do something with the image
                    Console.WriteLine("got the edited image");
                    //imageView.image = editedImage;
                }

                //- get the image metadata
                NSDictionary imageMetadata = e.Info[UIImagePickerController.MediaMetadata] as NSDictionary;
                if (imageMetadata != null)
                {
                    // do something with the metadata
                    Console.WriteLine("got image metadata");
                }

            }
            // if it's a video
            else
            {
                // get video url
                NSUrl mediaURL = e.Info[UIImagePickerController.MediaURL] as NSUrl;
                if (mediaURL != null)
                {
                    //
                    Console.WriteLine(mediaURL.ToString());
                }
            }

            // dismiss the picker
            imagePicker.DismissModalViewController(true);
        }
    }

    #endregion

    #region -= constants =-

    class Constants
    {
        public static nfloat padding = 10;
        public static float cellWidth = (float)((UIScreen.MainScreen.Bounds.Width - 3 * padding) / 2);
        public static float cellHeight = cellWidth;
    }

    #endregion
}

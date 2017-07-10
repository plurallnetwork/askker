using Askker.App.iOS.HorizontalSwipe;
using Askker.App.iOS.TableControllers;
using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
using BigTed;
using CoreFoundation;
using Foundation;
using SDWebImage;
using System;
using System.Collections.Generic;
using System.IO;
using UIKit;

namespace Askker.App.iOS
{
    public partial class CreateSurveyOptionsStep : CustomUIViewController, IMultiStepProcessStep
    {
        public static OptionsStepView _optionsStepView { get; set; }
        public static SurveyOptionTableSource tableSource;

        public override void LoadView()
        {
            View = new UIView();
        }

        public override void ViewDidLoad()
        {
            BTProgressHUD.Show(null, -1, ProgressHUD.MaskType.Clear);

            this.RestrictRotation(UIInterfaceOrientationMask.Portrait);
            
            _optionsStepView = OptionsStepView.Create();            
            View.AddSubview(_optionsStepView);
            List<SurveyOptionTableItem> tableItems = new List<SurveyOptionTableItem>();

            _optionsStepView.TextButton.SetLeftBorder(UIColor.LightGray, 1);
            _optionsStepView.TextButton.BackgroundColor = UIColor.Green;
            _optionsStepView.TextButton.TintColor = UIColor.White;
            _optionsStepView.TextButton.SetTitle("Survey with Text", UIControlState.Normal);
            _optionsStepView.TextButton.Font = UIFont.SystemFontOfSize(12);
            var uiImage = UIImage.FromFile("images/icons/text.gif");
            var lineHeight = _optionsStepView.TextButton.TitleLabel.Font.LineHeight;
            _optionsStepView.TextButton.TitleEdgeInsets = new UIEdgeInsets(uiImage.Size.Height, -uiImage.Size.Width, 0, 0);
            _optionsStepView.TextButton.ImageEdgeInsets = new UIEdgeInsets(-lineHeight, uiImage.Size.Width + (uiImage.Size.Width / 2), 0, 0);

            _optionsStepView.ImageButton.BackgroundColor = UIColor.Green;
            _optionsStepView.ImageButton.TintColor = UIColor.White;
            _optionsStepView.ImageButton.SetTitle("Survey with Images", UIControlState.Normal);
            _optionsStepView.ImageButton.Font = UIFont.SystemFontOfSize(12);
            var uiImage1 = UIImage.FromFile("images/icons/image.png");
            var lineHeight1 = _optionsStepView.ImageButton.TitleLabel.Font.LineHeight;
            _optionsStepView.ImageButton.TitleEdgeInsets = new UIEdgeInsets(uiImage1.Size.Height, -uiImage1.Size.Width, 0, 0);
            _optionsStepView.ImageButton.ImageEdgeInsets = new UIEdgeInsets(-lineHeight1, uiImage1.Size.Width + (uiImage1.Size.Width / 2), 0, 0);

            _optionsStepView.DoneButton.BackgroundColor = UIColor.LightGray;
            _optionsStepView.DoneButton.TintColor = UIColor.White;
            _optionsStepView.DoneButton.SetTitle("Validate Survey", UIControlState.Normal);
            _optionsStepView.DoneButton.Font = UIFont.SystemFontOfSize(12);
            var uiImage2 = UIImage.FromFile("images/icons/done.png");
            var lineHeight2 = _optionsStepView.DoneButton.TitleLabel.Font.LineHeight;
            _optionsStepView.DoneButton.TitleEdgeInsets = new UIEdgeInsets(uiImage2.Size.Height, -uiImage2.Size.Width, 0, 0);
            _optionsStepView.DoneButton.ImageEdgeInsets = new UIEdgeInsets(-lineHeight2, uiImage2.Size.Width + (uiImage2.Size.Width / 2), 0, 0);

            if (CreateSurveyController.ScreenState == ScreenState.Edit.ToString())
            {
                _optionsStepView.DoneButton.BackgroundColor = UIColor.Green;

                if (CreateSurveyController.SurveyModel.type == SurveyType.Text.ToString())
                {
                    _optionsStepView.ImageButton.BackgroundColor = UIColor.LightGray;
                    _optionsStepView.TextButton.BackgroundColor = UIColor.Green;
                }
                else
                {
                    _optionsStepView.ImageButton.BackgroundColor = UIColor.Green;
                    _optionsStepView.TextButton.BackgroundColor = UIColor.LightGray;
                }

                foreach(var option in CreateSurveyController.SurveyModel.options)
                {
                    if(CreateSurveyController.SurveyModel.type == SurveyType.Text.ToString())
                    {
                        tableItems.Add(new SurveyOptionTableItem(option.text));
                    }
                    else
                    {
                        //UIImageView imageView = new UIImageView();
                        //Utils.SetImageFromNSUrlSession(option.image, imageView, this);

                        UIImage image = Utils.GetImageFromNSUrl(option.image);

                        try
                        {
                            using (NSData imageData = image.AsJPEG())
                            {
                                byte[] myByteArray = new byte[imageData.Length];
                                System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, myByteArray, 0, Convert.ToInt32(imageData.Length));
                                tableItems.Add(new SurveyOptionTableItem(option.text, ".jpg", myByteArray));
                                tableSource = new SurveyOptionTableSource(tableItems, this);
                                _optionsStepView.OptionsTable.Source = tableSource;
                                _optionsStepView.OptionsTable.ReloadData();
                            }
                            
                        }
                        catch (Exception ex)
                        {
                            BTProgressHUD.Dismiss();
                            Utils.HandleException(ex);
                        }

                        //var url = new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + option.image);
                        //var task = NSUrlSession.SharedSession.CreateDataTask(url, (data, response, error) =>
                        //{
                        //    try
                        //    {
                        //        DispatchQueue.MainQueue.DispatchAsync(() => {
                        //            using (NSData imageData = UIImage.LoadFromData(data))
                        //            {
                        //                byte[] myByteArray = new byte[imageData.Length];
                        //                System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, myByteArray, 0, Convert.ToInt32(imageData.Length));
                        //                tableItems.Add(new SurveyOptionTableItem(option.text, ".jpg", myByteArray));
                        //                tableSource = new SurveyOptionTableSource(tableItems, this);
                        //                _optionsStepView.OptionsTable.Source = tableSource;
                        //                _optionsStepView.OptionsTable.ReloadData();
                        //            }
                        //        });
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        Utils.HandleException(ex);
                        //    }

                        //});
                        //task.Resume();                        
                    }                    
                }
                CreateSurveyController._nextButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                CreateSurveyController._nextButton.BackgroundColor = UIColor.Green;
            }

            tableSource = new SurveyOptionTableSource(tableItems, this);
            _optionsStepView.OptionsTable.Source = tableSource;
            _optionsStepView.OptionsTable.ReloadData();

            _optionsStepView.TextButton.TouchUpInside += async (sender, e) =>
            {
                if (tableSource.GetTableItems().Count > 0 && CreateSurveyController.SurveyModel.type != SurveyType.Text.ToString())
                {
                    nint button = await Utils.ShowAlert("Options", "All image options will be deleted. Continue?", "Ok", "Cancel");

                    if(button == 0)
                    {
                        tableSource.Clear(_optionsStepView.OptionsTable);
                    }else if (button == 1)
                    {
                        return;
                    }
                }

                CreateSurveyController.SurveyModel.type = SurveyType.Text.ToString();
                CreateSurveyController._backButton.Hidden = true;
                CreateSurveyController._nextButton.Hidden = true;

                if (_optionsStepView.OptionsTable.Editing)
                    _optionsStepView.OptionsTable.SetEditing(false, true);
                tableSource.WillBeginTableEditing(_optionsStepView.OptionsTable);
                _optionsStepView.OptionsTable.SetEditing(true, true);

                _optionsStepView.TextButton.Hidden = true;
                _optionsStepView.ImageButton.Hidden = true;
                _optionsStepView.DoneButton.Hidden = false;
            };

            _optionsStepView.ImageButton.TouchUpInside += async (sender, e) =>
            {
                if (tableSource.GetTableItems().Count > 0 && CreateSurveyController.SurveyModel.type != SurveyType.Image.ToString())
                {
                    nint button = await Utils.ShowAlert("Options", "All text options will be deleted. Continue?", "Ok", "Cancel");

                    if (button == 0)
                    {
                        tableSource.Clear(_optionsStepView.OptionsTable);
                    }
                    else if (button == 1)
                    {
                        return;
                    }
                }

                CreateSurveyController.SurveyModel.type = SurveyType.Image.ToString();
                CreateSurveyController._backButton.Hidden = true;
                CreateSurveyController._nextButton.Hidden = true;

                if (_optionsStepView.OptionsTable.Editing)
                    _optionsStepView.OptionsTable.SetEditing(false, true);
                tableSource.WillBeginTableEditing(_optionsStepView.OptionsTable);
                _optionsStepView.OptionsTable.SetEditing(true, true);

                _optionsStepView.TextButton.Hidden = true;
                _optionsStepView.ImageButton.Hidden = true;
                _optionsStepView.DoneButton.Hidden = false;
            };

            _optionsStepView.DoneButton.TouchUpInside += (sender, e) =>
            {
                _optionsStepView.OptionsTable.SetEditing(false, true);
                tableSource.DidFinishTableEditing(_optionsStepView.OptionsTable);

                //List<SurveyOptionTableItem> items = tableSource.GetTableItems();
                //if (items.Count > 0)
                //{
                //    CreateSurveyController.SurveyModel.options = new List<Option>();
                    
                //    if (CreateSurveyController.SurveyModel.type == SurveyType.Image.ToString())
                //    {
                //        //if (CreateSurveyController.OptionImages == null)
                //        //{
                //        CreateSurveyController.OptionImages = new List<KeyValuePair<string, byte[]>>();
                //        //}
                //    }

                //    int optionId = 0;
                //    items.ForEach(i =>
                //    {
                //        if (!"<- Add new option".Equals(i.Text))
                //        {
                //            Option o = new Option();
                //            o.id = optionId;
                //            o.text = i.Text;
                //            o.image = "";

                //            if (CreateSurveyController.SurveyModel.type == SurveyType.Image.ToString() && i.Image != null)
                //            {
                //                CreateSurveyController.OptionImages.Add(new KeyValuePair<string, byte[]>(optionId.ToString() + i.ImageExtension, i.Image));
                //            }

                //            CreateSurveyController.SurveyModel.options.Add(o);


                //            optionId++;
                //        }
                //    });
                //}

                _optionsStepView.TextButton.Hidden = false;
                _optionsStepView.ImageButton.Hidden = false;
                _optionsStepView.DoneButton.Hidden = true;
                CreateSurveyController._backButton.Hidden = false;
                CreateSurveyController._nextButton.Hidden = false;

                if (CreateSurveyOptionsStep.tableSource.GetTableItems().Count <= 1)
                {
                    CreateSurveyController._nextButton.SetTitleColor(UIColor.LightGray, UIControlState.Normal);
                    CreateSurveyController._nextButton.BackgroundColor = UIColor.White;
                }
                else
                {
                    CreateSurveyController._nextButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                    CreateSurveyController._nextButton.BackgroundColor = UIColor.Green;
                }

            };

            if (CreateSurveyOptionsStep.tableSource.GetTableItems().Count <= 1)
            {
                CreateSurveyController._nextButton.SetTitleColor(UIColor.LightGray, UIControlState.Normal);
                CreateSurveyController._nextButton.BackgroundColor = UIColor.White;
            }
            else
            {
                CreateSurveyController._nextButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                CreateSurveyController._nextButton.BackgroundColor = UIColor.Green;
            }

            BTProgressHUD.Dismiss();
        }

        public override void ViewDidLayoutSubviews()
        {
            _optionsStepView.Frame = View.Bounds;            
        }

        public CreateSurveyOptionsStep (IntPtr handle) : base (handle)
        {
        }

        public CreateSurveyOptionsStep()
        {
        }
        
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            _optionsStepView.QuestionText.Text = CreateSurveyController.SurveyModel.question.text;
            StepActivated?.Invoke(this, new MultiStepProcessStepEventArgs { Index = StepIndex });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            StepDeactivated?.Invoke(this, new MultiStepProcessStepEventArgs { Index = StepIndex });
        }

        public int StepIndex { get; set; }

        public event EventHandler<MultiStepProcessStepEventArgs> StepActivated;
        public event EventHandler<MultiStepProcessStepEventArgs> StepDeactivated;
    }
}
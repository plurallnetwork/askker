using Askker.App.iOS.HorizontalSwipe;
using Askker.App.iOS.TableControllers;
using Askker.App.PortableLibrary.Enums;
using Askker.App.PortableLibrary.Models;
using CoreFoundation;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using UIKit;

namespace Askker.App.iOS
{
    public partial class CreateSurveyOptionsStep : UIViewController, IMultiStepProcessStep
    {
        public static OptionsStepView _optionsStepView { get; set; }
        public static SurveyOptionTableSource tableSource;

        public override void LoadView()
        {
            View = new UIView();
        }

        public override void ViewDidLoad()
        {
            _optionsStepView = OptionsStepView.Create();            
            View.AddSubview(_optionsStepView);
            List<SurveyOptionTableItem> tableItems = new List<SurveyOptionTableItem>();

            if (CreateSurveyController.ScreenState == ScreenState.Edit.ToString())
            {
                foreach(var option in CreateSurveyController.SurveyModel.options)
                {
                    if(CreateSurveyController.SurveyModel.type == SurveyType.Text.ToString())
                    {
                        tableItems.Add(new SurveyOptionTableItem(option.text));
                    }
                    else
                    {
                        var url = new NSUrl("https://s3-us-west-2.amazonaws.com/askker-desenv/" + option.image);
                        var task = NSUrlSession.SharedSession.CreateDataTask(url, (data, response, error) =>
                        {
                            try
                            {
                                DispatchQueue.MainQueue.DispatchAsync(() => {
                                    using (NSData imageData = Utils.CompressImage(UIImage.LoadFromData(data)))
                                    {
                                        byte[] myByteArray = new byte[imageData.Length];
                                        System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, myByteArray, 0, Convert.ToInt32(imageData.Length));
                                        tableItems.Add(new SurveyOptionTableItem(option.text, ".jpg", myByteArray));
                                        tableSource = new SurveyOptionTableSource(tableItems, this);
                                        _optionsStepView.OptionsTable.Source = tableSource;
                                        _optionsStepView.OptionsTable.ReloadData();
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message);
                            }
                            
                        });
                        task.Resume();                        
                    }                    
                }
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

            };

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
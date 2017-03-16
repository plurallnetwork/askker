using Askker.App.iOS.HorizontalSwipe;
using Askker.App.iOS.TableControllers;
using Askker.App.PortableLibrary.Models;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using UIKit;

namespace Askker.App.iOS
{
    public partial class CreateSurveyOptionsStep : UIViewController, IMultiStepProcessStep
    {
        private OptionsStepView _optionsStepView;
        TableSource tableSource;

        public override void LoadView()
        {
            View = new UIView();
        }

        public override void ViewDidLoad()
        {
            _optionsStepView = OptionsStepView.Create();            
            View.AddSubview(_optionsStepView);
            List<TableItem> tableItems = new List<TableItem>();
            tableSource = new TableSource(tableItems, this);
            _optionsStepView.OptionsTable.Source = tableSource;

            _optionsStepView.TextButton.TouchUpInside += (sender, e) =>
            {
                CreateSurveyController.SurveyModel.type = "text";

                if (_optionsStepView.OptionsTable.Editing)
                    _optionsStepView.OptionsTable.SetEditing(false, true); 
                tableSource.WillBeginTableEditing(_optionsStepView.OptionsTable);
                _optionsStepView.OptionsTable.SetEditing(true, true);

                _optionsStepView.TextButton.Hidden = true;
                _optionsStepView.ImageButton.Hidden = true;
                _optionsStepView.DoneButton.Hidden = false;                
            };

            _optionsStepView.ImageButton.TouchUpInside += (sender, e) =>
            {
                CreateSurveyController.SurveyModel.type = "image";
                
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

                _optionsStepView.TextButton.Hidden = false;
                _optionsStepView.ImageButton.Hidden = false;
                _optionsStepView.DoneButton.Hidden = true;
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
            List<TableItem> items = tableSource.GetTableItems();
            if(items.Count > 0)
            {
                if(CreateSurveyController.SurveyModel.options == null)
                {
                    CreateSurveyController.SurveyModel.options = new List<Option>();
                }

                if ("image".Equals(CreateSurveyController.SurveyModel.type))
                {
                    //if (CreateSurveyController.OptionImages == null)
                    //{
                    CreateSurveyController.OptionImages = new List<KeyValuePair<string, byte[]>>();
                    //}
                }

                int optionId = 0;
                items.ForEach(i =>
                {
                    if (!"<- Add new option".Equals(i.Heading))
                    {
                        Option o = new Option();
                        o.id = optionId;
                        o.text = i.Heading;
                        o.image = "";
                        
                        if ("image".Equals(CreateSurveyController.SurveyModel.type) && i.Image != null)
                        {
                            CreateSurveyController.OptionImages.Add(new KeyValuePair<string, byte[]>(optionId.ToString()+i.SubHeading, i.Image));                                                            
                        }

                        CreateSurveyController.SurveyModel.options.Add(o);


                        optionId++;
                    }
                });
            }
            StepDeactivated?.Invoke(this, new MultiStepProcessStepEventArgs { Index = StepIndex });
        }

        public int StepIndex { get; set; }

        public event EventHandler<MultiStepProcessStepEventArgs> StepActivated;
        public event EventHandler<MultiStepProcessStepEventArgs> StepDeactivated;
    }
}
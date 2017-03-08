using Askker.App.iOS.HorizontalSwipe;
using Askker.App.iOS.TableControllers;
using Askker.App.PortableLibrary.Models;
using Foundation;
using System;
using System.Collections.Generic;
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
            tableSource = new TableSource(tableItems);
            _optionsStepView.OptionsTable.Source = tableSource;

            _optionsStepView.TextButton.TouchUpInside += (sender, e) =>
            {
                if (_optionsStepView.OptionsTable.Editing)
                    _optionsStepView.OptionsTable.SetEditing(false, true); 
                tableSource.WillBeginTableEditing(_optionsStepView.OptionsTable);
                _optionsStepView.OptionsTable.SetEditing(true, true);

                _optionsStepView.TextButton.Hidden = true;
                _optionsStepView.ImageButton.Hidden = true;
                _optionsStepView.DoneButton.Hidden = false;

                CreateSurveyController.SurveyModel.Type = "Text";
            };

            _optionsStepView.DoneButton.TouchUpInside += (sender, e) =>
            {
                _optionsStepView.OptionsTable.SetEditing(false, true);
                tableSource.DidFinishTableEditing(_optionsStepView.OptionsTable);

                _optionsStepView.TextButton.Hidden = false;
                _optionsStepView.ImageButton.Hidden = false;
                _optionsStepView.DoneButton.Hidden = true;

                CreateSurveyController.SurveyModel.Type = "Image";
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
            _optionsStepView.QuestionText.Text = CreateSurveyController.SurveyModel.Question.Text;            
            StepActivated?.Invoke(this, new MultiStepProcessStepEventArgs { Index = StepIndex });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            List<TableItem> items = tableSource.GetTableItems();
            if(items.Count > 0)
            {
                if(CreateSurveyController.SurveyModel.Options == null)
                {
                    CreateSurveyController.SurveyModel.Options = new List<Option>();
                }
                int optionId = 0;
                items.ForEach(i =>
                {
                    Option o = new Option();
                    o.Id = optionId;
                    o.Text = i.Heading;
                    CreateSurveyController.SurveyModel.Options.Add(o);
                    optionId++;
                });
            }
            StepDeactivated?.Invoke(this, new MultiStepProcessStepEventArgs { Index = StepIndex });
        }

        public int StepIndex { get; set; }

        public event EventHandler<MultiStepProcessStepEventArgs> StepActivated;
        public event EventHandler<MultiStepProcessStepEventArgs> StepDeactivated;
    }
}
﻿using Askker.App.PortableLibrary.Enums;
using Foundation;
using System;
using UIKit;

namespace Askker.App.iOS
{
    public partial class HomeController : UIViewController
    {
        public HomeController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            profileOtherButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                var profileOtherController = this.Storyboard.InstantiateViewController("ProfileOtherController") as ProfileOtherController;
                if (profileOtherController != null)
                {
                    profileOtherController.UserId = "75e4441c-4414-4fb2-8966-62c53d8ef854";
                    this.PresentViewController(profileOtherController, true, null);
                }
            };

            createButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                var CreateSurveyController = this.Storyboard.InstantiateViewController("CreateSurveyController") as CreateSurveyController;
                if (CreateSurveyController != null)
                {
                    CreateSurveyController.ScreenState = ScreenState.Create.ToString();
                    this.PresentViewController(CreateSurveyController, true, null);
                }
            };

            editButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                var CreateSurveyController = this.Storyboard.InstantiateViewController("CreateSurveyController") as CreateSurveyController;
                if (CreateSurveyController != null)
                {
                    CreateSurveyController.ScreenState = ScreenState.Edit.ToString();
                    CreateSurveyController.UserId = "030a9abd-d9e5-4e17-a898-01dcc18cf67c";
                    CreateSurveyController.CreationDate = "20170418T183602";
                    this.PresentViewController(CreateSurveyController, true, null);
                }
            };
        }
        
        private void ProfileOtherButton_TouchUpInside(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        partial void BtnMenu_TouchUpInside(UIButton sender)
        {
            var rootController = this.Storyboard.InstantiateViewController("MenuNavController");
            if (rootController != null)
            {
                this.PresentViewController(rootController, true, null);
            }
        }

        
    }
}
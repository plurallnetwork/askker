using Foundation;
using Askker.App.PortableLibrary.Business;
using Askker.App.PortableLibrary.Models;
using System;

using UIKit;
using System.Collections.Generic;

namespace Askker.App.iOS
{
    public partial class LoginController : UIViewController
    {
        CredentialsService credentialsService;
        TokenModel tokenModel;

        public LoginController (IntPtr handle) : base (handle)
        {
            credentialsService = new CredentialsService();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            if (credentialsService.DoCredentialsExist())
            {
                tokenModel = credentialsService.GetTokenModel();

                var alert = UIAlertController.Create("Already logged in", "OK", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            // set the View Controller that’s powering the screen we’re
            // transitioning to

            var registerContoller = segue.DestinationViewController as RegisterController;
        }

        async partial void btnEnter_TouchUpInside(UIKit.UIButton sender)
        {
            if (string.Empty.Equals(txtUsername.Text))
            {
                var alert = UIAlertController.Create("E-mail", "Please fill in the E-mail", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
            else if (string.Empty.Equals(txtPassword.Text))
            {
                var alert = UIAlertController.Create("Password", "Please fill in the Password", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
            else
            {
                try
                {
                    LoginManager loginManager = new LoginManager();
                    UserLoginModel userLoginModel = new UserLoginModel(txtUsername.Text, txtPassword.Text);

                tokenModel = await loginManager.GetAuthorizationToken(userLoginModel);

                    bool doCredentialsExist = credentialsService.DoCredentialsExist();
                    if (!doCredentialsExist)
                    {
                        credentialsService.SaveCredentials(tokenModel);
                    }

                    FeedManager feedManager = new FeedManager();

                    // Save Survey Example
                    //SurveyModel surveyTeste = new SurveyModel();
                    //surveyTeste.UserId = tokenModel.Id;
                    //surveyTeste.Type = "Text";
                    //surveyTeste.ChoiceType = "UniqueChoice";
                    //surveyTeste.Question = new Question() { Text = "Qual a sua cor favorita?", Image = "" };

                    //List<Option> options = new List<Option>();
                    //options.Add(new Option() { Id = 0, Text = "Verde", Image = "" });
                    //options.Add(new Option() { Id = 1, Text = "Azul", Image = "" });
                    //surveyTeste.Options = options;

                    //surveyTeste.ColumnOptions = new List<ColumnOption>();
                    //surveyTeste.IsArchived = 0;
                    //surveyTeste.FinishDate = "";

                    //await feedManager.SaveSurvey(surveyTeste, tokenModel.Access_Token);

                    // Get Surveys Example
                    List<SurveyModel> surveys = await feedManager.GetSurveys(tokenModel.Id, tokenModel.Access_Token);

                    var alert = UIAlertController.Create("Surveys", "Total: " + surveys.Count.ToString(), UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Equals("901"))
                    {
                        var alert = UIAlertController.Create("Login", "The user name or password is incorrect.", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                        PresentViewController(alert, true, null);
                    }
                    else if (ex.Message.Equals("903"))
                    {
                        var alert = UIAlertController.Create("Login", "An error occurred while sending the e-mail confirmation.", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                        PresentViewController(alert, true, null);
                    }
                    else if (ex.Message.Equals("905"))
                    {
                        var alert = UIAlertController.Create("Login", "The user e-mail is not confirmed. A new e-mail confirmation has been sent to user.", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                        PresentViewController(alert, true, null);
                    }
                    else
                    {
                        var alert = UIAlertController.Create("Login", ex.Message, UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                        PresentViewController(alert, true, null);
                    }
                }
            }
        }
    }
}
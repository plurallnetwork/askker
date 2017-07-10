using Askker.App.PortableLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Models
{
    public class TargetAudienceModel
    {
        public TargetAudience TargetAudience { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public string ImageName { get; set; }
    }

    public class SurveyTargetAudiencesModel : TargetAudienceModel
    {
        public List<TargetAudienceModel> TargetAudienceItems { get; }

        public SurveyTargetAudiencesModel()
        {
            TargetAudienceItems = new List<TargetAudienceModel>();
            TargetAudienceItems.Add(new TargetAudienceModel() { TargetAudience = TargetAudience.Public, Title = "Public", Text = "This question will be visible to everybody!", ImageName = "Globe" });
            TargetAudienceItems.Add(new TargetAudienceModel() { TargetAudience = TargetAudience.Friends, Title = "Friends", Text = "This question will be visible to all your friends!", ImageName = "Friends" });
            TargetAudienceItems.Add(new TargetAudienceModel() { TargetAudience = TargetAudience.Private, Title = "Choose Friends", Text = "This question will be visible to the selected friends below:", ImageName = "Lock" });
        }
    }
}

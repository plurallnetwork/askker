using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Models
{
    public class UserModel
    {
        public string userName { get; set; }

        public string id { get; set; }

        public string name { get; set; }

        public int? age { get; set; }

        public string gender { get; set; }

        public string city { get; set; }

        public string state { get; set; }

        public string country { get; set; }

        public string education { get; set; }

        public string profilePicturePath { get; set; }

        public bool isActive { get; set; }

        public bool isShowingTour { get; set; }

        public string description { get; set; }

        public string descriptionPrivacyLevel { get; set; }

        public string profilePicturePrivacyLevel { get; set; }

        public string namePrivacyLevel { get; set; }

        public string genderPrivacyLevel { get; set; }

        public string emailPrivacyLevel { get; set; }

        public string agePrivacyLevel { get; set; }

        public string questionsMadePrivacyLevel { get; set; }

        public string answersGivenPrivacyLevel { get; set; }

        public string influencedByPrivacyLevel { get; set; }

        public string influenceOverPrivacyLevel { get; set; }
    }
}

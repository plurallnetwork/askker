using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Models
{
    public class SurveyCommentModel
    {
        public string surveyId { get; set; }

        public string commentDate { get; set; }

        public string userId { get; set; }

        public string userName { get; set; }

        public string profilePicture { get; set; }

        public string text { get; set; }

        public bool? userLiked { get; set; }

        public int totalLikes { get; set; }
    }
}

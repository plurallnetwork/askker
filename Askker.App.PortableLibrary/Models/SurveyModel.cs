using Askker.App.PortableLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Models
{
    public class SurveyModel
    {
        public string userId { get; set; }

        public string userName { get; set; }

        public string profilePicture { get; set; }

        public string creationDate { get; set; }

        public string type { get; set; }

        public string choiceType { get; set; }

        public int isArchived { get; set; }

        public Question question { get; set; }

        public List<Option> options { get; set; }

        public List<ColumnOption> columnOptions { get; set; }

        public string finishDate { get; set; }

        public int totalVotes { get; set; }

        public int totalComments { get; set; }

        public int totalLikes { get; set; }

        public int? optionSelected { get; set; }

        public bool? userLiked { get; set; }

        public string targetAudience { get; set; }

        public AudienceUsers targetAudienceUsers { get; set; }

        public AudienceGroups targetAudienceGroups { get; set; }

        public List<string> reportedByUsersIds { get; set; }
    }

    public class Question
    {
        public string text { get; set; }
        public string image { get; set; }
    }

    public class Option
    {
        public int id { get; set; }
        public string text { get; set; }
        public string image { get; set; }
    }

    public class ColumnOption
    {
        public int id { get; set; }
        public string text { get; set; }
    }

    public class AudienceUsers
    {
        public List<string> ids { get; set; }
        public List<string> names { get; set; }
    }

    public class AudienceGroups
    {
        public List<string> ids { get; set; }
        public List<string> names { get; set; }
    }
}

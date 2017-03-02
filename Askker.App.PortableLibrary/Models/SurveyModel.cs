using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Models
{
    public class SurveyModel
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public string profilePicture { get; set; }

        public string CreationDate { get; set; }

        public string Type { get; set; }

        public string ChoiceType { get; set; }

        public int IsArchived { get; set; }

        public Question Question { get; set; }

        public List<Option> Options { get; set; }

        public List<ColumnOption> ColumnOptions { get; set; }

        public string FinishDate { get; set; }

        public int TotalVotes { get; set; }
    }

    public class Question
    {
        public string Text { get; set; }
        public string Image { get; set; }
    }

    public class Option
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Image { get; set; }
    }

    public class ColumnOption
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}

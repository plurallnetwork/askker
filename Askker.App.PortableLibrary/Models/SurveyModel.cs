using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Models
{
    public sealed class SurveyModel
    {
        public string userId { get; set; }

        public string creationDate { get; set; }

        public string type { get; set; }

        public string choiceType { get; set; }

        public bool isArchived { get; set; }

        public Question question { get; set; }

        public List<Option> options { get; set; }

        public List<ColumnOption> columnOptions { get; set; }

        public DateTime finishDate { get; set; }

        public int totalVotes { get; set; }
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
}

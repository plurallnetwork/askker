using System.Collections.Generic;

namespace Askker.App.PortableLibrary.Models
{
    public class ReportModel
    {
        public List<List<int>> dataSets { get; set; }

        public List<string> labels { get; set; }

        public List<string> groups { get; set; }

        public int totalVotes { get; set; }
    }

    public class ReportDataSet
    {
        public List<int> data { get; set; }
        public List<string> labels { get; set; }
        public int totalVotes { get; set; }
    }
}

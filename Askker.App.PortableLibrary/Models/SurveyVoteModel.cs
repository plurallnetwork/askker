﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Models
{
    public class SurveyVoteModel
    {
        public string surveyId { get; set; }

        public int optionId { get; set; }

        public string voteDate { get; set; }

        public User user { get; set; }

        public int active { get; set; }
    }

    public class User
    {
        public string id { get; set; }

        public string name { get; set; }

        public string profilePicture { get; set; }

        public string gender { get; set; }

        public int? age { get; set; }

        public string city { get; set; }

        public string country { get; set; }
    }
}

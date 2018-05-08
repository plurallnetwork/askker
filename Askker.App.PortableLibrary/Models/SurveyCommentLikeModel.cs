using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Models
{
    public class SurveyCommentLikeModel
    {
        public string commentId { get; set; }

        public User user { get; set; }
    }
}

using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Business
{
    public class FeedManager
    {
        public async Task<List<SurveyModel>> GetSurveys(string userId, string authenticationToken)
        {
            try
            {
                FeedService feedService = new FeedService();

                var response = await feedService.GetSurveys(userId, authenticationToken);
                return JsonConvert.DeserializeObject<List<SurveyModel>>(response);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

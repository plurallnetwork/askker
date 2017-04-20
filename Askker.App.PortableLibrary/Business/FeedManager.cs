using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Business
{
    public class FeedManager
    {
        public async Task<List<SurveyModel>> GetFeed(string userId, string filter, string authenticationToken)
        {
            try
            {
                FeedService feedService = new FeedService();

                var response = await feedService.GetFeed(userId, filter, authenticationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<SurveyModel>>(json);
                }
                else
                {
                    throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<SurveyModel> GetSurvey(string userId, string creationDate, string authenticationToken)
        {
            try
            {
                FeedService feedService = new FeedService();

                var response = await feedService.GetSurvey(userId, creationDate, authenticationToken);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<SurveyModel>(json);
                }
                else
                {
                    throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task SaveSurvey(SurveyModel surveyModel, string authenticationToken, Stream questionImage, List<KeyValuePair<string, byte[]>> optionImages)
        {
            try
            {
                FeedService feedService = new FeedService();

                var response = await feedService.SaveSurvey(surveyModel, authenticationToken, questionImage, optionImages);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

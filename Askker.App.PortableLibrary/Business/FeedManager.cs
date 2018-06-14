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
        public async Task<List<SurveyModel>> GetFeed(string userId, bool filterMine, bool filterForMe, bool filterFinished, string authenticationToken)
        {
            try
            {
                FeedService feedService = new FeedService();

                var response = await feedService.GetFeed(userId, filterMine, filterForMe, filterFinished, authenticationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<SurveyModel>>(json);
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Unauthorized");
                    }
                    else
                    {
                        throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                    }
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
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Unauthorized");
                    }
                    else
                    {
                        throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                    }
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
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Unauthorized");
                    }
                    else
                    {
                        throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task FinishSurvey(SurveyModel surveyModel, string authenticationToken)
        {
            try
            {
                FeedService feedService = new FeedService();

                var response = await feedService.FinishSurvey(surveyModel, authenticationToken);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Unauthorized");
                    }
                    else
                    {
                        throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task CleanVotes(string surveyId, string authenticationToken)
        {
            try
            {
                FeedService feedService = new FeedService();

                var response = await feedService.CleanVotes(surveyId, authenticationToken);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Unauthorized");
                    }
                    else
                    {
                        throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteSurvey(string surveyId, string authenticationToken)
        {
            try
            {
                FeedService feedService = new FeedService();

                var response = await feedService.DeleteSurvey(surveyId, authenticationToken);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Unauthorized");
                    }
                    else
                    {
                        throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task ReportSurvey(string userId, string surveyId, string authenticationToken)
        {
            try
            {
                FeedService feedService = new FeedService();

                var response = await feedService.ReportSurvey(userId, surveyId, authenticationToken);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Unauthorized");
                    }
                    else
                    {
                        throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
